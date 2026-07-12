using TubePilot.Domain.Entities;

namespace TubePilot.Application.Interfaces
{
    public interface IPromptVersionRepository
    {
        Task<List<PromptVersion>> GetForPromptAsync(Guid promptId, CancellationToken cancellationToken = default);
        Task<int> GetLatestVersionNumberAsync(Guid promptId, CancellationToken cancellationToken = default);
        Task AddAsync(PromptVersion version, CancellationToken cancellationToken = default);
    }
}
