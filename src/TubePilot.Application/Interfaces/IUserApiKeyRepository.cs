using TubePilot.Domain.Entities;

namespace TubePilot.Application.Interfaces
{
    public interface IUserApiKeyRepository
    {
        Task<UserApiKey?> GetAsync(Guid userId, string providerName, CancellationToken cancellationToken = default);
        Task<List<UserApiKey>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken = default);
        Task UpsertAsync(UserApiKey entity, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid userId, string providerName, CancellationToken cancellationToken = default);
        Task TouchLastUsedAsync(Guid userId, string providerName, CancellationToken cancellationToken = default);
    }
}
