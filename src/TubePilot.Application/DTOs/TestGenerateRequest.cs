namespace TubePilot.Application.DTOs
{
    public class TestGenerateRequest
    {
        public string? Title { get; set; }
        public string Prompt { get; set; } = null!;
        public string ProviderName { get; set; } = "Claude";
        public string? Model { get; set; }
    }
}
