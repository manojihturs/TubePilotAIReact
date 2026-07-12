using TubePilot.Domain.Entities;

namespace TubePilot.Application.Interfaces
{
    public interface IVideoRenderingService
    {
        // True once a usable ffmpeg/ffprobe install has been located (auto-detected or configured).
        Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

        // Renders ordered scene images + narration audio (optionally mixed with a quieter,
        // looping background music track) into one MP4 at the given format/duration.
        // Returns the absolute path to the rendered file.
        Task<string> RenderAsync(
            List<string> sceneImagePaths,
            byte[] narrationAudio,
            RenderFormat format,
            int durationSeconds,
            string outputDir,
            string? backgroundMusicPath = null,
            CancellationToken cancellationToken = default);
    }
}
