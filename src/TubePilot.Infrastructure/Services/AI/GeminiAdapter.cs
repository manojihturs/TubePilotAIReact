using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TubePilot.Application.Interfaces;
using TubePilot.Domain.Entities;

namespace TubePilot.Infrastructure.Services.AI
{
    // Google's Gemini generateContent wire format.
    public class GeminiAdapter : IAiFormatAdapter
    {
        private readonly HttpClient _http;

        public string ApiFormat => AiApiFormats.Gemini;

        public GeminiAdapter(HttpClient http)
        {
            _http = http;
        }

        public async Task<AiResponse> GenerateAsync(string baseUrl, string apiKey, string model, AiRequest request, CancellationToken cancellationToken = default)
        {
            var payload = new GeminiRequest
            {
                Contents = new[]
                {
                    new GeminiContent
                    {
                        Role = "user",
                        Parts = new[] { new GeminiPart { Text = request.UserPrompt } }
                    }
                },
                SystemInstruction = string.IsNullOrWhiteSpace(request.SystemPrompt)
                    ? null
                    : new GeminiContent { Parts = new[] { new GeminiPart { Text = request.SystemPrompt } } },
                GenerationConfig = new GeminiGenerationConfig
                {
                    Temperature = request.Temperature,
                    MaxOutputTokens = request.MaxTokens > 0 ? request.MaxTokens : 1024
                }
            };

            var url = $"{baseUrl.TrimEnd('/')}/v1beta/models/{model}:generateContent?key={apiKey}";
            using var response = await _http.PostAsJsonAsync(url, payload, JsonOptions, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Gemini API returned {(int)response.StatusCode}: {body}");
            }

            var parsed = JsonSerializer.Deserialize<GeminiResponse>(body, JsonOptions)
                ?? throw new InvalidOperationException("Gemini API returned an empty response.");

            var text = parsed.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? string.Empty;

            return new AiResponse(
                Content: text,
                InputTokens: parsed.UsageMetadata?.PromptTokenCount ?? 0,
                OutputTokens: parsed.UsageMetadata?.CandidatesTokenCount ?? 0,
                ProviderName: ApiFormat,
                Model: model
            );
        }

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private class GeminiRequest
        {
            [JsonPropertyName("contents")] public GeminiContent[] Contents { get; set; } = Array.Empty<GeminiContent>();
            [JsonPropertyName("systemInstruction")] public GeminiContent? SystemInstruction { get; set; }
            [JsonPropertyName("generationConfig")] public GeminiGenerationConfig? GenerationConfig { get; set; }
        }

        private class GeminiContent
        {
            [JsonPropertyName("role")] public string? Role { get; set; }
            [JsonPropertyName("parts")] public GeminiPart[] Parts { get; set; } = Array.Empty<GeminiPart>();
        }

        private class GeminiPart
        {
            [JsonPropertyName("text")] public string Text { get; set; } = null!;
        }

        private class GeminiGenerationConfig
        {
            [JsonPropertyName("temperature")] public float Temperature { get; set; }
            [JsonPropertyName("maxOutputTokens")] public int MaxOutputTokens { get; set; }
        }

        private class GeminiResponse
        {
            [JsonPropertyName("candidates")] public List<GeminiCandidate>? Candidates { get; set; }
            [JsonPropertyName("usageMetadata")] public GeminiUsageMetadata? UsageMetadata { get; set; }
        }

        private class GeminiCandidate
        {
            [JsonPropertyName("content")] public GeminiContent? Content { get; set; }
        }

        private class GeminiUsageMetadata
        {
            [JsonPropertyName("promptTokenCount")] public int PromptTokenCount { get; set; }
            [JsonPropertyName("candidatesTokenCount")] public int CandidatesTokenCount { get; set; }
        }
    }
}
