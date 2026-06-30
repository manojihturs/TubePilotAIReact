using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using TubePilotAI.Application.Abstractions.ExternalServices;

namespace TubePilotAI.Infrastructure.ExternalServices.AI;

public sealed class OpenAIProvider(
    HttpClient httpClient,
    ILogger<OpenAIProvider> logger) : IAIProvider
{
    private readonly string _apiBaseUrl = "https://api.openai.com/v1/chat/completions";
    private string? _apiKey;

    public AIProviderType ProviderType => AIProviderType.OpenAI;
    public string ProviderName => "OpenAI";

    public async Task<AIProviderResponse> GenerateAsync(AIProviderRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _apiKey ??= Environment.GetEnvironmentVariable("OPENAI_API_KEY");

            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                logger.LogWarning("OpenAI API key not configured. Returning mock response.");
                return new AIProviderResponse 
                { 
                    Content = "[Mock OpenAI - API key not configured]",
                    Provider = AIProviderType.OpenAI,
                    Model = request.Model ?? "gpt-4-turbo",
                    IsMock = true,
                    CreatedAtUtc = DateTime.UtcNow
                };
            }

            var model = request.Model ?? "gpt-4-turbo";
            var requestBody = new OpenAIRequest
            {
                Model = model,
                Messages = new[]
                {
                    new OpenAIMessage { Role = "system", Content = request.SystemMessage ?? "You are a helpful assistant." },
                    new OpenAIMessage { Role = "user", Content = request.Prompt }
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
                logger.LogError("OpenAI API error: {statusCode} - {error}", response.StatusCode, errorContent);
                return new AIProviderResponse 
                { 
                    Content = $"[OpenAI API Error: {response.StatusCode}]",
                    Provider = AIProviderType.OpenAI,
                    Model = model,
                    IsMock = true,
                    CreatedAtUtc = DateTime.UtcNow
                };
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var openaiResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var content = openaiResponse?.Choices?.FirstOrDefault()?.Message?.Content ?? "[No content generated]";
            var promptTokens = openaiResponse?.Usage?.PromptTokens ?? 0;
            var completionTokens = openaiResponse?.Usage?.CompletionTokens ?? 0;

            return new AIProviderResponse
            {
                Content = content,
                Provider = AIProviderType.OpenAI,
                Model = model,
                PromptTokens = promptTokens,
                CompletionTokens = completionTokens,
                IsMock = false,
                CreatedAtUtc = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "OpenAI API exception");
            return new AIProviderResponse 
            { 
                Content = $"[OpenAI Error: {ex.Message}]",
                Provider = AIProviderType.OpenAI,
                Model = request.Model ?? "gpt-4-turbo",
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

    #region OpenAI API Models

    private sealed class OpenAIRequest
    {
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("messages")]
        public OpenAIMessage[]? Messages { get; set; }

        [JsonPropertyName("temperature")]
        public float Temperature { get; set; }

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; }
    }

    private sealed class OpenAIMessage
    {
        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    private sealed class OpenAIResponse
    {
        [JsonPropertyName("choices")]
        public OpenAIChoice[]? Choices { get; set; }

        [JsonPropertyName("usage")]
        public OpenAIUsage? Usage { get; set; }
    }

    private sealed class OpenAIChoice
    {
        [JsonPropertyName("message")]
        public OpenAIMessage? Message { get; set; }
    }

    private sealed class OpenAIUsage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }
    }

    #endregion
}
