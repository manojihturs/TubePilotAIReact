namespace TubePilot.Domain.Entities
{
    public enum RenderFormat { Desktop, Shorts }
    public enum RenderStatus { Queued, Rendering, Complete, Failed }

    // Rendering is genuinely async (minutes, not seconds) unlike text generation —
    // tracked here so the frontend can poll status rather than blocking a request.
    public class RenderJob
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public RenderFormat Format { get; set; }
        public int DurationSeconds { get; set; }
        public string Language { get; set; } = "en";
        public RenderStatus Status { get; set; }
        public string? OutputPath { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
