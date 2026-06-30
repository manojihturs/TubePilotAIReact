namespace TubePilotAI.Application.Abstractions.ExternalServices;

public interface IAIProviderResolver
{
    IReadOnlyCollection<IAIProvider> GetAll();

    IAIProvider GetProvider(AIProviderType providerType);
}
