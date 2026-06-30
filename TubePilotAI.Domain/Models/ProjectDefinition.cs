using System;
using System.Collections.Generic;

namespace TubePilotAI.Domain.Models
{
    /// <summary>
    /// Domain model representing a generated AI project and its high-level metadata.
    /// </summary>
    public class ProjectDefinition
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Topic { get; set; } = string.Empty;

        public string Language { get; set; } = "en";

        public string OutputFolder { get; set; } = string.Empty;

        public string AiProvider { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Asset> Assets { get; set; } = new List<Asset>();

        public List<PromptTemplate> PromptTemplates { get; set; } = new List<PromptTemplate>();
    }
}
