using System.ComponentModel.DataAnnotations;
using TubePilotAI.Domain.Enums;

namespace TubePilotAI.Application.DTOs;

public sealed class PromptTemplateQuery
{
    public string? Search { get; set; }

    public string? Category { get; set; }

    public PromptTemplateStatus? Status { get; set; }
}

public sealed class CreatePromptTemplateRequest
{
    [Required]
    [StringLength(120, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(80, MinimumLength = 2)]
    public string Category { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [StringLength(8000, MinimumLength = 10)]
    public string TemplateText { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? SystemMessage { get; set; }

    public List<string> Variables { get; set; } = [];

    public PromptTemplateStatus Status { get; set; } = PromptTemplateStatus.Draft;

    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string CreatedBy { get; set; } = string.Empty;

    public bool IsDefault { get; set; }
}

public sealed class UpdatePromptTemplateRequest
{
    [Required]
    [StringLength(120, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(80, MinimumLength = 2)]
    public string Category { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [StringLength(8000, MinimumLength = 10)]
    public string TemplateText { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? SystemMessage { get; set; }

    public List<string> Variables { get; set; } = [];

    public PromptTemplateStatus Status { get; set; } = PromptTemplateStatus.Draft;

    public bool IsDefault { get; set; }
}
