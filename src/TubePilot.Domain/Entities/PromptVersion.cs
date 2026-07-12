namespace TubePilot.Domain.Entities
{
    // Append-only history: never overwrite TemplateText/OutputSpecJson on edit,
    // always insert a new PromptVersion. Gives free rollback with zero extra logic.
    public class PromptVersion
    {
        public Guid Id { get; set; }
        public Guid PromptId { get; set; }
        public int VersionNumber { get; set; }
        public string TemplateText { get; set; } = null!;
        public string? OutputSpecJson { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
