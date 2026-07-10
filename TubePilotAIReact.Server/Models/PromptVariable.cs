using System.Text.Json.Serialization;

namespace TubePilotAIReact.Server.Models;

public class PromptVariable
{
    public int Id { get; set; }
    public int PromptId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DefaultValue { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    [JsonIgnore]
    public Prompt? Prompt { get; set; }
}
