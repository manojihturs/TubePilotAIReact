namespace TubePilot.Application.Interfaces
{
    // Decorative display fonts used for the rank/year number and the subtitle line —
    // distinct from the body/language font used for the row title, since these are always
    // Latin numerals/short English-style labels regardless of narration language.
    public enum DecorativeFontRole { RankNumber, Subtitle }

    public interface IFontProvider
    {
        // Downloads (once, then caches on disk) and returns the local path to a bold,
        // readable font file that supports the given language ("en", "ta", ...).
        Task<string> GetFontPathAsync(string language, CancellationToken cancellationToken = default);

        // Downloads (once, then caches) a fixed decorative display font for the given role —
        // Anton for the big rank/year number, Bebas Neue for the subtitle.
        Task<string> GetDecorativeFontPathAsync(DecorativeFontRole role, CancellationToken cancellationToken = default);
    }
}
