namespace TubePilotAI.Application.DTOs
{
    public class GenerationStatusDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public string Status { get; set; } = "Queued";
        public int ProgressPercent { get; set; }
        public string[] Logs { get; set; } = new string[0];
    }
}
