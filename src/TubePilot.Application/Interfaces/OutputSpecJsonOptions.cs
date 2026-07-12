using System.Text.Json;
using System.Text.Json.Serialization;

namespace TubePilot.Application.Interfaces
{
    // Shared JSON options for (de)serializing OutputSpec — centralized so every caller
    // gets the string-enum converter (a prior bug: without it, "type":"Text" fails to
    // parse and silently falls back to a default spec instead of the real one).
    public static class OutputSpecJsonOptions
    {
        public static readonly JsonSerializerOptions Default = Create();

        private static JsonSerializerOptions Create()
        {
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }
    }
}
