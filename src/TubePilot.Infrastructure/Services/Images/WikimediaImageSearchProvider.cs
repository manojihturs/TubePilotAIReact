using System.Text.Json;
using System.Text.Json.Serialization;
using TubePilot.Application.Interfaces;

namespace TubePilot.Infrastructure.Services.Images
{
    // Wikimedia Commons — free, safe licensing, good coverage for well-known public figures,
    // movies, historical topics. No API key required. Used as the first (and for now, only)
    // image source, per the doc's own recommendation over direct Google Images scraping.
    public class WikimediaImageSearchProvider : IImageSearchProvider
    {
        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly HttpClient _http;

        public string ProviderName => "Wikimedia";

        public WikimediaImageSearchProvider(HttpClient http)
        {
            _http = http;
            if (_http.BaseAddress == null)
            {
                _http.BaseAddress = new Uri("https://commons.wikimedia.org/");
            }
            // Wikimedia's API rejects requests with no User-Agent (returns 403) — .NET's
            // HttpClient sends none by default, unlike curl. Required, not optional.
            if (_http.DefaultRequestHeaders.UserAgent.Count == 0)
            {
                _http.DefaultRequestHeaders.UserAgent.ParseAdd("TubePilotAI/1.0 (https://github.com/tubepilotai; contact@tubepilotai.local)");
            }
        }

        public async Task<List<ImageCandidate>> SearchAsync(string query, int maxResults, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<ImageCandidate>();

            var encodedQuery = Uri.EscapeDataString(query);
            var url = $"w/api.php?action=query&generator=search&gsrnamespace=6&gsrsearch={encodedQuery}" +
                      $"&gsrlimit={Math.Clamp(maxResults, 1, 20)}&prop=imageinfo&iiprop=url|extmetadata&iiurlwidth=400&format=json";

            using var response = await _http.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode) return new List<ImageCandidate>();

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var parsed = JsonSerializer.Deserialize<WikimediaResponse>(body, JsonOptions);
            var pages = parsed?.Query?.Pages?.Values;
            if (pages == null) return new List<ImageCandidate>();

            var candidates = new List<ImageCandidate>();
            foreach (var page in pages)
            {
                var info = page.ImageInfo?.FirstOrDefault();
                if (info == null || string.IsNullOrEmpty(info.Url)) continue;
                if (!ImageExtensions.Any(ext => info.Url.EndsWith(ext, StringComparison.OrdinalIgnoreCase))) continue;

                candidates.Add(new ImageCandidate(
                    ThumbnailUrl: info.ThumbUrl ?? info.Url,
                    FullSizeUrl: info.Url,
                    SourceUrl: info.DescriptionUrl ?? info.Url,
                    License: info.ExtMetadata?.LicenseShortName?.Value ?? "Unknown"
                ));
            }

            return candidates;
        }

        private class WikimediaResponse
        {
            [JsonPropertyName("query")] public WikimediaQuery? Query { get; set; }
        }

        private class WikimediaQuery
        {
            [JsonPropertyName("pages")] public Dictionary<string, WikimediaPage>? Pages { get; set; }
        }

        private class WikimediaPage
        {
            [JsonPropertyName("imageinfo")] public List<WikimediaImageInfo>? ImageInfo { get; set; }
        }

        private class WikimediaImageInfo
        {
            [JsonPropertyName("url")] public string? Url { get; set; }
            [JsonPropertyName("thumburl")] public string? ThumbUrl { get; set; }
            [JsonPropertyName("descriptionurl")] public string? DescriptionUrl { get; set; }
            [JsonPropertyName("extmetadata")] public WikimediaExtMetadata? ExtMetadata { get; set; }
        }

        private class WikimediaExtMetadata
        {
            [JsonPropertyName("LicenseShortName")] public WikimediaMetaValue? LicenseShortName { get; set; }
        }

        private class WikimediaMetaValue
        {
            [JsonPropertyName("value")] public string? Value { get; set; }
        }
    }
}
