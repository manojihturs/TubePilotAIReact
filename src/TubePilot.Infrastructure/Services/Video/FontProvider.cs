using Microsoft.Extensions.Configuration;
using TubePilot.Application.Interfaces;

namespace TubePilot.Infrastructure.Services.Video
{
    // Downloads free, open-licensed (OFL) Google Fonts on first use and caches them under
    // {StorageRoot}/Fonts/ — avoids bundling large binary font files in source control while
    // still guaranteeing correct, legible text in any supported language (no AI-drawn text).
    public class FontProvider : IFontProvider
    {
        private static readonly Dictionary<string, string> FontUrls = new()
        {
            ["ta"] = "https://raw.githubusercontent.com/google/fonts/main/ofl/notosanstamil/NotoSansTamil%5Bwdth%2Cwght%5D.ttf",
            ["en"] = "https://raw.githubusercontent.com/google/fonts/main/ofl/notosans/NotoSans%5Bwdth%2Cwght%5D.ttf",
        };

        private static readonly Dictionary<DecorativeFontRole, (string FileName, string Url)> DecorativeFontUrls = new()
        {
            [DecorativeFontRole.RankNumber] = ("anton.ttf", "https://raw.githubusercontent.com/google/fonts/main/ofl/anton/Anton-Regular.ttf"),
            [DecorativeFontRole.Subtitle] = ("bebasneue.ttf", "https://raw.githubusercontent.com/google/fonts/main/ofl/bebasneue/BebasNeue-Regular.ttf"),
        };

        private readonly HttpClient _http;
        private readonly string _fontsDir;
        private static readonly SemaphoreSlim DownloadLock = new(1, 1);

        public FontProvider(HttpClient http, IConfiguration configuration)
        {
            _http = http;
            var storageRoot = configuration["Storage:RootPath"] ?? Path.Combine(AppContext.BaseDirectory, "App_Data", "Projects");
            _fontsDir = Path.Combine(Path.GetDirectoryName(storageRoot.TrimEnd(Path.DirectorySeparatorChar))!, "Fonts");
        }

        public Task<string> GetFontPathAsync(string language, CancellationToken cancellationToken = default)
        {
            var url = FontUrls.GetValueOrDefault(language, FontUrls["en"]);
            return DownloadAndCacheAsync($"{language}.ttf", url, cancellationToken);
        }

        public Task<string> GetDecorativeFontPathAsync(DecorativeFontRole role, CancellationToken cancellationToken = default)
        {
            var (fileName, url) = DecorativeFontUrls[role];
            return DownloadAndCacheAsync(fileName, url, cancellationToken);
        }

        private async Task<string> DownloadAndCacheAsync(string fileName, string url, CancellationToken cancellationToken)
        {
            var path = Path.Combine(_fontsDir, fileName);
            if (File.Exists(path)) return path;

            await DownloadLock.WaitAsync(cancellationToken);
            try
            {
                if (File.Exists(path)) return path;

                Directory.CreateDirectory(_fontsDir);
                var bytes = await _http.GetByteArrayAsync(url, cancellationToken);
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
