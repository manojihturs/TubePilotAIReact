using Microsoft.EntityFrameworkCore;
using TubePilot.Application.Interfaces;
using TubePilot.Domain.Entities;
using TubePilot.Infrastructure.Data;

namespace TubePilot.Infrastructure.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly TubePilotDbContext _db;

        public ProjectRepository(TubePilotDbContext db)
        {
            _db = db;
        }

        public async Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _db.Projects.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<List<Project>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _db.Projects.AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Project project, CancellationToken cancellationToken = default)
        {
            if (project.Id == Guid.Empty) project.Id = Guid.NewGuid();
            project.CreatedAt = DateTime.UtcNow;
            await _db.Projects.AddAsync(project, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
        {
            _db.Projects.Update(project);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
