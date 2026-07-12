namespace TubePilot.Application.DTOs
{
    public class PromptVersionDto
    {
        public Guid Id { get; set; }
        public int VersionNumber { get; set; }
        public string TemplateText { get; set; } = null!;
        public string? OutputSpecJson { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
