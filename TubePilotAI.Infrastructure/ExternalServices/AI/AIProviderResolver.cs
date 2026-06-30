using TubePilotAI.Application.Abstractions.ExternalServices;

namespace TubePilotAI.Infrastructure.ExternalServices.AI;

public sealed class AIProviderResolver(IEnumerable<IAIProvider> providers) : IAIProviderResolver
{
    private readonly IReadOnlyDictionary<AIProviderType, IAIProvider> providersByType = providers
        .GroupBy(provider => provider.ProviderType)
        .ToDictionary(group => group.Key, group => group.First());

    public IReadOnlyCollection<IAIProvider> GetAll()
    {
        return providersByType.Values.ToArray();
    }

    public IAIProvider GetProvider(AIProviderType providerType)
    {
        if (providersByType.TryGetValue(providerType, out var provider))
        {
            return provider;
        }

        throw new InvalidOperationException($"AI provider '{providerType}' is not registered.");
    }
}
