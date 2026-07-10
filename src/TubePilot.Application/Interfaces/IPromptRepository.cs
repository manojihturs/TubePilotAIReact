using TubePilot.Domain.Entities;

namespace TubePilot.Application.Interfaces
{
    public interface IPromptRepository
    {
        Task<(IEnumerable<Prompt> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, Guid? categoryId = null, string? search = null, string? sortBy = null, bool desc = false, CancellationToken cancellationToken = default);
        Task<Prompt?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(Prompt prompt, CancellationToken cancellationToken = default);
        Task UpdateAsync(Prompt prompt, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default); // soft delete via IsSystem? We'll mark status
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
