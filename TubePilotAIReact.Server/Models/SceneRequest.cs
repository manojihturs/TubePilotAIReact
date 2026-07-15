namespace TubePilotAIReact.Server.Models;

public class SceneRequest
{
    public required string Id { get; set; }
    public required string NarrationText { get; set; }
    public required string VisualPrompt { get; set; }
    public double DurationSeconds { get; set; }
}
