using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TubePilot.Domain.Entities;

namespace TubePilot.Application.Interfaces
{
    public interface IPromptExecutionRepository
    {
        Task<PromptExecution> GetByIdAsync(Guid id);
        Task<PromptExecution> AddAsync(PromptExecution entity);
        Task<PromptExecution> UpdateAsync(PromptExecution entity);
        Task DeleteAsync(PromptExecution entity);
        Task<(IEnumerable<PromptExecution> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, Guid? promptId, Guid? providerId, Guid? modelId, string status);
    }
}
