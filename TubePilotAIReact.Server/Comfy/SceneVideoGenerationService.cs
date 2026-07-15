using System.Collections.Concurrent;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using TubePilotAIReact.Server.Models;
using TubePilotAIReact.Server.Storage;

namespace TubePilotAIReact.Server.Comfy;

/// <summary>
/// Orchestrates per-scene motion clip generation: text-to-image -> upload -> image-to-video -> save,
/// bounded by the Comfy Cloud subscription tier's concurrent-job limit.
/// </summary>
public class SceneVideoGenerationService(
    ComfyCloudClient comfy,
    WorkflowTemplateLoader workflows,
    IOptions<ComfyCloudOptions> comfyOptions,
    IOptions<StorageOptions> storageOptions,
    ILogger<SceneVideoGenerationService> logger)
{
    private const int MaxAttemptsPerScene = 2;

    private readonly ConcurrentDictionary<string, ProjectGenerationStatus> _statuses = new();

    public ProjectGenerationStatus? GetStatus(string projectFolderName) =>
        _statuses.GetValueOrDefault(projectFolderName);

    /// <summary>Kicks off generation in the background and returns immediately with the initial status.</summary>
    public ProjectGenerationStatus StartGeneration(GenerateScenesRequest request)
    {
        var status = new ProjectGenerationStatus
        {
            ProjectFolderName = request.ProjectFolderName,
            Scenes = request.Scenes.Select(s => new SceneGenerationStatus { SceneId = s.Id }).ToList()
        };
        _statuses[request.ProjectFolderName] = status;

        // Runs independently of the HTTP request lifetime - do not tie this to the request's CancellationToken.
        _ = RunAsync(request, status);

        return status;
    }

    private async Task RunAsync(GenerateScenesRequest request, ProjectGenerationStatus status)
    {
        var maxConcurrency = Math.Max(1, comfyOptions.Value.MaxConcurrentJobs);
        using var gate = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        var assetsDir = Path.Combine(storageOptions.Value.ProjectStorageRoot, request.ProjectFolderName, "Assets");
        Directory.CreateDirectory(assetsDir);

        var tasks = request.Scenes.Select(async (scene, index) =>
        {
            await gate.WaitAsync();
            try
            {
                await GenerateSceneAsync(scene, index, assetsDir, status);
            }
            finally
            {
                gate.Release();
            }
        });

        await Task.WhenAll(tasks);
        status.IsComplete = true;
    }

    private async Task GenerateSceneAsync(SceneRequest scene, int index, string assetsDir, ProjectGenerationStatus status)
    {
        var sceneStatus = status.Scenes.First(s => s.SceneId == scene.Id);
        sceneStatus.State = SceneGenerationState.Running;

        for (var attempt = 1; attempt <= MaxAttemptsPerScene; attempt++)
        {
            try
            {
                sceneStatus.ClipPath = await GenerateSceneClipAsync(scene, index, assetsDir);
                sceneStatus.State = SceneGenerationState.Succeeded;
                sceneStatus.Error = null;
                return;
            }
            catch (Exception ex) when (attempt < MaxAttemptsPerScene)
            {
                logger.LogWarning(ex, "Scene {SceneId} generation attempt {Attempt} failed, retrying", scene.Id, attempt);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Scene {SceneId} generation failed after {Attempts} attempts", scene.Id, MaxAttemptsPerScene);
                sceneStatus.State = SceneGenerationState.Failed;
                sceneStatus.Error = ex.Message;
            }
        }
    }

    private async Task<string> GenerateSceneClipAsync(SceneRequest scene, int index, string assetsDir)
    {
        var opts = comfyOptions.Value;

        var t2iWorkflow = workflows.LoadPatched(
            opts.TextToImageWorkflowPath,
            (opts.TextToImagePromptNode, JsonValue.Create(scene.VisualPrompt)));
        var t2iPromptId = await comfy.SubmitWorkflowAsync(t2iWorkflow);
        var stillFile = await comfy.WaitForOutputAsync(t2iPromptId);
        var stillBytes = await comfy.DownloadOutputAsync(stillFile);

        var uploaded = await comfy.UploadImageAsync(stillBytes, $"scene-{index + 1:000}-{scene.Id}.png");

        var i2vWorkflow = workflows.LoadPatched(
            opts.ImageToVideoWorkflowPath,
            (opts.ImageToVideoImageNode, JsonValue.Create(uploaded.Filename)),
            (opts.ImageToVideoPromptNode, JsonValue.Create(scene.VisualPrompt)));
        var i2vPromptId = await comfy.SubmitWorkflowAsync(i2vWorkflow);
        var clipFile = await comfy.WaitForOutputAsync(i2vPromptId);
        var clipBytes = await comfy.DownloadOutputAsync(clipFile);

        var clipPath = Path.Combine(assetsDir, $"scene-{index + 1:000}.mp4");
        await File.WriteAllBytesAsync(clipPath, clipBytes);
        return clipPath;
    }
}
