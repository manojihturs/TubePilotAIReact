using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TubePilotAI.Application.Abstractions.ExternalServices;
using TubePilotAI.Application.DTOs;
using TubePilotAI.Application.Features.ContentGeneration;
using TubePilotAI.Domain.Entities;
using TubePilotAI.Domain.Enums;
using TubePilotAI.Infrastructure.ExternalServices.AI;
using TubePilotAI.Infrastructure.Persistence.Context;

namespace TubePilotAI.Infrastructure.Services;

public sealed class ContentGenerationService(
    TubePilotDbContext dbContext,
    IAIProviderResolver providerResolver) : IContentGenerationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly string exportRoot = Path.Combine(AppContext.BaseDirectory, "Exports");

    public async Task<IReadOnlyList<ContentGenerationJobDto>> GetJobsAsync(CancellationToken cancellationToken = default)
    {
        var jobs = await dbContext.ContentGenerationJobs
            .AsNoTracking()
            .OrderByDescending(job => job.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return jobs.Select(job => new ContentGenerationJobDto(
            job.Id,
            job.Title,
            job.PromptTemplateId,
            job.PromptText,
            DeserializeSelectedProviders(job.SelectedProvidersJson),
            job.Status,
            job.ExportFolderPath,
            job.ErrorMessage,
            job.CreatedAtUtc,
            job.CompletedAtUtc)).ToArray();
    }

    public async Task<ContentGenerationResultDto?> GetJobAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        var job = await dbContext.ContentGenerationJobs
            .AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == jobId, cancellationToken);

        return job is null ? null : DeserializeResult(job);
    }

    public async Task<ContentGenerationResultDto> GenerateAsync(GenerateContentJobRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var promptTemplateText = await ResolvePromptAsync(request.PromptTemplateId, cancellationToken);
        var promptText = BuildPrompt(request.Title, promptTemplateText, request.PromptOverride);
        var providers = ResolveProviders(request.SelectedProviders);

        var job = new ContentGenerationJob
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            PromptTemplateId = request.PromptTemplateId,
            PromptText = promptText,
            SelectedProvidersJson = JsonSerializer.Serialize(request.SelectedProviders, JsonOptions),
            Status = ContentGenerationStatus.Running,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.ContentGenerationJobs.Add(job);
        await dbContext.SaveChangesAsync(cancellationToken);

        var providerResults = new List<ContentGenerationProviderResultDto>();

        foreach (var providerEntry in providers)
        {
            var response = await providerEntry.Provider.GenerateAsync(
                new AIProviderRequest
                {
                    Prompt = promptText,
                    SystemMessage = "Generate research, SEO metadata, thumbnails, scene text, and voiceover text for YouTube content.",
                    Model = null,
                    Temperature = 0.7m,
                    MaxTokens = 2048,
                    Metadata = new Dictionary<string, string>
                    {
                        ["title"] = request.Title.Trim(),
                        ["level"] = "0"
                    }
                },
                cancellationToken);

            providerResults.Add(new ContentGenerationProviderResultDto(providerEntry.ProviderKind, response.Model, response.Content));
        }

        var primary = providerResults.First();
        var content = BuildContentPackage(job.Id, request.Title.Trim(), promptText, providerResults, primary.RawResponse);
        job.Status = ContentGenerationStatus.Completed;
        job.ResultJson = JsonSerializer.Serialize(content, JsonOptions);
        job.CompletedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return content;
    }

    public async Task<string> ExportAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        var job = await dbContext.ContentGenerationJobs
            .FirstOrDefaultAsync(candidate => candidate.Id == jobId, cancellationToken)
            ?? throw new KeyNotFoundException("Generation job not found.");

        var result = DeserializeResult(job);
        var safeTitle = MakeSafeFolderName(result.Title);
        var titleFolder = Path.Combine(exportRoot, safeTitle);
        var dataFolder = Path.Combine(titleFolder, "data");
        var assetsFolder = Path.Combine(titleFolder, "assets");
        var thumbnailFolder = Path.Combine(assetsFolder, "thumbnail");
        Directory.CreateDirectory(titleFolder);
        Directory.CreateDirectory(dataFolder);
        Directory.CreateDirectory(thumbnailFolder);

        await WriteText(dataFolder, "video-title.txt", result.VideoTitle, cancellationToken);
        await WriteText(dataFolder, "description.txt", result.Description, cancellationToken);
        await WriteText(dataFolder, "hashtags.txt", result.Hashtags, cancellationToken);
        await WriteText(dataFolder, "research.csv", BuildResearchCsv(result.ResearchRows), cancellationToken);
        await WriteText(dataFolder, "narration-script.txt", result.NarrationScript, cancellationToken);
        await WriteText(dataFolder, "scene-text.txt", result.SceneText, cancellationToken);
        await WriteText(dataFolder, "voiceover-text.txt", result.VoiceoverText, cancellationToken);

        await WriteText(assetsFolder, "thumbnail-text.txt", result.ThumbnailText, cancellationToken);
        await WriteText(assetsFolder, "background-image-prompt.txt", result.BackgroundImagePrompt, cancellationToken);
        await WriteText(assetsFolder, "thumbnail-hd.prompt.txt", BuildThumbnailPrompt(result.Title, result.BackgroundImagePrompt), cancellationToken);
        await File.WriteAllBytesAsync(Path.Combine(thumbnailFolder, "thumbnail-hd.png"), CreateMockThumbnailImage(), cancellationToken);

        var manifestPath = Path.Combine(titleFolder, "manifest.json");
        var manifest = JsonSerializer.Serialize(result, JsonOptions);
        await File.WriteAllTextAsync(manifestPath, manifest, cancellationToken);

        job.ExportFolderPath = titleFolder;
        await dbContext.SaveChangesAsync(cancellationToken);

        return titleFolder;
    }

    private static void Validate(GenerateContentJobRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException("Title is required.");
        }

        if (request.SelectedProviders.Count == 0)
        {
            throw new ArgumentException("At least one AI provider must be selected.");
        }
    }

    private async Task<string> ResolvePromptAsync(Guid? promptTemplateId, CancellationToken cancellationToken)
    {
        if (promptTemplateId is null)
        {
            var defaultTemplate = await dbContext.PromptTemplates
                .AsNoTracking()
                .Where(template => template.IsDefault)
                .OrderByDescending(template => template.UpdatedAtUtc ?? template.CreatedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);

            if (defaultTemplate is not null)
            {
                return defaultTemplate.TemplateText;
            }

            return "Generate research data, video metadata, and production assets from the title.";
        }

        var promptTemplate = await dbContext.PromptTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(template => template.Id == promptTemplateId, cancellationToken)
            ?? throw new KeyNotFoundException("Prompt template not found.");

        return promptTemplate.TemplateText;
    }

    private static string BuildPrompt(string title, string promptTemplateText, string? promptOverride)
    {
        var promptBody = string.IsNullOrWhiteSpace(promptOverride) ? promptTemplateText : promptOverride;
        return promptBody
            .Replace("{{title}}", title, StringComparison.OrdinalIgnoreCase)
            .Replace("{title}", title, StringComparison.OrdinalIgnoreCase)
            .Replace("[title]", title, StringComparison.OrdinalIgnoreCase);
    }

    private IReadOnlyList<(ContentGenerationProviderKind ProviderKind, IAIProvider Provider)> ResolveProviders(IEnumerable<ContentGenerationProviderKind> selectedProviders)
    {
        return selectedProviders.Select(providerKind =>
        {
            var provider = providerResolver.GetProvider(providerKind switch
            {
                ContentGenerationProviderKind.OpenAI => AIProviderType.OpenAI,
                ContentGenerationProviderKind.Gemini => AIProviderType.Gemini,
                ContentGenerationProviderKind.Claude => AIProviderType.Claude,
                ContentGenerationProviderKind.DeepSeek => AIProviderType.DeepSeek,
                ContentGenerationProviderKind.Ollama => AIProviderType.Ollama,
                _ => throw new ArgumentOutOfRangeException(nameof(selectedProviders), providerKind, null)
            });

            return (providerKind, provider);
        }).ToArray();
    }

    private static ContentGenerationResultDto BuildContentPackage(
        Guid jobId,
        string title,
        string promptText,
        IReadOnlyList<ContentGenerationProviderResultDto> providerResults,
        string primaryRawResponse)
    {
        var researchRows = BuildResearchRows(title);
        var safeTitle = MakeVideoTitle(title);
        var description = BuildDescription(title, promptText, primaryRawResponse);
        var hashtags = BuildHashtags(title);
        var thumbnailText = BuildThumbnailText(title);
        var backgroundImagePrompt = BuildBackgroundPrompt(title);
        var narrationScript = BuildNarrationScript(title, researchRows);
        var sceneText = BuildSceneText(title);
        var voiceoverText = BuildVoiceoverText(title);

        return new ContentGenerationResultDto(
            jobId,
            title,
            promptText,
            providerResults,
            researchRows,
            safeTitle,
            description,
            hashtags,
            thumbnailText,
            backgroundImagePrompt,
            narrationScript,
            sceneText,
            voiceoverText,
            null,
            ContentGenerationStatus.Completed,
            null,
            DateTime.UtcNow,
            DateTime.UtcNow);
    }

    private static ContentGenerationResultDto DeserializeResult(ContentGenerationJob job)
    {
        try
        {
            var result = JsonSerializer.Deserialize<ContentGenerationResultDto>(job.ResultJson, JsonOptions);
            return result is null
                ? BuildFallbackResult(job)
                : result with { ExportFolderPath = job.ExportFolderPath, Status = job.Status, ErrorMessage = job.ErrorMessage, CompletedAtUtc = job.CompletedAtUtc };
        }
        catch
        {
            return BuildFallbackResult(job);
        }
    }

    private static ContentGenerationResultDto BuildFallbackResult(ContentGenerationJob job)
    {
        return new ContentGenerationResultDto(
            job.Id,
            job.Title,
            job.PromptText,
            [],
            BuildResearchRows(job.Title),
            MakeVideoTitle(job.Title),
            BuildDescription(job.Title, job.PromptText, "Mock response"),
            BuildHashtags(job.Title),
            BuildThumbnailText(job.Title),
            BuildBackgroundPrompt(job.Title),
            BuildNarrationScript(job.Title, BuildResearchRows(job.Title)),
            BuildSceneText(job.Title),
            BuildVoiceoverText(job.Title),
            job.ExportFolderPath,
            job.Status,
            job.ErrorMessage,
            job.CreatedAtUtc,
            job.CompletedAtUtc);
    }

    private static IReadOnlyList<ContentResearchRowDto> BuildResearchRows(string title)
    {
        var normalized = title.Trim();
        return
        [
            new ContentResearchRowDto($"{normalized} ideas", 5400, 42, 78, "High intent research topic"),
            new ContentResearchRowDto($"{normalized} tutorial", 3200, 38, 84, "Evergreen instructional topic"),
            new ContentResearchRowDto($"{normalized} workflow", 2100, 33, 81, "Process-driven topic with strong search value")
        ];
    }

    private static string MakeVideoTitle(string title) => $"{title} | Best Trend Title";

    private static string BuildDescription(string title, string prompt, string rawResponse)
        => $"Social media content package for {title}. Prompt: {prompt}. Primary response: {rawResponse[..Math.Min(rawResponse.Length, 180)]}";

    private static string BuildHashtags(string title)
        => $"#{NormalizeTag(title)} #SocialMedia #ContentCreation #TubePilotAI";

    private static string BuildThumbnailText(string title)
        => $"{title.ToUpperInvariant()} | THUMBNAIL";

    private static string BuildBackgroundPrompt(string title)
        => $"High-contrast cinematic background for a social media thumbnail about {title}, clean composition, bold lighting, creator workspace.";

    private static string BuildNarrationScript(string title, IReadOnlyList<ContentResearchRowDto> researchRows)
        => $"Intro: Today we are building a social media content package for {title}.\n" +
           $"Research focus: {string.Join(", ", researchRows.Select(row => row.Keyword))}.\n" +
           "Body: Explain the process, show the content structure, and summarize the export package.\n" +
           "Outro: Encourage the audience to reuse the workflow for their own titles.";

    private static string BuildSceneText(string title)
        => $"Scene 1: Title reveal for {title}\nScene 2: Research summary\nScene 3: Script and thumbnail breakdown\nScene 4: Export package overview";

    private static string BuildVoiceoverText(string title)
        => $"Voiceover for {title}: clear, confident, and paced for a short-form social media workflow.";

    private static string NormalizeTag(string value)
        => new string(value.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();

    private static string MakeSafeFolderName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = new string(value.Select(ch => invalid.Contains(ch) ? '-' : ch).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(sanitized) ? "Untitled" : sanitized;
    }

    private static async Task WriteCsvAsync(string folder, IReadOnlyList<ContentResearchRowDto> rows, CancellationToken cancellationToken)
    {
        await File.WriteAllLinesAsync(Path.Combine(folder, "research.csv"), BuildResearchCsvLines(rows), cancellationToken);
    }

    private static async Task WriteText(string folder, string fileName, string content, CancellationToken cancellationToken)
    {
        await File.WriteAllTextAsync(Path.Combine(folder, fileName), content, cancellationToken);
    }

    private static string BuildResearchCsv(IReadOnlyList<ContentResearchRowDto> rows)
        => string.Join(Environment.NewLine, BuildResearchCsvLines(rows));

    private static IReadOnlyList<string> BuildResearchCsvLines(IReadOnlyList<ContentResearchRowDto> rows)
    {
        var lines = new List<string> { "Keyword,SearchVolume,CompetitionScore,OpportunityScore,Notes" };
        lines.AddRange(rows.Select(row => $"{Escape(row.Keyword)},{row.SearchVolume},{row.CompetitionScore},{row.OpportunityScore},{Escape(row.Notes)}"));
        return lines;
    }

    private static string BuildThumbnailPrompt(string title, string backgroundImagePrompt)
        => $"HD thumbnail asset prompt for {title}. {backgroundImagePrompt}. Bold, readable, social-media-ready, clean composition.";

    private static byte[] CreateMockThumbnailImage()
    {
        return Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO9XJ9sAAAAASUVORK5CYII=");
    }

    private static string Escape(string value)
        => $"\"{value.Replace("\"", "\"\"")}\"";

    private static IReadOnlyList<ContentGenerationProviderKind> DeserializeSelectedProviders(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<ContentGenerationProviderKind[]>(json, JsonOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }
}
