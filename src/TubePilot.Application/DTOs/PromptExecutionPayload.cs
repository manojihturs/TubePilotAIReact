using System.Collections.Generic;

namespace TubePilot.Application.DTOs
{
    public class PromptExecutionPayload
    {
        public string RenderedPrompt { get; set; }
        public int EstimatedTokens { get; set; }
        public IDictionary<string, string> Variables { get; set; }
    }
}
