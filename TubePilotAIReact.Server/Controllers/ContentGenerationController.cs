using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TubePilotAI.Application.DTOs;
using TubePilotAI.Application.Features.ContentGeneration;
using TubePilotAI.Infrastructure.Persistence.Context;

namespace TubePilotAIReact.Server.Controllers;

[ApiController]
[Route("api/content-generation")]
public sealed class ContentGenerationController(IContentGenerationService contentGenerationService, TubePilotDbContext dbContext) : ControllerBase
{
    [HttpGet("jobs")]
    public async Task<ActionResult<IReadOnlyList<ContentGenerationJobDto>>> GetJobs(CancellationToken cancellationToken)
    {
        return Ok(await contentGenerationService.GetJobsAsync(cancellationToken));
    }

    [HttpGet("jobs/{id:guid}")]
    public async Task<ActionResult<ContentGenerationResultDto>> GetJob(Guid id, CancellationToken cancellationToken)
    {
        var job = await contentGenerationService.GetJobAsync(id, cancellationToken);
        return job is null ? NotFound() : Ok(job);
    }

    [HttpGet("prompt-templates")]
    public async Task<ActionResult<IReadOnlyList<object>>> GetPromptTemplates(CancellationToken cancellationToken)
    {
        var templates = await dbContext.PromptTemplates
            .AsNoTracking()
            .OrderByDescending(template => template.CreatedAtUtc)
            .Select(template => new
            {
                template.Id,
                template.Name,
                template.Category,
                template.Description,
                template.TemplateText,
                template.SystemMessage,
                template.IsDefault
            })
            .ToListAsync(cancellationToken);

        return Ok(templates);
    }

    [HttpPost("generate")]
    public async Task<ActionResult<ContentGenerationResultDto>> Generate(
        [FromBody] GenerateContentJobRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await contentGenerationService.GenerateAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("jobs/{id:guid}/export")]
    public async Task<ActionResult> Export(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var path = await contentGenerationService.ExportAsync(id, cancellationToken);
            return Ok(new { path });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
