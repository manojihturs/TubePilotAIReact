namespace TubePilot.Application.DTOs
{
    public class CreateProjectRequest
    {
        public string Title { get; set; } = null!;
        public Guid? PromptId { get; set; }
    }
}
