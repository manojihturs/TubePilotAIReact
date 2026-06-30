using System.Threading.Tasks;
using TubePilotAI.Application.DTOs;

namespace TubePilotAI.Application.Interfaces
{
    /// <summary>
    /// Coordinates the generation of a TubePilotAI project from a topic and settings.
    /// </summary>
    public interface IProjectGeneratorService
    {
        Task<GenerateProjectResponseDto> EnqueueGenerationAsync(GenerateProjectRequestDto request);

        Task<GenerationStatusDto> GetStatusAsync(string projectId);
    }
}
