using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TubePilot.Application.Interfaces;

namespace TubePilot.Infrastructure.Services.AI
{
    // Groq hosts open-source models (Llama, Gemma, Mixtral) behind an OpenAI-compatible
    // chat completions API, with a free tier — no local GPU required.
    public class GroqProvider : IAiProvider
    {
        private readonly HttpClient _http;

        public string ProviderName => "Groq";

        public GroqProvider(HttpClient http)
        {
            _http = http;
            if (_http.BaseAddress == null)
            {
                _http.BaseAddress = new Uri("https://api.groq.com/");
            }
        }

        public async Task<AiResponse> GenerateAsync(string apiKey, AiRequest request, CancellationToken cancellationToken = default)
        {
            var model = string.IsNullOrWhiteSpace(request.Model) ? "llama-3.3-70b-versatile" : request.Model;

            var messages = new List<GroqMessage>();
            if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
            {
                messages.Add(new GroqMessage { Role = "system", Content = request.SystemPrompt });
            }
            messages.Add(new GroqMessage { Role = "user", Content = request.UserPrompt });

            var payload = new GroqChatRequest
            {
                Model = model,
                Messages = messages.ToArray(),
                Temperature = request.Temperature,
                MaxTokens = request.MaxTokens > 0 ? request.MaxTokens : 1024
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "openai/v1/chat/completions");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            httpRequest.Content = JsonContent.Create(payload);
            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            using var response = await _http.SendAsync(httpRequest, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Groq API returned {(int)response.StatusCode}: {body}");
            }

            var parsed = JsonSerializer.Deserialize<GroqChatResponse>(body, JsonOptions)
                ?? throw new InvalidOperationException("Groq API returned an empty response.");

            var text = parsed.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;

            return new AiResponse(
                Content: text,
                InputTokens: parsed.Usage?.PromptTokens ?? 0,
                OutputTokens: parsed.Usage?.CompletionTokens ?? 0,
                ProviderName: ProviderName,
                Model: model
            );
        }

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private class GroqChatRequest
        {
            [JsonPropertyName("model")] public string Model { get; set; } = null!;
            [JsonPropertyName("messages")] public GroqMessage[] Messages { get; set; } = Array.Empty<GroqMessage>();
            [JsonPropertyName("temperature")] public float Temperature { get; set; }
            [JsonPropertyName("max_tokens")] public int MaxTokens { get; set; }
        }

        private class GroqMessage
        {
            [JsonPropertyName("role")] public string Role { get; set; } = null!;
            [JsonPropertyName("content")] public string Content { get; set; } = null!;
        }

        private class GroqChatResponse
        {
            [JsonPropertyName("choices")] public List<GroqChoice>? Choices { get; set; }
            [JsonPropertyName("usage")] public GroqUsage? Usage { get; set; }
        }

        private class GroqChoice
        {
            [JsonPropertyName("message")] public GroqMessage? Message { get; set; }
        }

        private class GroqUsage
        {
            [JsonPropertyName("prompt_tokens")] public int PromptTokens { get; set; }
            [JsonPropertyName("completion_tokens")] public int CompletionTokens { get; set; }
        }
    }
}
