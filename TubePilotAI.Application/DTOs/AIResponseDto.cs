using System.Collections.Generic;

namespace TubePilotAI.Application.DTOs
{
    public class AIResponseDto
    {
        public string Text { get; set; } = string.Empty;
        public List<string> Items { get; set; } = new List<string>();
        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}
