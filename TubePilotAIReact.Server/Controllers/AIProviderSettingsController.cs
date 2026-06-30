using Microsoft.AspNetCore.Mvc;
using TubePilotAI.Application.Abstractions.ExternalServices;
using TubePilotAI.Application.DTOs;
using TubePilotAI.Application.Features.AIProviderSettings;

namespace TubePilotAIReact.Server.Controllers;

[ApiController]
[Route("api/ai-provider-settings")]
public sealed class AIProviderSettingsController(IAIProviderSettingService aiProviderSettingService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AIProviderSettingDto>>> Get(CancellationToken cancellationToken)
        => Ok(await aiProviderSettingService.GetAsync(cancellationToken));

    [HttpPost]
    public async Task<ActionResult<AIProviderSettingDto>> Upsert(
        [FromBody] UpsertAIProviderSettingRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await aiProviderSettingService.UpsertAsync(request, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{provider}")]
    public async Task<IActionResult> Delete(ContentGenerationProviderKind provider, CancellationToken cancellationToken)
        => await aiProviderSettingService.DeleteAsync(provider, cancellationToken) ? NoContent() : NotFound();
}
