namespace TubePilot.Application.DTOs
{
    // Never carries the API key back to the client.
    public class AiToolDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string ApiFormat { get; set; } = null!;
        public string BaseUrl { get; set; } = null!;
        public string Model { get; set; } = null!;
        public int Priority { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime? LastUsedAt { get; set; }
    }

    public class CreateAiToolRequest
    {
        public string Name { get; set; } = null!;
        public string ApiFormat { get; set; } = null!;
        public string BaseUrl { get; set; } = null!;
        public string Model { get; set; } = null!;
        public string ApiKey { get; set; } = null!;
        public int Priority { get; set; }
    }

    public class UpdateAiToolRequest
    {
        public string Name { get; set; } = null!;
        public string BaseUrl { get; set; } = null!;
        public string Model { get; set; } = null!;
        // Optional — omit to keep the existing stored key.
        public string? ApiKey { get; set; }
        public int Priority { get; set; }
        public bool IsEnabled { get; set; } = true;
    }
}
