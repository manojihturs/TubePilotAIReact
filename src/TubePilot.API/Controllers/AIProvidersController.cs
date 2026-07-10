using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TubePilot.Application.DTOs;
using TubePilot.Application.Interfaces;
using TubePilot.Domain.Entities;
using System.Linq;
using System.Collections.Generic;

namespace TubePilot.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIProvidersController : ControllerBase
    {
        private readonly IAIProviderRepository _repo;

        public AIProvidersController(IAIProviderRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string search = null, [FromQuery] string sortBy = null, [FromQuery] string sortDir = null, [FromQuery] bool? isEnabled = null)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var (items, total) = await _repo.GetPagedAsync(page, pageSize, search, sortBy, sortDir, isEnabled);

            var dtos = items.Select(x => new AIProviderDto
            {
                Id = x.Id,
                Name = x.Name,
                DisplayName = x.DisplayName,
                ProviderType = x.ProviderType,
                BaseUrl = x.BaseUrl,
                OrganizationId = x.OrganizationId,
                DefaultModel = x.DefaultModel,
                SupportsVision = x.SupportsVision,
                SupportsImageGeneration = x.SupportsImageGeneration,
                SupportsStreaming = x.SupportsStreaming,
                SupportsThinking = x.SupportsThinking,
                SupportsJSONMode = x.SupportsJSONMode,
                SupportsFunctionCalling = x.SupportsFunctionCalling,
                DailyLimit = x.DailyLimit,
                MonthlyLimit = x.MonthlyLimit,
                Priority = x.Priority,
                IsEnabled = x.IsEnabled,
                CreatedBy = x.CreatedBy,
                CreatedDate = x.CreatedDate,
                UpdatedBy = x.UpdatedBy,
                UpdatedDate = x.UpdatedDate
            }).ToList();

            var result = new
            {
                success = true,
                data = dtos,
                meta = new { page, pageSize, totalItems = total, totalPages = (int)Math.Ceiling((double)total / pageSize) }
            };

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return NotFound(new { success = false, message = "Not found" });

            var dto = new AIProviderDto
            {
                Id = entity.Id,
                Name = entity.Name,
                DisplayName = entity.DisplayName,
                ProviderType = entity.ProviderType,
                BaseUrl = entity.BaseUrl,
                OrganizationId = entity.OrganizationId,
                DefaultModel = entity.DefaultModel,
                SupportsVision = entity.SupportsVision,
                SupportsImageGeneration = entity.SupportsImageGeneration,
                SupportsStreaming = entity.SupportsStreaming,
                SupportsThinking = entity.SupportsThinking,
                SupportsJSONMode = entity.SupportsJSONMode,
                SupportsFunctionCalling = entity.SupportsFunctionCalling,
                DailyLimit = entity.DailyLimit,
                MonthlyLimit = entity.MonthlyLimit,
                Priority = entity.Priority,
                IsEnabled = entity.IsEnabled,
                CreatedBy = entity.CreatedBy,
                CreatedDate = entity.CreatedDate,
                UpdatedBy = entity.UpdatedBy,
                UpdatedDate = entity.UpdatedDate
            };

            return Ok(new { success = true, data = dto });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAIProviderRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(new { success = false, message = "Invalid" });

            // uniqueness check
            if (await _repo.ExistsByNameAsync(request.Name))
            {
                return Conflict(new { success = false, message = "Provider with same name already exists" });
            }

            var entity = new AIProvider
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                DisplayName = request.DisplayName,
                ProviderType = request.ProviderType,
                BaseUrl = request.BaseUrl,
                ApiKeyEncrypted = request.ApiKey, // encryption to be applied elsewhere
                OrganizationId = request.OrganizationId,
                DefaultModel = request.DefaultModel,
                SupportsVision = request.SupportsVision,
                SupportsImageGeneration = request.SupportsImageGeneration,
                SupportsStreaming = request.SupportsStreaming,
                SupportsThinking = request.SupportsThinking,
                SupportsJSONMode = request.SupportsJSONMode,
                SupportsFunctionCalling = request.SupportsFunctionCalling,
                DailyLimit = request.DailyLimit,
                MonthlyLimit = request.MonthlyLimit,
                Priority = request.Priority,
                IsEnabled = request.IsEnabled,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow,
                SoftDelete = false
            };

            var created = await _repo.AddAsync(entity);

            var dto = new AIProviderDto
            {
                Id = created.Id,
                Name = created.Name,
                DisplayName = created.DisplayName,
                ProviderType = created.ProviderType,
                BaseUrl = created.BaseUrl,
                OrganizationId = created.OrganizationId,
                DefaultModel = created.DefaultModel,
                SupportsVision = created.SupportsVision,
                SupportsImageGeneration = created.SupportsImageGeneration,
                SupportsStreaming = created.SupportsStreaming,
                SupportsThinking = created.SupportsThinking,
                SupportsJSONMode = created.SupportsJSONMode,
                SupportsFunctionCalling = created.SupportsFunctionCalling,
                DailyLimit = created.DailyLimit,
                MonthlyLimit = created.MonthlyLimit,
                Priority = created.Priority,
                IsEnabled = created.IsEnabled,
                CreatedBy = created.CreatedBy,
                CreatedDate = created.CreatedDate
            };

            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, new { success = true, data = dto });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAIProviderRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(new { success = false, message = "Invalid" });
            if (id != request.Id) return BadRequest(new { success = false, message = "Id mismatch" });

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound(new { success = false, message = "Not found" });

            if (await _repo.ExistsByNameAsync(request.Name, request.Id))
            {
                return Conflict(new { success = false, message = "Provider with same name already exists" });
            }

            existing.Name = request.Name;
            existing.DisplayName = request.DisplayName;
            existing.ProviderType = request.ProviderType;
            existing.BaseUrl = request.BaseUrl;
            if (!string.IsNullOrWhiteSpace(request.ApiKey)) existing.ApiKeyEncrypted = request.ApiKey; // encryption elsewhere
            existing.OrganizationId = request.OrganizationId;
            existing.DefaultModel = request.DefaultModel;
            existing.SupportsVision = request.SupportsVision;
            existing.SupportsImageGeneration = request.SupportsImageGeneration;
            existing.SupportsStreaming = request.SupportsStreaming;
            existing.SupportsThinking = request.SupportsThinking;
            existing.SupportsJSONMode = request.SupportsJSONMode;
            existing.SupportsFunctionCalling = request.SupportsFunctionCalling;
            existing.DailyLimit = request.DailyLimit;
            existing.MonthlyLimit = request.MonthlyLimit;
            existing.Priority = request.Priority;
            existing.IsEnabled = request.IsEnabled;
            existing.UpdatedBy = "system";
            existing.UpdatedDate = DateTime.UtcNow;

            var updated = await _repo.UpdateAsync(existing);

            var dto = new AIProviderDto
            {
                Id = updated.Id,
                Name = updated.Name,
                DisplayName = updated.DisplayName,
                ProviderType = updated.ProviderType,
                BaseUrl = updated.BaseUrl,
                OrganizationId = updated.OrganizationId,
                DefaultModel = updated.DefaultModel,
                SupportsVision = updated.SupportsVision,
                SupportsImageGeneration = updated.SupportsImageGeneration,
                SupportsStreaming = updated.SupportsStreaming,
                SupportsThinking = updated.SupportsThinking,
                SupportsJSONMode = updated.SupportsJSONMode,
                SupportsFunctionCalling = updated.SupportsFunctionCalling,
                DailyLimit = updated.DailyLimit,
                MonthlyLimit = updated.MonthlyLimit,
                Priority = updated.Priority,
                IsEnabled = updated.IsEnabled,
                CreatedBy = updated.CreatedBy,
                CreatedDate = updated.CreatedDate,
                UpdatedBy = updated.UpdatedBy,
                UpdatedDate = updated.UpdatedDate
            };

            return Ok(new { success = true, data = dto });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound(new { success = false, message = "Not found" });

            await _repo.DeleteAsync(existing);
            return NoContent();
        }
    }
}
