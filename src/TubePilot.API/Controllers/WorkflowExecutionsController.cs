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
    public class WorkflowExecutionsController : ControllerBase
    {
        private readonly IWorkflowExecutionRepository _repo;

        public WorkflowExecutionsController(IWorkflowExecutionRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] Guid? workflowId = null, [FromQuery] string status = null)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 200);
            var (items, total) = await _repo.GetPagedAsync(page, pageSize, workflowId, status);
            var dtos = items.Select(x => new { x.Id, x.WorkflowId, Workflow = x.Workflow != null ? new { x.Workflow.Id, x.Workflow.Name } : null, x.CurrentStep, x.Status, x.StartedAt, x.CompletedAt, x.StartedBy }).ToList();
            return Ok(new { success = true, data = dtos, meta = new { page, pageSize, totalItems = total, totalPages = (int)Math.Ceiling((double)total / pageSize) } });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var e = await _repo.GetByIdAsync(id);
            if (e == null) return NotFound(new { success = false, message = "Not found" });
            var dto = new { e.Id, e.WorkflowId, Workflow = e.Workflow != null ? new { e.Workflow.Id, e.Workflow.Name } : null, e.CurrentStep, e.Status, e.InputJson, e.ResultJson, e.StartedAt, e.CompletedAt, e.StartedBy };
            return Ok(new { success = true, data = dto });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] WorkflowExecution req)
        {
            if (req == null) return BadRequest(new { success = false });
            var created = await _repo.AddAsync(req);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, new { success = true, data = new { created.Id } });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] WorkflowExecution req)
        {
            if (req == null || id != req.Id) return BadRequest(new { success = false });
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound(new { success = false });

            existing.CurrentStep = req.CurrentStep;
            existing.Status = req.Status;
            existing.InputJson = req.InputJson;
            existing.ResultJson = req.ResultJson;
            existing.CompletedAt = req.CompletedAt;

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
