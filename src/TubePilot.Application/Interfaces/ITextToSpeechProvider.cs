namespace TubePilot.Application.Interfaces
{
    public interface ITextToSpeechProvider
    {
        // Returns MP3 audio bytes for the given text in the given language (e.g. "en", "ta").
        Task<byte[]> SynthesizeAsync(string text, string language, CancellationToken cancellationToken = default);
    }
}
