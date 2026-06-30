using Microsoft.Extensions.DependencyInjection;
using TubePilotAI.Application.Abstractions.ExternalServices;
using TubePilotAI.Infrastructure.ExternalServices.AI;

namespace TubePilotAI.Infrastructure.DependencyInjection;

public static class AIProviderServiceCollectionExtensions
{
    public static IServiceCollection AddAIProviders(this IServiceCollection services)
    {
        // Register HttpClient for providers that need it
        services.AddHttpClient<GeminiProvider>();
        services.AddHttpClient<OpenAIProvider>();
        services.AddHttpClient<ClaudeProvider>();
        services.AddHttpClient<DeepSeekProvider>();
        services.AddHttpClient<OllamaProvider>();

        services.AddScoped<IAIProvider, OpenAIProvider>();
        services.AddScoped<IAIProvider, GeminiProvider>();
        services.AddScoped<IAIProvider, ClaudeProvider>();
        services.AddScoped<IAIProvider, DeepSeekProvider>();
        services.AddScoped<IAIProvider, OllamaProvider>();
        services.AddScoped<IAIProviderResolver, AIProviderResolver>();

        return services;
    }
}
