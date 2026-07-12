namespace TubePilot.Domain.Entities
{
    public class UserApiKey
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string ProviderName { get; set; } = null!;
        public string EncryptedKey { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
    }
}
