namespace TubePilotAI.Application.Abstractions.ExternalServices;

public sealed class AIProviderResponse
{
    public AIProviderType Provider { get; set; }

    public string Model { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public int PromptTokens { get; set; }

    public int CompletionTokens { get; set; }

    public int TotalTokens => PromptTokens + CompletionTokens;

    public bool IsMock { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }
}
