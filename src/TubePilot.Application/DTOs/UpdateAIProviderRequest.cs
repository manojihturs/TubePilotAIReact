using System;
using System.ComponentModel.DataAnnotations;

namespace TubePilot.Application.DTOs
{
    public class UpdateAIProviderRequest
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string DisplayName { get; set; }

        [Required]
        public string ProviderType { get; set; }

        public string BaseUrl { get; set; }

        // ApiKey is optional on update
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
        public bool IsEnabled { get; set; }
    }
}
