using TubePilot.Domain.Entities;

namespace TubePilot.Application.Interfaces
{
    public interface IProjectRepository
    {
        Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<Project>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken = default);
        Task AddAsync(Project project, CancellationToken cancellationToken = default);
        Task UpdateAsync(Project project, CancellationToken cancellationToken = default);
    }
}
