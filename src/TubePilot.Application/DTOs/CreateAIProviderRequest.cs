using System.ComponentModel.DataAnnotations;
using System;

namespace TubePilot.Application.DTOs
{
    public class CreateAIProviderRequest
    {
        [Required]
        public string Name { get; set; }

        public string DisplayName { get; set; }

        [Required]
        public string ProviderType { get; set; }

        public string BaseUrl { get; set; }

        public string ApiKey { get; set; }

        public Guid? OrganizationId { get; set; }

        public string DefaultModel { get; set; }

        public bool SupportsVision { get; set; }
        public bool SupportsImageGeneration { get; set; }
        public bool SupportsStreaming { get; set; }
        public bool SupportsThinking { get; set; }
        public bool SupportsJSONMode { get; set; }
        public bool SupportsFunctionCalling { get; set; }

        public long? DailyLimit { get; set; }
        public long? MonthlyLimit { get; set; }

        public int Priority { get; set; }
        public bool IsEnabled { get; set; } = true;
    }
}
