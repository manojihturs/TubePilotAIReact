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
    }
}
