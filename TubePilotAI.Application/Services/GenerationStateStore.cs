using System.Collections.Concurrent;
using System.Collections.Generic;

namespace TubePilotAI.Application.Services
{
    /// <summary>
    /// In-memory store for generation job statuses and logs. Level 0 simple implementation.
    /// </summary>
    public static class GenerationStateStore
    {
        public class State
        {
            public string ProjectId { get; set; } = string.Empty;
            public string Status { get; set; } = "Queued";
            public int Progress { get; set; }
            public List<string> Logs { get; set; } = new List<string>();
        }

        private static ConcurrentDictionary<string, State> _states = new ConcurrentDictionary<string, State>();

        public static void Set(State state) => _states[state.ProjectId] = state;

        public static State? Get(string projectId) => _states.TryGetValue(projectId, out var s) ? s : null;
    }
}
