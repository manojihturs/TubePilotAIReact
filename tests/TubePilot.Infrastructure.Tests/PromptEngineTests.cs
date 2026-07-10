using System.Collections.Generic;
using System.Threading.Tasks;
using TubePilot.Infrastructure.Services;
using Xunit;

namespace TubePilot.Infrastructure.Tests
{
    public class PromptEngineTests
    {
        private readonly PromptEngine _engine = new PromptEngine();

        [Fact]
        public void Render_Replaces_SupportedVariables()
        {
            var template = "Hello {{TOPIC}} by {{ACTOR}} in {{YEAR}}";
            var vars = new Dictionary<string, string>
            {
                { "TOPIC", "The Matrix" },
                { "ACTOR", "Keanu Reeves" },
                { "YEAR", "1999" }
            };

            var rendered = _engine.Render(template, vars);
            Assert.Contains("The Matrix", rendered);
            Assert.Contains("Keanu Reeves", rendered);
            Assert.Contains("1999", rendered);
        }

        [Fact]
        public void Validate_Finds_Missing_And_Unknown_Variables()
        {
            var template = "Use {{TOPIC}} and {{UNKNOWN}}";
            var res = _engine.Validate(template, new Dictionary<string, string> { { "TOPIC", "v" } });
            Assert.False(res.IsValid);
            Assert.Contains("UNKNOWN", res.UnknownVariables);
        }

        [Fact]
        public void Preview_Truncates_Long_Render()
        {
            var template = "A" + new string('x', 500);
            var preview = _engine.Preview(template, null, 50);
            Assert.True(preview.Length <= 53); // 50 + ...
        }

        [Fact]
        public async Task PrepareExecution_Returns_Payload()
        {
            var template = "Hello {{TOPIC}}";
            var payload = await _engine.PrepareExecutionAsync(template, new Dictionary<string, string> { { "TOPIC", "World" } });
            Assert.Equal("Hello World", payload.RenderedPrompt);
            Assert.True(payload.EstimatedTokens > 0);
        }
    }
}
