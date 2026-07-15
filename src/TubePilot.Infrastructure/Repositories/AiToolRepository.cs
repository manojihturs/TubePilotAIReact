using Microsoft.EntityFrameworkCore;
using TubePilot.Application.Interfaces;
using TubePilot.Domain.Entities;
using TubePilot.Infrastructure.Data;

namespace TubePilot.Infrastructure.Repositories
{
    public class AiToolRepository : IAiToolRepository
    {
        private readonly TubePilotDbContext _db;

        public AiToolRepository(TubePilotDbContext db)
        {
            _db = db;
        }

        public async Task<AiTool?> GetAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
        {
            return await _db.AiTools.FirstOrDefaultAsync(x => x.UserId == userId && x.Id == id, cancellationToken);
        }

        public async Task<List<AiTool>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _db.AiTools.AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.Priority)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<AiTool>> GetEnabledForUserOrderedAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _db.AiTools.AsNoTracking()
                .Where(x => x.UserId == userId && x.IsEnabled)
                .OrderBy(x => x.Priority)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(AiTool entity, CancellationToken cancellationToken = default)
        {
            if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            await _db.AiTools.AddAsync(entity, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(AiTool entity, CancellationToken cancellationToken = default)
        {
            _db.AiTools.Update(entity);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
        {
            var existing = await _db.AiTools.FirstOrDefaultAsync(x => x.UserId == userId && x.Id == id, cancellationToken);
            if (existing == null) return false;

            _db.AiTools.Remove(existing);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task TouchLastUsedAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var existing = await _db.AiTools.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (existing == null) return;

            existing.LastUsedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
