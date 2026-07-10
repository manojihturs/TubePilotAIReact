using System;

namespace TubePilot.Domain.Entities
{
    public class AIProvider
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string ProviderType { get; set; }

        public string BaseUrl { get; set; }

        public string ApiKeyEncrypted { get; set; }

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

        // Auditing
        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public string UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public bool SoftDelete { get; set; }
    }
}
