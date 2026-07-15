using Microsoft.Extensions.DependencyInjection;
using TubePilot.Application.Interfaces;

namespace TubePilot.Infrastructure.Services.Video
{
    public class VideoClipProviderFactory : IVideoClipProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public VideoClipProviderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IVideoClipSearchProvider GetProvider(string providerName)
        {
            if (string.IsNullOrWhiteSpace(providerName))
                throw new ArgumentException("Provider name is required.", nameof(providerName));

            return _serviceProvider.GetRequiredKeyedService<IVideoClipSearchProvider>(providerName);
        }
    }
}
