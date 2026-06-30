using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using TubePilotAI.Application.Abstractions.ExternalServices;

namespace TubePilotAI.Infrastructure.ExternalServices.AI;

public sealed class DeepSeekProvider(
    HttpClient httpClient,
    ILogger<DeepSeekProvider> logger) : IAIProvider
{
    private readonly string _apiBaseUrl = "https://api.deepseek.com/chat/completions";
    private string? _apiKey;

    public AIProviderType ProviderType => AIProviderType.DeepSeek;
    public string ProviderName => "DeepSeek";

    public async Task<AIProviderResponse> GenerateAsync(AIProviderRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _apiKey ??= Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY");

            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                logger.LogWarning("DeepSeek API key not configured. Returning mock response.");
                return new AIProviderResponse 
                { 
                    Content = "[Mock DeepSeek - API key not configured]",
                    Provider = AIProviderType.DeepSeek,
                    Model = request.Model ?? "deepseek-chat",
                    IsMock = true,
                    CreatedAtUtc = DateTime.UtcNow
                };
            }

            var model = request.Model ?? "deepseek-chat";
            var requestBody = new DeepSeekRequest
            {
                Model = model,
                Messages = new[]
                {
                    new DeepSeekMessage { Role = "system", Content = request.SystemMessage ?? "You are a helpful assistant." },
                    new DeepSeekMessage { Role = "user", Content = request.Prompt }
                },
                Temperature = (float)request.Temperature,
                MaxTokens = request.MaxTokens
            };

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await httpClient.PostAsync(_apiBaseUrl, jsonContent, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError("DeepSeek API error: {statusCode} - {error}", response.StatusCode, errorContent);
                return new AIProviderResponse 
                { 
                    Content = $"[DeepSeek API Error: {response.StatusCode}]",
                    Provider = AIProviderType.DeepSeek,
                    Model = model,
                    IsMock = true,
                    CreatedAtUtc = DateTime.UtcNow
                };
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var deepseekResponse = JsonSerializer.Deserialize<DeepSeekResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var content = deepseekResponse?.Choices?.FirstOrDefault()?.Message?.Content ?? "[No content generated]";
            var promptTokens = deepseekResponse?.Usage?.PromptTokens ?? 0;
            var completionTokens = deepseekResponse?.Usage?.CompletionTokens ?? 0;

            return new AIProviderResponse
            {
                Content = content,
                Provider = AIProviderType.DeepSeek,
                Model = model,
                PromptTokens = promptTokens,
                CompletionTokens = completionTokens,
                IsMock = false,
                CreatedAtUtc = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "DeepSeek API exception");
            return new AIProviderResponse 
            { 
                Content = $"[DeepSeek Error: {ex.Message}]",
                Provider = AIProviderType.DeepSeek,
                Model = request.Model ?? "deepseek-chat",
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

    #region DeepSeek API Models

    private sealed class DeepSeekRequest
    {
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("messages")]
        public DeepSeekMessage[]? Messages { get; set; }

        [JsonPropertyName("temperature")]
        public float Temperature { get; set; }

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; }
    }

    private sealed class DeepSeekMessage
    {
        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    private sealed class DeepSeekResponse
    {
        [JsonPropertyName("choices")]
        public DeepSeekChoice[]? Choices { get; set; }

        [JsonPropertyName("usage")]
        public DeepSeekUsage? Usage { get; set; }
    }

    private sealed class DeepSeekChoice
    {
        [JsonPropertyName("message")]
        public DeepSeekMessage? Message { get; set; }
    }

    private sealed class DeepSeekUsage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }
    }

    #endregion
}
