namespace TubePilot.Application.DTOs
{
    public class GeneratedOutputDto
    {
        public string OutputItemName { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string FolderName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public int RowCount { get; set; }
    }

    public class GenerateResultDto
    {
        public Guid ProjectId { get; set; }
        public List<GeneratedOutputDto> Outputs { get; set; } = new();

        // Per-item failures that survived every connected provider fallback — the item was
        // skipped, but everything else in the project still generated. Empty when everything
        // succeeded (on the first try or after falling back).
        public List<string> Warnings { get; set; } = new();
    }
}
