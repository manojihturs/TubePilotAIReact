using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TubePilot.Application.DTOs;
using TubePilot.Application.Interfaces;
using TubePilot.Domain.Entities;

namespace TubePilot.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PromptCategoriesController : ControllerBase
    {
        private readonly IPromptCategoryRepository _repo;

        public PromptCategoriesController(IPromptCategoryRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] string? sortBy = null, [FromQuery] string sortDir = "asc")
        {
            var desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            var (items, total) = await _repo.GetPagedAsync(page, pageSize, search, sortBy, desc);
            var dtos = items.Select(i => new PromptCategoryDto
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                DisplayOrder = i.DisplayOrder,
                Icon = i.Icon,
                Color = i.Color,
                IsActive = i.IsActive,
                CreatedBy = i.CreatedBy,
                UpdatedBy = i.UpdatedBy,
                CreatedDate = i.CreatedDate,
                UpdatedDate = i.UpdatedDate
            }).ToList();

            var meta = new PaginationMeta
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                TotalPages = (int)Math.Ceiling((double)total / pageSize)
            };

            var response = new ApiResponse<IEnumerable<PromptCategoryDto>>
            {
                Success = true,
                Data = dtos,
                Meta = meta
            };

            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return NotFound(new ApiResponse<string> { Success = false, Message = "Not found" });
            var dto = new PromptCategoryDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                DisplayOrder = item.DisplayOrder,
                Icon = item.Icon,
                Color = item.Color,
                IsActive = item.IsActive,
                CreatedBy = item.CreatedBy,
                UpdatedBy = item.UpdatedBy,
                CreatedDate = item.CreatedDate,
                UpdatedDate = item.UpdatedDate
            };
            return Ok(new ApiResponse<PromptCategoryDto> { Success = true, Data = dto });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreatePromptCategoryRequest req)
        {
            var userEmail = User?.FindFirst(ClaimTypes.Email)?.Value ?? User?.Identity?.Name ?? "system";
            var entity = new PromptCategory
            {
                Id = Guid.NewGuid(),
                Name = req.Name,
                Description = req.Description,
                DisplayOrder = req.DisplayOrder,
                Icon = req.Icon,
                Color = req.Color,
                IsActive = req.IsActive,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = userEmail
            };
            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();
            var dto = new PromptCategoryDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                DisplayOrder = entity.DisplayOrder,
                Icon = entity.Icon,
                Color = entity.Color,
                IsActive = entity.IsActive,
                CreatedBy = entity.CreatedBy,
                CreatedDate = entity.CreatedDate
            };
            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, new ApiResponse<PromptCategoryDto> { Success = true, Data = dto });
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePromptCategoryRequest req)
        {
            if (id != req.Id) return BadRequest(new ApiResponse<string> { Success = false, Message = "Id mismatch" });
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound(new ApiResponse<string> { Success = false, Message = "Not found" });

            var userEmail = User?.FindFirst(ClaimTypes.Email)?.Value ?? User?.Identity?.Name ?? "system";

            existing.Name = req.Name;
            existing.Description = req.Description;
            existing.DisplayOrder = req.DisplayOrder;
            existing.Icon = req.Icon;
            existing.Color = req.Color;
            existing.IsActive = req.IsActive;
            existing.UpdatedDate = DateTime.UtcNow;
            existing.UpdatedBy = userEmail;

            await _repo.UpdateAsync(existing);
            await _repo.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound(new ApiResponse<string> { Success = false, Message = "Not found" });
            await _repo.DeleteAsync(id);
            await _repo.SaveChangesAsync();
            return NoContent();
        }
    }
}
