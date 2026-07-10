using TubePilotAI.Application.Abstractions.ExternalServices;

namespace TubePilotAI.Infrastructure.ExternalServices.AI;

public sealed class OllamaProvider : IAIProvider
{
    public AIProviderType ProviderType => AIProviderType.Ollama;
    public string ProviderName => "Ollama";

    public Task<AIProviderResponse> GenerateAsync(AIProviderRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new AIProviderResponse { Content = "Mock Ollama response based on request." });
    }

    public Task<string> GenerateContentAsync(string prompt, string systemInstruction)
    {
        return Task.FromResult($"[Ollama Mock] System: {systemInstruction}, Prompt: {prompt}");
    }
}
