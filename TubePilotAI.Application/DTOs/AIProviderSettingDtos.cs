using TubePilotAI.Application.Abstractions.ExternalServices;

namespace TubePilotAI.Application.DTOs;

public sealed record AIProviderSettingDto(
    ContentGenerationProviderKind Provider,
    bool IsConfigured,
    string? MaskedApiKey,
    DateTime? UpdatedAtUtc);

public sealed class UpsertAIProviderSettingRequest
{
    public ContentGenerationProviderKind Provider { get; set; }

    public string ApiKey { get; set; } = string.Empty;
}
