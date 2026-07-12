namespace TubePilot.Application.DTOs
{
    public class RenderRequest
    {
        public string Format { get; set; } = "Desktop"; // "Desktop" | "Shorts"
        public int DurationSeconds { get; set; } = 60;
        public string Language { get; set; } = "en"; // "en" | "ta"
    }

    public class RenderJobDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string Format { get; set; } = null!;
        public int DurationSeconds { get; set; }
        public string Language { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? OutputPath { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
