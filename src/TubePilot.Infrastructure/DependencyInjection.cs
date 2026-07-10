using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TubePilot.Application.Interfaces;
using TubePilot.Infrastructure.Data;
using TubePilot.Infrastructure.Repositories;

namespace TubePilot.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<TubePilotDbContext>(opt =>
                opt.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPromptCategoryRepository, PromptCategoryRepository>();
            services.AddScoped<IPromptRepository, PromptRepository>();
            services.AddScoped<IPromptVariableRepository, PromptVariableRepository>();
            services.AddScoped<IAIProviderRepository, AIProviderRepository>();
            services.AddScoped<IAIModelRepository, AIModelRepository>();
            services.AddScoped<IPromptEngine, TubePilot.Infrastructure.Services.PromptEngine>();
            services.AddScoped<IPromptExecutionRepository, PromptExecutionRepository>();
            services.AddScoped<IWorkflowRepository, WorkflowRepository>();
            services.AddScoped<IWorkflowExecutionRepository, WorkflowExecutionRepository>();

            return services;
        }
    }
}
