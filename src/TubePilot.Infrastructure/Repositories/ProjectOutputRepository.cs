using Microsoft.EntityFrameworkCore;
using TubePilot.Application.Interfaces;
using TubePilot.Domain.Entities;
using TubePilot.Infrastructure.Data;

namespace TubePilot.Infrastructure.Repositories
{
    public class ProjectOutputRepository : IProjectOutputRepository
    {
        private readonly TubePilotDbContext _db;

        public ProjectOutputRepository(TubePilotDbContext db)
        {
            _db = db;
        }

        public async Task<List<ProjectOutput>> GetForProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            return await _db.ProjectOutputs.AsNoTracking()
                .Where(x => x.ProjectId == projectId)
                .ToListAsync(cancellationToken);
        }

        public async Task<ProjectOutput?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _db.ProjectOutputs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task AddAsync(ProjectOutput output, CancellationToken cancellationToken = default)
        {
            if (output.Id == Guid.Empty) output.Id = Guid.NewGuid();
            output.CreatedAt = DateTime.UtcNow;
            await _db.ProjectOutputs.AddAsync(output, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
