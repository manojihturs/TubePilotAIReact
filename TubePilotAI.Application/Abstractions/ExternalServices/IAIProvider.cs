namespace TubePilotAI.Application.Abstractions.ExternalServices;

public interface IAIProvider
{
    AIProviderType ProviderType { get; }

    string ProviderName { get; }

    Task<AIProviderResponse> GenerateAsync(AIProviderRequest request, CancellationToken cancellationToken = default);

    Task<string> GenerateContentAsync(string prompt, string systemInstruction);
}
