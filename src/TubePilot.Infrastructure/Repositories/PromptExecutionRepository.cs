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
    public class PromptExecutionRepository : IPromptExecutionRepository
    {
        private readonly TubePilotDbContext _db;

        public PromptExecutionRepository(TubePilotDbContext db)
        {
            _db = db;
        }

        public async Task<PromptExecution> AddAsync(PromptExecution entity)
        {
            if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
            await _db.PromptExecutions.AddAsync(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(PromptExecution entity)
        {
            _db.PromptExecutions.Remove(entity);
            await _db.SaveChangesAsync();
        }

        public async Task<PromptExecution> GetByIdAsync(Guid id)
        {
            return await _db.PromptExecutions
                .Include(x => x.Prompt)
                .Include(x => x.Provider)
                .Include(x => x.Model)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<(IEnumerable<PromptExecution> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, Guid? promptId, Guid? providerId, Guid? modelId, string status)
        {
            var query = _db.PromptExecutions.AsNoTracking().Include(x => x.Prompt).Include(x => x.Provider).Include(x => x.Model).AsQueryable();

            if (promptId.HasValue) query = query.Where(x => x.PromptId == promptId.Value);
            if (providerId.HasValue) query = query.Where(x => x.ProviderId == providerId.Value);
            if (modelId.HasValue) query = query.Where(x => x.ModelId == modelId.Value);
            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);

            var total = await query.CountAsync();
            var items = await query.OrderByDescending(x => x.CreatedDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return (items, total);
        }

        public async Task<PromptExecution> UpdateAsync(PromptExecution entity)
        {
            _db.PromptExecutions.Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }
    }
}
