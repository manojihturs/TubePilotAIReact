using System.Collections.Generic;
using System.Threading.Tasks;
using TubePilot.Application.DTOs;

namespace TubePilot.Application.Interfaces
{
    public interface IPromptEngine
    {
        /// <summary>
        /// Render a prompt template by replacing variables with provided values.
        /// </summary>
        string Render(string template, IDictionary<string, string> variables);

        /// <summary>
        /// Validate a prompt template. Optionally validate against provided variables (report missing values).
        /// </summary>
        PromptValidationResult Validate(string template, IDictionary<string, string> variables = null);

        /// <summary>
        /// Produce a preview of the prompt (rendered and truncated).
        /// </summary>
        string Preview(string template, IDictionary<string, string> variables = null, int maxLength = 200);

        /// <summary>
        /// Rough token estimation for a prompt text.
        /// </summary>
        int EstimateTokens(string text);

        /// <summary>
        /// Prepare execution payload for a prompt: rendered text, token estimate and metadata.
        /// </summary>
        Task<PromptExecutionPayload> PrepareExecutionAsync(string template, IDictionary<string, string> variables);
    }
}
