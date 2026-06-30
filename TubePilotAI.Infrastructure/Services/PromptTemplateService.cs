using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TubePilotAI.Application.DTOs;
using TubePilotAI.Application.Features.PromptTemplates;
using TubePilotAI.Application.Validation;
using TubePilotAI.Domain.Entities;
using TubePilotAI.Infrastructure.Persistence.Context;

namespace TubePilotAI.Infrastructure.Services;

public sealed class PromptTemplateService(TubePilotDbContext dbContext) : IPromptTemplateService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<PromptTemplateDto>> GetAsync(
        PromptTemplateQuery query,
        CancellationToken cancellationToken = default)
    {
        var promptTemplates = dbContext.PromptTemplates.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            promptTemplates = promptTemplates.Where(promptTemplate =>
                promptTemplate.Name.Contains(search) ||
                promptTemplate.Category.Contains(search) ||
                (promptTemplate.Description != null && promptTemplate.Description.Contains(search)) ||
                promptTemplate.TemplateText.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            var category = query.Category.Trim();
            promptTemplates = promptTemplates.Where(promptTemplate => promptTemplate.Category == category);
        }

        if (query.Status is not null)
        {
            promptTemplates = promptTemplates.Where(promptTemplate => promptTemplate.Status == query.Status);
        }

        var results = await promptTemplates
            .OrderBy(promptTemplate => promptTemplate.Category)
            .ThenBy(promptTemplate => promptTemplate.Name)
            .ToListAsync(cancellationToken);

        return results.Select(ToDto).ToArray();
    }

    public async Task<PromptTemplateDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var promptTemplate = await dbContext.PromptTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(template => template.Id == id, cancellationToken);

        return promptTemplate is null ? null : ToDto(promptTemplate);
    }

    public async Task<PromptTemplateDto> CreateAsync(
        CreatePromptTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = PromptTemplateValidator.Validate(request);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", errors));
        }

        var normalizedName = request.Name.Trim();
        var nameExists = await dbContext.PromptTemplates
            .AnyAsync(promptTemplate => promptTemplate.Name == normalizedName, cancellationToken);

        if (nameExists)
        {
            throw new InvalidOperationException("A prompt template with this name already exists.");
        }

        var promptTemplate = new PromptTemplate
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            Category = request.Category.Trim(),
            Description = NormalizeOptional(request.Description),
            TemplateText = request.TemplateText.Trim(),
            SystemMessage = NormalizeOptional(request.SystemMessage),
            VariablesJson = SerializeVariables(request.Variables),
            Status = request.Status,
            IsDefault = request.IsDefault,
            CreatedBy = request.CreatedBy.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        if (promptTemplate.IsDefault)
        {
            await ClearOtherDefaultsAsync(null, cancellationToken);
        }

        dbContext.PromptTemplates.Add(promptTemplate);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(promptTemplate);
    }

    public async Task<PromptTemplateDto?> UpdateAsync(
        Guid id,
        UpdatePromptTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = PromptTemplateValidator.Validate(request);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", errors));
        }

        var promptTemplate = await dbContext.PromptTemplates
            .FirstOrDefaultAsync(template => template.Id == id, cancellationToken);

        if (promptTemplate is null)
        {
            return null;
        }

        var normalizedName = request.Name.Trim();
        var nameExists = await dbContext.PromptTemplates
            .AnyAsync(template => template.Id != id && template.Name == normalizedName, cancellationToken);

        if (nameExists)
        {
            throw new InvalidOperationException("A prompt template with this name already exists.");
        }

        promptTemplate.Name = normalizedName;
        promptTemplate.Category = request.Category.Trim();
        promptTemplate.Description = NormalizeOptional(request.Description);
        promptTemplate.TemplateText = request.TemplateText.Trim();
        promptTemplate.SystemMessage = NormalizeOptional(request.SystemMessage);
        promptTemplate.VariablesJson = SerializeVariables(request.Variables);
        promptTemplate.Status = request.Status;
        promptTemplate.IsDefault = request.IsDefault;
        promptTemplate.UpdatedAtUtc = DateTime.UtcNow;

        if (promptTemplate.IsDefault)
        {
            await ClearOtherDefaultsAsync(id, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(promptTemplate);
    }

    public async Task<PromptTemplateDto?> SetDefaultAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var promptTemplate = await dbContext.PromptTemplates
            .FirstOrDefaultAsync(template => template.Id == id, cancellationToken);

        if (promptTemplate is null)
        {
            return null;
        }

        await ClearOtherDefaultsAsync(id, cancellationToken);
        promptTemplate.IsDefault = true;
        promptTemplate.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(promptTemplate);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var promptTemplate = await dbContext.PromptTemplates
            .FirstOrDefaultAsync(template => template.Id == id, cancellationToken);

        if (promptTemplate is null)
        {
            return false;
        }

        dbContext.PromptTemplates.Remove(promptTemplate);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static PromptTemplateDto ToDto(PromptTemplate promptTemplate)
    {
        return new PromptTemplateDto(
            promptTemplate.Id,
            promptTemplate.Name,
            promptTemplate.Category,
            promptTemplate.Description,
            promptTemplate.TemplateText,
            promptTemplate.SystemMessage,
            DeserializeVariables(promptTemplate.VariablesJson),
            promptTemplate.Status,
            promptTemplate.IsDefault,
            promptTemplate.CreatedBy,
            promptTemplate.CreatedAtUtc,
            promptTemplate.UpdatedAtUtc);
    }

    private static string SerializeVariables(IEnumerable<string> variables)
    {
        var normalizedVariables = variables
            .Select(variable => variable.Trim())
            .Where(variable => !string.IsNullOrWhiteSpace(variable))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(variable => variable)
            .ToArray();

        return JsonSerializer.Serialize(normalizedVariables, JsonOptions);
    }

    private static IReadOnlyList<string> DeserializeVariables(string variablesJson)
    {
        if (string.IsNullOrWhiteSpace(variablesJson))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<string[]>(variablesJson, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private async Task ClearOtherDefaultsAsync(Guid? currentId, CancellationToken cancellationToken)
    {
        var defaults = await dbContext.PromptTemplates
            .Where(template => template.IsDefault && template.Id != currentId)
            .ToListAsync(cancellationToken);

        foreach (var template in defaults)
        {
            template.IsDefault = false;
        }
    }
}
