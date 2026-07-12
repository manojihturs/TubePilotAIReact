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
        private readonly IPromptVersionRepository _versionRepo;

        public PromptsController(IPromptRepository repo, IPromptVersionRepository versionRepo)
        {
            _repo = repo;
            _versionRepo = versionRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] Guid? categoryId = null, [FromQuery] string? search = null, [FromQuery] string? sortBy = null, [FromQuery] string sortDir = "desc")
        {
            var desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            var (items, total) = await _repo.GetPagedAsync(page, pageSize, categoryId, search, sortBy, desc);
            var dtos = items.Select(ToDto).ToList();

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
            return Ok(new ApiResponse<PromptDto> { Success = true, Data = ToDto(item) });
        }

        [HttpGet("{id:guid}/versions")]
        public async Task<IActionResult> GetVersions(Guid id)
        {
            var versions = await _versionRepo.GetForPromptAsync(id);
            var dtos = versions.Select(v => new PromptVersionDto
            {
                Id = v.Id,
                VersionNumber = v.VersionNumber,
                TemplateText = v.TemplateText,
                OutputSpecJson = v.OutputSpecJson,
                CreatedAt = v.CreatedAt
            });
            return Ok(new ApiResponse<IEnumerable<PromptVersionDto>> { Success = true, Data = dtos });
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
                OutputSpecJson = req.OutputSpecJson,
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

            // First version, append-only from day one.
            await _versionRepo.AddAsync(new PromptVersion
            {
                PromptId = entity.Id,
                VersionNumber = 1,
                TemplateText = entity.PromptText,
                OutputSpecJson = entity.OutputSpecJson
            });

            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, new ApiResponse<PromptDto> { Success = true, Data = ToDto(entity) });
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePromptRequest req)
        {
            if (id != req.Id) return BadRequest(new ApiResponse<string> { Success = false, Message = "Id mismatch" });
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound(new ApiResponse<string> { Success = false, Message = "Not found" });

            var userEmail = User?.FindFirst(ClaimTypes.Email)?.Value ?? User?.Identity?.Name ?? "system";

            var contentChanged = existing.PromptText != req.PromptText || existing.OutputSpecJson != req.OutputSpecJson;

            existing.CategoryId = req.CategoryId;
            existing.Name = req.Name;
            existing.Description = req.Description;
            existing.PromptText = req.PromptText;
            existing.OutputSpecJson = req.OutputSpecJson;
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

            // The prompt text or its declared output shape changed — that's as significant
            // a change as the text itself, so it gets its own version, never overwritten in place.
            if (contentChanged)
            {
                var nextVersion = await _versionRepo.GetLatestVersionNumberAsync(id) + 1;
                await _versionRepo.AddAsync(new PromptVersion
                {
                    PromptId = id,
                    VersionNumber = nextVersion,
                    TemplateText = existing.PromptText,
                    OutputSpecJson = existing.OutputSpecJson
                });
            }

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

        private static PromptDto ToDto(Prompt i) => new()
        {
            Id = i.Id,
            CategoryId = i.CategoryId,
            Name = i.Name,
            Description = i.Description,
            PromptText = i.PromptText,
            OutputSpecJson = i.OutputSpecJson,
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
        };
    }
}
