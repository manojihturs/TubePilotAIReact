using System.Threading.Tasks;
using TubePilotAI.Application.DTOs;

namespace TubePilotAI.Infrastructure.AI
{
    /// <summary>
    /// OpenAI provider stub. Replace with real integration in future levels.
    /// </summary>
    public class OpenAIProvider : StubAIProviderBase
    {
        public override string ProviderName => "OpenAI";
    }
}
