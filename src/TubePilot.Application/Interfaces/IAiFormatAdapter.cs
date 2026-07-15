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

    // One adapter per real HTTP wire format (not per vendor). A user's AiTool row supplies
    // the base URL, model, and API key at call time — the adapter just knows how to shape
    // the request/response for that format, so any number of same-format tools (different
    // hosts/keys) share one adapter instance.
    public interface IAiFormatAdapter
    {
        string ApiFormat { get; }
        Task<AiResponse> GenerateAsync(string baseUrl, string apiKey, string model, AiRequest request, CancellationToken cancellationToken = default);
    }

    public interface IAiFormatAdapterFactory
    {
        IAiFormatAdapter GetAdapter(string apiFormat);
    }
}
