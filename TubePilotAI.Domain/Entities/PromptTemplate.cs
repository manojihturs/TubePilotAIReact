using TubePilotAI.Domain.Enums;

namespace TubePilotAI.Domain.Entities;

public sealed class PromptTemplate
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string TemplateText { get; set; } = string.Empty;

    public string? SystemMessage { get; set; }

    public string VariablesJson { get; set; } = "[]";

    public PromptTemplateStatus Status { get; set; } = PromptTemplateStatus.Draft;

    public bool IsDefault { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}
