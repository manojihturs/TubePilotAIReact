using TubePilot.Domain.Entities;

namespace TubePilot.Application.Interfaces
{
    public interface IAiToolRepository
    {
        Task<AiTool?> GetAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
        Task<List<AiTool>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken = default);
        // Enabled tools only, in the order generation should try them.
        Task<List<AiTool>> GetEnabledForUserOrderedAsync(Guid userId, CancellationToken cancellationToken = default);
        Task AddAsync(AiTool entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(AiTool entity, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
        Task TouchLastUsedAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
