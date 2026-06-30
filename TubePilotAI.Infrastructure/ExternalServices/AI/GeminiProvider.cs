using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using TubePilotAI.Application.Abstractions.ExternalServices;

namespace TubePilotAI.Infrastructure.ExternalServices.AI;

public sealed class GeminiProvider(
    HttpClient httpClient,
    ILogger<GeminiProvider> logger) : IAIProvider
{
    private readonly string _apiBaseUrl = "https://generativelanguage.googleapis.com/v1beta/models";
    private string? _apiKey;

    public AIProviderType ProviderType => AIProviderType.Gemini;
    public string ProviderName => "Gemini";

    public async Task<AIProviderResponse> GenerateAsync(AIProviderRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _apiKey ??= Environment.GetEnvironmentVariable("GEMINI_API_KEY");

            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                logger.LogWarning("Gemini API key not configured. Returning mock response.");
                return new AIProviderResponse 
                { 
                    Content = "[Mock Gemini - API key not configured]",
                    Provider = AIProviderType.Gemini,
                    Model = request.Model ?? "gemini-pro",
                    IsMock = true,
                    CreatedAtUtc = DateTime.UtcNow
                };
            }

            var model = request.Model ?? "gemini-1.5-flash";
            var requestBody = new GeminiRequest
            {
                Contents = new[]
                {
                    new GeminiContent
                    {
                        Parts = new[]
                        {
                            new GeminiPart { Text = request.SystemMessage + "\n\n" + request.Prompt }
                        }
                    }
                },
                GenerationConfig = new GeminiGenerationConfig
                {
                    Temperature = (float)request.Temperature,
                    MaxOutputTokens = request.MaxTokens,
                    TopP = 0.95f,
                    TopK = 40
                }
            };

            var url = $"{_apiBaseUrl}/{model}:generateContent?key={_apiKey}";
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await httpClient.PostAsync(url, jsonContent, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError("Gemini API error: {statusCode} - {error}", response.StatusCode, errorContent);
                return new AIProviderResponse 
                { 
                    Content = $"[Gemini API Error: {response.StatusCode}]",
                    Provider = AIProviderType.Gemini,
                    Model = model,
                    IsMock = true,
                    CreatedAtUtc = DateTime.UtcNow
                };
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var content = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? "[No content generated]";
            var promptTokens = geminiResponse?.UsageMetadata?.PromptTokenCount ?? 0;
            var completionTokens = geminiResponse?.UsageMetadata?.CandidatesTokenCount ?? 0;

            return new AIProviderResponse
            {
                Content = content,
                Provider = AIProviderType.Gemini,
                Model = model,
                PromptTokens = promptTokens,
                CompletionTokens = completionTokens,
                IsMock = false,
                CreatedAtUtc = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Gemini API exception");
            return new AIProviderResponse 
            { 
                Content = $"[Gemini Error: {ex.Message}]",
                Provider = AIProviderType.Gemini,
                Model = request.Model ?? "gemini-1.5-flash",
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

    #region Gemini API Models

    private sealed class GeminiRequest
    {
        [JsonPropertyName("contents")]
        public GeminiContent[]? Contents { get; set; }

        [JsonPropertyName("generationConfig")]
        public GeminiGenerationConfig? GenerationConfig { get; set; }
    }

    private sealed class GeminiContent
    {
        [JsonPropertyName("parts")]
        public GeminiPart[]? Parts { get; set; }
    }

    private sealed class GeminiPart
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    private sealed class GeminiGenerationConfig
    {
        [JsonPropertyName("temperature")]
        public float Temperature { get; set; }

        [JsonPropertyName("maxOutputTokens")]
        public int MaxOutputTokens { get; set; }

        [JsonPropertyName("topP")]
        public float TopP { get; set; }

        [JsonPropertyName("topK")]
        public int TopK { get; set; }
    }

    private sealed class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public GeminiCandidate[]? Candidates { get; set; }

        [JsonPropertyName("usageMetadata")]
        public GeminiUsageMetadata? UsageMetadata { get; set; }
    }

    private sealed class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent? Content { get; set; }
    }

    private sealed class GeminiUsageMetadata
    {
        [JsonPropertyName("promptTokenCount")]
        public int PromptTokenCount { get; set; }

        [JsonPropertyName("candidatesTokenCount")]
        public int CandidatesTokenCount { get; set; }
    }

    #endregion
}
