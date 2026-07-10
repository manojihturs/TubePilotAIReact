using System;
using System.ComponentModel.DataAnnotations;

namespace TubePilot.Application.DTOs
{
    public class UpdatePromptRequest
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public Guid CategoryId { get; set; }
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = null!;
        [StringLength(1000)]
        public string? Description { get; set; }
        [Required]
        public string PromptText { get; set; } = null!;
        public double? Temperature { get; set; }
        public double? TopP { get; set; }
        public int? MaxTokens { get; set; }
        [StringLength(200)]
        public string? Model { get; set; }
        [StringLength(200)]
        public string? Provider { get; set; }
        [StringLength(100)]
        public string? Version { get; set; }
        [StringLength(50)]
        public string? Status { get; set; }
        [StringLength(500)]
        public string? Tags { get; set; }
        public bool IsSystem { get; set; }
        public bool IsPublic { get; set; }
    }
}
