using Microsoft.Extensions.Configuration;
using TubePilot.Application.Interfaces;

namespace TubePilot.Infrastructure.Services.Video
{
    // Curated, keyless royalty-free instrumental tracks hosted on Wikimedia Commons
    // (LEMMiNO, CC BY-SA 4.0 — free to use with attribution, unlike the AI providers this
    // app otherwise relies on, no account/API key needed to fetch). Downloaded once and
    // cached under {StorageRoot's parent}/Music/, same pattern as FontProvider.
    public class BackgroundMusicProvider : IBackgroundMusicProvider
    {
        private static readonly (string FileName, string Url)[] Tracks =
        {
            ("biosignature.flac", "https://upload.wikimedia.org/wikipedia/commons/2/27/LEMMiNO_-_Biosignature.flac"),
            ("siberian.flac", "https://upload.wikimedia.org/wikipedia/commons/2/2e/LEMMiNO_-_Siberian.flac"),
            ("encounters.flac", "https://upload.wikimedia.org/wikipedia/commons/3/31/LEMMiNO_-_Encounters.flac"),
            ("nocturnal.flac", "https://upload.wikimedia.org/wikipedia/commons/d/d2/LEMMiNO_-_Nocturnal.flac"),
            ("blackout.flac", "https://upload.wikimedia.org/wikipedia/commons/f/fe/LEMMiNO_-_Blackout.flac"),
        };

        private readonly HttpClient _http;
        private readonly string _musicDir;
        private static readonly SemaphoreSlim DownloadLock = new(1, 1);
        private static readonly Random Rng = new();

        public BackgroundMusicProvider(HttpClient http, IConfiguration configuration)
        {
            _http = http;
            var storageRoot = configuration["Storage:RootPath"] ?? Path.Combine(AppContext.BaseDirectory, "App_Data", "Projects");
            _musicDir = Path.Combine(Path.GetDirectoryName(storageRoot.TrimEnd(Path.DirectorySeparatorChar))!, "Music");

            // Wikimedia rejects requests with no User-Agent (403) — .NET's HttpClient sends
            // none by default, unlike curl. Same requirement as WikimediaImageSearchProvider.
            if (_http.DefaultRequestHeaders.UserAgent.Count == 0)
            {
                _http.DefaultRequestHeaders.UserAgent.ParseAdd("TubePilotAI/1.0 (https://github.com/tubepilotai; contact@tubepilotai.local)");
            }
        }

        public async Task<string> GetTrackPathAsync(string? mood = null, CancellationToken cancellationToken = default)
        {
            var track = Tracks[Rng.Next(Tracks.Length)];
            var path = Path.Combine(_musicDir, track.FileName);

            if (File.Exists(path)) return path;

            await DownloadLock.WaitAsync(cancellationToken);
            try
            {
                if (File.Exists(path)) return path;

                Directory.CreateDirectory(_musicDir);
                var bytes = await _http.GetByteArrayAsync(track.Url, cancellationToken);
                await File.WriteAllBytesAsync(path, bytes, cancellationToken);
                return path;
            }
            finally
            {
                DownloadLock.Release();
            }
        }
    }
}
