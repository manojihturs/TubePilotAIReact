using System.Text.Json;
using System.Text.Json.Serialization;
using TubePilot.Application.Interfaces;

namespace TubePilot.Infrastructure.Services.Video
{
    // Pexels Videos — free API key (no credit card, generous free quota), real filmed
    // stock footage. https://www.pexels.com/api/documentation/#videos-search
    public class PexelsVideoProvider : IVideoClipSearchProvider
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private readonly HttpClient _http;

        public string ProviderName => "Pexels";

        public PexelsVideoProvider(HttpClient http)
        {
            _http = http;
            if (_http.BaseAddress == null)
            {
                _http.BaseAddress = new Uri("https://api.pexels.com/");
            }
        }

        public async Task<List<VideoClipCandidate>> SearchAsync(string query, string apiKey, int maxResults, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<VideoClipCandidate>();

            var encoded = Uri.EscapeDataString(query);
            using var request = new HttpRequestMessage(HttpMethod.Get,
                $"videos/search?query={encoded}&per_page={Math.Clamp(maxResults, 1, 15)}&orientation=landscape");
            request.Headers.Add("Authorization", apiKey);

            using var response = await _http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new InvalidOperationException("Pexels API key is invalid.");
                }
                return new List<VideoClipCandidate>();
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var parsed = JsonSerializer.Deserialize<PexelsSearchResponse>(body, JsonOptions);
            if (parsed?.Videos == null) return new List<VideoClipCandidate>();

            var candidates = new List<VideoClipCandidate>();
            foreach (var video in parsed.Videos)
            {
                // Prefer a moderate-resolution HD file — full 4K originals are large and
                // unnecessary once the render pipeline scales/crops to the target frame size.
                var file = video.VideoFiles?
                    .Where(f => f.Link != null && f.Width is > 0)
                    .OrderBy(f => Math.Abs((f.Width ?? 0) - 1280))
                    .FirstOrDefault();
                if (file?.Link == null) continue;

                candidates.Add(new VideoClipCandidate(
                    PreviewImageUrl: video.Image ?? file.Link,
                    DownloadUrl: file.Link,
                    SourceUrl: video.Url ?? file.Link,
                    License: "Pexels License (free to use)",
                    DurationSeconds: video.Duration ?? 0));
            }
            return candidates;
        }

        private class PexelsSearchResponse
        {
            [JsonPropertyName("videos")] public List<PexelsVideo>? Videos { get; set; }
        }
        private class PexelsVideo
        {
            [JsonPropertyName("duration")] public double? Duration { get; set; }
            [JsonPropertyName("image")] public string? Image { get; set; }
            [JsonPropertyName("url")] public string? Url { get; set; }
            [JsonPropertyName("video_files")] public List<PexelsVideoFile>? VideoFiles { get; set; }
        }
        private class PexelsVideoFile
        {
            [JsonPropertyName("link")] public string? Link { get; set; }
            [JsonPropertyName("width")] public int? Width { get; set; }
        }
    }
}
