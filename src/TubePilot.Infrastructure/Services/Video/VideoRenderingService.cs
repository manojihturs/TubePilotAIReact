using System.Diagnostics;
using System.Globalization;
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
        // Ken Burns + crossfade tuning. A short 0.6s transition keeps cuts snappy rather than
        // syrupy on a fast countdown video.
        private const double TransitionSeconds = 0.6;
        private const double MaxZoom = 1.15;
        // Slight zoom held constant during a pan so there's room to move without exposing
        // the source image's edge.
        private const double PanZoom = 1.12;
        private const int Fps = 30;

        // Cycled by scene index (not truly random) so renders stay reproducible/debuggable —
        // still gives real visual variety across a countdown's scenes. All are FFmpeg xfade
        // built-ins, no extra cost.
        private static readonly string[] Transitions = { "fade", "slideleft", "slideright", "wipeleft", "wiperight" };

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
            List<VideoScene> scenes,
            byte[] narrationAudio,
            RenderFormat format,
            int durationSeconds,
            string outputDir,
            string? backgroundMusicPath = null,
            CancellationToken cancellationToken = default)
        {
            if (scenes.Count == 0)
            {
                throw new InvalidOperationException("No confirmed scenes to render.");
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
            var perSceneDuration = Math.Max(1.5, cappedDuration / scenes.Count);

            var outputFileName = format == RenderFormat.Shorts ? "video-shorts.mp4" : "video-desktop.mp4";
            var outputPath = Path.Combine(outputDir, outputFileName);

            // The video stream is trimmed to the exact capped duration explicitly inside the
            // filter graph (a trailing `trim=duration=...` stage), not left to "-shortest" to
            // figure out. Relying on "-shortest" alone was proven unreliable here: with an
            // infinitely-looping (`-stream_loop -1`) background-music input feeding the audio
            // mix, "-shortest" let the video stream run to its full untrimmed length instead
            // of stopping at the shorter audio stream. An explicit trim removes the ambiguity.
            var cappedDurationText = cappedDuration.ToString(CultureInfo.InvariantCulture);
            var hasMusic = !string.IsNullOrWhiteSpace(backgroundMusicPath) && File.Exists(backgroundMusicPath);

            var (inputArgs, filterScriptPath, finalVideoLabel, nextInputIndex) = await BuildSceneChainAsync(
                scenes, perSceneDuration, cappedDurationText, width, height, outputDir, cancellationToken);

            var narrationInputIndex = nextInputIndex;
            var audioArgs = $"-t {cappedDurationText} -i \"{audioPath}\" ";
            string audioMapArg;
            if (hasMusic)
            {
                var musicInputIndex = narrationInputIndex + 1;
                audioArgs += $"-stream_loop -1 -i \"{backgroundMusicPath}\" ";
                // Music loops under the narration at low volume so the narration stays the
                // clear focal point; amix's "first" duration keeps the mix locked to the
                // (already-capped) narration track's length rather than the looped,
                // effectively infinite music track.
                await File.AppendAllTextAsync(filterScriptPath,
                    $"[{narrationInputIndex}:a]volume=1.0[a1];[{musicInputIndex}:a]volume=0.12[a2];" +
                    "[a1][a2]amix=inputs=2:duration=first:dropout_transition=2[aout]\n",
                    cancellationToken);
                audioMapArg = "-map \"[aout]\"";
            }
            else
            {
                audioMapArg = $"-map {narrationInputIndex}:a";
            }

            var args = $"{inputArgs}{audioArgs}" +
                       $"-filter_complex_script \"{filterScriptPath}\" " +
                       $"-map \"[{finalVideoLabel}]\" {audioMapArg} " +
                       $"-c:v libx264 -pix_fmt yuv420p -r {Fps} -c:a aac -shortest \"{outputPath}\"";

            var conversion = FFmpeg.Conversions.New().SetOverwriteOutput(true).AddParameter(args, ParameterPosition.PostInput);
            var result = await conversion.Start(cancellationToken);

            if (!File.Exists(outputPath))
            {
                throw new InvalidOperationException($"FFmpeg reported success but no output file was produced. Log: {result.Arguments}");
            }

            return outputPath;
        }

        // Builds one FFmpeg input + filter stage per scene, then chains them with xfade
        // crossfades. Photo scenes replay the Ken Burns (zoompan) treatment; video-clip
        // scenes are trimmed to their share of the runtime and get their pre-rendered
        // transparent text-overlay PNG composited on top via `overlay` (far more robust
        // than escaping dynamic, AI-generated text for FFmpeg's own drawtext filter).
        //
        // Written to a -filter_complex_script file rather than inlined into the process
        // argument string: with an uncapped row count a project can now have 20-30+
        // scenes, and a chain that long comfortably risks Windows' ~32K command-line
        // length limit if inlined.
        private static async Task<(string InputArgs, string FilterScriptPath, string FinalVideoLabel, int NextInputIndex)> BuildSceneChainAsync(
            List<VideoScene> scenes,
            double perSceneDuration,
            string cappedDurationText,
            int width,
            int height,
            string outputDir,
            CancellationToken cancellationToken)
        {
            var inv = CultureInfo.InvariantCulture;
            var singleScene = scenes.Count == 1;

            // Every scene clip is rendered slightly longer than its "visible" share so the
            // crossfade has real footage to blend into/out of — the overlap is consumed by
            // the transition rather than shortening how long each scene is actually on screen.
            var clipDuration = singleScene ? perSceneDuration : perSceneDuration + TransitionSeconds;
            var clipDurationText = clipDuration.ToString(inv);
            var frameCount = Math.Max(1, (int)Math.Round(clipDuration * Fps));
            var zoomIncrement = (MaxZoom - 1.0) / frameCount;

            // Scene video/photo sources are inputs 0..N-1; any clip's text-overlay PNG is
            // appended afterward so every index is known before the filter graph is written.
            var overlayInputIndex = new int?[scenes.Count];
            var nextInputIndex = scenes.Count;
            var overlayInputArgs = new StringBuilder();
            for (var i = 0; i < scenes.Count; i++)
            {
                if (scenes[i].IsVideoClip && !string.IsNullOrEmpty(scenes[i].TextOverlayPngPath))
                {
                    overlayInputIndex[i] = nextInputIndex++;
                    overlayInputArgs.Append($"-loop 1 -i \"{scenes[i].TextOverlayPngPath}\" ");
                }
            }

            var inputArgs = new StringBuilder();
            var filter = new StringBuilder();

            for (var i = 0; i < scenes.Count; i++)
            {
                var scene = scenes[i];
                if (scene.IsVideoClip)
                {
                    // Real footage may run shorter than its allotted scene time — loop the
                    // source indefinitely and let the trim below cut it to exactly the length
                    // needed, the same trick already used for looping background music.
                    inputArgs.Append($"-stream_loop -1 -i \"{scene.FilePath}\" ");
                    filter.Append(
                        $"[{i}:v]scale={width}:{height}:force_original_aspect_ratio=increase,crop={width}:{height}," +
                        $"trim=duration={clipDurationText},setpts=PTS-STARTPTS[base{i}];");

                    // fps={Fps} is required here, not cosmetic: xfade refuses to blend two
                    // inputs whose timebases don't match, and a real video clip's native
                    // encoded timebase (e.g. 1/15360) almost never matches the zoompan branch's
                    // explicit fps=30 output — confirmed by reproducing the exact
                    // "input link main timebase ... do not match" xfade failure without this.
                    if (overlayInputIndex[i] is int overlayIdx)
                    {
                        // shortest=1 caps the composited output to the (already trimmed) base
                        // clip's length rather than the indefinitely-looped overlay PNG input.
                        filter.Append($"[base{i}][{overlayIdx}:v]overlay=0:0:shortest=1,fps={Fps},format=yuv420p[v{i}];");
                    }
                    else
                    {
                        filter.Append($"[base{i}]fps={Fps},format=yuv420p[v{i}];");
                    }
                }
                else
                {
                    inputArgs.Append($"-loop 1 -t {clipDurationText} -i \"{scene.FilePath}\" ");
                    var zoompanExpr = BuildZoompanExpression(i % 4, frameCount, zoomIncrement, inv);
                    filter.Append(
                        $"[{i}:v]scale={width}:{height}:force_original_aspect_ratio=increase,crop={width}:{height}," +
                        $"zoompan={zoompanExpr}:d={frameCount}:s={width}x{height}:fps={Fps},format=yuv420p[v{i}];");
                }
            }

            inputArgs.Append(overlayInputArgs);

            string rawVideoLabel;
            if (singleScene)
            {
                rawVideoLabel = "v0";
            }
            else
            {
                var currentLabel = "v0";
                var running = clipDuration;
                for (var i = 1; i < scenes.Count; i++)
                {
                    var offset = running - TransitionSeconds;
                    var outLabel = i == scenes.Count - 1 ? "vraw" : $"x{i}";
                    var transitionName = Transitions[i % Transitions.Length];
                    filter.Append(
                        $"[{currentLabel}][v{i}]xfade=transition={transitionName}:duration={TransitionSeconds.ToString(inv)}:offset={offset.ToString("0.###", inv)}[{outLabel}];");
                    currentLabel = outLabel;
                    running = running + clipDuration - TransitionSeconds;
                }
                rawVideoLabel = currentLabel;
            }

            // Explicit trim to the exact capped duration — see the caller's comment on why
            // this can't be left to "-shortest" alone once a looping background-music input
            // is in the graph.
            const string finalVideoLabel = "vout";
            filter.Append($"[{rawVideoLabel}]trim=duration={cappedDurationText},setpts=PTS-STARTPTS[{finalVideoLabel}];");

            var filterScriptPath = Path.Combine(outputDir, "filter-complex.txt");
            await File.WriteAllTextAsync(filterScriptPath, filter.ToString(), cancellationToken);

            return (inputArgs.ToString(), filterScriptPath, finalVideoLabel, nextInputIndex);
        }

        // Cycled by scene index so a countdown's photo scenes don't all move the same way —
        // zoom-in (existing Ken Burns), plus zoom-out, pan-left, and pan-right as real
        // alternates, all still just the native zoompan filter (no extra cost).
        // `on` is zoompan's current *output* frame number (0-based); `zoom` is the previous
        // frame's zoom level, which is how the filter tracks incremental zoom across frames.
        private static string BuildZoompanExpression(int variant, int frameCount, double zoomIncrement, CultureInfo inv)
        {
            var maxZoomText = MaxZoom.ToString(inv);
            var panZoomText = PanZoom.ToString(inv);
            var lastFrame = Math.Max(1, frameCount - 1);

            return variant switch
            {
                // Zoom out: start at MaxZoom on the first output frame, then decrease each frame.
                1 => $"z='if(eq(on,0),{maxZoomText},max(zoom-{zoomIncrement.ToString("0.000000", inv)},1.0))':" +
                     "x='iw/2-(iw/zoom/2)':y='ih/2-(ih/zoom/2)'",
                // Pan left: fixed slight zoom, x sweeps from the image's right edge to its left.
                2 => $"z='{panZoomText}':" +
                     $"x='(iw-iw/zoom)*(1-on/{lastFrame})':y='ih/2-(ih/zoom/2)'",
                // Pan right: fixed slight zoom, x sweeps from the image's left edge to its right.
                3 => $"z='{panZoomText}':" +
                     $"x='(iw-iw/zoom)*(on/{lastFrame})':y='ih/2-(ih/zoom/2)'",
                // Zoom in (default/original): steady zoom increase, centered.
                _ => $"z='min(zoom+{zoomIncrement.ToString("0.000000", inv)},{maxZoomText})':" +
                     "x='iw/2-(iw/zoom/2)':y='ih/2-(ih/zoom/2)'",
            };
        }
    }
}
