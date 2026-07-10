using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TubePilot.Domain.Entities;

namespace TubePilot.Application.Interfaces
{
    public interface IWorkflowRepository
    {
        Task<Workflow> GetByIdAsync(Guid id);
        Task<Workflow> AddAsync(Workflow workflow);
        Task<Workflow> UpdateAsync(Workflow workflow);
        Task DeleteAsync(Workflow workflow);
        Task<(IEnumerable<Workflow> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string search);
    }
}
