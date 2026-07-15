namespace TubePilotAIReact.Server.Comfy;

public class ComfyCloudOptions
{
    public const string SectionName = "ComfyCloud";

    public string BaseUrl { get; set; } = "https://cloud.comfy.org";
    public string ApiKey { get; set; } = "";
    public int MaxConcurrentJobs { get; set; } = 1;
    public int PollIntervalSeconds { get; set; } = 3;
    public int JobTimeoutMinutes { get; set; } = 25;

    public string TextToImageWorkflowPath { get; set; } = "Comfy/Workflows/text-to-image.json";
    public WorkflowNodeInput TextToImagePromptNode { get; set; } = new();

    public string ImageToVideoWorkflowPath { get; set; } = "Comfy/Workflows/image-to-video.json";
    public WorkflowNodeInput ImageToVideoImageNode { get; set; } = new();
    public WorkflowNodeInput ImageToVideoPromptNode { get; set; } = new();
}

public class WorkflowNodeInput
{
    public string NodeId { get; set; } = "";
    public string InputName { get; set; } = "";
}
