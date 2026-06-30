using TubePilotAI.Application.DTOs;
using TubePilotAI.Domain.Enums;

namespace TubePilotAI.Application.Validation;

public static class ProjectValidator
{
    private static readonly HashSet<ProjectStatus> ValidStatuses = Enum.GetValues<ProjectStatus>().ToHashSet();
    private static readonly HashSet<ProjectPriority> ValidPriorities = Enum.GetValues<ProjectPriority>().ToHashSet();

    public static IReadOnlyList<string> Validate(CreateProjectRequest request)
    {
        return ValidateCore(
            request.Name,
            request.Description,
            request.OwnerName,
            request.Status,
            request.Priority,
            request.StartDate,
            request.DueDate,
            request.Budget,
            request.Tags);
    }

    public static IReadOnlyList<string> Validate(UpdateProjectRequest request)
    {
        return ValidateCore(
            request.Name,
            request.Description,
            request.OwnerName,
            request.Status,
            request.Priority,
            request.StartDate,
            request.DueDate,
            request.Budget,
            request.Tags);
    }

    private static List<string> ValidateCore(
        string name,
        string? description,
        string ownerName,
        ProjectStatus status,
        ProjectPriority priority,
        DateOnly? startDate,
        DateOnly? dueDate,
        decimal budget,
        IReadOnlyCollection<string> tags)
    {
        var errors = new List<string>();

        AddLengthError(errors, "Name", name, 3, 160, required: true);
        AddLengthError(errors, "Description", description, 0, 1200, required: false);
        AddLengthError(errors, "Owner name", ownerName, 2, 120, required: true);

        if (!ValidStatuses.Contains(status))
        {
            errors.Add("Status is invalid.");
        }

        if (!ValidPriorities.Contains(priority))
        {
            errors.Add("Priority is invalid.");
        }

        if (startDate is not null && dueDate is not null && dueDate < startDate)
        {
            errors.Add("Due date cannot be earlier than start date.");
        }

        if (budget < 0)
        {
            errors.Add("Budget cannot be negative.");
        }

        var duplicateTags = tags
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .GroupBy(tag => tag.Trim(), StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateTags.Length > 0)
        {
            errors.Add($"Tags must be unique. Duplicate values: {string.Join(", ", duplicateTags)}.");
        }

        foreach (var tag in tags)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                errors.Add("Tags cannot include blank values.");
                continue;
            }

            if (tag.Length > 60)
            {
                errors.Add($"Tag '{tag}' cannot be longer than 60 characters.");
            }
        }

        return errors;
    }

    private static void AddLengthError(
        ICollection<string> errors,
        string field,
        string? value,
        int min,
        int max,
        bool required)
    {
        var trimmed = value?.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            if (required)
            {
                errors.Add($"{field} is required.");
            }

            return;
        }

        if (trimmed.Length < min)
        {
            errors.Add($"{field} must be at least {min} characters.");
        }

        if (trimmed.Length > max)
        {
            errors.Add($"{field} cannot be longer than {max} characters.");
        }
    }
}
