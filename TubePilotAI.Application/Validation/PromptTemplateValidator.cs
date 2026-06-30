using TubePilotAI.Application.DTOs;
using TubePilotAI.Domain.Enums;

namespace TubePilotAI.Application.Validation;

public static class PromptTemplateValidator
{
    private static readonly HashSet<PromptTemplateStatus> ValidStatuses = Enum.GetValues<PromptTemplateStatus>().ToHashSet();

    public static IReadOnlyList<string> Validate(CreatePromptTemplateRequest request)
    {
        var errors = ValidateCore(
            request.Name,
            request.Category,
            request.Description,
            request.TemplateText,
            request.SystemMessage,
            request.Variables,
            request.Status);

        if (string.IsNullOrWhiteSpace(request.CreatedBy))
        {
            errors.Add("Created by is required.");
        }

        return errors;
    }

    public static IReadOnlyList<string> Validate(UpdatePromptTemplateRequest request)
    {
        return ValidateCore(
            request.Name,
            request.Category,
            request.Description,
            request.TemplateText,
            request.SystemMessage,
            request.Variables,
            request.Status);
    }

    private static List<string> ValidateCore(
        string name,
        string category,
        string? description,
        string templateText,
        string? systemMessage,
        IReadOnlyCollection<string> variables,
        PromptTemplateStatus status)
    {
        var errors = new List<string>();

        AddLengthError(errors, "Name", name, 3, 120, required: true);
        AddLengthError(errors, "Category", category, 2, 80, required: true);
        AddLengthError(errors, "Description", description, 0, 500, required: false);
        AddLengthError(errors, "Template text", templateText, 10, 8000, required: true);
        AddLengthError(errors, "System message", systemMessage, 0, 2000, required: false);

        if (!ValidStatuses.Contains(status))
        {
            errors.Add("Status is invalid.");
        }

        var duplicateVariables = variables
            .Where(variable => !string.IsNullOrWhiteSpace(variable))
            .GroupBy(variable => variable.Trim(), StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateVariables.Length > 0)
        {
            errors.Add($"Variables must be unique. Duplicate values: {string.Join(", ", duplicateVariables)}.");
        }

        foreach (var variable in variables)
        {
            if (string.IsNullOrWhiteSpace(variable))
            {
                errors.Add("Variables cannot include blank values.");
                continue;
            }

            if (variable.Length > 80)
            {
                errors.Add($"Variable '{variable}' cannot be longer than 80 characters.");
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
