using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TubePilotAI.Application.DTOs;
using TubePilotAI.Application.Features.Projects;
using TubePilotAI.Application.Validation;
using TubePilotAI.Domain.Entities;
using TubePilotAI.Infrastructure.Persistence.Context;

namespace TubePilotAI.Infrastructure.Services;

public sealed class ProjectService(TubePilotDbContext dbContext) : IProjectService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<ProjectDto>> GetAsync(ProjectQuery query, CancellationToken cancellationToken = default)
    {
        var projects = dbContext.Projects.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            projects = projects.Where(project =>
                project.Name.Contains(search) ||
                project.OwnerName.Contains(search) ||
                (project.Description != null && project.Description.Contains(search)));
        }

        if (query.Status is not null)
        {
            projects = projects.Where(project => project.Status == query.Status);
        }

        if (query.Priority is not null)
        {
            projects = projects.Where(project => project.Priority == query.Priority);
        }

        var results = await projects
            .OrderBy(project => project.Status)
            .ThenBy(project => project.DueDate ?? DateOnly.MaxValue)
            .ThenBy(project => project.Name)
            .ToListAsync(cancellationToken);

        return results.Select(ToDto).ToArray();
    }

    public async Task<ProjectDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await dbContext.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        return project is null ? null : ToDto(project);
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        var errors = ProjectValidator.Validate(request);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", errors));
        }

        var normalizedName = request.Name.Trim();
        var nameExists = await dbContext.Projects
            .AnyAsync(project => project.Name == normalizedName, cancellationToken);

        if (nameExists)
        {
            throw new InvalidOperationException("A project with this name already exists.");
        }

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            Description = NormalizeOptional(request.Description),
            OwnerName = request.OwnerName.Trim(),
            Status = request.Status,
            Priority = request.Priority,
            StartDate = request.StartDate,
            DueDate = request.DueDate,
            Budget = request.Budget,
            TagsJson = SerializeTags(request.Tags),
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(project);
    }

    public async Task<ProjectDto?> UpdateAsync(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken = default)
    {
        var errors = ProjectValidator.Validate(request);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", errors));
        }

        var project = await dbContext.Projects
            .FirstOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        if (project is null)
        {
            return null;
        }

        var normalizedName = request.Name.Trim();
        var nameExists = await dbContext.Projects
            .AnyAsync(candidate => candidate.Id != id && candidate.Name == normalizedName, cancellationToken);

        if (nameExists)
        {
            throw new InvalidOperationException("A project with this name already exists.");
        }

        project.Name = normalizedName;
        project.Description = NormalizeOptional(request.Description);
        project.OwnerName = request.OwnerName.Trim();
        project.Status = request.Status;
        project.Priority = request.Priority;
        project.StartDate = request.StartDate;
        project.DueDate = request.DueDate;
        project.Budget = request.Budget;
        project.TagsJson = SerializeTags(request.Tags);
        project.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(project);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await dbContext.Projects
            .FirstOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        if (project is null)
        {
            return false;
        }

        dbContext.Projects.Remove(project);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static ProjectDto ToDto(Project project)
    {
        return new ProjectDto(
            project.Id,
            project.Name,
            project.Description,
            project.OwnerName,
            project.Status,
            project.Priority,
            project.StartDate,
            project.DueDate,
            project.Budget,
            DeserializeTags(project.TagsJson),
            project.CreatedAtUtc,
            project.UpdatedAtUtc);
    }

    private static string SerializeTags(IEnumerable<string> tags)
    {
        var normalizedTags = tags
            .Select(tag => tag.Trim())
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(tag => tag)
            .ToArray();

        return JsonSerializer.Serialize(normalizedTags, JsonOptions);
    }

    private static IReadOnlyList<string> DeserializeTags(string tagsJson)
    {
        if (string.IsNullOrWhiteSpace(tagsJson))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<string[]>(tagsJson, JsonOptions) ?? [];
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
}
