using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TubePilot.Application.DTOs;
using TubePilot.Application.Interfaces;
using TubePilot.Domain.Entities;

namespace TubePilot.API.Controllers
{
    // Sprint 6: real per-row image search + user review/select workflow. The image query
    // is built from whatever columns the prompt's OutputSpec declared for that item
    // (ImageQueryColumns) — no hardcoded per-category query logic.
    [ApiController]
    [Route("api/projects/{projectId:guid}/rows")]
    [Authorize]
    public class RowsController : ControllerBase
    {
        private readonly IProjectRepository _projectRepo;
        private readonly IProjectOutputRepository _outputRepo;
        private readonly IDataRowRepository _dataRowRepo;
        private readonly IImageSearchProvider _imageSearch;
        private readonly IImageGenerationProvider _imageGeneration;
        private readonly IFileSystemService _fileSystem;
        private readonly IHttpClientFactory _httpClientFactory;

        public RowsController(
            IProjectRepository projectRepo,
            IProjectOutputRepository outputRepo,
            IDataRowRepository dataRowRepo,
            IImageSearchProvider imageSearch,
            IImageGenerationProvider imageGeneration,
            IFileSystemService fileSystem,
            IHttpClientFactory httpClientFactory)
        {
            _projectRepo = projectRepo;
            _outputRepo = outputRepo;
            _dataRowRepo = dataRowRepo;
            _imageSearch = imageSearch;
            _imageGeneration = imageGeneration;
            _fileSystem = fileSystem;
            _httpClientFactory = httpClientFactory;
        }

        private Guid CurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(idClaim, out var id) ? id : Guid.Empty;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(Guid projectId, CancellationToken cancellationToken)
        {
            var project = await _projectRepo.GetByIdAsync(projectId, cancellationToken);
            if (project == null || project.UserId != CurrentUserId())
            {
                return NotFound(new ApiResponse<string> { Success = false, Message = "Project not found." });
            }

            var outputs = await _outputRepo.GetForProjectAsync(projectId, cancellationToken);
            var tableOutputs = outputs.Where(o => o.Type == ProjectOutputType.Table).ToList();

            var result = new List<DataRowDto>();
            foreach (var output in tableOutputs)
            {
                var rows = await _dataRowRepo.GetForProjectOutputAsync(output.Id, cancellationToken);
                result.AddRange(rows.Select(r => ToDto(r, output.OutputItemName)));
            }

            return Ok(new ApiResponse<IEnumerable<DataRowDto>> { Success = true, Data = result });
        }

        [HttpGet("~/api/projects/{projectId:guid}/ready-for-video")]
        public async Task<IActionResult> ReadyForVideo(Guid projectId, CancellationToken cancellationToken)
        {
            var project = await _projectRepo.GetByIdAsync(projectId, cancellationToken);
            if (project == null || project.UserId != CurrentUserId())
            {
                return NotFound(new ApiResponse<string> { Success = false, Message = "Project not found." });
            }

            var outputs = await _outputRepo.GetForProjectAsync(projectId, cancellationToken);
            var ready = true;
            foreach (var output in outputs.Where(o => o.Type == ProjectOutputType.Table))
            {
                var rows = await _dataRowRepo.GetForProjectOutputAsync(output.Id, cancellationToken);
                if (rows.Any(r => r.ImageStatus == ImageStatus.Pending || r.ImageStatus == ImageStatus.CandidatesFetched))
                {
                    ready = false;
                    break;
                }
            }

            return Ok(new ApiResponse<bool> { Success = true, Data = ready });
        }

