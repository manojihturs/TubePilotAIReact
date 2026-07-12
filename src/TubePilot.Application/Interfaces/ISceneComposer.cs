namespace TubePilot.Application.Interfaces
{
    public record SceneTextOverlay(int Rank, string Title, string? Badge);

    public interface ISceneComposer
    {
        // Takes a source image (a confirmed row image) and burns in real, crisp text
        // (rank number, title, optional badge) using a proper font for the given language.
        // Returns the path to the composed scene image (fixed output size for video muxing).
        Task<string> ComposeSceneAsync(
            string sourceImagePath,
            SceneTextOverlay overlay,
            string language,
            string outputPath,
            int width,
            int height,
            CancellationToken cancellationToken = default);
    }
}
