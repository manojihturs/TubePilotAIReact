using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TubePilot.Application.Interfaces;
using TubePilot.Domain.Entities;

namespace TubePilot.Infrastructure.Services.AI
{
    // The OpenAI-compatible chat completions wire format — shared by OpenAI itself, Groq,
    // NVIDIA NIM, DeepSeek, OpenRouter, Together, and local Ollama servers. Which vendor a
    // tool actually talks to is entirely determined by the AiTool row's BaseUrl, not by any
    // code here.
    public class OpenAiChatAdapter : IAiFormatAdapter
    {
        private readonly HttpClient _http;

        public string ApiFormat => AiApiFormats.OpenAiChat;

        public OpenAiChatAdapter(HttpClient http)
        {
            _http = http;
        }

        public async Task<AiResponse> GenerateAsync(string baseUrl, string apiKey, string model, AiRequest request, CancellationToken cancellationToken = default)
        {
            var messages = new List<ChatMessage>();
            if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
            {
                messages.Add(new ChatMessage { Role = "system", Content = request.SystemPrompt });
            }
            messages.Add(new ChatMessage { Role = "user", Content = request.UserPrompt });

            var payload = new ChatCompletionsRequest
            {
                Model = model,
                Messages = messages.ToArray(),
                Temperature = request.Temperature,
                MaxTokens = request.MaxTokens > 0 ? request.MaxTokens : 1024
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, CombineUrl(baseUrl, "chat/completions"));
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            httpRequest.Content = JsonContent.Create(payload);
            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            using var response = await _http.SendAsync(httpRequest, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"OpenAI-compatible API returned {(int)response.StatusCode}: {body}");
            }

            var parsed = JsonSerializer.Deserialize<ChatCompletionsResponse>(body, JsonOptions)
                ?? throw new InvalidOperationException("OpenAI-compatible API returned an empty response.");

            var text = parsed.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;

            return new AiResponse(
                Content: text,
                InputTokens: parsed.Usage?.PromptTokens ?? 0,
                OutputTokens: parsed.Usage?.CompletionTokens ?? 0,
                ProviderName: ApiFormat,
                Model: model
            );
        }

        private static string CombineUrl(string baseUrl, string path) => $"{baseUrl.TrimEnd('/')}/{path}";

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private class ChatCompletionsRequest
        {
            [JsonPropertyName("model")] public string Model { get; set; } = null!;
            [JsonPropertyName("messages")] public ChatMessage[] Messages { get; set; } = Array.Empty<ChatMessage>();
            [JsonPropertyName("temperature")] public float Temperature { get; set; }
            [JsonPropertyName("max_tokens")] public int MaxTokens { get; set; }
        }

        private class ChatMessage
        {
            [JsonPropertyName("role")] public string Role { get; set; } = null!;
            [JsonPropertyName("content")] public string Content { get; set; } = null!;
        }

        private class ChatCompletionsResponse
        {
            [JsonPropertyName("choices")] public List<ChatChoice>? Choices { get; set; }
            [JsonPropertyName("usage")] public ChatUsage? Usage { get; set; }
        }

        private class ChatChoice
        {
            [JsonPropertyName("message")] public ChatMessage? Message { get; set; }
        }

        private class ChatUsage
        {
            [JsonPropertyName("prompt_tokens")] public int PromptTokens { get; set; }
            [JsonPropertyName("completion_tokens")] public int CompletionTokens { get; set; }
        }
    }
}
