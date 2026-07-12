using TubePilot.Application.Interfaces;
using TubePilot.Domain.Entities;
using TubePilot.Infrastructure.Data;

namespace TubePilot.Infrastructure.Repositories
{
    public class GenerationRecordRepository : IGenerationRecordRepository
    {
        private readonly TubePilotDbContext _db;

        public GenerationRecordRepository(TubePilotDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(GenerationRecord record, CancellationToken cancellationToken = default)
        {
            if (record.Id == Guid.Empty) record.Id = Guid.NewGuid();
            record.CreatedAt = DateTime.UtcNow;
            await _db.GenerationRecords.AddAsync(record, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
