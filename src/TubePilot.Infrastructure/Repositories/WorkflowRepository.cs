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
    public class WorkflowRepository : IWorkflowRepository
    {
        private readonly TubePilotDbContext _db;

        public WorkflowRepository(TubePilotDbContext db)
        {
            _db = db;
        }

        public async Task<Workflow> AddAsync(Workflow workflow)
        {
            if (workflow.Id == Guid.Empty) workflow.Id = Guid.NewGuid();
            workflow.CreatedDate = DateTime.UtcNow;
            await _db.Workflows.AddAsync(workflow);
            await _db.SaveChangesAsync();
            return workflow;
        }

        public async Task DeleteAsync(Workflow workflow)
        {
            _db.Workflows.Remove(workflow);
            await _db.SaveChangesAsync();
        }

        public async Task<Workflow> GetByIdAsync(Guid id)
        {
            return await _db.Workflows.Include(w => w.Steps.OrderBy(s => s.Order)).FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<(IEnumerable<Workflow> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string search)
        {
            var q = _db.Workflows.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(search)) q = q.Where(x => x.Name.Contains(search) || (x.Description != null && x.Description.Contains(search)));
            var total = await q.CountAsync();
            var items = await q.OrderByDescending(x => x.CreatedDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }

        public async Task<Workflow> UpdateAsync(Workflow workflow)
        {
            workflow.UpdatedDate = DateTime.UtcNow;
            _db.Workflows.Update(workflow);
            await _db.SaveChangesAsync();
            return workflow;
        }
    }
}
