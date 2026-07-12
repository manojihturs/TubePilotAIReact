using System;

namespace TubePilot.Application.DTOs
{
    public class PromptDto
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string PromptText { get; set; } = null!;
        public string? OutputSpecJson { get; set; }
        public double? Temperature { get; set; }
        public double? TopP { get; set; }
        public int? MaxTokens { get; set; }
        public string? Model { get; set; }
        public string? Provider { get; set; }
        public string? Version { get; set; }
        public string? Status { get; set; }
        public string? Tags { get; set; }
        public bool IsSystem { get; set; }
        public bool IsPublic { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
