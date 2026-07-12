using TubePilot.Domain.Entities;

namespace TubePilot.Application.Interfaces
{
    public interface IProjectOutputRepository
    {
        Task<List<ProjectOutput>> GetForProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
        Task<ProjectOutput?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(ProjectOutput output, CancellationToken cancellationToken = default);
    }
}
