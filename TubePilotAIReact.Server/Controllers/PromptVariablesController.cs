using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TubePilotAIReact.Server.Data;
using TubePilotAIReact.Server.Models;

namespace TubePilotAIReact.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PromptVariablesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PromptVariablesController> _logger;

    public PromptVariablesController(ApplicationDbContext context, ILogger<PromptVariablesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all prompt variables
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PromptVariable>>> GetAll([FromQuery] int? promptId = null)
    {
        try
        {
            IQueryable<PromptVariable> query = _context.PromptVariables;

            if (promptId.HasValue)
            {
                query = query.Where(v => v.PromptId == promptId.Value);
            }

            var variables = await query.OrderBy(v => v.Name).ToListAsync();
            return Ok(variables);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching prompt variables");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching variables");
        }
    }

    /// <summary>
    /// Get a specific prompt variable by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromptVariable>> GetById(int id)
    {
        try
        {
            var variable = await _context.PromptVariables
                .FirstOrDefaultAsync(v => v.Id == id);

            if (variable == null)
            {
                return NotFound($"Variable with ID {id} not found");
            }

            return Ok(variable);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching prompt variable {VariableId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching the variable");
        }
    }

    /// <summary>
    /// Create a new prompt variable
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PromptVariable>> Create([FromBody] CreatePromptVariableRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Variable name is required");
        }

        try
        {
            // Verify prompt exists
            var promptExists = await _context.Prompts.AnyAsync(p => p.Id == request.PromptId);
            if (!promptExists)
            {
                return BadRequest($"Prompt with ID {request.PromptId} does not exist");
            }

            var variable = new PromptVariable
            {
                PromptId = request.PromptId,
                Name = request.Name,
                Description = request.Description,
                DefaultValue = request.DefaultValue ?? string.Empty
            };

            _context.PromptVariables.Add(variable);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = variable.Id }, variable);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating prompt variable");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the variable");
        }
    }

    /// <summary>
    /// Update an existing prompt variable
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromptVariable>> Update(int id, [FromBody] UpdatePromptVariableRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Variable name is required");
        }

        try
        {
            var variable = await _context.PromptVariables.FindAsync(id);
            if (variable == null)
            {
                return NotFound($"Variable with ID {id} not found");
            }

            // Verify prompt exists if being changed
            if (variable.PromptId != request.PromptId)
            {
                var promptExists = await _context.Prompts.AnyAsync(p => p.Id == request.PromptId);
                if (!promptExists)
                {
                    return BadRequest($"Prompt with ID {request.PromptId} does not exist");
                }
            }

            variable.PromptId = request.PromptId;
            variable.Name = request.Name;
            variable.Description = request.Description;
            variable.DefaultValue = request.DefaultValue ?? string.Empty;
            variable.UpdatedAt = DateTime.UtcNow;

            _context.PromptVariables.Update(variable);
            await _context.SaveChangesAsync();

            return Ok(variable);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating prompt variable {VariableId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the variable");
        }
    }

    /// <summary>
    /// Delete a prompt variable
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var variable = await _context.PromptVariables.FindAsync(id);
            if (variable == null)
            {
                return NotFound($"Variable with ID {id} not found");
            }

            _context.PromptVariables.Remove(variable);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting prompt variable {VariableId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the variable");
        }
    }
}

/// <summary>
/// Request models for PromptVariable operations
/// </summary>
public class CreatePromptVariableRequest
{
    public int PromptId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DefaultValue { get; set; }
}

public class UpdatePromptVariableRequest
{
    public int PromptId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DefaultValue { get; set; }
}
