using Microsoft.Extensions.DependencyInjection;
using TubePilot.Application.Interfaces;

namespace TubePilot.Infrastructure.Services.AI
{
    public class AiProviderFactory : IAiProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public AiProviderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IAiProvider GetProvider(string providerName)
        {
            if (string.IsNullOrWhiteSpace(providerName))
                throw new ArgumentException("Provider name is required.", nameof(providerName));

            return _serviceProvider.GetRequiredKeyedService<IAiProvider>(providerName);
        }
    }
}