        [HttpPost("{rowId:guid}/image-candidates")]
        public async Task<IActionResult> GetImageCandidates(Guid projectId, Guid rowId, [FromBody] ImageCandidatesRequest? request, CancellationToken cancellationToken)
        {
            var context = await LoadRowContextAsync(projectId, rowId, cancellationToken);
            if (context == null)
            {
                return NotFound(new ApiResponse<string> { Success = false, Message = "Row not found." });
            }

            var query = !string.IsNullOrWhiteSpace(request?.Query)
                ? request!.Query!
                : BuildImageQuery(context.RowData, context.Item);

            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = "Could not build a search query for this row — no ImageQueryColumns matched any data." });
            }

            var candidates = await _imageSearch.SearchAsync(query, 6, cancellationToken);

            context.Row.ImageStatus = ImageStatus.CandidatesFetched;
            await _dataRowRepo.UpdateAsync(context.Row, cancellationToken);

            return Ok(new ApiResponse<ImageCandidatesResponseDto>
            {
                Success = true,
                Data = new ImageCandidatesResponseDto
                {
                    RowId = rowId,
                    Query = query,
                    Candidates = candidates.Select(c => new ImageCandidateDto
                    {
                        ThumbnailUrl = c.ThumbnailUrl,
                        FullSizeUrl = c.FullSizeUrl,
                        SourceUrl = c.SourceUrl,
                        License = c.License
                    }).ToList()
                }
            });
        }

        [HttpPost("{rowId:guid}/image-select")]
        public async Task<IActionResult> SelectImage(Guid projectId, Guid rowId, [FromBody] ImageSelectRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.FullSizeUrl))
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = "fullSizeUrl is required." });
            }

            var context = await LoadRowContextAsync(projectId, rowId, cancellationToken);
            if (context == null)
            {
                return NotFound(new ApiResponse<string> { Success = false, Message = "Row not found." });
            }

            var httpClient = _httpClientFactory.CreateClient();
            // Image hosts (e.g. Wikimedia's upload.wikimedia.org) reject anonymous requests
            // with no User-Agent — same requirement as the search API itself.
            if (httpClient.DefaultRequestHeaders.UserAgent.Count == 0)
            {
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("TubePilotAI/1.0 (https://github.com/tubepilotai; contact@tubepilotai.local)");
            }

            byte[] imageBytes;
            try
            {
                imageBytes = await httpClient.GetByteArrayAsync(request.FullSizeUrl, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway, new ApiResponse<string> { Success = false, Message = $"Failed to download the selected image: {ex.Message}" });
            }

            var extension = Path.GetExtension(new Uri(request.FullSizeUrl).LocalPath);
            if (string.IsNullOrEmpty(extension)) extension = ".jpg";
            var fileName = $"{rowId:N}{extension}";

            string savedPath;
            using (var stream = new MemoryStream(imageBytes))
            {
                savedPath = await _fileSystem.SaveImageAsync(context.Project.FolderPath, fileName, stream, cancellationToken);
            }

            context.Row.ConfirmedImagePath = savedPath;
            context.Row.ImageStatus = ImageStatus.Confirmed;
            await _dataRowRepo.UpdateAsync(context.Row, cancellationToken);

            return Ok(new ApiResponse<DataRowDto> { Success = true, Data = ToDto(context.Row, context.Output.OutputItemName) });
        }

        [HttpPost("{rowId:guid}/image-generate")]
        public async Task<IActionResult> GenerateImage(Guid projectId, Guid rowId, [FromBody] ImageGenerateRequest? request, CancellationToken cancellationToken)
        {
            var context = await LoadRowContextAsync(projectId, rowId, cancellationToken);
            if (context == null)
            {
                return NotFound(new ApiResponse<string> { Success = false, Message = "Row not found." });
            }

            var prompt = !string.IsNullOrWhiteSpace(request?.Prompt)
                ? request!.Prompt!
                : BuildImageGenerationPrompt(context.RowData, context.Item);

            byte[] imageBytes;
            try
            {
                imageBytes = await _imageGeneration.GenerateAsync(prompt, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway, new ApiResponse<string> { Success = false, Message = $"Image generation failed: {ex.Message}" });
            }

            var fileName = $"{rowId:N}.jpg";
            string savedPath;
            using (var stream = new MemoryStream(imageBytes))
            {
                savedPath = await _fileSystem.SaveImageAsync(context.Project.FolderPath, fileName, stream, cancellationToken);
            }

            context.Row.ConfirmedImagePath = savedPath;
            context.Row.ImageStatus = ImageStatus.Confirmed;
            await _dataRowRepo.UpdateAsync(context.Row, cancellationToken);

            return Ok(new ApiResponse<DataRowDto> { Success = true, Data = ToDto(context.Row, context.Output.OutputItemName) });
        }

        private static string BuildImageGenerationPrompt(Dictionary<string, string> row, OutputItem item)
        {
            var subject = BuildImageQuery(row, item);
            return $"A high-quality, relevant illustrative image for \"{subject}\", photorealistic, professional composition, high detail, no text, no watermark, no logos";
        }

        private static string BuildImageQuery(Dictionary<string, string> row, OutputItem item)
        {
            var baseQuery = string.Join(" ", item.ImageQueryColumns
                .Select(col => row.GetValueOrDefault(col, string.Empty))
                .Where(v => !string.IsNullOrWhiteSpace(v)));

            return string.IsNullOrWhiteSpace(item.ImageQuerySuffix)
                ? baseQuery
                : $"{baseQuery} {item.ImageQuerySuffix}";
        }

        private async Task<RowContext?> LoadRowContextAsync(Guid projectId, Guid rowId, CancellationToken cancellationToken)
        {
            var project = await _projectRepo.GetByIdAsync(projectId, cancellationToken);
            if (project == null || project.UserId != CurrentUserId()) return null;

            var row = await _dataRowRepo.GetByIdAsync(rowId, cancellationToken);
            if (row == null) return null;

            var output = await _outputRepo.GetByIdAsync(row.ProjectOutputId, cancellationToken);
            if (output == null || output.ProjectId != projectId) return null;

            var spec = JsonSerializer.Deserialize<OutputSpec>(project.OutputSpecJson, OutputSpecJsonOptions.Default) ?? new OutputSpec();
            var item = spec.Items.FirstOrDefault(i => i.Name == output.OutputItemName);
            if (item == null) return null;

            var rowData = JsonSerializer.Deserialize<Dictionary<string, string>>(row.DataJson, OutputSpecJsonOptions.Default) ?? new Dictionary<string, string>();

            return new RowContext(project, output, row, item, rowData);
        }

        private static DataRowDto ToDto(DataRow row, string outputItemName)
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(row.DataJson, OutputSpecJsonOptions.Default) ?? new Dictionary<string, string>();
            return new DataRowDto
            {
                Id = row.Id,
                ProjectOutputId = row.ProjectOutputId,
                OutputItemName = outputItemName,
                RowIndex = row.RowIndex,
                Data = data,
                ImageStatus = row.ImageStatus.ToString(),
                ConfirmedImagePath = row.ConfirmedImagePath
            };
        }

        private record RowContext(Project Project, ProjectOutput Output, DataRow Row, OutputItem Item, Dictionary<string, string> RowData);
    }
}
