using System.Net;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using TubePilotAIReact.Server.Comfy;
using Xunit;

namespace TubePilotAIReact.Server.Tests;

public class ComfyCloudClientTests
{
    private static ComfyCloudClient CreateClient(FakeHttpMessageHandler handler, ComfyCloudOptions? options = null)
    {
        var httpClient = new HttpClient(handler);
        options ??= new ComfyCloudOptions
        {
            BaseUrl = "https://cloud.comfy.org",
            ApiKey = "test-api-key",
            PollIntervalSeconds = 0,
            JobTimeoutMinutes = 1
        };
        return new ComfyCloudClient(httpClient, Options.Create(options));
    }

    [Fact]
    public async Task UploadImageAsync_SendsMultipartRequest_WithApiKeyHeaderAndParsedFilename()
    {
        var handler = new FakeHttpMessageHandler(_ =>
            Task.FromResult(FakeHttpMessageHandler.Json("""{"name":"scene-001.png","subfolder":"","type":"input"}""")));
        var client = CreateClient(handler);

        var result = await client.UploadImageAsync([1, 2, 3], "scene-001.png");

        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.Equal("/api/upload/image", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal("test-api-key", handler.LastRequest.Headers.GetValues("X-API-Key").Single());
        Assert.Contains("multipart/form-data", handler.LastRequest.Content!.Headers.ContentType!.MediaType);
        Assert.Contains("name=type", handler.LastRequestBody, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("name=image", handler.LastRequestBody, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("scene-001.png", handler.LastRequestBody);
        Assert.Equal("scene-001.png", result.Filename);
    }

    [Fact]
    public async Task SubmitWorkflowAsync_WrapsWorkflowInPromptEnvelope_AndReturnsPromptId()
    {
        var handler = new FakeHttpMessageHandler(_ =>
            Task.FromResult(FakeHttpMessageHandler.Json("""{"prompt_id":"abc-123"}""")));
        var client = CreateClient(handler);
        var workflow = new JsonObject { ["6"] = new JsonObject { ["inputs"] = new JsonObject { ["text"] = "a wolf howling" } } };

        var promptId = await client.SubmitWorkflowAsync(workflow);

        Assert.Equal("/api/prompt", handler.LastRequest!.RequestUri!.AbsolutePath);
        Assert.Contains("\"prompt\"", handler.LastRequestBody);
        Assert.Contains("a wolf howling", handler.LastRequestBody);
        Assert.Equal("abc-123", promptId);
    }

    [Fact]
    public async Task GetJobStatusAsync_ReturnsStatusField()
    {
        var handler = new FakeHttpMessageHandler(req =>
        {
            Assert.Equal("/api/job/abc-123/status", req.RequestUri!.AbsolutePath);
            return Task.FromResult(FakeHttpMessageHandler.Json("""{"status":"in_progress"}"""));
        });
        var client = CreateClient(handler);

        var status = await client.GetJobStatusAsync("abc-123");

        Assert.Equal("in_progress", status);
    }

    [Fact]
    public async Task DownloadOutputAsync_BuildsViewQueryString_AndReturnsBytes()
    {
        var handler = new FakeHttpMessageHandler(req =>
        {
            Assert.Equal("/api/view", req.RequestUri!.AbsolutePath);
            Assert.Contains("filename=scene-001.mp4", req.RequestUri.Query);
            Assert.Contains("type=output", req.RequestUri.Query);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent([9, 9, 9])
            });
        });
        var client = CreateClient(handler);

        var bytes = await client.DownloadOutputAsync(new ComfyOutputFile("scene-001.mp4", "", "output"));

        Assert.Equal([9, 9, 9], bytes);
    }

    [Fact]
    public async Task WaitForOutputAsync_PollsUntilCompleted_ThenExtractsVideoFromHistory()
    {
        var callCount = 0;
        var handler = new FakeHttpMessageHandler(req =>
        {
            if (req.RequestUri!.AbsolutePath.EndsWith("/status"))
            {
                callCount++;
                var status = callCount < 2 ? "in_progress" : "completed";
                return Task.FromResult(FakeHttpMessageHandler.Json($$"""{"status":"{{status}}"}"""));
            }

            // /api/history/{promptId}
            return Task.FromResult(FakeHttpMessageHandler.Json("""
                {
                  "abc-123": {
                    "outputs": {
                      "9": {
                        "videos": [
                          { "filename": "scene-001.mp4", "subfolder": "", "type": "output" }
                        ]
                      }
                    }
                  }
                }
                """));
        });
        var client = CreateClient(handler);

        var output = await client.WaitForOutputAsync("abc-123");

        Assert.Equal("scene-001.mp4", output.Filename);
        Assert.Equal("output", output.Type);
    }

    [Fact]
    public async Task WaitForOutputAsync_ThrowsComfyJobFailedException_WhenJobFails()
    {
        var handler = new FakeHttpMessageHandler(_ =>
            Task.FromResult(FakeHttpMessageHandler.Json("""{"status":"failed"}""")));
        var client = CreateClient(handler);

        await Assert.ThrowsAsync<ComfyJobFailedException>(() => client.WaitForOutputAsync("abc-123"));
    }
}
