namespace TubePilot.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        // For Phase 0 demo we store a password field; replace with hashed passwords/Identity in Phase 1
        public string Password { get; set; } = null!;
        public string? Role { get; set; }
    }
}