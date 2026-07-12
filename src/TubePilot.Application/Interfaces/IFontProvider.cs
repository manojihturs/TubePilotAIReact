namespace TubePilot.Application.Interfaces
{
    public interface IFontProvider
    {
        // Downloads (once, then caches on disk) and returns the local path to a bold,
        // readable font file that supports the given language ("en", "ta", ...).
        Task<string> GetFontPathAsync(string language, CancellationToken cancellationToken = default);
    }
}
