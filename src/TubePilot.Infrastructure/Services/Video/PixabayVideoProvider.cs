using System.Text.Json;
using System.Text.Json.Serialization;
using TubePilot.Application.Interfaces;

namespace TubePilot.Infrastructure.Services.Video
{
    // Pixabay Videos — free API key (no credit card, generous free quota), real filmed
    // stock footage. https://pixabay.com/api/docs/#api_search_videos
    public class PixabayVideoProvider : IVideoClipSearchProvider
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private readonly HttpClient _http;

        public string ProviderName => "Pixabay";

        public PixabayVideoProvider(HttpClient http)
        {
            _http = http;
            if (_http.BaseAddress == null)
            {
                _http.BaseAddress = new Uri("https://pixabay.com/");
            }
        }

        public async Task<List<VideoClipCandidate>> SearchAsync(string query, string apiKey, int maxResults, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<VideoClipCandidate>();

            var encoded = Uri.EscapeDataString(query);
            var encodedKey = Uri.EscapeDataString(apiKey);
            using var response = await _http.GetAsync(
                $"api/videos/?key={encodedKey}&q={encoded}&per_page={Math.Clamp(maxResults, 3, 15)}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest ||
                    response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    if (errorBody.Contains("key", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException("Pixabay API key is invalid.");
                    }
                }
                return new List<VideoClipCandidate>();
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var parsed = JsonSerializer.Deserialize<PixabaySearchResponse>(body, JsonOptions);
            if (parsed?.Hits == null) return new List<VideoClipCandidate>();

            var candidates = new List<VideoClipCandidate>();
            foreach (var hit in parsed.Hits)
            {
                var file = hit.Videos?.Medium ?? hit.Videos?.Small ?? hit.Videos?.Large;
                if (string.IsNullOrEmpty(file?.Url)) continue;

                candidates.Add(new VideoClipCandidate(
                    PreviewImageUrl: hit.PictureId != null ? $"https://i.vimeocdn.com/video/{hit.PictureId}_640x360.jpg" : file.Url,
                    DownloadUrl: file.Url,
                    SourceUrl: hit.PageUrl ?? file.Url,
                    License: "Pixabay License (free to use)",
                    DurationSeconds: hit.Duration ?? 0));
            }
            return candidates;
        }

        private class PixabaySearchResponse
        {
            [JsonPropertyName("hits")] public List<PixabayHit>? Hits { get; set; }
        }
        private class PixabayHit
        {
            [JsonPropertyName("duration")] public double? Duration { get; set; }
            [JsonPropertyName("pageURL")] public string? PageUrl { get; set; }
            [JsonPropertyName("picture_id")] public string? PictureId { get; set; }
            [JsonPropertyName("videos")] public PixabayVideoSet? Videos { get; set; }
        }
        private class PixabayVideoSet
        {
            [JsonPropertyName("large")] public PixabayVideoFile? Large { get; set; }
            [JsonPropertyName("medium")] public PixabayVideoFile? Medium { get; set; }
            [JsonPropertyName("small")] public PixabayVideoFile? Small { get; set; }
        }
        private class PixabayVideoFile
        {
            [JsonPropertyName("url")] public string? Url { get; set; }
        }
    }
}
