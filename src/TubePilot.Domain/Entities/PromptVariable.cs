using System;

namespace TubePilot.Domain.Entities
{
    public class PromptVariable
    {
        public Guid Id { get; set; }
        public Guid PromptId { get; set; }
        public string Name { get; set; } = null!; // e.g., TOPIC
        public string Placeholder { get; set; } = null!; // e.g., {{TOPIC}}
        public string? Description { get; set; }
        public string? DataType { get; set; }
        public bool IsRequired { get; set; } = false;
        public string? DefaultValue { get; set; }
        public bool SoftDelete { get; set; } = false;
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Navigation
        public Prompt? Prompt { get; set; }
    }
}
