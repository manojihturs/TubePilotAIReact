using Microsoft.EntityFrameworkCore;
using TubePilot.Application.Interfaces;
using TubePilot.Domain.Entities;
using TubePilot.Infrastructure.Data;

namespace TubePilot.Infrastructure.Repositories
{
    public class UserApiKeyRepository : IUserApiKeyRepository
    {
        private readonly TubePilotDbContext _db;

        public UserApiKeyRepository(TubePilotDbContext db)
        {
            _db = db;
        }

        public async Task<UserApiKey?> GetAsync(Guid userId, string providerName, CancellationToken cancellationToken = default)
        {
            return await _db.UserApiKeys.FirstOrDefaultAsync(
                x => x.UserId == userId && x.ProviderName == providerName, cancellationToken);
        }

        public async Task<List<UserApiKey>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _db.UserApiKeys.AsNoTracking()
                .Where(x => x.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        public async Task UpsertAsync(UserApiKey entity, CancellationToken cancellationToken = default)
        {
            var existing = await _db.UserApiKeys.FirstOrDefaultAsync(
                x => x.UserId == entity.UserId && x.ProviderName == entity.ProviderName, cancellationToken);

            if (existing == null)
            {
                if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
                entity.CreatedAt = DateTime.UtcNow;
                await _db.UserApiKeys.AddAsync(entity, cancellationToken);
            }
            else
            {
                existing.EncryptedKey = entity.EncryptedKey;
            }

            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> DeleteAsync(Guid userId, string providerName, CancellationToken cancellationToken = default)
        {
            var existing = await _db.UserApiKeys.FirstOrDefaultAsync(
                x => x.UserId == userId && x.ProviderName == providerName, cancellationToken);
            if (existing == null) return false;

            _db.UserApiKeys.Remove(existing);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task TouchLastUsedAsync(Guid userId, string providerName, CancellationToken cancellationToken = default)
        {
            var existing = await _db.UserApiKeys.FirstOrDefaultAsync(
                x => x.UserId == userId && x.ProviderName == providerName, cancellationToken);
            if (existing == null) return;

            existing.LastUsedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
