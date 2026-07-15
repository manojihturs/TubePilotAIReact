namespace TubePilotAIReact.Server.Models;

public class GenerateScenesRequest
{
    public required string ProjectFolderName { get; set; }
    public required List<SceneRequest> Scenes { get; set; }
}
