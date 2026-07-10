using Microsoft.EntityFrameworkCore;
using TubePilot.Application.Interfaces;
using TubePilot.Domain.Entities;
using TubePilot.Infrastructure.Data;

namespace TubePilot.Infrastructure.Repositories
{
    public class PromptVariableRepository : IPromptVariableRepository
    {
        private readonly TubePilotDbContext _db;

        public PromptVariableRepository(TubePilotDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(PromptVariable variable, CancellationToken cancellationToken = default)
        {
            await _db.Set<PromptVariable>().AddAsync(variable, cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.Set<PromptVariable>().FindAsync(new object[] { id }, cancellationToken);
            if (entity == null) return;
            entity.SoftDelete = true;
            entity.UpdatedDate = DateTime.UtcNow;
            _db.Set<PromptVariable>().Update(entity);
        }

        public async Task<PromptVariable?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.Set<PromptVariable>().FindAsync(new object[] { id }, cancellationToken);
            if (entity == null || entity.SoftDelete) return null;
            return entity;
        }

        public async Task<(IEnumerable<PromptVariable> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, Guid? promptId = null, string? search = null, string? sortBy = null, bool desc = false, CancellationToken cancellationToken = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _db.Set<PromptVariable>().Where(x => !x.SoftDelete).AsQueryable();

            if (promptId.HasValue)
            {
                query = query.Where(x => x.PromptId == promptId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(x => (x.Name != null && x.Name.ToLower().Contains(s)) || (x.Placeholder != null && x.Placeholder.ToLower().Contains(s)) || (x.Description != null && x.Description.ToLower().Contains(s)));
            }

            sortBy = sortBy?.ToLowerInvariant();
            query = sortBy switch
            {
                "name" => desc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
                "createddate" => desc ? query.OrderByDescending(x => x.CreatedDate) : query.OrderBy(x => x.CreatedDate),
                _ => desc ? query.OrderByDescending(x => x.CreatedDate) : query.OrderBy(x => x.CreatedDate),
            };

            var total = await query.CountAsync(cancellationToken);
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
            return (items, total);
        }

        public async Task UpdateAsync(PromptVariable variable, CancellationToken cancellationToken = default)
        {
            var existing = await _db.Set<PromptVariable>().FindAsync(new object[] { variable.Id }, cancellationToken);
            if (existing == null) return;
            existing.Name = variable.Name;
            existing.Placeholder = variable.Placeholder;
            existing.Description = variable.Description;
            existing.DataType = variable.DataType;
            existing.IsRequired = variable.IsRequired;
            existing.DefaultValue = variable.DefaultValue;
            existing.UpdatedBy = variable.UpdatedBy;
            existing.UpdatedDate = DateTime.UtcNow;
            existing.PromptId = variable.PromptId;

            _db.Set<PromptVariable>().Update(existing);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
