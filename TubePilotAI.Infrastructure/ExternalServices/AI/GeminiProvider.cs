using TubePilotAI.Application.Abstractions.ExternalServices;

namespace TubePilotAI.Infrastructure.ExternalServices.AI;

public sealed class GeminiProvider : IAIProvider
{
    public AIProviderType ProviderType => AIProviderType.Gemini;
    public string ProviderName => "Gemini";

    public Task<AIProviderResponse> GenerateAsync(AIProviderRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new AIProviderResponse { Content = "Mock Gemini response based on request." });
    }

    public Task<string> GenerateContentAsync(string prompt, string systemInstruction)
    {
        return Task.FromResult($"[Gemini Mock] System: {systemInstruction}, Prompt: {prompt}");
    }
}
