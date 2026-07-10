using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TubePilotAIReact.Server.Data;
using TubePilotAIReact.Server.Models;

namespace TubePilotAIReact.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PromptsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PromptsController> _logger;

    public PromptsController(ApplicationDbContext context, ILogger<PromptsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all prompts
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Prompt>>> GetAll([FromQuery] int? categoryId = null)
    {
        try
        {
            IQueryable<Prompt> query = _context.Prompts;

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var prompts = await query.OrderBy(p => p.Title).ToListAsync();
            return Ok(prompts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching prompts");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching prompts");
        }
    }

    /// <summary>
    /// Get a specific prompt by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Prompt>> GetById(int id)
    {
        try
        {
            var prompt = await _context.Prompts
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prompt == null)
            {
                return NotFound($"Prompt with ID {id} not found");
            }

            return Ok(prompt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching prompt {PromptId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching the prompt");
        }
    }

    /// <summary>
    /// Create a new prompt
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Prompt>> Create([FromBody] CreatePromptRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest("Title and Content are required");
        }

        try
        {
            // Verify category exists
            var categoryExists = await _context.PromptCategories.AnyAsync(c => c.Id == request.CategoryId);
            if (!categoryExists)
            {
                return BadRequest($"Category with ID {request.CategoryId} does not exist");
            }

            var prompt = new Prompt
            {
                Title = request.Title,
                Content = request.Content,
                CategoryId = request.CategoryId
            };

            _context.Prompts.Add(prompt);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = prompt.Id }, prompt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating prompt");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the prompt");
        }
    }

    /// <summary>
    /// Update an existing prompt
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Prompt>> Update(int id, [FromBody] UpdatePromptRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest("Title and Content are required");
        }

        try
        {
            var prompt = await _context.Prompts.FindAsync(id);
            if (prompt == null)
            {
                return NotFound($"Prompt with ID {id} not found");
            }

            // Verify category exists if being changed
            if (prompt.CategoryId != request.CategoryId)
            {
                var categoryExists = await _context.PromptCategories.AnyAsync(c => c.Id == request.CategoryId);
                if (!categoryExists)
                {
                    return BadRequest($"Category with ID {request.CategoryId} does not exist");
                }
            }

            prompt.Title = request.Title;
            prompt.Content = request.Content;
            prompt.CategoryId = request.CategoryId;
            prompt.UpdatedAt = DateTime.UtcNow;

            _context.Prompts.Update(prompt);
            await _context.SaveChangesAsync();

            return Ok(prompt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating prompt {PromptId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the prompt");
        }
    }

    /// <summary>
    /// Delete a prompt
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var prompt = await _context.Prompts.FindAsync(id);
            if (prompt == null)
            {
                return NotFound($"Prompt with ID {id} not found");
            }

            _context.Prompts.Remove(prompt);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting prompt {PromptId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the prompt");
        }
    }
}

/// <summary>
/// Request models for Prompt operations
/// </summary>
public class CreatePromptRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int CategoryId { get; set; }
}

public class UpdatePromptRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int CategoryId { get; set; }
}
