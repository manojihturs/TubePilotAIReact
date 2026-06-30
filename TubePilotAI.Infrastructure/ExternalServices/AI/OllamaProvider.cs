using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using TubePilotAI.Application.Abstractions.ExternalServices;

namespace TubePilotAI.Infrastructure.ExternalServices.AI;

public sealed class OllamaProvider(
    HttpClient httpClient,
    ILogger<OllamaProvider> logger) : IAIProvider
{
    private string? _baseUrl;

    public AIProviderType ProviderType => AIProviderType.Ollama;
    public string ProviderName => "Ollama";

    public async Task<AIProviderResponse> GenerateAsync(AIProviderRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _baseUrl ??= Environment.GetEnvironmentVariable("OLLAMA_BASE_URL") ?? "http://localhost:11434";

            var model = request.Model ?? "mistral";
            var prompt = (request.SystemMessage ?? "") + "\n\n" + request.Prompt;

            var requestBody = new OllamaRequest
            {
                Model = model,
                Prompt = prompt,
                Temperature = (float)request.Temperature,
                Stream = false
            };

            var apiUrl = $"{_baseUrl}/api/generate";
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await httpClient.PostAsync(apiUrl, jsonContent, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError("Ollama API error: {statusCode} - {error}", response.StatusCode, errorContent);
                return new AIProviderResponse 
                { 
                    Content = $"[Ollama Error: {response.StatusCode} - Make sure Ollama is running at {_baseUrl}]",
                    Provider = AIProviderType.Ollama,
                    Model = model,
                    IsMock = true,
                    CreatedAtUtc = DateTime.UtcNow
                };
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var content = ollamaResponse?.Response ?? "[No content generated]";

            return new AIProviderResponse
            {
                Content = content,
                Provider = AIProviderType.Ollama,
                Model = model,
                PromptTokens = ollamaResponse?.PromptEvalCount ?? 0,
                CompletionTokens = ollamaResponse?.EvalCount ?? 0,
                IsMock = false,
                CreatedAtUtc = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ollama API exception");
            return new AIProviderResponse 
            { 
                Content = $"[Ollama Error: {ex.Message}]",
                Provider = AIProviderType.Ollama,
                Model = request.Model ?? "mistral",
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

    #region Ollama API Models

    private sealed class OllamaRequest
    {
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("prompt")]
        public string? Prompt { get; set; }

        [JsonPropertyName("temperature")]
        public float Temperature { get; set; }

        [JsonPropertyName("stream")]
        public bool Stream { get; set; }
    }

    private sealed class OllamaResponse
    {
        [JsonPropertyName("response")]
        public string? Response { get; set; }

        [JsonPropertyName("prompt_eval_count")]
        public int PromptEvalCount { get; set; }

        [JsonPropertyName("eval_count")]
        public int EvalCount { get; set; }
    }

    #endregion
}
