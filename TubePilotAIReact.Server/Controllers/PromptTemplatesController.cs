using Microsoft.AspNetCore.Mvc;
using TubePilotAI.Application.DTOs;
using TubePilotAI.Application.Features.PromptTemplates;
using TubePilotAI.Domain.Enums;

namespace TubePilotAIReact.Server.Controllers;

[ApiController]
[Route("api/prompt-templates")]
public sealed class PromptTemplatesController(IPromptTemplateService promptTemplateService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PromptTemplateDto>>> Get(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] PromptTemplateStatus? status,
        CancellationToken cancellationToken)
    {
        var promptTemplates = await promptTemplateService.GetAsync(
            new PromptTemplateQuery
            {
                Search = search,
                Category = category,
                Status = status
            },
            cancellationToken);

        return Ok(promptTemplates);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PromptTemplateDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var promptTemplate = await promptTemplateService.GetByIdAsync(id, cancellationToken);

        return promptTemplate is null ? NotFound() : Ok(promptTemplate);
    }

    [HttpPost]
    public async Task<ActionResult<PromptTemplateDto>> Create(
        [FromBody] CreatePromptTemplateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var promptTemplate = await promptTemplateService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = promptTemplate.Id }, promptTemplate);
        }
        catch (ArgumentException ex)
        {
            return ValidationProblem(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PromptTemplateDto>> Update(
        Guid id,
        [FromBody] UpdatePromptTemplateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var promptTemplate = await promptTemplateService.UpdateAsync(id, request, cancellationToken);
            return promptTemplate is null ? NotFound() : Ok(promptTemplate);
        }
        catch (ArgumentException ex)
        {
            return ValidationProblem(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/default")]
    public async Task<ActionResult<PromptTemplateDto>> SetDefault(Guid id, CancellationToken cancellationToken)
    {
        var promptTemplate = await promptTemplateService.SetDefaultAsync(id, cancellationToken);
        return promptTemplate is null ? NotFound() : Ok(promptTemplate);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await promptTemplateService.DeleteAsync(id, cancellationToken);

        return deleted ? NoContent() : NotFound();
    }
}
