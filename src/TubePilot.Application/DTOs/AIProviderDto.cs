using System;

namespace TubePilot.Application.DTOs
{
    public class AIProviderDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string ProviderType { get; set; }
        public string BaseUrl { get; set; }
        // Note: ApiKeyEncrypted is not returned in DTOs
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
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
