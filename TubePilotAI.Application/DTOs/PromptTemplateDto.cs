using TubePilotAI.Domain.Enums;

namespace TubePilotAI.Application.DTOs;

public sealed record PromptTemplateDto(
    Guid Id,
    string Name,
    string Category,
    string? Description,
    string TemplateText,
    string? SystemMessage,
    IReadOnlyList<string> Variables,
    PromptTemplateStatus Status,
    bool IsDefault,
    string CreatedBy,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
