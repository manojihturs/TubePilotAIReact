namespace TubePilotAI.Domain.Entities;

public sealed class GeneratedContent
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Hashtags { get; set; } = string.Empty;
    public string ThumbnailText { get; set; } = string.Empty;
    public string ThumbnailPrompt { get; set; } = string.Empty;
    public string NarrationScript { get; set; } = string.Empty;
    public string SceneBreakdown { get; set; } = string.Empty;
    public string VoiceoverScript { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}
