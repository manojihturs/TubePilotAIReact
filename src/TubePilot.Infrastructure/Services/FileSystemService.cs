using Microsoft.Extensions.Configuration;
using TubePilot.Application.Interfaces;

namespace TubePilot.Infrastructure.Services
{
    public class FileSystemService : IFileSystemService
    {
        // Two folders always exist regardless of what the prompt's OutputSpec declares.
        private static readonly string[] AlwaysPresent = { "Input", "Export" };

        private readonly string _rootStoragePath;

        public FileSystemService(IConfiguration configuration)
        {
            _rootStoragePath = configuration["Storage:RootPath"] ?? Path.Combine(AppContext.BaseDirectory, "App_Data", "Projects");
        }

        public string CreateProjectStructure(Guid projectId, string projectTitle, OutputSpec spec)
        {
            var safeName = SanitizeFolderName(projectTitle);
            var basePath = Path.Combine(_rootStoragePath, $"{safeName}-{projectId:N}");

            foreach (var folder in AlwaysPresent)
            {
                Directory.CreateDirectory(Path.Combine(basePath, folder));
            }

            // Folders beyond Input/Export come entirely from what this specific prompt's spec declares.
            foreach (var item in spec.Items)
            {
                if (!string.IsNullOrWhiteSpace(item.FolderName))
                {
                    Directory.CreateDirectory(Path.Combine(basePath, item.FolderName));
                }
            }

            if (spec.Items.Any(i => i.RequiresImages))
            {
                Directory.CreateDirectory(Path.Combine(basePath, "Assets"));
            }

            return basePath;
        }

        public string WriteOutputItem(string projectPath, OutputItem item, object content)
        {
            var targetDir = Path.Combine(projectPath, item.FolderName);
            Directory.CreateDirectory(targetDir);

            switch (item.Type)
            {
                case OutputItemType.Table:
                    var csvPath = Path.Combine(targetDir, $"{item.Name}.csv");
                    WriteCsv(csvPath, (List<Dictionary<string, string>>)content);
                    return csvPath;

                case OutputItemType.Text:
                    var textPath = Path.Combine(targetDir, $"{item.Name}.txt");
                    File.WriteAllText(textPath, (string)content);
                    return textPath;

                case OutputItemType.ImageSet:
                default:
                    // Images are written by the review/select workflow (Sprint 6), not here.
                    return targetDir;
            }
        }

        public async Task<string> SaveImageAsync(string projectPath, string fileName, Stream imageStream, CancellationToken cancellationToken = default)
        {
            var assetsDir = Path.Combine(projectPath, "Assets");
            Directory.CreateDirectory(assetsDir);

            var targetPath = Path.Combine(assetsDir, fileName);
            await using var fileStream = File.Create(targetPath);
            await imageStream.CopyToAsync(fileStream, cancellationToken);

            return targetPath;
        }

        public async Task<string> SaveThumbnailAsync(string projectPath, byte[] imageBytes, CancellationToken cancellationToken = default)
        {
            var thumbnailDir = Path.Combine(projectPath, "Thumbnail");
            Directory.CreateDirectory(thumbnailDir);

            var targetPath = Path.Combine(thumbnailDir, "thumbnail.jpg");
            await File.WriteAllBytesAsync(targetPath, imageBytes, cancellationToken);

            return targetPath;
        }

        private static void WriteCsv(string path, List<Dictionary<string, string>> rows)
        {
            var columns = rows.Count > 0 ? rows[0].Keys.ToList() : new List<string>();
            using var writer = new StreamWriter(path, false);

            writer.WriteLine(string.Join(",", columns.Select(EscapeCsvField)));
            foreach (var row in rows)
            {
                writer.WriteLine(string.Join(",", columns.Select(c => EscapeCsvField(row.GetValueOrDefault(c, string.Empty)))));
            }
        }

        private static string EscapeCsvField(string field)
        {
            if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }
            return field;
        }

        private static string SanitizeFolderName(string title)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var cleaned = new string(title.Where(c => !invalid.Contains(c)).ToArray()).Trim();
            cleaned = cleaned.Replace(' ', '-');
            return string.IsNullOrWhiteSpace(cleaned) ? "Untitled" : cleaned;
        }
    }
}
