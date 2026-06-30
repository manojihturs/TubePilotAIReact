using System.Threading.Tasks;
using TubePilotAI.Application.DTOs;

namespace TubePilotAI.Application.Interfaces
{
    /// <summary>
    /// Abstraction for AI providers. Implementations return a uniform response model.
    /// </summary>
    public interface IAIProvider
    {
        string ProviderName { get; }

        Task<AIResponseDto> GenerateResearchAsync(AIRequestDto request);

        Task<AIResponseDto> GenerateScriptAsync(AIRequestDto request);

        Task<AIResponseDto> GenerateThumbnailsAsync(AIRequestDto request);
    }
}
