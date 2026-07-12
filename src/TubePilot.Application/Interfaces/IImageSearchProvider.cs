namespace TubePilot.Application.Interfaces
{
    public record ImageCandidate(string ThumbnailUrl, string FullSizeUrl, string SourceUrl, string License);

    public interface IImageSearchProvider
    {
        string ProviderName { get; }
        Task<List<ImageCandidate>> SearchAsync(string query, int maxResults, CancellationToken cancellationToken = default);
    }
}
