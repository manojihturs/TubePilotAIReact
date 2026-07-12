namespace TubePilot.Domain.Entities
{
    public enum ProjectOutputType { Table, Text, ImageSet }

    // One row per OutputItem actually produced for a specific project — no assumption
    // of fixed file slots, matches exactly what the prompt's OutputSpec declared.
    public class ProjectOutput
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string OutputItemName { get; set; } = null!;
        public ProjectOutputType Type { get; set; }
        public string FolderName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
