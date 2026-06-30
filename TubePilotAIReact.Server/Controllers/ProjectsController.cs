using Microsoft.AspNetCore.Mvc;
using TubePilotAI.Application.DTOs;
using TubePilotAI.Application.Features.Projects;
using TubePilotAI.Infrastructure.Services;

namespace TubePilotAIReact.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProjectsController(IProjectService projectService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjectDto>>> GetAsync(
        [FromQuery] ProjectQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await projectService.GetAsync(query, cancellationToken));
    }

    [HttpGet("{id:guid}", Name = "GetProjectById")]
    public async Task<ActionResult<ProjectDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var project = await projectService.GetByIdAsync(id, cancellationToken);
        return project is null ? NotFound() : Ok(project);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> CreateAsync(
        [FromBody] CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var project = await projectService.CreateAsync(request, cancellationToken);
            return CreatedAtRoute("GetProjectById", new { id = project.Id }, project);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProjectDto>> UpdateAsync(
        Guid id,
        [FromBody] UpdateProjectRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var project = await projectService.UpdateAsync(id, request, cancellationToken);
            return project is null ? NotFound() : Ok(project);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await projectService.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/export")]
    public async Task<ActionResult> ExportAsync(
        Guid id,
        [FromServices] ProjectExportService exportService,
        CancellationToken cancellationToken)
    {
        try
        {
            var path = await exportService.ExportProjectAsync(id, cancellationToken);
            return Ok(new { Path = path });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
