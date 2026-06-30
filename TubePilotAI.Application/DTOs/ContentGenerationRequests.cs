using System.ComponentModel.DataAnnotations;

namespace TubePilotAI.Application.DTOs;

public sealed class GenerateContentRequest
{
    [Required]
    public Guid ProjectId { get; set; }

    [Required]
    public string Brief { get; set; } = string.Empty;
}

public sealed record GeneratedContentDto(
    Guid Id,
    Guid ProjectId,
    string Title,
    string Description,
    string Hashtags,
    string ThumbnailText,
    string ThumbnailPrompt,
    string NarrationScript,
    string SceneBreakdown,
    string VoiceoverScript,
    DateTime CreatedAtUtc);
