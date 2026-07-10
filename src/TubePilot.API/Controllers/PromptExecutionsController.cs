using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using TubePilot.Application.Interfaces;
using TubePilot.Domain.Entities;

namespace TubePilot.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PromptExecutionsController : ControllerBase
    {
        private readonly IPromptExecutionRepository _repo;

        public PromptExecutionsController(IPromptExecutionRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] Guid? promptId = null, [FromQuery] Guid? providerId = null, [FromQuery] Guid? modelId = null, [FromQuery] string status = null)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var (items, total) = await _repo.GetPagedAsync(page, pageSize, promptId, providerId, modelId, status);

            var dtos = items.Select(x => new
            {
                x.Id,
                x.PromptId,
                Prompt = x.Prompt != null ? new { x.Prompt.Id, x.Prompt.Name } : null,
                x.ProviderId,
                Provider = x.Provider != null ? new { x.Provider.Id, x.Provider.Name } : null,
                x.ModelId,
                Model = x.Model != null ? new { x.Model.Id, x.Model.Name } : null,
                x.RenderedPrompt,
                x.Response,
                x.InputTokens,
                x.OutputTokens,
                x.EstimatedCost,
                x.Latency,
                x.Status,
                x.CreatedDate,
                x.ExecutedBy
            }).ToList();

            return Ok(new { success = true, data = dtos, meta = new { page, pageSize, totalItems = total, totalPages = (int)Math.Ceiling((double)total / pageSize) } });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var e = await _repo.GetByIdAsync(id);
            if (e == null) return NotFound(new { success = false, message = "Not found" });

            var dto = new
            {
                e.Id,
                e.PromptId,
                Prompt = e.Prompt != null ? new { e.Prompt.Id, e.Prompt.Name } : null,
                e.ProviderId,
                Provider = e.Provider != null ? new { e.Provider.Id, e.Provider.Name } : null,
                e.ModelId,
                Model = e.Model != null ? new { e.Model.Id, e.Model.Name } : null,
                e.VariablesJson,
                e.RenderedPrompt,
                e.Response,
                e.InputTokens,
                e.OutputTokens,
                e.EstimatedCost,
                e.Latency,
                e.Status,
                e.CreatedDate,
                e.ExecutedBy
            };

            return Ok(new { success = true, data = dto });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PromptExecution req)
        {
            if (req == null) return BadRequest(new { success = false, message = "Invalid" });

            req.Id = Guid.NewGuid();
            req.CreatedDate = DateTime.UtcNow;

            var created = await _repo.AddAsync(req);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, new { success = true, data = new { created.Id } });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PromptExecution req)
        {
            if (req == null || id != req.Id) return BadRequest(new { success = false, message = "Invalid" });

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound(new { success = false, message = "Not found" });

            // Update allowed fields
            existing.ProviderId = req.ProviderId;
            existing.ModelId = req.ModelId;
            existing.VariablesJson = req.VariablesJson;
            existing.RenderedPrompt = req.RenderedPrompt;
            existing.Response = req.Response;
            existing.InputTokens = req.InputTokens;
            existing.OutputTokens = req.OutputTokens;
            existing.EstimatedCost = req.EstimatedCost;
            existing.Latency = req.Latency;
            existing.Status = req.Status;
            existing.ExecutedBy = req.ExecutedBy;

            var updated = await _repo.UpdateAsync(existing);

            return Ok(new { success = true, data = new { updated.Id } });
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
