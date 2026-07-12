using TubePilot.Domain.Entities;

namespace TubePilot.Application.Interfaces
{
    public interface IGenerationRecordRepository
    {
        Task AddAsync(GenerationRecord record, CancellationToken cancellationToken = default);
    }
}
