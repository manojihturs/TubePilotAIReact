using TubePilotAI.Application.Abstractions.ExternalServices;

namespace TubePilotAI.Infrastructure.ExternalServices.AI;

public sealed class OpenAIProvider : IAIProvider
{
    public AIProviderType ProviderType => AIProviderType.OpenAI;
    public string ProviderName => "OpenAI";

    public Task<AIProviderResponse> GenerateAsync(AIProviderRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new AIProviderResponse { Content = "Mock OpenAI response based on request." });
    }

    public Task<string> GenerateContentAsync(string prompt, string systemInstruction)
    {
        return Task.FromResult($"[OpenAI Mock] System: {systemInstruction}, Prompt: {prompt}");
    }
}
