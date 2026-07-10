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
    public class WorkflowsController : ControllerBase
    {
        private readonly IWorkflowRepository _repo;

        public WorkflowsController(IWorkflowRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string search = null)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 200);
            var (items, total) = await _repo.GetPagedAsync(page, pageSize, search);
            var dtos = items.Select(x => new { x.Id, x.Name, x.Description, x.IsEnabled, x.CreatedDate, x.UpdatedDate }).ToList();
            return Ok(new { success = true, data = dtos, meta = new { page, pageSize, totalItems = total, totalPages = (int)Math.Ceiling((double)total / pageSize) } });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var w = await _repo.GetByIdAsync(id);
            if (w == null) return NotFound(new { success = false, message = "Not found" });
            var dto = new { w.Id, w.Name, w.Description, w.IsEnabled, Steps = w.Steps.OrderBy(s => s.Order).Select(s => new { s.Id, s.Name, s.Type, s.Order, s.ConfigJson }) };
            return Ok(new { success = true, data = dto });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Workflow req)
        {
            if (req == null) return BadRequest(new { success = false });
            var created = await _repo.AddAsync(req);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, new { success = true, data = new { created.Id } });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Workflow req)
        {
            if (req == null || id != req.Id) return BadRequest(new { success = false });
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound(new { success = false });
            existing.Name = req.Name;
            existing.Description = req.Description;
            existing.IsEnabled = req.IsEnabled;
            existing.Steps = req.Steps;
            var updated = await _repo.UpdateAsync(existing);
            return Ok(new { success = true, data = new { updated.Id } });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound(new { success = false });
            await _repo.DeleteAsync(existing);
            return NoContent();
        }
    }
}
