using System;
using System.ComponentModel.DataAnnotations;

namespace TubePilot.Application.DTOs
{
    public class UpdatePromptCategoryRequest
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = null!;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Range(0, int.MaxValue)]
        public int DisplayOrder { get; set; }

        [StringLength(200)]
        public string? Icon { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
