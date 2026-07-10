using System;
using System.ComponentModel.DataAnnotations;

namespace TubePilot.Application.DTOs
{
    public class UpdatePromptVariableRequest
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid PromptId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [StringLength(150)]
        public string Placeholder { get; set; } = null!;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? DataType { get; set; }

        public bool IsRequired { get; set; }

        [StringLength(200)]
        public string? DefaultValue { get; set; }
    }
}
