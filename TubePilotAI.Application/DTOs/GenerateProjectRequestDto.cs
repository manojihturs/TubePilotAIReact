namespace TubePilotAI.Application.DTOs
{
    public class GenerateProjectRequestDto
    {
        public string Topic { get; set; } = string.Empty;
        public string AiProvider { get; set; } = string.Empty;
        public string PromptTemplate { get; set; } = string.Empty;
        public string Language { get; set; } = "en";
        public string OutputFolder { get; set; } = string.Empty;
    }
}
