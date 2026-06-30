using TubePilotAI.Domain.Enums;

namespace TubePilotAI.Domain.Entities;

public sealed class ContentGenerationJob
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public Guid? PromptTemplateId { get; set; }

    public PromptTemplate? PromptTemplate { get; set; }

    public string PromptText { get; set; } = string.Empty;

    public string SelectedProvidersJson { get; set; } = "[]";

    public ContentGenerationStatus Status { get; set; } = ContentGenerationStatus.Draft;

    public string? ExportFolderPath { get; set; }

    public string ResultJson { get; set; } = "{}";

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }
}
