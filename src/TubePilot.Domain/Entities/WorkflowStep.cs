using System;

namespace TubePilot.Domain.Entities
{
    public class WorkflowStep
    {
        public Guid Id { get; set; }
        public Guid WorkflowId { get; set; }
        public Workflow Workflow { get; set; }

        // e.g., Research, Movie Data, Voice Over, Images, Thumbnail, SEO, Social Media, Export
        public string Name { get; set; }
        public string Type { get; set; }
        public int Order { get; set; }
        public string ConfigJson { get; set; }
        public bool IsEnabled { get; set; }
    }
}
