using System.ComponentModel.DataAnnotations;
using TubePilotAI.Domain.Enums;

namespace TubePilotAI.Application.DTOs;

public sealed class ProjectQuery
{
    public string? Search { get; set; }

    public ProjectStatus? Status { get; set; }

    public ProjectPriority? Priority { get; set; }
}

public sealed class CreateProjectRequest
{
    [Required]
    [StringLength(160, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1200)]
    public string? Description { get; set; }

    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string OwnerName { get; set; } = string.Empty;

    public ProjectStatus Status { get; set; } = ProjectStatus.Draft;

    public ProjectPriority Priority { get; set; } = ProjectPriority.Medium;

    public DateOnly? StartDate { get; set; }

    public DateOnly? DueDate { get; set; }

    [Range(0, 999999999)]
    public decimal Budget { get; set; }

    public List<string> Tags { get; set; } = [];
}

public sealed class UpdateProjectRequest
{
    [Required]
    [StringLength(160, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1200)]
    public string? Description { get; set; }

    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string OwnerName { get; set; } = string.Empty;

    public ProjectStatus Status { get; set; } = ProjectStatus.Draft;

    public ProjectPriority Priority { get; set; } = ProjectPriority.Medium;

    public DateOnly? StartDate { get; set; }

    public DateOnly? DueDate { get; set; }

    [Range(0, 999999999)]
    public decimal Budget { get; set; }

    public List<string> Tags { get; set; } = [];
}
