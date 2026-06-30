using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TubePilotAI.Application.DTOs;
using TubePilotAI.Application.Interfaces;

namespace TubePilotAIReact.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectGeneratorController : ControllerBase
    {
        private readonly IProjectGeneratorService _generator;

        public ProjectGeneratorController(IProjectGeneratorService generator)
        {
            _generator = generator;
        }

        [HttpPost("generate")]
        public async Task<ActionResult<GenerateProjectResponseDto>> Generate([FromBody] GenerateProjectRequestDto request)
        {
            var resp = await _generator.EnqueueGenerationAsync(request);
            return Ok(resp);
        }

        [HttpGet("{projectId}/status")]
        public async Task<ActionResult<GenerationStatusDto>> Status(string projectId)
        {
            var status = await _generator.GetStatusAsync(projectId);
            if (status == null) return NotFound();
            return Ok(status);
        }
    }
}
