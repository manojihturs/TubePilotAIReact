using System;
using System.Collections.Generic;

namespace TubePilot.Domain.Entities
{
    public class Workflow
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Steps in order
        public ICollection<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
    }
}
