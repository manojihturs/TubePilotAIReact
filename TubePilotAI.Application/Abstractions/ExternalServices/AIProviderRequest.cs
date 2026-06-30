namespace TubePilotAI.Application.Abstractions.ExternalServices;

public sealed class AIProviderRequest
{
    public string Prompt { get; set; } = string.Empty;

    public string? SystemMessage { get; set; }

    public string? Model { get; set; }

    public decimal Temperature { get; set; } = 0.7m;

    public int MaxTokens { get; set; } = 1024;

    public Dictionary<string, string> Metadata { get; set; } = [];
}
