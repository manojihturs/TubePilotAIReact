using TubePilotAI.Application.DTOs;

namespace TubePilotAI.Application.Features.ContentGeneration;

public interface IContentGenerationService
{
    Task<IReadOnlyList<ContentGenerationJobDto>> GetJobsAsync(CancellationToken cancellationToken = default);

    Task<ContentGenerationResultDto?> GetJobAsync(Guid jobId, CancellationToken cancellationToken = default);

    Task<ContentGenerationResultDto> GenerateAsync(GenerateContentJobRequest request, CancellationToken cancellationToken = default);

    Task<string> ExportAsync(Guid jobId, CancellationToken cancellationToken = default);
}
