using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TubePilot.Application.Interfaces;
using TubePilot.Domain.Entities;
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

            // Fixed set of wire-format adapters (how to talk HTTP), not a fixed set of
            // vendors (who to talk to) — users define their own named AiTool rows pointing
            // any base URL/model/key at one of these three formats.
            services.AddHttpClient<AnthropicMessagesAdapter>();
            services.AddKeyedScoped<IAiFormatAdapter>(AiApiFormats.AnthropicMessages, (sp, _) => sp.GetRequiredService<AnthropicMessagesAdapter>());

            services.AddHttpClient<OpenAiChatAdapter>();
            services.AddKeyedScoped<IAiFormatAdapter>(AiApiFormats.OpenAiChat, (sp, _) => sp.GetRequiredService<OpenAiChatAdapter>());

            services.AddHttpClient<GeminiAdapter>();
            services.AddKeyedScoped<IAiFormatAdapter>(AiApiFormats.Gemini, (sp, _) => sp.GetRequiredService<GeminiAdapter>());

            services.AddScoped<IAiFormatAdapterFactory, AiFormatAdapterFactory>();
            services.AddScoped<IAiToolRepository, AiToolRepository>();

            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IPromptVersionRepository, PromptVersionRepository>();
            services.AddScoped<IProjectOutputRepository, ProjectOutputRepository>();
            services.AddScoped<IDataRowRepository, DataRowRepository>();
            services.AddScoped<IGenerationRecordRepository, GenerationRecordRepository>();
            services.AddScoped<IFileSystemService, FileSystemService>();

            // Real-photo search: Wikipedia article lead images first, Wikimedia Commons fallback.
            services.AddHttpClient<WikipediaImageSearchProvider>();
            services.AddScoped<IImageSearchProvider, WikipediaImageSearchProvider>();

            services.AddHttpClient<PollinationsImageProvider>();
            services.AddScoped<IImageGenerationProvider, PollinationsImageProvider>();

            // Real stock-footage clip search — free user-supplied API key, genuine motion video.
            services.AddHttpClient<PexelsVideoProvider>();
            services.AddKeyedScoped<IVideoClipSearchProvider>("Pexels", (sp, _) => sp.GetRequiredService<PexelsVideoProvider>());
            services.AddHttpClient<PixabayVideoProvider>();
            services.AddKeyedScoped<IVideoClipSearchProvider>("Pixabay", (sp, _) => sp.GetRequiredService<PixabayVideoProvider>());
            services.AddScoped<IVideoClipProviderFactory, VideoClipProviderFactory>();

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
