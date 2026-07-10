using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TubePilot.Domain.Entities;

namespace TubePilot.Application.Interfaces
{
    public interface IAIProviderRepository
    {
        Task<AIProvider> GetByIdAsync(Guid id);
        Task<AIProvider> AddAsync(AIProvider entity);
        Task<AIProvider> UpdateAsync(AIProvider entity);
        Task DeleteAsync(AIProvider entity);
        Task<(IEnumerable<AIProvider> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string search, string sortBy, string sortDir, bool? isEnabled);
        Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null);
    }
}
