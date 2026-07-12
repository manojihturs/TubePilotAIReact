using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TubePilot.Application.DTOs;
using TubePilot.Application.Interfaces;
using TubePilot.Domain.Entities;

namespace TubePilot.API.Controllers
{
    // Level 1: confirmed rows (image + narration + text overlay) become scenes, assembled
    // into a scrollable/countdown video via FFmpeg. Rendering only starts once every
    // image-requiring row is confirmed. Genuinely async (minutes, not seconds) — kicked off
    // as a background task and polled via RenderJobs rather than blocking the request.
    [ApiController]
    [Route("api/projects/{projectId:guid}/render")]
    [Authorize]
    public class RenderController : ControllerBase
    {
        private readonly IProjectRepository _projectRepo;
        private readonly IProjectOutputRepository _outputRepo;
        private readonly IDataRowRepository _dataRowRepo;
        private readonly IRenderJobRepository _renderJobRepo;
        private readonly IVideoRenderingService _videoRendering;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RenderController> _logger;

        public RenderController(
            IProjectRepository projectRepo,
            IProjectOutputRepository outputRepo,
            IDataRowRepository dataRowRepo,
            IRenderJobRepository renderJobRepo,
            IVideoRenderingService videoRendering,
            IServiceScopeFactory scopeFactory,
            ILogger<RenderController> logger)
        {
            _projectRepo = projectRepo;
            _outputRepo = outputRepo;
            _dataRowRepo = dataRowRepo;
            _renderJobRepo = renderJobRepo;
            _videoRendering = videoRendering;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        private Guid CurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(idClaim, out var id) ? id : Guid.Empty;
        }

        [HttpGet]
        public async Task<IActionResult> GetJobs(Guid projectId, CancellationToken cancellationToken)
        {
            var project = await _projectRepo.GetByIdAsync(projectId, cancellationToken);
            if (project == null || project.UserId != CurrentUserId())
            {
                return NotFound(new ApiResponse<string> { Success = false, Message = "Not found" });
            }

            var jobs = await _renderJobRepo.GetForProjectAsync(projectId, cancellationToken);
            return Ok(new ApiResponse<IEnumerable<RenderJobDto>> { Success = true, Data = jobs.Select(ToDto) });
        }

        [HttpGet("availability")]
        public async Task<IActionResult> CheckAvailability(CancellationToken cancellationToken)
        {
            var available = await _videoRendering.IsAvailableAsync(cancellationToken);
            return Ok(new ApiResponse<bool> { Success = true, Data = available, Message = available ? null : "FFmpeg was not found on this server. Video rendering cannot run until it's installed." });
        }

