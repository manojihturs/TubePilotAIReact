namespace TubePilot.Application.DTOs
{
    public class GenerateThumbnailRequest
    {
        // Optional — defaults to a prompt built from the project's title if not given.
        public string? Prompt { get; set; }
    }
}
