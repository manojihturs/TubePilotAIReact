namespace TubePilot.Application.DTOs
{
    public class GenerateRequest
    {
        public Guid ProjectId { get; set; }
        public string ProviderName { get; set; } = "Claude";
        public string? Model { get; set; }
    }
}