        [HttpPost]
        public async Task<IActionResult> StartRender(Guid projectId, [FromBody] RenderRequest request, CancellationToken cancellationToken)
        {
            var project = await _projectRepo.GetByIdAsync(projectId, cancellationToken);
            if (project == null || project.UserId != CurrentUserId())
            {
                return NotFound(new ApiResponse<string> { Success = false, Message = "Not found" });
            }

            if (!Enum.TryParse<RenderFormat>(request.Format, true, out var format))
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = "Format must be 'Desktop' or 'Shorts'." });
            }

            if (format == RenderFormat.Shorts && request.DurationSeconds > 60)
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = "Shorts/Reels duration cannot exceed 60 seconds." });
            }

            if (!await _videoRendering.IsAvailableAsync(cancellationToken))
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new ApiResponse<string>
                {
                    Success = false,
                    Message = "FFmpeg is not installed on this server. Install it and restart the API before rendering."
                });
            }

            var outputs = await _outputRepo.GetForProjectAsync(projectId, cancellationToken);
            var tableOutputs = outputs.Where(o => o.Type == ProjectOutputType.Table).ToList();
            var allConfirmed = true;
            var confirmedRowCount = 0;
            foreach (var output in tableOutputs)
            {
                var rows = await _dataRowRepo.GetForProjectOutputAsync(output.Id, cancellationToken);
                var imageRows = rows.Where(r => r.ImageStatus != ImageStatus.NotRequired).ToList();
                if (imageRows.Any(r => r.ImageStatus != ImageStatus.Confirmed)) allConfirmed = false;
                confirmedRowCount += imageRows.Count(r => r.ImageStatus == ImageStatus.Confirmed);
            }

            if (!allConfirmed)
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = "Every image-requiring row must be confirmed before rendering." });
            }

            if (confirmedRowCount == 0)
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = "No confirmed rows to render." });
            }

            var job = new RenderJob
            {
                ProjectId = projectId,
                Format = format,
                DurationSeconds = format == RenderFormat.Shorts ? Math.Min(request.DurationSeconds, 60) : request.DurationSeconds,
                Language = string.IsNullOrWhiteSpace(request.Language) ? "en" : request.Language,
                Status = RenderStatus.Queued
            };
            await _renderJobRepo.AddAsync(job, cancellationToken);

            // Fire-and-forget on its own DI scope — the HTTP request's scoped services
            // (DbContext etc.) are disposed as soon as this action returns.
            _ = Task.Run(() => RunRenderJobAsync(job.Id), CancellationToken.None);

            return Accepted(new ApiResponse<RenderJobDto> { Success = true, Data = ToDto(job) });
        }

        private async Task RunRenderJobAsync(Guid jobId)
        {
            using var scope = _scopeFactory.CreateScope();
            var provider = scope.ServiceProvider;
            var renderJobRepo = provider.GetRequiredService<IRenderJobRepository>();
            var projectRepo = provider.GetRequiredService<IProjectRepository>();
            var outputRepo = provider.GetRequiredService<IProjectOutputRepository>();
            var dataRowRepo = provider.GetRequiredService<IDataRowRepository>();
            var sceneComposer = provider.GetRequiredService<ISceneComposer>();
            var tts = provider.GetRequiredService<ITextToSpeechProvider>();
            var backgroundMusic = provider.GetRequiredService<IBackgroundMusicProvider>();
            var videoRendering = provider.GetRequiredService<IVideoRenderingService>();

            var job = await renderJobRepo.GetByIdAsync(jobId);
            if (job == null) return;

            try
            {
                job.Status = RenderStatus.Rendering;
                await renderJobRepo.UpdateAsync(job);

                var project = await projectRepo.GetByIdAsync(job.ProjectId)
                    ?? throw new InvalidOperationException("Project no longer exists.");

                var spec = JsonSerializer.Deserialize<OutputSpec>(project.OutputSpecJson, OutputSpecJsonOptions.Default) ?? new OutputSpec();
                var outputs = await outputRepo.GetForProjectAsync(job.ProjectId);

                // Gather confirmed rows across every Table output, in row order.
                var scenePaths = new List<string>();
                var tempDir = Path.Combine(project.FolderPath, "Export", $"_scenes_{job.Id:N}");
                Directory.CreateDirectory(tempDir);

                var rank = 1;
                foreach (var output in outputs.Where(o => o.Type == ProjectOutputType.Table))
                {
                    var item = spec.Items.FirstOrDefault(i => i.Name == output.OutputItemName);
                    var rows = await dataRowRepo.GetForProjectOutputAsync(output.Id);
                    foreach (var row in rows.Where(r => r.ImageStatus == ImageStatus.Confirmed && r.ConfirmedImagePath != null))
                    {
                        var data = JsonSerializer.Deserialize<Dictionary<string, string>>(row.DataJson, OutputSpecJsonOptions.Default) ?? new();
                        var title = item?.Columns.Count > 0 && data.TryGetValue(item.Columns[0], out var t) ? t : "Untitled";
                        var badge = item?.Columns.Count > 2 && data.TryGetValue(item.Columns[2], out var b) ? b : null;

                        var scenePath = Path.Combine(tempDir, $"scene-{rank:D3}.jpg");
                        await sceneComposer.ComposeSceneAsync(
                            row.ConfirmedImagePath!,
                            new SceneTextOverlay(rank, title, badge),
                            job.Language,
                            scenePath,
                            job.Format == RenderFormat.Shorts ? 1080 : 1920,
                            job.Format == RenderFormat.Shorts ? 1920 : 1080);
                        scenePaths.Add(scenePath);
                        rank++;
                    }
                }

                if (scenePaths.Count == 0)
                {
                    throw new InvalidOperationException("No confirmed row images available to render.");
                }

                // Narration: prefer an output literally named "Narration", else the first Text output.
                var narrationOutput = outputs.FirstOrDefault(o => o.Type == ProjectOutputType.Text && o.OutputItemName.Equals("Narration", StringComparison.OrdinalIgnoreCase))
                    ?? outputs.FirstOrDefault(o => o.Type == ProjectOutputType.Text);
                var narrationText = narrationOutput != null && System.IO.File.Exists(narrationOutput.FilePath)
                    ? await System.IO.File.ReadAllTextAsync(narrationOutput.FilePath)
                    : string.Join(". ", scenePaths.Select((_, i) => $"Number {i + 1}"));

                var audioBytes = await tts.SynthesizeAsync(narrationText, job.Language);
                var musicPath = await backgroundMusic.GetTrackPathAsync(cancellationToken: CancellationToken.None);

                var exportDir = Path.Combine(project.FolderPath, "Export");
                var outputPath = await videoRendering.RenderAsync(scenePaths, audioBytes, job.Format, job.DurationSeconds, exportDir, musicPath);

                job.Status = RenderStatus.Complete;
                job.OutputPath = outputPath;
                job.CompletedAt = DateTime.UtcNow;
                await renderJobRepo.UpdateAsync(job);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Render job {JobId} failed", jobId);
                job.Status = RenderStatus.Failed;
                job.ErrorMessage = ex.Message;
                job.CompletedAt = DateTime.UtcNow;
                await renderJobRepo.UpdateAsync(job);
            }
        }

        private static RenderJobDto ToDto(RenderJob job) => new()
        {
            Id = job.Id,
            ProjectId = job.ProjectId,
            Format = job.Format.ToString(),
            DurationSeconds = job.DurationSeconds,
            Language = job.Language,
            Status = job.Status.ToString(),
            OutputPath = job.OutputPath,
            ErrorMessage = job.ErrorMessage,
            CreatedAt = job.CreatedAt,
            CompletedAt = job.CompletedAt
        };
    }
}
