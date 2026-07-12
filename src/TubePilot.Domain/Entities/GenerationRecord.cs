namespace TubePilot.Domain.Entities
{
    // Cost-tracking foundation — capture token counts and provider/model per generation
    // from day one, even before usage analytics UI exists on top of it.
    public class GenerationRecord
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string OutputItemName { get; set; } = null!;
        public string ProviderUsed { get; set; } = null!;
        public string ModelUsed { get; set; } = null!;
        public int InputTokens { get; set; }
        public int OutputTokens { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
