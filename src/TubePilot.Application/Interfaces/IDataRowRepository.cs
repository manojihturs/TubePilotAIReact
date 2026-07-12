using TubePilot.Domain.Entities;

namespace TubePilot.Application.Interfaces
{
    public interface IDataRowRepository
    {
        Task<List<DataRow>> GetForProjectOutputAsync(Guid projectOutputId, CancellationToken cancellationToken = default);
        Task<DataRow?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<DataRow> rows, CancellationToken cancellationToken = default);
        Task UpdateAsync(DataRow row, CancellationToken cancellationToken = default);
    }
}
