using TubePilot.Domain.Entities;

namespace TubePilot.Application.Interfaces
{
    public interface IPromptCategoryRepository
    {
        Task<IEnumerable<PromptCategory>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<(IEnumerable<PromptCategory> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search = null, string? sortBy = null, bool desc = false, CancellationToken cancellationToken = default);
        Task<PromptCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(PromptCategory category, CancellationToken cancellationToken = default);
        Task UpdateAsync(PromptCategory category, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default); // soft delete
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
