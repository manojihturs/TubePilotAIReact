namespace TubePilot.Application.Interfaces
{
    // Separate from IImageSearchProvider (which finds existing real-world photos per row) —
    // this generates one new AI image from a text prompt, used for the single video thumbnail.
    public interface IImageGenerationProvider
    {
        string ProviderName { get; }
        Task<byte[]> GenerateAsync(string prompt, CancellationToken cancellationToken = default);
    }
}
