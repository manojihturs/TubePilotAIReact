using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TubePilot.Application.DTOs;
using TubePilot.Application.Interfaces;
using TubePilot.Domain.Entities;

namespace TubePilot.API.Controllers
{
    // Sprint 3: Projects table + real folder tree on disk.
    // Sprint 4: folder shape now comes from the selected prompt's own OutputSpecJson —
    // falls back to a generic single-Text spec only when no prompt (or no spec) is given.
    [ApiController]
    [Route("api/projects")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectRepository _repo;
        private readonly IFileSystemService _fileSystem;
        private readonly IPromptRepository _promptRepo;
        private readonly IImageGenerationProvider _imageGeneration;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(
            IProjectRepository repo,
            IFileSystemService fileSystem,
            IPromptRepository promptRepo,
            IImageGenerationProvider imageGeneration,
            ILogger<ProjectsController> logger)
        {
            _repo = repo;
            _fileSystem = fileSystem;
            _promptRepo = promptRepo;
            _imageGeneration = imageGeneration;
            _logger = logger;
        }

        private Guid CurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(idClaim, out var id) ? id : Guid.Empty;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var userId = CurrentUserId();
            var projects = await _repo.GetAllForUserAsync(userId, cancellationToken);
            return Ok(new ApiResponse<IEnumerable<ProjectDto>> { Success = true, Data = projects.Select(ToDto) });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var project = await _repo.GetByIdAsync(id, cancellationToken);
            if (project == null || project.UserId != CurrentUserId())
            {
                return NotFound(new ApiResponse<string> { Success = false, Message = "Not found" });
            }
            return Ok(new ApiResponse<ProjectDto> { Success = true, Data = ToDto(project) });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProjectRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = "Title is required." });
            }

            var userId = CurrentUserId();
            var projectId = Guid.NewGuid();

            var spec = await ResolveOutputSpecAsync(request.PromptId, cancellationToken);

            var folderPath = _fileSystem.CreateProjectStructure(projectId, request.Title, spec);

            var project = new Project
            {
                Id = projectId,
                UserId = userId,
                PromptId = request.PromptId,
                Title = request.Title,
                FolderPath = folderPath,
                OutputSpecJson = JsonSerializer.Serialize(spec, OutputSpecJsonOptions.Default)
            };

            await _repo.AddAsync(project, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = project.Id }, new ApiResponse<ProjectDto> { Success = true, Data = ToDto(project) });
        }

        [HttpPost("{id:guid}/thumbnail")]
        public async Task<IActionResult> GenerateThumbnail(Guid id, [FromBody] GenerateThumbnailRequest? request, CancellationToken cancellationToken)
        {
            var project = await _repo.GetByIdAsync(id, cancellationToken);
            if (project == null || project.UserId != CurrentUserId())
            {
                return NotFound(new ApiResponse<string> { Success = false, Message = "Not found" });
            }

            var prompt = !string.IsNullOrWhiteSpace(request?.Prompt)
                ? request!.Prompt!
                : $"A vibrant, eye-catching YouTube thumbnail for a video titled '{project.Title}', high detail, bold text-friendly composition, relevant to the subject matter";

            byte[] imageBytes;
            try
            {
                imageBytes = await _imageGeneration.GenerateAsync(prompt, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway, new ApiResponse<string> { Success = false, Message = $"Thumbnail generation failed: {ex.Message}" });
            }

            var savedPath = await _fileSystem.SaveThumbnailAsync(project.FolderPath, imageBytes, cancellationToken);

            project.ThumbnailPath = savedPath;
            await _repo.UpdateAsync(project, cancellationToken);

            return Ok(new ApiResponse<ProjectDto> { Success = true, Data = ToDto(project) });
        }

        [HttpGet("{id:guid}/files/{**path}")]
        public async Task<IActionResult> GetFile(Guid id, string path, CancellationToken cancellationToken)
        {
            var project = await _repo.GetByIdAsync(id, cancellationToken);
            if (project == null || project.UserId != CurrentUserId())
            {
                return NotFound(new ApiResponse<string> { Success = false, Message = "Not found" });
            }

            var projectRoot = Path.GetFullPath(project.FolderPath);
            var fullPath = Path.GetFullPath(Path.Combine(projectRoot, path));

            // Path-traversal guard — the resolved path must stay inside this project's folder.
            if (!fullPath.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = "Invalid path." });
            }

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound(new ApiResponse<string> { Success = false, Message = "File not found." });
            }

            var bytes = await System.IO.File.ReadAllBytesAsync(fullPath, cancellationToken);
            return File(bytes, GetContentType(fullPath));
        }

        private static string GetContentType(string path) => Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".csv" => "text/csv",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };

        private async Task<OutputSpec> ResolveOutputSpecAsync(Guid? promptId, CancellationToken cancellationToken)
        {
            if (promptId.HasValue)
            {
                var prompt = await _promptRepo.GetByIdAsync(promptId.Value, cancellationToken);
                if (prompt != null && !string.IsNullOrWhiteSpace(prompt.OutputSpecJson))
                {
                    try
                    {
                        var parsed = JsonSerializer.Deserialize<OutputSpec>(prompt.OutputSpecJson, OutputSpecJsonOptions.Default);
                        if (parsed != null && parsed.Items.Count > 0)
                        {
                            return parsed;
                        }
                    }
                    catch (JsonException ex)
                    {
                        // Malformed spec on the prompt — fall through to the generic default
                        // rather than failing project creation outright, but never silently.
                        _logger.LogWarning(ex, "Prompt {PromptId} has an OutputSpecJson that failed to parse; using generic fallback spec.", promptId);
                    }
                }
            }

            // Generic fallback for prompts with no declared OutputSpec: a single Text item,
            // no images, no table — the smallest shape that's still valid.
            return new OutputSpec
            {
                Items = new List<OutputItem>
                {
                    new OutputItem
                    {
                        Name = "Content",
                        Type = OutputItemType.Text,
                        RequiresImages = false,
                        FolderName = "Script"
                    }
                }
            };
        }

        private static ProjectDto ToDto(Project p) => new()
        {
            Id = p.Id,
            Title = p.Title,
            FolderPath = p.FolderPath,
            ThumbnailPath = p.ThumbnailPath,
            CreatedAt = p.CreatedAt
        };
    }
}
