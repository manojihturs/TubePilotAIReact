using Microsoft.EntityFrameworkCore;
using TubePilot.Application.Interfaces;
using TubePilot.Domain.Entities;
using TubePilot.Infrastructure.Data;

namespace TubePilot.Infrastructure.Repositories
{
    public class PromptRepository : IPromptRepository
    {
        private readonly TubePilotDbContext _db;

        public PromptRepository(TubePilotDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Prompt prompt, CancellationToken cancellationToken = default)
        {
            await _db.Set<Prompt>().AddAsync(prompt, cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.Set<Prompt>().FindAsync(new object[] { id }, cancellationToken);
            if (entity == null) return;
            // soft delete: mark status or IsPublic false - for now mark Status = "Deleted"
            entity.Status = "Deleted";
            entity.UpdatedDate = DateTime.UtcNow;
            _db.Set<Prompt>().Update(entity);
        }

        public async Task<Prompt?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.Set<Prompt>().FindAsync(new object[] { id }, cancellationToken);
            if (entity == null) return null;
            if (string.Equals(entity.Status, "Deleted", StringComparison.OrdinalIgnoreCase)) return null;
            return entity;
        }

        public async Task<(IEnumerable<Prompt> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, Guid? categoryId = null, string? search = null, string? sortBy = null, bool desc = false, CancellationToken cancellationToken = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _db.Set<Prompt>().AsQueryable();
            // Avoid using string.Equals with StringComparison in EF queries - not translatable to SQL
            // Use ToLower comparison which EF can translate
            query = query.Where(x => x.Status == null || x.Status.ToLower() != "deleted");

            if (categoryId.HasValue)
            {
                query = query.Where(x => x.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(x => (x.Name != null && x.Name.ToLower().Contains(s)) || (x.Description != null && x.Description.ToLower().Contains(s)) || (x.PromptText != null && x.PromptText.ToLower().Contains(s)) || (x.Tags != null && x.Tags.ToLower().Contains(s)));
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

        public async Task UpdateAsync(Prompt prompt, CancellationToken cancellationToken = default)
        {
            var existing = await _db.Set<Prompt>().FindAsync(new object[] { prompt.Id }, cancellationToken);
            if (existing == null) return;
            existing.Name = prompt.Name;
            existing.Description = prompt.Description;
            existing.PromptText = prompt.PromptText;
            existing.Temperature = prompt.Temperature;
            existing.TopP = prompt.TopP;
            existing.MaxTokens = prompt.MaxTokens;
            existing.Model = prompt.Model;
            existing.Provider = prompt.Provider;
            existing.Version = prompt.Version;
            existing.Status = prompt.Status;
            existing.Tags = prompt.Tags;
            existing.IsSystem = prompt.IsSystem;
            existing.IsPublic = prompt.IsPublic;
            existing.UpdatedBy = prompt.UpdatedBy;
            existing.UpdatedDate = DateTime.UtcNow;
            existing.CategoryId = prompt.CategoryId;

            _db.Set<Prompt>().Update(existing);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
