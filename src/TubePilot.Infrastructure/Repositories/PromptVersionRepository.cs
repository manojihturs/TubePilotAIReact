using Microsoft.EntityFrameworkCore;
using TubePilot.Application.Interfaces;
using TubePilot.Domain.Entities;
using TubePilot.Infrastructure.Data;

namespace TubePilot.Infrastructure.Repositories
{
    public class PromptVersionRepository : IPromptVersionRepository
    {
        private readonly TubePilotDbContext _db;

        public PromptVersionRepository(TubePilotDbContext db)
        {
            _db = db;
        }

        public async Task<List<PromptVersion>> GetForPromptAsync(Guid promptId, CancellationToken cancellationToken = default)
        {
            return await _db.PromptVersions.AsNoTracking()
                .Where(x => x.PromptId == promptId)
                .OrderByDescending(x => x.VersionNumber)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetLatestVersionNumberAsync(Guid promptId, CancellationToken cancellationToken = default)
        {
            var latest = await _db.PromptVersions.AsNoTracking()
                .Where(x => x.PromptId == promptId)
                .OrderByDescending(x => x.VersionNumber)
                .Select(x => (int?)x.VersionNumber)
                .FirstOrDefaultAsync(cancellationToken);
            return latest ?? 0;
        }

        public async Task AddAsync(PromptVersion version, CancellationToken cancellationToken = default)
        {
            if (version.Id == Guid.Empty) version.Id = Guid.NewGuid();
            version.CreatedAt = DateTime.UtcNow;
            await _db.PromptVersions.AddAsync(version, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
