using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TubePilot.Application.DTOs;
using TubePilot.Application.Interfaces;
using TubePilot.Domain.Entities;

namespace TubePilot.API.Controllers
{
    // Sprint 5: the doc's own flagged highest-risk sprint. Loops over the project's
    // OutputSpec.Items, makes one scoped AI call per item, parses the response according
    // to that item's Type, and writes it to disk + DB — driven entirely by the spec,
    // not by code that assumes any particular shape.
    [ApiController]
    [Route("api/generate")]
    [Authorize]
    public class GenerateController : ControllerBase
    {
        private readonly IProjectRepository _projectRepo;
        private readonly IProjectOutputRepository _outputRepo;
        private readonly IDataRowRepository _dataRowRepo;
        private readonly IGenerationRecordRepository _generationRecordRepo;
        private readonly IUserApiKeyRepository _apiKeyRepo;
        private readonly IApiKeyEncryptionService _encryption;
        private readonly IAiProviderFactory _providerFactory;
        private readonly IFileSystemService _fileSystem;
        private readonly IPromptRepository _promptRepo;

        public GenerateController(
            IProjectRepository projectRepo,
            IProjectOutputRepository outputRepo,
            IDataRowRepository dataRowRepo,
            IGenerationRecordRepository generationRecordRepo,
            IUserApiKeyRepository apiKeyRepo,
            IApiKeyEncryptionService encryption,
            IAiProviderFactory providerFactory,
            IFileSystemService fileSystem,
            IPromptRepository promptRepo)
        {
            _projectRepo = projectRepo;
            _outputRepo = outputRepo;
            _dataRowRepo = dataRowRepo;
            _generationRecordRepo = generationRecordRepo;
            _apiKeyRepo = apiKeyRepo;
            _encryption = encryption;
            _providerFactory = providerFactory;
            _fileSystem = fileSystem;
            _promptRepo = promptRepo;
        }

