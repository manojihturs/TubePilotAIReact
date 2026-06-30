using System.Threading.Tasks;
using TubePilotAI.Application.DTOs;
using TubePilotAI.Application.Interfaces;

namespace TubePilotAI.Infrastructure.AI
{
    /// <summary>
    /// Base stub AI provider returning predictable sample text and images.
    /// </summary>
    public abstract class StubAIProviderBase : IAIProvider
    {
        public abstract string ProviderName { get; }

        public Task<AIResponseDto> GenerateResearchAsync(AIRequestDto request)
        {
            var resp = new AIResponseDto
            {
                Text = $"Research for '{request.Topic}' (provider={ProviderName})",
            };
            resp.Items.Add($"Short research summary about {request.Topic}.");
            resp.ImageUrls.Add("https://via.placeholder.com/800x450.png?text=research");
            return Task.FromResult(resp);
        }

        public Task<AIResponseDto> GenerateScriptAsync(AIRequestDto request)
        {
            var resp = new AIResponseDto
            {
                Text = $"Script for '{request.Topic}' (provider={ProviderName})",
            };
            for (int i = 0; i < request.Items; i++) resp.Items.Add($"Scene {i+1} for {request.Topic}");
            resp.ImageUrls.Add("https://via.placeholder.com/1920x1080.png?text=scene");
            return Task.FromResult(resp);
        }

        public Task<AIResponseDto> GenerateThumbnailsAsync(AIRequestDto request)
        {
            var resp = new AIResponseDto
            {
                Text = $"Thumbnail prompts for '{request.Topic}' (provider={ProviderName})",
            };
            resp.Items.Add($"A cinematic thumbnail for {request.Topic}");
            resp.ImageUrls.Add("https://via.placeholder.com/1280x720.png?text=thumbnail");
            return Task.FromResult(resp);
        }
    }
}
