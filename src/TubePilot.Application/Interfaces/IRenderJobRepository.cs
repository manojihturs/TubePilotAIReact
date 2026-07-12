using TubePilot.Domain.Entities;

namespace TubePilot.Application.Interfaces
{
    public interface IRenderJobRepository
    {
        Task<RenderJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<RenderJob>> GetForProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
        Task AddAsync(RenderJob job, CancellationToken cancellationToken = default);
        Task UpdateAsync(RenderJob job, CancellationToken cancellationToken = default);
    }
}
