using System;
using System.ComponentModel.DataAnnotations;

namespace TubePilot.Application.DTOs
{
    public class CreateAIModelRequest
    {
        [Required]
        public Guid ProviderId { get; set; }

        [Required]
        public string Name { get; set; }

        public string DisplayName { get; set; }
        public int? ContextWindow { get; set; }
        public int? MaxInputTokens { get; set; }
        public int? MaxOutputTokens { get; set; }
        public bool SupportsVision { get; set; }
        public bool SupportsAudio { get; set; }
        public bool SupportsVideo { get; set; }
        public bool SupportsTools { get; set; }
        public bool SupportsStreaming { get; set; }
        public bool SupportsReasoning { get; set; }
        public bool SupportsJSON { get; set; }
        public bool SupportsImageGeneration { get; set; }
        public decimal? InputPrice { get; set; }
        public decimal? OutputPrice { get; set; }
        public bool IsDefault { get; set; }
        public bool IsEnabled { get; set; } = true;
    }
}
