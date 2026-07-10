using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using TubePilot.Application.DTOs;
using TubePilot.Application.Interfaces;
using TubePilot.Domain.Entities;

namespace TubePilot.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIModelsController : ControllerBase
    {
        private readonly IAIModelRepository _repo;

        public AIModelsController(IAIModelRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string search = null, [FromQuery] string sortBy = null, [FromQuery] string sortDir = null, [FromQuery] Guid? providerId = null, [FromQuery] bool? isEnabled = null)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var (items, total) = await _repo.GetPagedAsync(page, pageSize, search, sortBy, sortDir, providerId, isEnabled);

            var dtos = items.Select(x => new AIModelDto
            {
                Id = x.Id,
                ProviderId = x.ProviderId,
                Name = x.Name,
                DisplayName = x.DisplayName,
                ContextWindow = x.ContextWindow,
                MaxInputTokens = x.MaxInputTokens,
                MaxOutputTokens = x.MaxOutputTokens,
                SupportsVision = x.SupportsVision,
                SupportsAudio = x.SupportsAudio,
                SupportsVideo = x.SupportsVideo,
                SupportsTools = x.SupportsTools,
                SupportsStreaming = x.SupportsStreaming,
                SupportsReasoning = x.SupportsReasoning,
                SupportsJSON = x.SupportsJSON,
                SupportsImageGeneration = x.SupportsImageGeneration,
                InputPrice = x.InputPrice,
                OutputPrice = x.OutputPrice,
                IsDefault = x.IsDefault,
                IsEnabled = x.IsEnabled,
                CreatedDate = x.CreatedDate,
                UpdatedDate = x.UpdatedDate
            }).ToList();

            return Ok(new { success = true, data = dtos, meta = new { page, pageSize, totalItems = total, totalPages = (int)Math.Ceiling((double)total / pageSize) } });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var e = await _repo.GetByIdAsync(id);
            if (e == null) return NotFound(new { success = false, message = "Not found" });

            var dto = new AIModelDto
            {
                Id = e.Id,
                ProviderId = e.ProviderId,
                Name = e.Name,
                DisplayName = e.DisplayName,
                ContextWindow = e.ContextWindow,
                MaxInputTokens = e.MaxInputTokens,
                MaxOutputTokens = e.MaxOutputTokens,
                SupportsVision = e.SupportsVision,
                SupportsAudio = e.SupportsAudio,
                SupportsVideo = e.SupportsVideo,
                SupportsTools = e.SupportsTools,
                SupportsStreaming = e.SupportsStreaming,
                SupportsReasoning = e.SupportsReasoning,
                SupportsJSON = e.SupportsJSON,
                SupportsImageGeneration = e.SupportsImageGeneration,
                InputPrice = e.InputPrice,
                OutputPrice = e.OutputPrice,
                IsDefault = e.IsDefault,
                IsEnabled = e.IsEnabled,
                CreatedDate = e.CreatedDate,
                UpdatedDate = e.UpdatedDate
            };

            return Ok(new { success = true, data = dto });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAIModelRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(new { success = false, message = "Invalid" });

            if (await _repo.ExistsByNameForProviderAsync(request.ProviderId, request.Name))
            {
                return Conflict(new { success = false, message = "Model with same name already exists for provider" });
            }

            var entity = new AIModel
            {
                Id = Guid.NewGuid(),
                ProviderId = request.ProviderId,
                Name = request.Name,
                DisplayName = request.DisplayName,
                ContextWindow = request.ContextWindow,
                MaxInputTokens = request.MaxInputTokens,
                MaxOutputTokens = request.MaxOutputTokens,
                SupportsVision = request.SupportsVision,
                SupportsAudio = request.SupportsAudio,
                SupportsVideo = request.SupportsVideo,
                SupportsTools = request.SupportsTools,
                SupportsStreaming = request.SupportsStreaming,
                SupportsReasoning = request.SupportsReasoning,
                SupportsJSON = request.SupportsJSON,
                SupportsImageGeneration = request.SupportsImageGeneration,
                InputPrice = request.InputPrice,
                OutputPrice = request.OutputPrice,
                IsDefault = request.IsDefault,
                IsEnabled = request.IsEnabled,
                CreatedDate = DateTime.UtcNow
            };

            var created = await _repo.AddAsync(entity);

            var dto = new AIModelDto
            {
                Id = created.Id,
                ProviderId = created.ProviderId,
                Name = created.Name,
                DisplayName = created.DisplayName,
                ContextWindow = created.ContextWindow,
                MaxInputTokens = created.MaxInputTokens,
                MaxOutputTokens = created.MaxOutputTokens,
                SupportsVision = created.SupportsVision,
                SupportsAudio = created.SupportsAudio,
                SupportsVideo = created.SupportsVideo,
                SupportsTools = created.SupportsTools,
                SupportsStreaming = created.SupportsStreaming,
                SupportsReasoning = created.SupportsReasoning,
                SupportsJSON = created.SupportsJSON,
                SupportsImageGeneration = created.SupportsImageGeneration,
                InputPrice = created.InputPrice,
                OutputPrice = created.OutputPrice,
                IsDefault = created.IsDefault,
                IsEnabled = created.IsEnabled,
                CreatedDate = created.CreatedDate
            };

            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, new { success = true, data = dto });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAIModelRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(new { success = false, message = "Invalid" });
            if (id != request.Id) return BadRequest(new { success = false, message = "Id mismatch" });

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound(new { success = false, message = "Not found" });

            if (await _repo.ExistsByNameForProviderAsync(request.ProviderId, request.Name, request.Id))
            {
                return Conflict(new { success = false, message = "Model with same name already exists for provider" });
            }

            existing.ProviderId = request.ProviderId;
            existing.Name = request.Name;
            existing.DisplayName = request.DisplayName;
            existing.ContextWindow = request.ContextWindow;
            existing.MaxInputTokens = request.MaxInputTokens;
            existing.MaxOutputTokens = request.MaxOutputTokens;
            existing.SupportsVision = request.SupportsVision;
            existing.SupportsAudio = request.SupportsAudio;
            existing.SupportsVideo = request.SupportsVideo;
            existing.SupportsTools = request.SupportsTools;
            existing.SupportsStreaming = request.SupportsStreaming;
            existing.SupportsReasoning = request.SupportsReasoning;
            existing.SupportsJSON = request.SupportsJSON;
            existing.SupportsImageGeneration = request.SupportsImageGeneration;
            existing.InputPrice = request.InputPrice;
            existing.OutputPrice = request.OutputPrice;
            existing.IsDefault = request.IsDefault;
            existing.IsEnabled = request.IsEnabled;
            existing.UpdatedDate = DateTime.UtcNow;

            var updated = await _repo.UpdateAsync(existing);

            var dto = new AIModelDto
            {
                Id = updated.Id,
                ProviderId = updated.ProviderId,
                Name = updated.Name,
                DisplayName = updated.DisplayName,
                ContextWindow = updated.ContextWindow,
                MaxInputTokens = updated.MaxInputTokens,
                MaxOutputTokens = updated.MaxOutputTokens,
                SupportsVision = updated.SupportsVision,
                SupportsAudio = updated.SupportsAudio,
                SupportsVideo = updated.SupportsVideo,
                SupportsTools = updated.SupportsTools,
                SupportsStreaming = updated.SupportsStreaming,
                SupportsReasoning = updated.SupportsReasoning,
                SupportsJSON = updated.SupportsJSON,
                SupportsImageGeneration = updated.SupportsImageGeneration,
                InputPrice = updated.InputPrice,
                OutputPrice = updated.OutputPrice,
                IsDefault = updated.IsDefault,
                IsEnabled = updated.IsEnabled,
                CreatedDate = updated.CreatedDate,
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
