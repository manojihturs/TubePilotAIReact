using Microsoft.EntityFrameworkCore;
using TubePilotAI.Application.Abstractions.ExternalServices;
using TubePilotAI.Application.DTOs;
using TubePilotAI.Application.Features.AIProviderSettings;
using TubePilotAI.Domain.Entities;
using TubePilotAI.Infrastructure.Persistence.Context;

namespace TubePilotAI.Infrastructure.Services;

public sealed class AIProviderSettingService(TubePilotDbContext dbContext) : IAIProviderSettingService
{
    public async Task<IReadOnlyList<AIProviderSettingDto>> GetAsync(CancellationToken cancellationToken = default)
    {
        var settings = await dbContext.AIProviderSettings
            .AsNoTracking()
            .OrderBy(setting => setting.Provider)
            .ToListAsync(cancellationToken);

        return Enum.GetValues<ContentGenerationProviderKind>()
            .Select(provider => ToDto(provider, settings.FirstOrDefault(setting => setting.Provider == provider.ToString())))
            .ToArray();
    }

    public async Task<AIProviderSettingDto> UpsertAsync(UpsertAIProviderSettingRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.ApiKey))
        {
            throw new ArgumentException("API key is required.");
        }

        var providerName = request.Provider.ToString();
        var setting = await dbContext.AIProviderSettings
            .FirstOrDefaultAsync(candidate => candidate.Provider == providerName, cancellationToken);

        if (setting is null)
        {
            setting = new AIProviderSetting
            {
                Id = Guid.NewGuid(),
                Provider = providerName,
                CreatedAtUtc = DateTime.UtcNow
            };
            dbContext.AIProviderSettings.Add(setting);
        }

        setting.ApiKey = request.ApiKey.Trim();
        setting.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(request.Provider, setting);
    }

    public async Task<bool> DeleteAsync(ContentGenerationProviderKind provider, CancellationToken cancellationToken = default)
    {
        var providerName = provider.ToString();
        var setting = await dbContext.AIProviderSettings
            .FirstOrDefaultAsync(candidate => candidate.Provider == providerName, cancellationToken);

        if (setting is null)
        {
            return false;
        }

        dbContext.AIProviderSettings.Remove(setting);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static AIProviderSettingDto ToDto(ContentGenerationProviderKind provider, AIProviderSetting? setting)
    {
        return new AIProviderSettingDto(
            provider,
            setting is not null && !string.IsNullOrWhiteSpace(setting.ApiKey),
            setting is null || string.IsNullOrWhiteSpace(setting.ApiKey) ? null : Mask(setting.ApiKey),
            setting?.UpdatedAtUtc ?? setting?.CreatedAtUtc);
    }

    private static string Mask(string apiKey)
    {
        var trimmed = apiKey.Trim();
        if (trimmed.Length <= 8)
        {
            return new string('*', trimmed.Length);
        }

        return $"{trimmed[..4]}****{trimmed[^4..]}";
    }
}
