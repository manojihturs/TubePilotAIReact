namespace TubePilot.Application.DTOs
{
    public class GenerateRequest
    {
        public Guid ProjectId { get; set; }
        // Optional — try this AiTool first, then fall through the rest of the user's
        // enabled tools in priority order. Omit to just use priority order from the start.
        public Guid? PreferredAiToolId { get; set; }
    }
}
