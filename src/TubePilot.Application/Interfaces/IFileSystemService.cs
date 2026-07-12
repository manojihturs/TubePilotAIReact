namespace TubePilot.Application.Interfaces
{
    public enum OutputItemType { Table, Text, ImageSet }

    public class OutputItem
    {
        public string Name { get; set; } = null!;
        public OutputItemType Type { get; set; }
        public List<string> Columns { get; set; } = new();
        public bool RequiresImages { get; set; }
        public string FolderName { get; set; } = null!;
        public List<string> ImageQueryColumns { get; set; } = new();
        // Appended to every row's built query — e.g. "movie poster" so a row like
        // "The Mummy 2017" searches as "The Mummy 2017 movie poster" instead of a bare
        // name/year that Wikimedia matches against unrelated photos.
        public string? ImageQuerySuffix { get; set; }
    }

    public class OutputSpec
    {
        public List<OutputItem> Items { get; set; } = new();
    }

    public interface IFileSystemService
    {
        // Creates Input/ and Export/ (always) plus one folder per spec item, and Assets/ if any
        // item requires images. Returns the absolute path to the created project folder.
        string CreateProjectStructure(Guid projectId, string projectTitle, OutputSpec spec);

        // Writes one output item's content into its own declared folder. Table content is a
        // List<Dictionary<string,string>> written as CSV; Text content is a plain string.
        // Returns the absolute path to the written file.
        string WriteOutputItem(string projectPath, OutputItem item, object content);

        // Saves a confirmed row image into the project's Assets/ folder. Returns the absolute path.
        Task<string> SaveImageAsync(string projectPath, string fileName, Stream imageStream, CancellationToken cancellationToken = default);

        // Saves the single AI-generated video thumbnail into the project's Thumbnail/ folder
        // (created on demand — not part of the per-prompt OutputSpec). Returns the absolute path.
        Task<string> SaveThumbnailAsync(string projectPath, byte[] imageBytes, CancellationToken cancellationToken = default);
    }
}
