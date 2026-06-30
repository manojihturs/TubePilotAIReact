namespace TubePilotAI.Application.DTOs
{
    public class AIRequestDto
    {
        public string Topic { get; set; } = string.Empty;
        public string Language { get; set; } = "en";
        public string Template { get; set; } = string.Empty;
        public int Items { get; set; } = 10;
    }
}
