namespace TubePilot.Application.DTOs
{
    public class ImageCandidateDto
    {
        public string ThumbnailUrl { get; set; } = null!;
        public string FullSizeUrl { get; set; } = null!;
        public string SourceUrl { get; set; } = null!;
        public string License { get; set; } = null!;
    }

    public class ImageCandidatesRequest
    {
        // Optional manual override — user can re-trigger the search with an edited query
        // if the auto-built query returned the wrong result.
        public string? Query { get; set; }
    }

    public class ImageCandidatesResponseDto
    {
        public Guid RowId { get; set; }
        public string Query { get; set; } = null!;
        public List<ImageCandidateDto> Candidates { get; set; } = new();
    }

    public class ImageSelectRequest
    {
        public string FullSizeUrl { get; set; } = null!;
    }

    public class ImageGenerateRequest
    {
        // Optional override — defaults to a poster-style prompt built from the row's own columns.
        public string? Prompt { get; set; }
    }

    public class DataRowDto
    {
        public Guid Id { get; set; }
        public Guid ProjectOutputId { get; set; }
        public string OutputItemName { get; set; } = null!;
        public int RowIndex { get; set; }
        public Dictionary<string, string> Data { get; set; } = new();
        public string ImageStatus { get; set; } = null!;
        public string? ConfirmedImagePath { get; set; }
    }
}
