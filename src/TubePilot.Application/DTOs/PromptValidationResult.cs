using System.Collections.Generic;

namespace TubePilot.Application.DTOs
{
    public class PromptValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> MissingVariables { get; set; } = new List<string>();
        public List<string> UnknownVariables { get; set; } = new List<string>();
        public List<string> Messages { get; set; } = new List<string>();
    }
}
