using System.Text.Json.Serialization;

namespace TubePilotAIReact.Server.Models;

public class PromptCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    [JsonIgnore]
    public ICollection<Prompt> Prompts { get; set; } = new List<Prompt>();
}
