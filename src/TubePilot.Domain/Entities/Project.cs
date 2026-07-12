namespace TubePilot.Domain.Entities
{
    public class Project
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? PromptId { get; set; }
        public string Title { get; set; } = null!;
        public string FolderPath { get; set; } = null!;
        // The exact OutputSpec that shaped this project's folder tree at creation time —
        // stored here (not re-read from the prompt) so later generation never drifts from
        // what the folders were actually built to hold, even if the prompt is edited after.
        public string OutputSpecJson { get; set; } = null!;
        public string? ThumbnailPath { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
