namespace TubePilot.Application.Interfaces
{
    public record AiRequest(
        string? SystemPrompt,
        string UserPrompt,
        string Model,
        float Temperature,
        int MaxTokens
    );

    public record AiResponse(
        string Content,
        int InputTokens,
        int OutputTokens,
        string ProviderName,
        string Model
    );

    public interface IAiProvider
    {
        string ProviderName { get; }
        Task<AiResponse> GenerateAsync(string apiKey, AiRequest request, CancellationToken cancellationToken = default);
    }
}
