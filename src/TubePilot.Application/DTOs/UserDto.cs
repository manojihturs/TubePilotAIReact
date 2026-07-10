namespace TubePilot.Application.DTOs
{
    public class UserDto
    {
        public System.Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string? Role { get; set; }
    }
}