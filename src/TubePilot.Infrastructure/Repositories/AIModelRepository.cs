using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TubePilot.Application.Interfaces;
using TubePilot.Domain.Entities;
using TubePilot.Infrastructure.Data;

namespace TubePilot.Infrastructure.Repositories
{
    public class AIModelRepository : IAIModelRepository
    {
        private readonly TubePilotDbContext _db;

        public AIModelRepository(TubePilotDbContext db)
        {
            _db = db;
        }

        public async Task<AIModel> AddAsync(AIModel entity)
        {
            if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
            await _db.AIModels.AddAsync(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(AIModel entity)
        {
            _db.AIModels.Remove(entity);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> ExistsByNameForProviderAsync(Guid providerId, string name, Guid? excludeId = null)
        {
            var q = _db.AIModels.AsNoTracking().Where(x => x.ProviderId == providerId && x.Name == name);
            if (excludeId.HasValue) q = q.Where(x => x.Id != excludeId.Value);
            return await q.AnyAsync();
        }

        public async Task<AIModel> GetByIdAsync(Guid id)
        {
            return await _db.AIModels.Include(x => x.Provider).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<(IEnumerable<AIModel> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string search, string sortBy, string sortDir, Guid? providerId, bool? isEnabled)
        {
            var query = _db.AIModels.AsNoTracking().Include(x => x.Provider).AsQueryable();

            if (providerId.HasValue)
            {
                query = query.Where(x => x.ProviderId == providerId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(x => x.Name.Contains(s) || (x.DisplayName != null && x.DisplayName.Contains(s)));
            }

            if (isEnabled.HasValue)
            {
                query = query.Where(x => x.IsEnabled == isEnabled.Value);
            }

            var isDesc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            if (string.Equals(sortBy, "name", StringComparison.OrdinalIgnoreCase))
            {
                query = isDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name);
            }
            else if (string.Equals(sortBy, "createdDate", StringComparison.OrdinalIgnoreCase))
            {
                query = isDesc ? query.OrderByDescending(x => x.CreatedDate) : query.OrderBy(x => x.CreatedDate);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedDate);
            }

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return (items, total);
        }

        public async Task<AIModel> UpdateAsync(AIModel entity)
        {
            _db.AIModels.Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }
    }
}
