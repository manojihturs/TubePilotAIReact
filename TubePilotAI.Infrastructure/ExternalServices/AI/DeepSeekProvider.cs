using TubePilotAI.Application.Abstractions.ExternalServices;

namespace TubePilotAI.Infrastructure.ExternalServices.AI;

public sealed class DeepSeekProvider : IAIProvider
{
    public AIProviderType ProviderType => AIProviderType.DeepSeek;
    public string ProviderName => "DeepSeek";

    public Task<AIProviderResponse> GenerateAsync(AIProviderRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new AIProviderResponse { Content = "Mock DeepSeek response based on request." });
    }

    public Task<string> GenerateContentAsync(string prompt, string systemInstruction)
    {
        return Task.FromResult($"[DeepSeek Mock] System: {systemInstruction}, Prompt: {prompt}");
    }
}
