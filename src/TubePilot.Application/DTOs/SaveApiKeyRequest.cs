namespace TubePilot.Application.DTOs
{
    public class SaveApiKeyRequest
    {
        public string ProviderName { get; set; } = null!;
        public string ApiKey { get; set; } = null!;
    }
}
