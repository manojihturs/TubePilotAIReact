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
    public class WorkflowExecutionRepository : IWorkflowExecutionRepository
    {
        private readonly TubePilotDbContext _db;

        public WorkflowExecutionRepository(TubePilotDbContext db)
        {
            _db = db;
        }

        public async Task<WorkflowExecution> AddAsync(WorkflowExecution exec)
        {
            if (exec.Id == Guid.Empty) exec.Id = Guid.NewGuid();
            exec.StartedAt = DateTime.UtcNow;
            await _db.WorkflowExecutions.AddAsync(exec);
            await _db.SaveChangesAsync();
            return exec;
        }

        public async Task DeleteAsync(WorkflowExecution exec)
        {
            _db.WorkflowExecutions.Remove(exec);
            await _db.SaveChangesAsync();
        }

        public async Task<WorkflowExecution> GetByIdAsync(Guid id)
        {
            return await _db.WorkflowExecutions.Include(x => x.Workflow).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<(IEnumerable<WorkflowExecution> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, Guid? workflowId, string status)
        {
            var q = _db.WorkflowExecutions.AsNoTracking().Include(x => x.Workflow).AsQueryable();
            if (workflowId.HasValue) q = q.Where(x => x.WorkflowId == workflowId.Value);
            if (!string.IsNullOrWhiteSpace(status)) q = q.Where(x => x.Status == status);
            var total = await q.CountAsync();
            var items = await q.OrderByDescending(x => x.StartedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }

        public async Task<WorkflowExecution> UpdateAsync(WorkflowExecution exec)
        {
            _db.WorkflowExecutions.Update(exec);
            await _db.SaveChangesAsync();
            return exec;
        }
    }
}
