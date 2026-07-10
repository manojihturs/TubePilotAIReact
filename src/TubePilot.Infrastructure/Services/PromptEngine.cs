using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TubePilot.Application.DTOs;
using TubePilot.Application.Interfaces;

namespace TubePilot.Infrastructure.Services
{
    public class PromptEngine : IPromptEngine
    {
        // Variable pattern: {{VAR}}
        private static readonly Regex _variableRegex = new(@"\{\{\s*(?<name>[A-Za-z0-9_]+)\s*\}\}", RegexOptions.Compiled);

        // Known supported variables
        private static readonly HashSet<string> _supportedVariables = new(StringComparer.OrdinalIgnoreCase)
        {
            "TOPIC", "ACTOR", "YEAR", "LANGUAGE", "CATEGORY", "STYLE", "VOICE", "DURATION"
        };

        public string Render(string template, IDictionary<string, string> variables)
        {
            if (template == null) return string.Empty;

            return _variableRegex.Replace(template, m =>
            {
                var name = m.Groups["name"].Value;
                if (variables != null && variables.TryGetValue(name, out var val))
                {
                    return val ?? string.Empty;
                }

                // If variable known but not provided, leave placeholder or replace with empty
                if (_supportedVariables.Contains(name)) return string.Empty;

                // Unknown variables kept as-is so callers can detect
                return m.Value;
            });
        }

        public PromptValidationResult Validate(string template, IDictionary<string, string> variables = null)
        {
            var result = new PromptValidationResult { IsValid = true };

            if (string.IsNullOrWhiteSpace(template))
            {
                result.IsValid = false;
                result.Messages.Add("Template is empty.");
                return result;
            }

            var found = _variableRegex.Matches(template).Select(m => m.Groups["name"].Value).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            foreach (var v in found)
            {
                if (!_supportedVariables.Contains(v))
                {
                    result.UnknownVariables.Add(v);
                    result.IsValid = false;
                }
            }

            if (variables != null)
            {
                foreach (var kv in variables)
                {
                    if (!_supportedVariables.Contains(kv.Key))
                    {
                        result.UnknownVariables.Add(kv.Key);
                        result.IsValid = false;
                    }
                }

                // Missing supported variables used in template
                var missing = found.Where(f => _supportedVariables.Contains(f) && (variables == null || !variables.ContainsKey(f) || string.IsNullOrWhiteSpace(variables[f]))).ToList();
                if (missing.Any())
                {
                    result.MissingVariables.AddRange(missing);
                    result.IsValid = false;
                }
            }

            return result;
        }

        public string Preview(string template, IDictionary<string, string> variables = null, int maxLength = 200)
        {
            var rendered = Render(template, variables);
            if (rendered.Length <= maxLength) return rendered;
            return rendered.Substring(0, maxLength) + "...";
        }

        public int EstimateTokens(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;

            // Rough heuristic: 1 token ~ 4 characters for English; use chars/4
            var chars = text.Length;
            return Math.Max(1, chars / 4);
        }

        public Task<PromptExecutionPayload> PrepareExecutionAsync(string template, IDictionary<string, string> variables)
        {
            var rendered = Render(template, variables);
            var estimate = EstimateTokens(rendered);

            var payload = new PromptExecutionPayload
            {
                RenderedPrompt = rendered,
                EstimatedTokens = estimate,
                Variables = variables ?? new Dictionary<string, string>()
            };

            return Task.FromResult(payload);
        }
    }
}
