using System.Text.Json;
using System.Text.Json.Serialization;
using TubePilot.Application.Interfaces;

namespace TubePilot.Infrastructure.Services.Images
{
    // Real-photo image search for named things (cars, brands, people, products, movies).
    // First asks Wikipedia for the lead image of the best-matching article — that's the
    // actual, on-topic photo of the subject, not an AI rendering. Falls back to a Wikimedia
    // Commons file search when the topic has no Wikipedia article with a lead image.
    // Keyless and free; no image is ever synthesized here.
    public class WikipediaImageSearchProvider : IImageSearchProvider
    {
        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly HttpClient _http;

        public string ProviderName => "Wikipedia";

        public WikipediaImageSearchProvider(HttpClient http)
        {
            _http = http;
            // Both Wikipedia and Commons reject requests with no User-Agent (403). Required.
            if (_http.DefaultRequestHeaders.UserAgent.Count == 0)
            {
                _http.DefaultRequestHeaders.UserAgent.ParseAdd("TubePilotAI/1.0 (https://github.com/tubepilotai; contact@tubepilotai.local)");
            }
        }

        public async Task<List<ImageCandidate>> SearchAsync(string query, int maxResults, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<ImageCandidate>();

            var fromWikipedia = await SearchWikipediaAsync(query, maxResults, cancellationToken);
            if (fromWikipedia.Count > 0) return fromWikipedia;

            return await SearchCommonsAsync(query, maxResults, cancellationToken);
        }

        // Article lead images via the MediaWiki pageimages API: search articles by the query,
        // then return each top article's own lead photo (piprop=original) — the most on-topic
        // real image available for a named subject.
        private async Task<List<ImageCandidate>> SearchWikipediaAsync(string query, int maxResults, CancellationToken cancellationToken)
        {
            var encoded = Uri.EscapeDataString(query);
            var url = "https://en.wikipedia.org/w/api.php?action=query&format=json&generator=search" +
                      $"&gsrsearch={encoded}&gsrnamespace=0&gsrlimit={Math.Clamp(maxResults, 1, 10)}" +
                      "&prop=pageimages&piprop=original|thumbnail&pithumbsize=400";

            try
            {
                using var response = await _http.GetAsync(url, cancellationToken);
                if (!response.IsSuccessStatusCode) return new List<ImageCandidate>();

                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                var parsed = JsonSerializer.Deserialize<WikiPageImagesResponse>(body, JsonOptions);
                var pages = parsed?.Query?.Pages?.Values;
                if (pages == null) return new List<ImageCandidate>();

                var candidates = new List<ImageCandidate>();
                foreach (var page in pages.OrderBy(p => p.Index))
                {
                    var full = page.Original?.Source;
                    if (string.IsNullOrEmpty(full)) continue;
                    if (!ImageExtensions.Any(ext => full.EndsWith(ext, StringComparison.OrdinalIgnoreCase))) continue;

                    candidates.Add(new ImageCandidate(
                        ThumbnailUrl: page.Thumbnail?.Source ?? full,
                        FullSizeUrl: full,
                        SourceUrl: $"https://en.wikipedia.org/?curid={page.PageId}",
                        License: "Wikipedia / Wikimedia"
                    ));
                }
                return candidates;
            }
            catch (HttpRequestException)
            {
                return new List<ImageCandidate>();
            }
        }

        // Fallback: raw Wikimedia Commons file search (same query the old provider used).
        private async Task<List<ImageCandidate>> SearchCommonsAsync(string query, int maxResults, CancellationToken cancellationToken)
        {
            var encoded = Uri.EscapeDataString(query);
            var url = "https://commons.wikimedia.org/w/api.php?action=query&generator=search&gsrnamespace=6" +
                      $"&gsrsearch={encoded}&gsrlimit={Math.Clamp(maxResults, 1, 20)}" +
                      "&prop=imageinfo&iiprop=url|extmetadata&iiurlwidth=400&format=json";

            try
            {
                using var response = await _http.GetAsync(url, cancellationToken);
                if (!response.IsSuccessStatusCode) return new List<ImageCandidate>();

                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                var parsed = JsonSerializer.Deserialize<CommonsResponse>(body, JsonOptions);
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
            catch (HttpRequestException)
            {
                return new List<ImageCandidate>();
            }
        }

        private class WikiPageImagesResponse
        {
            [JsonPropertyName("query")] public WikiQuery? Query { get; set; }
        }
        private class WikiQuery
        {
            [JsonPropertyName("pages")] public Dictionary<string, WikiPage>? Pages { get; set; }
        }
        private class WikiPage
        {
            [JsonPropertyName("pageid")] public long PageId { get; set; }
            [JsonPropertyName("index")] public int Index { get; set; }
            [JsonPropertyName("original")] public WikiImage? Original { get; set; }
            [JsonPropertyName("thumbnail")] public WikiImage? Thumbnail { get; set; }
        }
        private class WikiImage
        {
            [JsonPropertyName("source")] public string? Source { get; set; }
        }

        private class CommonsResponse
        {
            [JsonPropertyName("query")] public CommonsQuery? Query { get; set; }
        }
        private class CommonsQuery
        {
            [JsonPropertyName("pages")] public Dictionary<string, CommonsPage>? Pages { get; set; }
        }
        private class CommonsPage
        {
            [JsonPropertyName("imageinfo")] public List<CommonsImageInfo>? ImageInfo { get; set; }
        }
        private class CommonsImageInfo
        {
            [JsonPropertyName("url")] public string? Url { get; set; }
            [JsonPropertyName("thumburl")] public string? ThumbUrl { get; set; }
            [JsonPropertyName("descriptionurl")] public string? DescriptionUrl { get; set; }
            [JsonPropertyName("extmetadata")] public CommonsExtMetadata? ExtMetadata { get; set; }
        }
        private class CommonsExtMetadata
        {
            [JsonPropertyName("LicenseShortName")] public CommonsMetaValue? LicenseShortName { get; set; }
        }
        private class CommonsMetaValue
        {
            [JsonPropertyName("value")] public string? Value { get; set; }
        }
    }
}
