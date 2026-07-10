using Microsoft.Extensions.DependencyInjection;

namespace TubePilot.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Add application services, handlers, validators here in Phase 0
            return services;
        }
    }
}
