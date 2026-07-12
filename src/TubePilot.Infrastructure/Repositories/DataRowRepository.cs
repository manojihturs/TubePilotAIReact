using Microsoft.EntityFrameworkCore;
using TubePilot.Application.Interfaces;
using TubePilot.Domain.Entities;
using TubePilot.Infrastructure.Data;

namespace TubePilot.Infrastructure.Repositories
{
    public class DataRowRepository : IDataRowRepository
    {
        private readonly TubePilotDbContext _db;

        public DataRowRepository(TubePilotDbContext db)
        {
            _db = db;
        }

        public async Task<List<DataRow>> GetForProjectOutputAsync(Guid projectOutputId, CancellationToken cancellationToken = default)
        {
            return await _db.DataRows.AsNoTracking()
                .Where(x => x.ProjectOutputId == projectOutputId)
                .OrderBy(x => x.RowIndex)
                .ToListAsync(cancellationToken);
        }

        public async Task<DataRow?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _db.DataRows.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task AddRangeAsync(IEnumerable<DataRow> rows, CancellationToken cancellationToken = default)
        {
            var list = rows.ToList();
            foreach (var row in list)
            {
                if (row.Id == Guid.Empty) row.Id = Guid.NewGuid();
            }
            await _db.DataRows.AddRangeAsync(list, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(DataRow row, CancellationToken cancellationToken = default)
        {
            _db.DataRows.Update(row);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
