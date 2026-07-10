using System;

namespace TubePilot.Domain.Entities
{
    public class PromptExecution
    {
        public Guid Id { get; set; }

        public Guid PromptId { get; set; }
        public TubePilot.Domain.Entities.Prompt Prompt { get; set; }

        public Guid? ProviderId { get; set; }
        public TubePilot.Domain.Entities.AIProvider Provider { get; set; }

        public Guid? ModelId { get; set; }
        public TubePilot.Domain.Entities.AIModel Model { get; set; }

        public string VariablesJson { get; set; }

        public string RenderedPrompt { get; set; }

        public string Response { get; set; }

        public int? InputTokens { get; set; }

        public int? OutputTokens { get; set; }

        public decimal? EstimatedCost { get; set; }

        public long? Latency { get; set; }

        public string Status { get; set; }

        public DateTime CreatedDate { get; set; }

        public string ExecutedBy { get; set; }
    }
}
