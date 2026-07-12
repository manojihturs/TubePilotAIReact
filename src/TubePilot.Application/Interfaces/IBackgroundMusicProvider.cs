namespace TubePilot.Application.Interfaces
{
    public interface IBackgroundMusicProvider
    {
        // Returns a local file path to a cached, royalty-free background music track
        // (downloading it on first use if needed). Picks a track by mood keyword when given,
        // otherwise a random track from the curated set.
        Task<string> GetTrackPathAsync(string? mood = null, CancellationToken cancellationToken = default);
    }
}
