using System.Text;
using TubePilot.Application.Interfaces;

namespace TubePilot.Infrastructure.Services.Audio
{
    // Free, keyless TTS via the same unofficial Google Translate endpoint the popular
    // Python `gTTS` library uses. No API key, supports many languages including Tamil ("ta").
    // The endpoint caps each request at ~200 characters, so longer narration is split on
    // sentence boundaries into chunks and the resulting MP3 byte streams are concatenated.
    public class GoogleTranslateTtsProvider : ITextToSpeechProvider
    {
        private const int MaxChunkLength = 200;
        private readonly HttpClient _http;

        public GoogleTranslateTtsProvider(HttpClient http)
        {
            _http = http;
            if (_http.BaseAddress == null)
            {
                _http.BaseAddress = new Uri("https://translate.google.com/");
            }
            if (_http.DefaultRequestHeaders.UserAgent.Count == 0)
            {
                _http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; TubePilotAI/1.0)");
            }
        }

        public async Task<byte[]> SynthesizeAsync(string text, string language, CancellationToken cancellationToken = default)
        {
            var chunks = SplitIntoChunks(text.Trim(), MaxChunkLength);
            using var output = new MemoryStream();

            foreach (var chunk in chunks)
            {
                if (string.IsNullOrWhiteSpace(chunk)) continue;

                var encoded = Uri.EscapeDataString(chunk);
                var url = $"translate_tts?ie=UTF-8&q={encoded}&tl={language}&client=tw-ob";

                using var response = await _http.GetAsync(url, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new InvalidOperationException($"TTS request failed ({(int)response.StatusCode}): {body}");
                }

                var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                await output.WriteAsync(bytes, cancellationToken);
            }

            if (output.Length == 0)
            {
                throw new InvalidOperationException("TTS produced no audio — input text was empty after splitting.");
            }

            return output.ToArray();
        }

        private static List<string> SplitIntoChunks(string text, int maxLength)
        {
            var sentences = text.Split(new[] { ". ", "! ", "? ", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var chunks = new List<string>();
            var current = new StringBuilder();

            foreach (var sentence in sentences)
            {
                var candidate = sentence.Trim();
                if (candidate.Length == 0) continue;

                if (current.Length + candidate.Length + 2 > maxLength)
                {
                    if (current.Length > 0)
                    {
                        chunks.Add(current.ToString());
                        current.Clear();
                    }

                    // A single sentence longer than the cap — hard-split it.
                    if (candidate.Length > maxLength)
                    {
                        for (var i = 0; i < candidate.Length; i += maxLength)
                        {
                            chunks.Add(candidate.Substring(i, Math.Min(maxLength, candidate.Length - i)));
                        }
                        continue;
                    }
                }

                if (current.Length > 0) current.Append(". ");
                current.Append(candidate);
            }

            if (current.Length > 0) chunks.Add(current.ToString());
            return chunks;
        }
    }
}
