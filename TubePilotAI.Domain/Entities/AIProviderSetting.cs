namespace TubePilotAI.Domain.Entities;

public sealed class AIProviderSetting
{
    public Guid Id { get; set; }

    public string Provider { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}
