namespace TubePilotAI.Application.DTOs
{
    public class GenerateProjectResponseDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public string Status { get; set; } = "Queued";
    }
}
