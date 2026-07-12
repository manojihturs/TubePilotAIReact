using TubePilot.Application.Interfaces;

namespace TubePilot.Infrastructure.Services.Images
{
    // Pollinations.ai — free, keyless AI image generation (backed by Flux). Used for the
    // single video thumbnail so Level 0 doesn't require a paid DALL-E/OpenAI key just to
    // ship this feature; a paid provider can be added later behind the same interface.
    public class PollinationsImageProvider : IImageGenerationProvider
    {
        private readonly HttpClient _http;

        public string ProviderName => "Pollinations";

        public PollinationsImageProvider(HttpClient http)
        {
            _http = http;
            if (_http.BaseAddress == null)
            {
                _http.BaseAddress = new Uri("https://image.pollinations.ai/");
            }
            if (_http.DefaultRequestHeaders.UserAgent.Count == 0)
            {
                _http.DefaultRequestHeaders.UserAgent.ParseAdd("TubePilotAI/1.0 (https://github.com/tubepilotai; contact@tubepilotai.local)");
            }
            _http.Timeout = TimeSpan.FromSeconds(60);
        }

        public async Task<byte[]> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
        {
            var encodedPrompt = Uri.EscapeDataString(prompt);
            var url = $"prompt/{encodedPrompt}?width=1280&height=720&nologo=true";

            using var response = await _http.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException($"Pollinations image generation returned {(int)response.StatusCode}: {body}");
            }

            return await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }
    }
}
