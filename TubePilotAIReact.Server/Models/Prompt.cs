using System.Text.Json.Serialization;

namespace TubePilotAIReact.Server.Models;

public class Prompt
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    [JsonIgnore]
    public PromptCategory? Category { get; set; }

    [JsonIgnore]
    public ICollection<PromptVariable> Variables { get; set; } = new List<PromptVariable>();
}
