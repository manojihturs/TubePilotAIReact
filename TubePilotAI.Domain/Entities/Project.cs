using TubePilotAI.Domain.Enums;

namespace TubePilotAI.Domain.Entities;

public sealed class Project
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string OwnerName { get; set; } = string.Empty;

    public ProjectStatus Status { get; set; } = ProjectStatus.Draft;

    public ProjectPriority Priority { get; set; } = ProjectPriority.Medium;

    public DateOnly? StartDate { get; set; }

    public DateOnly? DueDate { get; set; }

    public decimal Budget { get; set; }

    public string TagsJson { get; set; } = "[]";

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<GeneratedContent> GeneratedContents { get; set; } = new List<GeneratedContent>();
}
