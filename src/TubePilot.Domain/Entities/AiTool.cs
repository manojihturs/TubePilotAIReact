namespace TubePilot.Domain.Entities
{
    // A user-defined AI text-generation tool: any number per user, each pointing at a real
    // HTTP API. ApiFormat picks which of the fixed wire-format adapters (Anthropic Messages,
    // OpenAI-compatible chat completions, Google Gemini) to speak — not the vendor name, so
    // one "openai-chat" tool can be OpenAI, Groq, NVIDIA NIM, DeepSeek, OpenRouter, or a local
    // Ollama server, just by pointing BaseUrl at the right host. Nothing about a specific
    // vendor is hardcoded in the domain or generation pipeline.
    public class AiTool
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = null!;
        public string ApiFormat { get; set; } = null!;
        public string BaseUrl { get; set; } = null!;
        public string Model { get; set; } = null!;
        public string EncryptedApiKey { get; set; } = null!;
        // Lower runs first. Generation walks enabled tools in this order and falls through
        // to the next one on failure (auth error, rate limit, outage) without aborting.
        public int Priority { get; set; }
        public bool IsEnabled { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
    }

    public static class AiApiFormats
    {
        public const string AnthropicMessages = "anthropic-messages";
        public const string OpenAiChat = "openai-chat";
        public const string Gemini = "gemini";

        public static readonly string[] All = { AnthropicMessages, OpenAiChat, Gemini };
    }
}
