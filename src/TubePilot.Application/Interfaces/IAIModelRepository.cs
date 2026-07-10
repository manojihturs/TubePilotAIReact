using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TubePilot.Domain.Entities;

namespace TubePilot.Application.Interfaces
{
    public interface IAIModelRepository
    {
        Task<AIModel> GetByIdAsync(Guid id);
        Task<AIModel> AddAsync(AIModel entity);
        Task<AIModel> UpdateAsync(AIModel entity);
        Task DeleteAsync(AIModel entity);
        Task<(IEnumerable<AIModel> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string search, string sortBy, string sortDir, Guid? providerId, bool? isEnabled);
        Task<bool> ExistsByNameForProviderAsync(Guid providerId, string name, Guid? excludeId = null);
    }
}
