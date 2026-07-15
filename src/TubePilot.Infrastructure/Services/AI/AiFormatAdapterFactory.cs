using Microsoft.Extensions.DependencyInjection;
using TubePilot.Application.Interfaces;

namespace TubePilot.Infrastructure.Services.AI
{
    public class AiFormatAdapterFactory : IAiFormatAdapterFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public AiFormatAdapterFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IAiFormatAdapter GetAdapter(string apiFormat)
        {
            if (string.IsNullOrWhiteSpace(apiFormat))
                throw new ArgumentException("API format is required.", nameof(apiFormat));

            return _serviceProvider.GetRequiredKeyedService<IAiFormatAdapter>(apiFormat);
        }
    }
}
