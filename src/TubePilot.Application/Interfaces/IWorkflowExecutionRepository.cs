using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TubePilot.Domain.Entities;

namespace TubePilot.Application.Interfaces
{
    public interface IWorkflowExecutionRepository
    {
        Task<WorkflowExecution> GetByIdAsync(Guid id);
        Task<WorkflowExecution> AddAsync(WorkflowExecution exec);
        Task<WorkflowExecution> UpdateAsync(WorkflowExecution exec);
        Task DeleteAsync(WorkflowExecution exec);
        Task<(IEnumerable<WorkflowExecution> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, Guid? workflowId, string status);
    }
}
