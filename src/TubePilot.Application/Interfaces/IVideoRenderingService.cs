using TubePilot.Domain.Entities;

namespace TubePilot.Application.Interfaces
{
    // A single scene's video source: either a real photo (Ken-Burns-animated, text already
    // burned in by SceneComposer) or a real stock-footage clip (trimmed to length, with a
    // transparent text-overlay PNG composited on top at render time).
    public record VideoScene(string FilePath, bool IsVideoClip, string? TextOverlayPngPath);

    public interface IVideoRenderingService
    {
        // True once a usable ffmpeg/ffprobe install has been located (auto-detected or configured).
        Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

        // Renders ordered scenes + narration audio (optionally mixed with a quieter, looping
        // background music track) into one MP4 at the given format/duration. Photo scenes get
        // a Ken Burns pan/zoom; video-clip scenes are trimmed to their share of the runtime
        // with their text overlay composited on top. Adjacent scenes crossfade regardless of
        // type. Returns the absolute path to the rendered file.
        Task<string> RenderAsync(
            List<VideoScene> scenes,
            byte[] narrationAudio,
            RenderFormat format,
            int durationSeconds,
            string outputDir,
            string? backgroundMusicPath = null,
            CancellationToken cancellationToken = default);
    }
}
