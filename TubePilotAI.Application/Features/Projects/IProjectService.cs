using TubePilotAI.Application.DTOs;

namespace TubePilotAI.Application.Features.Projects;

public interface IProjectService
{
    Task<IReadOnlyList<ProjectDto>> GetAsync(ProjectQuery query, CancellationToken cancellationToken = default);

    Task<ProjectDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ProjectDto> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken = default);

    Task<ProjectDto?> UpdateAsync(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
