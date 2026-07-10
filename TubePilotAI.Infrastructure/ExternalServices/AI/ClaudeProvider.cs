using TubePilotAI.Application.Abstractions.ExternalServices;

namespace TubePilotAI.Infrastructure.ExternalServices.AI;

public sealed class ClaudeProvider : IAIProvider
{
    public AIProviderType ProviderType => AIProviderType.Claude;
    public string ProviderName => "Claude";

    public Task<AIProviderResponse> GenerateAsync(AIProviderRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new AIProviderResponse { Content = "Mock Claude response based on request." });
    }

    public Task<string> GenerateContentAsync(string prompt, string systemInstruction)
    {
        return Task.FromResult($"[Claude Mock] System: {systemInstruction}, Prompt: {prompt}");
    }
}
