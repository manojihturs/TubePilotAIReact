using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TubePilotAIReact.Server.Data;
using TubePilotAIReact.Server.Models;

namespace TubePilotAIReact.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PromptCategoriesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PromptCategoriesController> _logger;

    public PromptCategoriesController(ApplicationDbContext context, ILogger<PromptCategoriesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all prompt categories
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PromptCategory>>> GetAll()
    {
        try
        {
            var categories = await _context.PromptCategories
                .OrderBy(c => c.Name)
                .ToListAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching prompt categories");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching categories");
        }
    }

    /// <summary>
    /// Get a specific prompt category by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromptCategory>> GetById(int id)
    {
        try
        {
            var category = await _context.PromptCategories
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound($"Category with ID {id} not found");
            }

            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching category {CategoryId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching the category");
        }
    }

    /// <summary>
    /// Create a new prompt category
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PromptCategory>> Create([FromBody] CreatePromptCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Category name is required");
        }

        try
        {
            var category = new PromptCategory
            {
                Name = request.Name,
                Description = request.Description
            };

            _context.PromptCategories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating prompt category");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the category");
        }
    }

    /// <summary>
    /// Update an existing prompt category
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromptCategory>> Update(int id, [FromBody] UpdatePromptCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Category name is required");
        }

        try
        {
            var category = await _context.PromptCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound($"Category with ID {id} not found");
            }

            category.Name = request.Name;
            category.Description = request.Description;
            category.UpdatedAt = DateTime.UtcNow;

            _context.PromptCategories.Update(category);
            await _context.SaveChangesAsync();

            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating prompt category {CategoryId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the category");
        }
    }

    /// <summary>
    /// Delete a prompt category
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var category = await _context.PromptCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound($"Category with ID {id} not found");
            }

            _context.PromptCategories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting prompt category {CategoryId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the category");
        }
    }
}

/// <summary>
/// Request models for PromptCategory operations
/// </summary>
public class CreatePromptCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdatePromptCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
