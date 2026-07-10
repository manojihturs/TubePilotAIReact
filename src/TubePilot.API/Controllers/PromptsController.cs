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
    public class PromptsController : ControllerBase
    {
        private readonly IPromptRepository _repo;

        public PromptsController(IPromptRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] Guid? categoryId = null, [FromQuery] string? search = null, [FromQuery] string? sortBy = null, [FromQuery] string sortDir = "desc")
        {
            var desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            var (items, total) = await _repo.GetPagedAsync(page, pageSize, categoryId, search, sortBy, desc);
            var dtos = items.Select(i => new PromptDto
            {
                Id = i.Id,
                CategoryId = i.CategoryId,
                Name = i.Name,
                Description = i.Description,
                PromptText = i.PromptText,
                Temperature = i.Temperature,
                TopP = i.TopP,
                MaxTokens = i.MaxTokens,
                Model = i.Model,
                Provider = i.Provider,
                Version = i.Version,
                Status = i.Status,
                Tags = i.Tags,
                IsSystem = i.IsSystem,
                IsPublic = i.IsPublic,
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

            return Ok(new ApiResponse<IEnumerable<PromptDto>> { Success = true, Data = dtos, Meta = meta });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return NotFound(new ApiResponse<string> { Success = false, Message = "Not found" });
            var dto = new PromptDto
            {
                Id = item.Id,
                CategoryId = item.CategoryId,
                Name = item.Name,
                Description = item.Description,
                PromptText = item.PromptText,
                Temperature = item.Temperature,
                TopP = item.TopP,
                MaxTokens = item.MaxTokens,
                Model = item.Model,
                Provider = item.Provider,
                Version = item.Version,
                Status = item.Status,
                Tags = item.Tags,
                IsSystem = item.IsSystem,
                IsPublic = item.IsPublic,
                CreatedBy = item.CreatedBy,
                UpdatedBy = item.UpdatedBy,
                CreatedDate = item.CreatedDate,
                UpdatedDate = item.UpdatedDate
            };
            return Ok(new ApiResponse<PromptDto> { Success = true, Data = dto });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreatePromptRequest req)
        {
            var userEmail = User?.FindFirst(ClaimTypes.Email)?.Value ?? User?.Identity?.Name ?? "system";
            var entity = new Prompt
            {
                Id = Guid.NewGuid(),
                CategoryId = req.CategoryId,
                Name = req.Name,
                Description = req.Description,
                PromptText = req.PromptText,
                Temperature = req.Temperature,
                TopP = req.TopP,
                MaxTokens = req.MaxTokens,
                Model = req.Model,
                Provider = req.Provider,
                Version = req.Version,
                Status = req.Status,
                Tags = req.Tags,
                IsSystem = req.IsSystem,
                IsPublic = req.IsPublic,
                CreatedBy = userEmail,
                CreatedDate = DateTime.UtcNow
            };
            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();
            var dto = new PromptDto
            {
                Id = entity.Id,
                CategoryId = entity.CategoryId,
                Name = entity.Name,
                Description = entity.Description,
                PromptText = entity.PromptText,
                Temperature = entity.Temperature,
                TopP = entity.TopP,
                MaxTokens = entity.MaxTokens,
                Model = entity.Model,
                Provider = entity.Provider,
                Version = entity.Version,
                Status = entity.Status,
                Tags = entity.Tags,
                IsSystem = entity.IsSystem,
                IsPublic = entity.IsPublic,
                CreatedBy = entity.CreatedBy,
                CreatedDate = entity.CreatedDate
            };
            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, new ApiResponse<PromptDto> { Success = true, Data = dto });
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePromptRequest req)
        {
            if (id != req.Id) return BadRequest(new ApiResponse<string> { Success = false, Message = "Id mismatch" });
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound(new ApiResponse<string> { Success = false, Message = "Not found" });

            var userEmail = User?.FindFirst(ClaimTypes.Email)?.Value ?? User?.Identity?.Name ?? "system";

            existing.CategoryId = req.CategoryId;
            existing.Name = req.Name;
            existing.Description = req.Description;
            existing.PromptText = req.PromptText;
            existing.Temperature = req.Temperature;
            existing.TopP = req.TopP;
            existing.MaxTokens = req.MaxTokens;
            existing.Model = req.Model;
            existing.Provider = req.Provider;
            existing.Version = req.Version;
            existing.Status = req.Status;
            existing.Tags = req.Tags;
            existing.IsSystem = req.IsSystem;
            existing.IsPublic = req.IsPublic;
            existing.UpdatedBy = userEmail;
            existing.UpdatedDate = DateTime.UtcNow;

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
