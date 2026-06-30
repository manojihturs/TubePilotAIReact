using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using TubePilotAI.Application.Abstractions.ExternalServices;

namespace TubePilotAI.Infrastructure.ExternalServices.AI;

public sealed class ClaudeProvider(
    HttpClient httpClient,
    ILogger<ClaudeProvider> logger) : IAIProvider
{
    private readonly string _apiBaseUrl = "https://api.anthropic.com/v1/messages";
    private string? _apiKey;

    public AIProviderType ProviderType => AIProviderType.Claude;
    public string ProviderName => "Claude";

    public async Task<AIProviderResponse> GenerateAsync(AIProviderRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _apiKey ??= Environment.GetEnvironmentVariable("CLAUDE_API_KEY");

            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                logger.LogWarning("Claude API key not configured. Returning mock response.");
                return new AIProviderResponse 
                { 
                    Content = "[Mock Claude - API key not configured]",
                    Provider = AIProviderType.Claude,
                    Model = request.Model ?? "claude-3-sonnet-20240229",
                    IsMock = true,
                    CreatedAtUtc = DateTime.UtcNow
                };
            }

            var model = request.Model ?? "claude-3-sonnet-20240229";
            var requestBody = new ClaudeRequest
            {
                Model = model,
                MaxTokens = request.MaxTokens,
                System = request.SystemMessage ?? "You are a helpful assistant.",
                Messages = new[]
                {
                    new ClaudeMessage { Role = "user", Content = request.Prompt }
                }
            };

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
            httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await httpClient.PostAsync(_apiBaseUrl, jsonContent, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError("Claude API error: {statusCode} - {error}", response.StatusCode, errorContent);
                return new AIProviderResponse 
                { 
                    Content = $"[Claude API Error: {response.StatusCode}]",
                    Provider = AIProviderType.Claude,
                    Model = model,
                    IsMock = true,
                    CreatedAtUtc = DateTime.UtcNow
                };
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var claudeResponse = JsonSerializer.Deserialize<ClaudeResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var content = claudeResponse?.Content?.FirstOrDefault()?.Text ?? "[No content generated]";
            var promptTokens = claudeResponse?.Usage?.InputTokens ?? 0;
            var completionTokens = claudeResponse?.Usage?.OutputTokens ?? 0;

            return new AIProviderResponse
            {
                Content = content,
                Provider = AIProviderType.Claude,
                Model = model,
                PromptTokens = promptTokens,
                CompletionTokens = completionTokens,
                IsMock = false,
                CreatedAtUtc = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Claude API exception");
            return new AIProviderResponse 
            { 
                Content = $"[Claude Error: {ex.Message}]",
                Provider = AIProviderType.Claude,
                Model = request.Model ?? "claude-3-sonnet-20240229",
                IsMock = true,
                CreatedAtUtc = DateTime.UtcNow
            };
        }
    }

    public async Task<string> GenerateContentAsync(string prompt, string systemInstruction)
    {
        var response = await GenerateAsync(new AIProviderRequest
        {
            Prompt = prompt,
            SystemMessage = systemInstruction
        });
        return response.Content;
    }

    #region Claude API Models

    private sealed class ClaudeRequest
    {
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; }

        [JsonPropertyName("system")]
        public string? System { get; set; }

        [JsonPropertyName("messages")]
        public ClaudeMessage[]? Messages { get; set; }
    }

    private sealed class ClaudeMessage
    {
        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    private sealed class ClaudeResponse
    {
        [JsonPropertyName("content")]
        public ClaudeContent[]? Content { get; set; }

        [JsonPropertyName("usage")]
        public ClaudeUsage? Usage { get; set; }
    }

    private sealed class ClaudeContent
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    private sealed class ClaudeUsage
    {
        [JsonPropertyName("input_tokens")]
        public int InputTokens { get; set; }

        [JsonPropertyName("output_tokens")]
        public int OutputTokens { get; set; }
    }

    #endregion
}
