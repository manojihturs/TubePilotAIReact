using TubePilotAI.Application.Abstractions.ExternalServices;
using TubePilotAI.Domain.Enums;

namespace TubePilotAI.Application.DTOs;

public sealed class ContentGenerationProviderSelection
{
    public ContentGenerationProviderKind Provider { get; set; }
}

public sealed class GenerateContentJobRequest
{
    public string Title { get; set; } = string.Empty;

    public Guid? PromptTemplateId { get; set; }

    public string? PromptOverride { get; set; }

    public List<ContentGenerationProviderKind> SelectedProviders { get; set; } = [];
}

public sealed record ContentResearchRowDto(
    string Keyword,
    int SearchVolume,
    int CompetitionScore,
    int OpportunityScore,
    string Notes);

public sealed record ContentGenerationProviderResultDto(
    ContentGenerationProviderKind Provider,
    string Model,
    string RawResponse);

public sealed record ContentGenerationResultDto(
    Guid JobId,
    string Title,
    string PromptText,
    IReadOnlyList<ContentGenerationProviderResultDto> ProviderResults,
    IReadOnlyList<ContentResearchRowDto> ResearchRows,
    string VideoTitle,
    string Description,
    string Hashtags,
    string ThumbnailText,
    string BackgroundImagePrompt,
    string NarrationScript,
    string SceneText,
    string VoiceoverText,
    string? ExportFolderPath,
    ContentGenerationStatus Status,
    string? ErrorMessage,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc);

public sealed record ContentGenerationJobDto(
    Guid JobId,
    string Title,
    Guid? PromptTemplateId,
    string PromptText,
    IReadOnlyList<ContentGenerationProviderKind> SelectedProviders,
    ContentGenerationStatus Status,
    string? ExportFolderPath,
    string? ErrorMessage,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc);
