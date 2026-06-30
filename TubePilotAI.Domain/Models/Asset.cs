using System;

namespace TubePilotAI.Domain.Models
{
    /// <summary>
    /// Represents a media asset (poster, thumbnail, background, etc.) used by a generated project.
    /// </summary>
    public class Asset
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string FileName { get; set; } = string.Empty;

        public string RelativePath { get; set; } = string.Empty;

        public long SizeBytes { get; set; }

        public string ContentType { get; set; } = string.Empty;

        public string SourceUrl { get; set; } = string.Empty;

        public string AssetType { get; set; } = string.Empty; // e.g., Poster, Thumbnail
    }
}
