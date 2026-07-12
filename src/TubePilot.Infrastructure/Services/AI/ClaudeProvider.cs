using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TubePilot.Application.Interfaces;

namespace TubePilot.Infrastructure.Services.AI
{
    public class ClaudeProvider : IAiProvider
    {
        private const string AnthropicVersion = "2023-06-01";
        private readonly HttpClient _http;

        public string ProviderName => "Claude";

        public ClaudeProvider(HttpClient http)
        {
            _http = http;
            if (_http.BaseAddress == null)
            {
                _http.BaseAddress = new Uri("https://api.anthropic.com/");
            }
        }

        public async Task<AiResponse> GenerateAsync(string apiKey, AiRequest request, CancellationToken cancellationToken = default)
        {
            var model = string.IsNullOrWhiteSpace(request.Model) ? "claude-sonnet-5" : request.Model;

            var payload = new ClaudeMessagesRequest
            {
                Model = model,
                MaxTokens = request.MaxTokens > 0 ? request.MaxTokens : 1024,
                Temperature = request.Temperature,
                System = request.SystemPrompt,
                Messages = new[]
                {
                    new ClaudeMessage { Role = "user", Content = request.UserPrompt }
                }
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "v1/messages");
            httpRequest.Headers.Add("x-api-key", apiKey);
            httpRequest.Headers.Add("anthropic-version", AnthropicVersion);
            httpRequest.Content = JsonContent.Create(payload);
            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            using var response = await _http.SendAsync(httpRequest, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Claude API returned {(int)response.StatusCode}: {body}");
            }

            var parsed = JsonSerializer.Deserialize<ClaudeMessagesResponse>(body, JsonOptions)
                ?? throw new InvalidOperationException("Claude API returned an empty response.");

            var text = string.Join(string.Empty, parsed.Content?.Select(c => c.Text) ?? Array.Empty<string>());

            return new AiResponse(
                Content: text,
                InputTokens: parsed.Usage?.InputTokens ?? 0,
                OutputTokens: parsed.Usage?.OutputTokens ?? 0,
                ProviderName: ProviderName,
                Model: model
            );
        }

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private class ClaudeMessagesRequest
        {
            [JsonPropertyName("model")] public string Model { get; set; } = null!;
            [JsonPropertyName("max_tokens")] public int MaxTokens { get; set; }
            [JsonPropertyName("temperature")] public float Temperature { get; set; }
            [JsonPropertyName("system")] public string? System { get; set; }
            [JsonPropertyName("messages")] public ClaudeMessage[] Messages { get; set; } = Array.Empty<ClaudeMessage>();
        }

        private class ClaudeMessage
        {
            [JsonPropertyName("role")] public string Role { get; set; } = null!;
            [JsonPropertyName("content")] public string Content { get; set; } = null!;
        }

        private class ClaudeMessagesResponse
        {
            [JsonPropertyName("content")] public List<ClaudeContentBlock>? Content { get; set; }
            [JsonPropertyName("usage")] public ClaudeUsage? Usage { get; set; }
        }

        private class ClaudeContentBlock
        {
            [JsonPropertyName("type")] public string? Type { get; set; }
            [JsonPropertyName("text")] public string? Text { get; set; }
        }

        private class ClaudeUsage
        {
            [JsonPropertyName("input_tokens")] public int InputTokens { get; set; }
            [JsonPropertyName("output_tokens")] public int OutputTokens { get; set; }
        }
    }
}
