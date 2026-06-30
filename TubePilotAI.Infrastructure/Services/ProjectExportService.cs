using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using TubePilotAI.Infrastructure.Persistence.Context;

namespace TubePilotAI.Infrastructure.Services;

public sealed class ProjectExportService(TubePilotDbContext dbContext, IConfiguration configuration)
{
    private readonly string _exportRoot = configuration.GetValue<string>("ExportRoot") ?? "Exports";

    public async Task<string> ExportProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var project = await dbContext.Projects
            .Include(p => p.GeneratedContents)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken) 
            ?? throw new KeyNotFoundException("Project not found.");

        var projectFolder = Path.Combine(_exportRoot, "Projects", project.Name);
        
        Directory.CreateDirectory(Path.Combine(projectFolder, "Research"));
        Directory.CreateDirectory(Path.Combine(projectFolder, "Data"));
        Directory.CreateDirectory(Path.Combine(projectFolder, "Script"));
        Directory.CreateDirectory(Path.Combine(projectFolder, "Metadata"));

        // Example: Save content to files
        foreach (var content in project.GeneratedContents)
        {
            await File.WriteAllTextAsync(Path.Combine(projectFolder, "Script", $"{content.Title}.txt"), content.NarrationScript, cancellationToken);
            await File.WriteAllTextAsync(Path.Combine(projectFolder, "Metadata", $"{content.Title}.json"), content.Title, cancellationToken);
        }

        return projectFolder;
    }
}
