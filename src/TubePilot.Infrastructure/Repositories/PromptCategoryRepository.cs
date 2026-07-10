using Microsoft.EntityFrameworkCore;
using TubePilot.Application.Interfaces;
using TubePilot.Domain.Entities;
using TubePilot.Infrastructure.Data;

namespace TubePilot.Infrastructure.Repositories
{
    public class PromptCategoryRepository : IPromptCategoryRepository
    {
        private readonly TubePilotDbContext _db;

        public PromptCategoryRepository(TubePilotDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(PromptCategory category, CancellationToken cancellationToken = default)
        {
            await _db.Set<PromptCategory>().AddAsync(category, cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.Set<PromptCategory>().FindAsync(new object[] { id }, cancellationToken);
            if (entity == null) return;
            entity.SoftDelete = true;
            entity.IsActive = false;
            entity.UpdatedDate = DateTime.UtcNow;
            _db.Set<PromptCategory>().Update(entity);
        }

        public async Task<IEnumerable<PromptCategory>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _db.Set<PromptCategory>().Where(x => !x.SoftDelete).OrderBy(x => x.DisplayOrder).ToListAsync(cancellationToken);
        }

        public async Task<(IEnumerable<PromptCategory> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search = null, string? sortBy = null, bool desc = false, CancellationToken cancellationToken = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _db.Set<PromptCategory>().Where(x => !x.SoftDelete).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(x => (x.Name != null && x.Name.ToLower().Contains(s)) || (x.Description != null && x.Description.ToLower().Contains(s)));
            }

            // Sorting
            sortBy = sortBy?.ToLowerInvariant();
            query = sortBy switch
            {
                "displayorder" => desc ? query.OrderByDescending(x => x.DisplayOrder) : query.OrderBy(x => x.DisplayOrder),
                "createddate" => desc ? query.OrderByDescending(x => x.CreatedDate) : query.OrderBy(x => x.CreatedDate),
                "name" => desc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
                _ => desc ? query.OrderByDescending(x => x.DisplayOrder) : query.OrderBy(x => x.DisplayOrder),
            };

            var total = await query.CountAsync(cancellationToken);
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
            return (items, total);
        }

        public async Task<PromptCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.Set<PromptCategory>().FindAsync(new object[] { id }, cancellationToken);
            if (entity == null || entity.SoftDelete) return null;
            return entity;
        }

        public async Task UpdateAsync(PromptCategory category, CancellationToken cancellationToken = default)
        {
            var existing = await _db.Set<PromptCategory>().FindAsync(new object[] { category.Id }, cancellationToken);
            if (existing == null) return;
            existing.Name = category.Name;
            existing.Description = category.Description;
            existing.DisplayOrder = category.DisplayOrder;
            existing.Icon = category.Icon;
            existing.Color = category.Color;
            existing.IsActive = category.IsActive;
            existing.UpdatedDate = DateTime.UtcNow;
            _db.Set<PromptCategory>().Update(existing);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
