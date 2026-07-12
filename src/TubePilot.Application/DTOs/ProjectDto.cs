namespace TubePilot.Application.DTOs
{
    public class ProjectDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string FolderPath { get; set; } = null!;
        public string? ThumbnailPath { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
