namespace TubePilot.Application.Interfaces
{
    public record VideoClipCandidate(
        string PreviewImageUrl,
        string DownloadUrl,
        string SourceUrl,
        string License,
        double DurationSeconds);

    // Real stock-footage video search (Pexels, Pixabay) — genuine filmed motion clips, not
    // AI-generated video. Unlike IImageSearchProvider (currently keyless via Wikimedia),
    // these require a free user-supplied API key, so it's passed per call rather than baked
    // into the provider at construction time.
    public interface IVideoClipSearchProvider
    {
        string ProviderName { get; }
        Task<List<VideoClipCandidate>> SearchAsync(string query, string apiKey, int maxResults, CancellationToken cancellationToken = default);
    }

    public interface IVideoClipProviderFactory
    {
        IVideoClipSearchProvider GetProvider(string providerName);
    }
}
