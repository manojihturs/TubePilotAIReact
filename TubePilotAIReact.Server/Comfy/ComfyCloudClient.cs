using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;

namespace TubePilotAIReact.Server.Comfy;

public record ComfyUploadResult(string Filename, string Subfolder, string Type);

public record ComfyOutputFile(string Filename, string Subfolder, string Type);

public class ComfyJobFailedException(string promptId, string? reason)
    : Exception($"Comfy Cloud job {promptId} failed: {reason ?? "unknown error"}");

public class ComfyJobTimeoutException(string promptId, TimeSpan timeout)
    : Exception($"Comfy Cloud job {promptId} did not complete within {timeout.TotalMinutes:0} minutes");

public class ComfyCloudClient
{
    private readonly HttpClient _http;
    private readonly ComfyCloudOptions _options;

    public ComfyCloudClient(HttpClient http, IOptions<ComfyCloudOptions> options)
    {
        _options = options.Value;
        http.BaseAddress = new Uri(_options.BaseUrl);
        http.DefaultRequestHeaders.Remove("X-API-Key");
        http.DefaultRequestHeaders.Add("X-API-Key", _options.ApiKey);
        _http = http;
    }

    public async Task<ComfyUploadResult> UploadImageAsync(byte[] imageBytes, string filename, CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContent, "image", filename);
        content.Add(new StringContent("input"), "type");
        content.Add(new StringContent("true"), "overwrite");

        using var response = await _http.PostAsync("/api/upload/image", content, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonObject>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Comfy Cloud upload returned an empty response.");

        return new ComfyUploadResult(
            json["name"]?.GetValue<string>() ?? filename,
            json["subfolder"]?.GetValue<string>() ?? "",
            json["type"]?.GetValue<string>() ?? "input");
    }

    public async Task<string> SubmitWorkflowAsync(JsonObject workflow, CancellationToken ct = default)
    {
        var payload = new JsonObject { ["prompt"] = workflow };
        using var response = await _http.PostAsJsonAsync("/api/prompt", payload, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonObject>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Comfy Cloud prompt submission returned an empty response.");

        return json["prompt_id"]?.GetValue<string>()
            ?? throw new InvalidOperationException("Comfy Cloud prompt submission response did not include a prompt_id.");
    }

    public async Task<string> GetJobStatusAsync(string promptId, CancellationToken ct = default)
    {
        using var response = await _http.GetAsync($"/api/job/{promptId}/status", ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonObject>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Comfy Cloud job status returned an empty response.");

        return json["status"]?.GetValue<string>()
            ?? throw new InvalidOperationException("Comfy Cloud job status response did not include a status.");
    }

    public async Task<JsonObject> GetHistoryAsync(string promptId, CancellationToken ct = default)
    {
        using var response = await _http.GetAsync($"/api/history/{promptId}", ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<JsonObject>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Comfy Cloud history returned an empty response.");
    }

    /// <summary>
    /// Polls job status until completion, then returns the first output file (image or video)
    /// found across the job's node outputs in its history record.
    /// </summary>
    public async Task<ComfyOutputFile> WaitForOutputAsync(string promptId, CancellationToken ct = default)
    {
        var timeout = TimeSpan.FromMinutes(_options.JobTimeoutMinutes);
        var pollInterval = TimeSpan.FromSeconds(_options.PollIntervalSeconds);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeout);

        try
        {
            while (true)
            {
                var status = await GetJobStatusAsync(promptId, cts.Token);
                switch (status)
                {
                    case "completed":
                        return ExtractFirstOutputFile(await GetHistoryAsync(promptId, cts.Token), promptId);
                    case "failed":
                    case "cancelled":
                        throw new ComfyJobFailedException(promptId, status);
                    default:
                        await Task.Delay(pollInterval, cts.Token);
                        break;
                }
            }
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new ComfyJobTimeoutException(promptId, timeout);
        }
    }

    public async Task<byte[]> DownloadOutputAsync(ComfyOutputFile file, CancellationToken ct = default)
    {
        var query = $"filename={Uri.EscapeDataString(file.Filename)}" +
                    $"&subfolder={Uri.EscapeDataString(file.Subfolder)}" +
                    $"&type={Uri.EscapeDataString(file.Type)}";

        using var response = await _http.GetAsync($"/api/view?{query}", ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync(ct);
    }

    private static ComfyOutputFile ExtractFirstOutputFile(JsonObject history, string promptId)
    {
        var record = history[promptId]?.AsObject() ?? history.First().Value?.AsObject();
        var outputs = record?["outputs"]?.AsObject();
        if (outputs is null)
            throw new InvalidOperationException($"Comfy Cloud history for job {promptId} did not include outputs.");

        foreach (var (_, node) in outputs)
        {
            var nodeObj = node?.AsObject();
            if (nodeObj is null) continue;

            foreach (var key in new[] { "videos", "images", "gifs" })
            {
                var files = nodeObj[key]?.AsArray();
                if (files is { Count: > 0 } && files[0] is JsonObject fileObj)
                {
                    return new ComfyOutputFile(
                        fileObj["filename"]?.GetValue<string>() ?? throw new InvalidOperationException("Output file missing filename."),
                        fileObj["subfolder"]?.GetValue<string>() ?? "",
                        fileObj["type"]?.GetValue<string>() ?? "output");
                }
            }
        }

        throw new InvalidOperationException($"Comfy Cloud job {promptId} completed but produced no image/video/gif output.");
    }
}
