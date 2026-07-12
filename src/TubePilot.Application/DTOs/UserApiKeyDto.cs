namespace TubePilot.Application.DTOs
{
    public class UserApiKeyDto
    {
        public string ProviderName { get; set; } = null!;
        public bool Saved { get; set; }
        public DateTime? LastUsedAt { get; set; }
    }
}
