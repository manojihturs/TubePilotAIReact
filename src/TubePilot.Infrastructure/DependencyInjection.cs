using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TubePilot.Application.Interfaces;
using TubePilot.Infrastructure.Data;
using TubePilot.Infrastructure.Repositories;
using TubePilot.Infrastructure.Services;
using TubePilot.Infrastructure.Services.AI;
using TubePilot.Infrastructure.Services.Audio;
using TubePilot.Infrastructure.Services.Images;
using TubePilot.Infrastructure.Services.Video;

namespace TubePilot.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<TubePilotDbContext>(opt =>
                opt.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPromptCategoryRepository, PromptCategoryRepository>();
            services.AddScoped<IPromptRepository, PromptRepository>();
            services.AddScoped<IPromptVariableRepository, PromptVariableRepository>();

            services.AddScoped<IUserApiKeyRepository, UserApiKeyRepository>();
            services.AddDataProtection();
            services.AddScoped<IApiKeyEncryptionService, ApiKeyEncryptionService>();

            services.AddHttpClient<ClaudeProvider>();
            services.AddKeyedScoped<IAiProvider>("Claude", (sp, _) => sp.GetRequiredService<ClaudeProvider>());

            services.AddHttpClient<GroqProvider>();
            services.AddKeyedScoped<IAiProvider>("Groq", (sp, _) => sp.GetRequiredService<GroqProvider>());

            services.AddHttpClient<GeminiProvider>();
            services.AddKeyedScoped<IAiProvider>("Gemini", (sp, _) => sp.GetRequiredService<GeminiProvider>());

            services.AddScoped<IAiProviderFactory, AiProviderFactory>();

            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IPromptVersionRepository, PromptVersionRepository>();
            services.AddScoped<IProjectOutputRepository, ProjectOutputRepository>();
            services.AddScoped<IDataRowRepository, DataRowRepository>();
            services.AddScoped<IGenerationRecordRepository, GenerationRecordRepository>();
            services.AddScoped<IFileSystemService, FileSystemService>();

            services.AddHttpClient<WikimediaImageSearchProvider>();
            services.AddScoped<IImageSearchProvider, WikimediaImageSearchProvider>();

            services.AddHttpClient<PollinationsImageProvider>();
            services.AddScoped<IImageGenerationProvider, PollinationsImageProvider>();

            services.AddScoped<IRenderJobRepository, RenderJobRepository>();
            services.AddHttpClient<GoogleTranslateTtsProvider>();
            services.AddScoped<ITextToSpeechProvider, GoogleTranslateTtsProvider>();
            services.AddHttpClient<FontProvider>();
            services.AddScoped<IFontProvider, FontProvider>();
            services.AddScoped<ISceneComposer, SceneComposer>();
            services.AddHttpClient<BackgroundMusicProvider>();
            services.AddScoped<IBackgroundMusicProvider, BackgroundMusicProvider>();
            services.AddScoped<IVideoRenderingService, VideoRenderingService>();

            return services;
        }
    }
}
