using System;

namespace TubePilotAI.Domain.Models
{
    /// <summary>
    /// Represents a prompt template stored in the domain for reuse by the generator and UI.
    /// </summary>
    public class PromptTemplate
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string Language { get; set; } = "en";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string Version { get; set; } = "1.0";
    }
}
