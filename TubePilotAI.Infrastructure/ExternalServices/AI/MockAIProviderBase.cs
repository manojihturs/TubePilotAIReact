using TubePilotAI.Application.Abstractions.ExternalServices;

namespace TubePilotAI.Infrastructure.ExternalServices.AI;

public abstract class MockAIProviderBase : IAIProvider
{
    public abstract AIProviderType ProviderType { get; }

    public abstract string ProviderName { get; }

    protected abstract string DefaultModel { get; }

    protected abstract string ResponseStyle { get; }

    public Task<string> GenerateContentAsync(string prompt, string systemInstruction)
    {
        return Task.FromResult($"[{ProviderName} Mock] System: {systemInstruction}, Prompt: {prompt}");
    }

    public Task<AIProviderResponse> GenerateAsync(AIProviderRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            throw new ArgumentException("Prompt is required.", nameof(request));
        }

        cancellationToken.ThrowIfCancellationRequested();

        var model = string.IsNullOrWhiteSpace(request.Model) ? DefaultModel : request.Model.Trim();
        var prompt = request.Prompt.Trim();
        var systemMessage = string.IsNullOrWhiteSpace(request.SystemMessage)
            ? "No system message supplied."
            : request.SystemMessage.Trim();

        var content = BuildMockContent(model, systemMessage, prompt);

        var response = new AIProviderResponse
        {
            Provider = ProviderType,
            Model = model,
            Content = content,
            PromptTokens = EstimateTokens(prompt) + EstimateTokens(systemMessage),
            CompletionTokens = EstimateTokens(content),
            IsMock = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        return Task.FromResult(response);
    }

    private string BuildMockContent(string model, string systemMessage, string prompt)
    {
        return
            $"[{ProviderName} mock response]\n" +
            $"Model: {model}\n" +
            $"Style: {ResponseStyle}\n" +
            $"System: {systemMessage}\n" +
            $"Prompt summary: {Summarize(prompt)}\n" +
            "Result: This is a deterministic mock AI response for local development and testing.";
    }

    private static string Summarize(string value)
    {
        const int maxLength = 180;
        return value.Length <= maxLength ? value : $"{value[..maxLength]}...";
    }

    private static int EstimateTokens(string value)
    {
        return Math.Max(1, (int)Math.Ceiling(value.Length / 4m));
    }
}
