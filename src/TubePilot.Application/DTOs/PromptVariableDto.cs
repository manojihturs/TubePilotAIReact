using System;

namespace TubePilot.Application.DTOs
{
    public class PromptVariableDto
    {
        public Guid Id { get; set; }
        public Guid PromptId { get; set; }
        public string Name { get; set; } = null!;
        public string Placeholder { get; set; } = null!;
        public string? Description { get; set; }
        public string? DataType { get; set; }
        public bool IsRequired { get; set; }
        public string? DefaultValue { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
