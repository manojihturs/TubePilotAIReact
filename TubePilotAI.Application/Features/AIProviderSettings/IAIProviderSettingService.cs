using TubePilotAI.Application.Abstractions.ExternalServices;
using TubePilotAI.Application.DTOs;

namespace TubePilotAI.Application.Features.AIProviderSettings;

public interface IAIProviderSettingService
{
    Task<IReadOnlyList<AIProviderSettingDto>> GetAsync(CancellationToken cancellationToken = default);

    Task<AIProviderSettingDto> UpsertAsync(UpsertAIProviderSettingRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(ContentGenerationProviderKind provider, CancellationToken cancellationToken = default);
}
