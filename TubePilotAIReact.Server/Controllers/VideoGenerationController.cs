using Microsoft.AspNetCore.Mvc;
using TubePilotAIReact.Server.Comfy;
using TubePilotAIReact.Server.Models;

namespace TubePilotAIReact.Server.Controllers;

[ApiController]
[Route("api/video-generation")]
public class VideoGenerationController(SceneVideoGenerationService generationService) : ControllerBase
{
    [HttpPost("{projectFolderName}/scenes")]
    public IActionResult GenerateScenes(string projectFolderName, [FromBody] GenerateScenesRequest request)
    {
        if (!string.Equals(projectFolderName, request.ProjectFolderName, StringComparison.Ordinal))
            return BadRequest("Route projectFolderName must match the request body's ProjectFolderName.");

        if (request.Scenes.Count == 0)
            return BadRequest("At least one scene is required.");

        var status = generationService.StartGeneration(request);

        return AcceptedAtAction(nameof(GetStatus), new { projectFolderName }, status);
    }

    [HttpGet("{projectFolderName}/status")]
    public IActionResult GetStatus(string projectFolderName)
    {
        var status = generationService.GetStatus(projectFolderName);
        return status is null ? NotFound() : Ok(status);
    }
}
