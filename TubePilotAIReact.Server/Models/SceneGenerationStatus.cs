namespace TubePilotAIReact.Server.Models;

public enum SceneGenerationState
{
    Pending,
    Running,
    Succeeded,
    Failed
}

public class SceneGenerationStatus
{
    public required string SceneId { get; set; }
    public SceneGenerationState State { get; set; } = SceneGenerationState.Pending;
    public string? ClipPath { get; set; }
    public string? Error { get; set; }
}

public class ProjectGenerationStatus
{
    public required string ProjectFolderName { get; set; }
    public bool IsComplete { get; set; }
    public required List<SceneGenerationStatus> Scenes { get; set; }
}
