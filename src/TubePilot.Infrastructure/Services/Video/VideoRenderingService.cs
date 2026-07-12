using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Configuration;
using TubePilot.Application.Interfaces;
using TubePilot.Domain.Entities;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace TubePilot.Infrastructure.Services.Video
{
    // Wraps FFmpeg (via Xabe.FFmpeg) to turn confirmed row scenes + narration audio into a
    // real MP4. If no system-wide ffmpeg is found on PATH, transparently downloads a static
    // build into app storage on first use via Xabe.FFmpeg.Downloader — no OS-level install
    // required, which matters once this becomes a SAAS deployment target.
    // Background job queue (Sprint 9-11 per the original plan) is intentionally not Hangfire
    // here — a lightweight in-process Task.Run + RenderJobs status table covers the same
    // "genuinely async, don't block the request" requirement without adding infra weight
    // this early; swap in Hangfire later if render volume needs a durable queue.
    public class VideoRenderingService : IVideoRenderingService
    {
        private static readonly SemaphoreSlim DownloadLock = new(1, 1);
        private static bool _downloadAttempted;
        private readonly string _localFFmpegDir;

        public VideoRenderingService(IConfiguration configuration)
        {
            var configuredPath = configuration["FFmpeg:ExecutablesPath"];
            if (!string.IsNullOrWhiteSpace(configuredPath))
            {
                FFmpeg.SetExecutablesPath(configuredPath);
            }

            var storageRoot = configuration["Storage:RootPath"] ?? Path.Combine(AppContext.BaseDirectory, "App_Data", "Projects");
            _localFFmpegDir = Path.Combine(Path.GetDirectoryName(storageRoot.TrimEnd(Path.DirectorySeparatorChar))!, "FFmpeg");
        }

        public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        {
            if (await ProbeAsync(null, cancellationToken)) return true;

            await EnsureDownloadedAsync(cancellationToken);
            return await ProbeAsync(_localFFmpegDir, cancellationToken);
        }

        private static async Task<bool> ProbeAsync(string? executablesDir, CancellationToken cancellationToken)
        {
            try
            {
                var exeName = OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg";
                var fileName = executablesDir != null ? Path.Combine(executablesDir, exeName) : "ffmpeg";
                if (executablesDir != null && !File.Exists(fileName)) return false;

                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = fileName,
                        Arguments = "-version",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                await process.WaitForExitAsync(cancellationToken);
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        private async Task EnsureDownloadedAsync(CancellationToken cancellationToken)
        {
            if (_downloadAttempted) return;

            await DownloadLock.WaitAsync(cancellationToken);
            try
            {
                if (_downloadAttempted) return;

                Directory.CreateDirectory(_localFFmpegDir);
                FFmpeg.SetExecutablesPath(_localFFmpegDir);

                var exeName = OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg";
                if (!File.Exists(Path.Combine(_localFFmpegDir, exeName)))
                {
                    await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, _localFFmpegDir);
                }
            }
            catch
            {
                // Leave IsAvailableAsync's follow-up probe to report unavailability.
            }
            finally
            {
                _downloadAttempted = true;
                DownloadLock.Release();
            }
        }

        public async Task<string> RenderAsync(
            List<string> sceneImagePaths,
            byte[] narrationAudio,
            RenderFormat format,
            int durationSeconds,
            string outputDir,
            string? backgroundMusicPath = null,
            CancellationToken cancellationToken = default)
        {
            if (sceneImagePaths.Count == 0)
            {
                throw new InvalidOperationException("No confirmed scene images to render.");
            }

            Directory.CreateDirectory(outputDir);

            var (width, height) = format == RenderFormat.Shorts ? (1080, 1920) : (1920, 1080);

            var audioPath = Path.Combine(outputDir, "narration.mp3");
            await File.WriteAllBytesAsync(audioPath, narrationAudio, cancellationToken);

            // The final video is trimmed to the real narration length rather than the
            // requested duration — narration is what the scenes are timed against, so a
            // shorter-than-requested narration should shorten the video, not leave trailing
            // scenes with music but no voice. Shorts/Reels still obey the platform's hard
            // 60s ceiling regardless of narration length.
            var narrationInfo = await FFmpeg.GetMediaInfo(audioPath, cancellationToken);
            var narrationSeconds = narrationInfo.Duration.TotalSeconds;
            var cappedDuration = format == RenderFormat.Shorts
                ? Math.Min(narrationSeconds, 60)
                : narrationSeconds;
            var perSceneDuration = Math.Max(1.5, cappedDuration / sceneImagePaths.Count);

            var concatListPath = Path.Combine(outputDir, $"concat-{format}.txt");
            var concatContent = new StringBuilder();
            foreach (var scenePath in sceneImagePaths)
            {
                var escaped = scenePath.Replace("'", "'\\''");
                concatContent.AppendLine($"file '{escaped}'");
                concatContent.AppendLine($"duration {perSceneDuration.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
            }
            // The concat demuxer requires the last file repeated once more without a duration line.
            var lastEscaped = sceneImagePaths[^1].Replace("'", "'\\''");
            concatContent.AppendLine($"file '{lastEscaped}'");
            await File.WriteAllTextAsync(concatListPath, concatContent.ToString(), cancellationToken);

            var outputFileName = format == RenderFormat.Shorts ? "video-shorts.mp4" : "video-desktop.mp4";
            var outputPath = Path.Combine(outputDir, outputFileName);

            // The cap is applied as an INPUT option on the narration audio, not as a global
            // output "-t". A global output "-t" combined with the concat demuxer + video
            // filter chain truncates the video stream far short of the requested duration
            // (reproduced directly: -shortest alone correctly yields the full duration, but
            // adding a global "-t" cuts video to ~3s regardless of the cap value). Trimming
            // the audio input instead and letting "-shortest" cap the output to it avoids
            // the bug entirely while still producing the correct capped length.
            var cappedDurationText = cappedDuration.ToString(System.Globalization.CultureInfo.InvariantCulture);

            string args;
            if (!string.IsNullOrWhiteSpace(backgroundMusicPath) && File.Exists(backgroundMusicPath))
            {
                // Music loops under the narration at low volume so the narration stays the
                // clear focal point; amix's "first" duration keeps the mix locked to the
                // (now correctly capped) narration track's length rather than the looped,
                // effectively infinite music track.
                args = $"-f concat -safe 0 -i \"{concatListPath}\" -t {cappedDurationText} -i \"{audioPath}\" -stream_loop -1 -i \"{backgroundMusicPath}\" " +
                       $"-filter_complex \"[0:v]scale={width}:{height}:force_original_aspect_ratio=increase,crop={width}:{height}[v];" +
                       $"[1:a]volume=1.0[a1];[2:a]volume=0.12[a2];[a1][a2]amix=inputs=2:duration=first:dropout_transition=2[aout]\" " +
                       $"-map \"[v]\" -map \"[aout]\" " +
                       $"-c:v libx264 -pix_fmt yuv420p -r 30 -c:a aac -shortest \"{outputPath}\"";
            }
            else
            {
                args = $"-f concat -safe 0 -i \"{concatListPath}\" -t {cappedDurationText} -i \"{audioPath}\" " +
                       $"-vf \"scale={width}:{height}:force_original_aspect_ratio=increase,crop={width}:{height}\" " +
                       $"-c:v libx264 -pix_fmt yuv420p -r 30 -c:a aac -shortest \"{outputPath}\"";
            }

            var conversion = FFmpeg.Conversions.New().SetOverwriteOutput(true).AddParameter(args, ParameterPosition.PostInput);
            var result = await conversion.Start(cancellationToken);

            if (!File.Exists(outputPath))
            {
                throw new InvalidOperationException($"FFmpeg reported success but no output file was produced. Log: {result.Arguments}");
            }

            return outputPath;
        }
    }
}
