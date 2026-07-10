using TubePilot.Domain.Entities;

namespace TubePilot.Application.Interfaces
{
    public interface IPromptVariableRepository
    {
        Task<(IEnumerable<PromptVariable> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, Guid? promptId = null, string? search = null, string? sortBy = null, bool desc = false, CancellationToken cancellationToken = default);
        Task<PromptVariable?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(PromptVariable variable, CancellationToken cancellationToken = default);
        Task UpdateAsync(PromptVariable variable, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default); // soft delete
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
