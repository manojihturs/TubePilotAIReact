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
        private readonly IAiToolRepository _aiToolRepo;
        private readonly IApiKeyEncryptionService _encryption;
        private readonly IAiFormatAdapterFactory _adapterFactory;
        private readonly IFileSystemService _fileSystem;
        private readonly IPromptRepository _promptRepo;

        public GenerateController(
            IProjectRepository projectRepo,
            IProjectOutputRepository outputRepo,
            IDataRowRepository dataRowRepo,
            IGenerationRecordRepository generationRecordRepo,
            IAiToolRepository aiToolRepo,
            IApiKeyEncryptionService encryption,
            IAiFormatAdapterFactory adapterFactory,
            IFileSystemService fileSystem,
            IPromptRepository promptRepo)
        {
            _projectRepo = projectRepo;
            _outputRepo = outputRepo;
            _dataRowRepo = dataRowRepo;
            _generationRecordRepo = generationRecordRepo;
            _aiToolRepo = aiToolRepo;
            _encryption = encryption;
            _adapterFactory = adapterFactory;
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

            // Fallback order: the user's own AI tools, in ascending Priority — with an
            // optionally-preferred tool moved to the front. Every tool is user-defined (name,
            // wire format, base URL, model, key); nothing here is hardcoded to a vendor. A
            // transient failure on one item (rate limit, invalid key, brief outage) tries the
            // next enabled tool instead of aborting the whole project's generation.
            var enabledTools = await _aiToolRepo.GetEnabledForUserOrderedAsync(userId, cancellationToken);
            if (request.PreferredAiToolId is Guid preferredId)
            {
                var preferred = enabledTools.FirstOrDefault(t => t.Id == preferredId);
                if (preferred != null)
                {
                    enabledTools.Remove(preferred);
                    enabledTools.Insert(0, preferred);
                }
            }

            if (enabledTools.Count == 0)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "No enabled AI tools connected. Add at least one AI tool (name, API key, and base URL) in Settings before generating."
                });
            }

            var filledPrompt = FillTopic(prompt.PromptText, project.Title);
            var result = new GenerateResultDto { ProjectId = project.Id };
            var toolsActuallyUsed = new HashSet<Guid>();

            foreach (var item in spec.Items)
            {
                if (item.Type == OutputItemType.ImageSet)
                {
                    // Handled by the image review workflow (Sprint 6), not generated here.
                    continue;
                }

                var systemPrompt = BuildScopedSystemPrompt(item);

                AiResponse? aiResponse = null;
                string? toolNameUsed = null;
                var attemptFailures = new List<string>();

                foreach (var tool in enabledTools)
                {
                    try
                    {
                        var apiKey = _encryption.Decrypt(tool.EncryptedApiKey);
                        var adapter = _adapterFactory.GetAdapter(tool.ApiFormat);
                        var toolRequest = new AiRequest(
                            SystemPrompt: systemPrompt,
                            UserPrompt: filledPrompt,
                            Model: tool.Model,
                            Temperature: 0.7f,
                            // Raised from 2048 so a long, uncapped ranking (20-30+ items) isn't
                            // truncated mid-JSON — the table item no longer limits its own length.
                            MaxTokens: 4096
                        );
                        aiResponse = await adapter.GenerateAsync(tool.BaseUrl, apiKey, tool.Model, toolRequest, cancellationToken);
                        toolNameUsed = tool.Name;
                        toolsActuallyUsed.Add(tool.Id);
                        break;
                    }
                    catch (InvalidOperationException ex)
                    {
                        attemptFailures.Add($"{tool.Name}: {ex.Message}");
                    }
                }

                if (aiResponse == null)
                {
                    // Every connected tool failed for this one item — skip it, but keep
                    // generating the rest of the project rather than aborting outright.
                    result.Warnings.Add(
                        $"Could not generate '{item.Name}' — every enabled AI tool failed. " +
                        $"Tried: {string.Join(" | ", attemptFailures)}");
                    continue;
                }

                await _generationRecordRepo.AddAsync(new GenerationRecord
                {
                    ProjectId = project.Id,
                    OutputItemName = item.Name,
                    ProviderUsed = toolNameUsed ?? aiResponse.ProviderName,
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
                        result.Warnings.Add($"'{item.Name}' came back from {aiResponse.ProviderName} as invalid structured data and was skipped: {ex.Message}");
                        continue;
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

            foreach (var toolId in toolsActuallyUsed)
            {
                await _aiToolRepo.TouchLastUsedAsync(toolId, cancellationToken);
            }

            if (result.Outputs.Count == 0)
            {
                // Nothing at all could be generated — every item failed on every enabled tool.
                // This is the only case that still fails the whole request.
                return StatusCode(StatusCodes.Status502BadGateway, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Generation failed for every part of this project across all enabled AI tools " +
                               $"({string.Join(", ", enabledTools.Select(t => t.Name))}). Details: {string.Join(" || ", result.Warnings)}"
                });
            }

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
                       "Include as many genuinely notable, factual items as the topic supports — do not artificially limit the count " +
                       "(a comprehensive list is typically 15-30 items, more if the topic warrants it), ordered from most to least significant.";
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