        private Guid CurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(idClaim, out var id) ? id : Guid.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> Generate([FromBody] GenerateRequest request, CancellationToken cancellationToken)
        {
            var userId = CurrentUserId();
            var project = await _projectRepo.GetByIdAsync(request.ProjectId, cancellationToken);
            if (project == null || project.UserId != userId)
            {
                return NotFound(new ApiResponse<string> { Success = false, Message = "Project not found." });
            }

            if (project.PromptId == null)
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = "Project has no associated prompt to generate from." });
            }

            var prompt = await _promptRepo.GetByIdAsync(project.PromptId.Value, cancellationToken);
            if (prompt == null)
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = "Associated prompt no longer exists." });
            }

            var spec = JsonSerializer.Deserialize<OutputSpec>(project.OutputSpecJson, OutputSpecJsonOptions.Default)
                ?? new OutputSpec();

            var savedKey = await _apiKeyRepo.GetAsync(userId, request.ProviderName, cancellationToken);
            if (savedKey == null)
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = $"No saved API key for provider '{request.ProviderName}'." });
            }
            var apiKey = _encryption.Decrypt(savedKey.EncryptedKey);
            var provider = _providerFactory.GetProvider(request.ProviderName);

            var filledPrompt = FillTopic(prompt.PromptText, project.Title);
            var result = new GenerateResultDto { ProjectId = project.Id };

            foreach (var item in spec.Items)
            {
                if (item.Type == OutputItemType.ImageSet)
                {
                    // Handled by the image review workflow (Sprint 6), not generated here.
                    continue;
                }

                var systemPrompt = BuildScopedSystemPrompt(item);
                var aiRequest = new AiRequest(
                    SystemPrompt: systemPrompt,
                    UserPrompt: filledPrompt,
                    Model: request.Model ?? string.Empty,
                    Temperature: 0.7f,
                    MaxTokens: 2048
                );

                AiResponse aiResponse;
                try
                {
                    aiResponse = await provider.GenerateAsync(apiKey, aiRequest, cancellationToken);
                }
                catch (InvalidOperationException ex)
                {
                    return StatusCode(StatusCodes.Status502BadGateway, new ApiResponse<string>
                    {
                        Success = false,
                        Message = $"Generation failed for '{item.Name}': {ex.Message}"
                    });
                }

                await _generationRecordRepo.AddAsync(new GenerationRecord
                {
                    ProjectId = project.Id,
                    OutputItemName = item.Name,
                    ProviderUsed = aiResponse.ProviderName,
                    ModelUsed = aiResponse.Model,
                    InputTokens = aiResponse.InputTokens,
                    OutputTokens = aiResponse.OutputTokens
                }, cancellationToken);

                string filePath;
                int rowCount = 0;

                if (item.Type == OutputItemType.Table)
                {
                    List<Dictionary<string, string>> rows;
                    try
                    {
                        rows = ParseTableRows(aiResponse.Content);
                    }
                    catch (JsonException ex)
                    {
                        return StatusCode(StatusCodes.Status502BadGateway, new ApiResponse<string>
                        {
                            Success = false,
                            Message = $"AI response for '{item.Name}' was not valid structured JSON: {ex.Message}"
                        });
                    }

                    filePath = _fileSystem.WriteOutputItem(project.FolderPath, item, rows);
                    rowCount = rows.Count;

                    var outputEntity = new ProjectOutput
                    {
                        ProjectId = project.Id,
                        OutputItemName = item.Name,
                        Type = ProjectOutputType.Table,
                        FolderName = item.FolderName,
                        FilePath = filePath
                    };
                    await _outputRepo.AddAsync(outputEntity, cancellationToken);

                    var dataRows = rows.Select((row, index) => new DataRow
                    {
                        ProjectOutputId = outputEntity.Id,
                        RowIndex = index,
                        DataJson = JsonSerializer.Serialize(row, OutputSpecJsonOptions.Default),
                        ImageStatus = item.RequiresImages ? ImageStatus.Pending : ImageStatus.NotRequired
                    });
                    await _dataRowRepo.AddRangeAsync(dataRows, cancellationToken);
                }
                else
                {
                    var text = aiResponse.Content.Trim();
                    filePath = _fileSystem.WriteOutputItem(project.FolderPath, item, text);

                    await _outputRepo.AddAsync(new ProjectOutput
                    {
                        ProjectId = project.Id,
                        OutputItemName = item.Name,
                        Type = ProjectOutputType.Text,
                        FolderName = item.FolderName,
                        FilePath = filePath
                    }, cancellationToken);
                }

                result.Outputs.Add(new GeneratedOutputDto
                {
                    OutputItemName = item.Name,
                    Type = item.Type.ToString(),
                    FolderName = item.FolderName,
                    FilePath = filePath,
                    RowCount = rowCount
                });
            }

            await _apiKeyRepo.TouchLastUsedAsync(userId, request.ProviderName, cancellationToken);

            return Ok(new ApiResponse<GenerateResultDto> { Success = true, Data = result });
        }

        private static string FillTopic(string template, string topic)
        {
            return template
                .Replace("{{topic}}", topic, StringComparison.OrdinalIgnoreCase)
                .Replace("{topic}", topic, StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildScopedSystemPrompt(OutputItem item)
        {
            if (item.Type == OutputItemType.Table)
            {
                var columns = string.Join(", ", item.Columns);
                return $"The prompt below may describe several distinct pieces of content. You are generating ONLY the table named " +
                       $"\"{item.Name}\" — ignore instructions relating to any other named part. " +
                       "You must respond with ONLY a raw JSON array of objects — no markdown, no code fences, no explanation before or after. " +
                       $"Each object must have exactly these keys: {columns}. " +
                       "Generate a realistic, well-researched set of items (aim for 8-10) for the topic given by the user.";
            }

            return $"The prompt below may describe several distinct pieces of content. You are generating ONLY the part named " +
                   $"\"{item.Name}\" — ignore instructions relating to any other named part. " +
                   "Write plain text only for that one part. No markdown formatting, no headers, no code fences, no labels like " +
                   $"\"{item.Name}:\" — just the content itself, plain prose only.";
        }

        private static List<Dictionary<string, string>> ParseTableRows(string content)
        {
            var cleaned = StripCodeFences(content);
            using var doc = JsonDocument.Parse(cleaned);
            var rows = new List<Dictionary<string, string>>();

            foreach (var element in doc.RootElement.EnumerateArray())
            {
                var row = new Dictionary<string, string>();
                foreach (var prop in element.EnumerateObject())
                {
                    row[prop.Name] = prop.Value.ValueKind switch
                    {
                        JsonValueKind.String => prop.Value.GetString() ?? string.Empty,
                        JsonValueKind.Null => string.Empty,
                        _ => prop.Value.GetRawText()
                    };
                }
                rows.Add(row);
            }

            return rows;
        }

        private static string StripCodeFences(string content)
        {
            var trimmed = content.Trim();
            if (trimmed.StartsWith("```"))
            {
                var firstNewline = trimmed.IndexOf('\n');
                if (firstNewline >= 0) trimmed = trimmed[(firstNewline + 1)..];
                if (trimmed.EndsWith("```")) trimmed = trimmed[..^3];
            }
            return trimmed.Trim();
        }
    }
}
