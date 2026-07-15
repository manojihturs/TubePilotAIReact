using System.Collections.Concurrent;
using System.Text.Json.Nodes;

namespace TubePilotAIReact.Server.Comfy;

/// <summary>
/// Loads Comfy Cloud "Export Workflow (API)" JSON templates from disk (cached),
/// and returns a patched deep clone per call with scene-specific node inputs applied.
/// </summary>
public class WorkflowTemplateLoader(IHostEnvironment env)
{
    private readonly ConcurrentDictionary<string, JsonObject> _cache = new();

    public JsonObject LoadPatched(string relativePath, params (WorkflowNodeInput Target, JsonNode? Value)[] patches)
    {
        var template = _cache.GetOrAdd(relativePath, LoadFromDisk);
        var clone = template.DeepClone().AsObject();

        foreach (var (target, value) in patches)
        {
            if (string.IsNullOrEmpty(target.NodeId))
                continue;

            var node = clone[target.NodeId]?.AsObject()
                ?? throw new InvalidOperationException($"Workflow '{relativePath}' has no node with id '{target.NodeId}'.");
            var inputs = node["inputs"]?.AsObject()
                ?? throw new InvalidOperationException($"Workflow node '{target.NodeId}' in '{relativePath}' has no 'inputs' object.");

            inputs[target.InputName] = value;
        }

        return clone;
    }

    private JsonObject LoadFromDisk(string relativePath)
    {
        var fullPath = Path.Combine(env.ContentRootPath, relativePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException(
                $"Comfy workflow template not found at '{fullPath}'. Build the workflow in the Comfy Cloud editor, " +
                "export it via \"Export Workflow (API)\", and place the JSON at this path.",
                fullPath);
        }

        var text = File.ReadAllText(fullPath);
        return JsonNode.Parse(text)?.AsObject()
            ?? throw new InvalidOperationException($"Workflow template '{relativePath}' is not valid JSON.");
    }
}
