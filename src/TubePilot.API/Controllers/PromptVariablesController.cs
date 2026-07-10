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
    public class PromptVariablesController : ControllerBase
    {
        private readonly IPromptVariableRepository _repo;

        public PromptVariablesController(IPromptVariableRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] Guid? promptId = null, [FromQuery] string? search = null, [FromQuery] string? sortBy = null, [FromQuery] string sortDir = "desc")
        {
            var desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            var (items, total) = await _repo.GetPagedAsync(page, pageSize, promptId, search, sortBy, desc);
            var dtos = items.Select(i => new PromptVariableDto
            {
                Id = i.Id,
                PromptId = i.PromptId,
                Name = i.Name,
                Placeholder = i.Placeholder,
                Description = i.Description,
                DataType = i.DataType,
                IsRequired = i.IsRequired,
                DefaultValue = i.DefaultValue,
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

            return Ok(new ApiResponse<IEnumerable<PromptVariableDto>> { Success = true, Data = dtos, Meta = meta });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return NotFound(new ApiResponse<string> { Success = false, Message = "Not found" });
            var dto = new PromptVariableDto
            {
                Id = item.Id,
                PromptId = item.PromptId,
                Name = item.Name,
                Placeholder = item.Placeholder,
                Description = item.Description,
                DataType = item.DataType,
                IsRequired = item.IsRequired,
                DefaultValue = item.DefaultValue,
                CreatedBy = item.CreatedBy,
                UpdatedBy = item.UpdatedBy,
                CreatedDate = item.CreatedDate,
                UpdatedDate = item.UpdatedDate
            };
            return Ok(new ApiResponse<PromptVariableDto> { Success = true, Data = dto });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreatePromptVariableRequest req)
        {
            var userEmail = User?.FindFirst(ClaimTypes.Email)?.Value ?? User?.Identity?.Name ?? "system";
            var entity = new PromptVariable
            {
                Id = Guid.NewGuid(),
                PromptId = req.PromptId,
                Name = req.Name,
                Placeholder = req.Placeholder,
                Description = req.Description,
                DataType = req.DataType,
                IsRequired = req.IsRequired,
                DefaultValue = req.DefaultValue,
                CreatedBy = userEmail,
                CreatedDate = DateTime.UtcNow
            };
            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();
            var dto = new PromptVariableDto
            {
                Id = entity.Id,
                PromptId = entity.PromptId,
                Name = entity.Name,
                Placeholder = entity.Placeholder,
                Description = entity.Description,
                DataType = entity.DataType,
                IsRequired = entity.IsRequired,
                DefaultValue = entity.DefaultValue,
                CreatedBy = entity.CreatedBy,
                CreatedDate = entity.CreatedDate
            };
            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, new ApiResponse<PromptVariableDto> { Success = true, Data = dto });
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePromptVariableRequest req)
        {
            if (id != req.Id) return BadRequest(new ApiResponse<string> { Success = false, Message = "Id mismatch" });
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound(new ApiResponse<string> { Success = false, Message = "Not found" });

            var userEmail = User?.FindFirst(ClaimTypes.Email)?.Value ?? User?.Identity?.Name ?? "system";

            existing.PromptId = req.PromptId;
            existing.Name = req.Name;
            existing.Placeholder = req.Placeholder;
            existing.Description = req.Description;
            existing.DataType = req.DataType;
            existing.IsRequired = req.IsRequired;
            existing.DefaultValue = req.DefaultValue;
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
