using System;

namespace TubePilot.Domain.Entities
{
    public class WorkflowExecution
    {
        public Guid Id { get; set; }
        public Guid WorkflowId { get; set; }
        public Workflow Workflow { get; set; }

        // Current step index or Id
        public int CurrentStep { get; set; }
        public string Status { get; set; }
        public string InputJson { get; set; }
        public string ResultJson { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string StartedBy { get; set; }
    }
}
