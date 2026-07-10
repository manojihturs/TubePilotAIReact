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
    public class AIProviderRepository : IAIProviderRepository
    {
        private readonly TubePilotDbContext _db;

        public AIProviderRepository(TubePilotDbContext db)
        {
            _db = db;
        }

        public async Task<AIProvider> AddAsync(AIProvider entity)
        {
            if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
            await _db.AIProviders.AddAsync(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(AIProvider entity)
        {
            entity.SoftDelete = true;
            _db.AIProviders.Update(entity);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null)
        {
            var query = _db.AIProviders.AsNoTracking().Where(x => x.Name == name && !x.SoftDelete);
            if (excludeId.HasValue) query = query.Where(x => x.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<AIProvider> GetByIdAsync(Guid id)
        {
            return await _db.AIProviders.FirstOrDefaultAsync(x => x.Id == id && !x.SoftDelete);
        }

        public async Task<(IEnumerable<AIProvider> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string search, string sortBy, string sortDir, bool? isEnabled)
        {
            var query = _db.AIProviders.AsNoTracking().Where(x => !x.SoftDelete);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(x => x.Name.Contains(s) || (x.DisplayName != null && x.DisplayName.Contains(s)) || (x.ProviderType != null && x.ProviderType.Contains(s)));
            }

            if (isEnabled.HasValue)
            {
                query = query.Where(x => x.IsEnabled == isEnabled.Value);
            }

            // Sorting
            var isDescending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            if (string.Equals(sortBy, "name", StringComparison.OrdinalIgnoreCase))
            {
                query = isDescending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name);
            }
            else if (string.Equals(sortBy, "priority", StringComparison.OrdinalIgnoreCase))
            {
                query = isDescending ? query.OrderByDescending(x => x.Priority) : query.OrderBy(x => x.Priority);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedDate);
            }

            var total = await query.CountAsync();

            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return (items, total);
        }

        public async Task<AIProvider> UpdateAsync(AIProvider entity)
        {
            _db.AIProviders.Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }
    }
}
