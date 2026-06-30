using TubePilotAI.Domain.Enums;

namespace TubePilotAI.Application.DTOs;

public sealed record ProjectDto(
    Guid Id,
    string Name,
    string? Description,
    string OwnerName,
    ProjectStatus Status,
    ProjectPriority Priority,
    DateOnly? StartDate,
    DateOnly? DueDate,
    decimal Budget,
    IReadOnlyList<string> Tags,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
