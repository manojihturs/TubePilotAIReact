using Microsoft.EntityFrameworkCore;
using TubePilot.Application.Interfaces;
using TubePilot.Domain.Entities;
using TubePilot.Infrastructure.Data;

namespace TubePilot.Infrastructure.Repositories
{
    public class RenderJobRepository : IRenderJobRepository
    {
        private readonly TubePilotDbContext _db;

        public RenderJobRepository(TubePilotDbContext db)
        {
            _db = db;
        }

        public async Task<RenderJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _db.RenderJobs.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<List<RenderJob>> GetForProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            return await _db.RenderJobs.AsNoTracking()
                .Where(x => x.ProjectId == projectId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(RenderJob job, CancellationToken cancellationToken = default)
        {
            if (job.Id == Guid.Empty) job.Id = Guid.NewGuid();
            job.CreatedAt = DateTime.UtcNow;
            await _db.RenderJobs.AddAsync(job, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(RenderJob job, CancellationToken cancellationToken = default)
        {
            _db.RenderJobs.Update(job);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
