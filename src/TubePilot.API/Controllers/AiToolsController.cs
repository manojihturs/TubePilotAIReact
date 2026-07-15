using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TubePilot.Application.DTOs;
using TubePilot.Application.Interfaces;
using TubePilot.Domain.Entities;

namespace TubePilot.API.Controllers
{
    // User-defined AI text-generation tools (name + wire format + base URL + model + key +
    // priority). Replaces the old fixed Claude/Groq/Gemini/NVidia provider list — any number
    // of tools, any vendor that speaks one of the supported wire formats.
    [ApiController]
    [Route("api/ai-tools")]
    [Authorize]
    public class AiToolsController : ControllerBase
    {
        private readonly IAiToolRepository _repo;
        private readonly IApiKeyEncryptionService _encryption;

        public AiToolsController(IAiToolRepository repo, IApiKeyEncryptionService encryption)
        {
            _repo = repo;
            _encryption = encryption;
        }

        private Guid CurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(idClaim, out var id) ? id : Guid.Empty;
        }

        private static AiToolDto ToDto(AiTool t) => new()
        {
            Id = t.Id,
            Name = t.Name,
            ApiFormat = t.ApiFormat,
            BaseUrl = t.BaseUrl,
            Model = t.Model,
            Priority = t.Priority,
            IsEnabled = t.IsEnabled,
            LastUsedAt = t.LastUsedAt
        };

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var tools = await _repo.GetAllForUserAsync(CurrentUserId(), cancellationToken);
            return Ok(tools.Select(ToDto));
        }

        [HttpGet("formats")]
        public IActionResult GetFormats() => Ok(AiApiFormats.All);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAiToolRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.ApiFormat) ||
                string.IsNullOrWhiteSpace(request.BaseUrl) || string.IsNullOrWhiteSpace(request.Model) ||
                string.IsNullOrWhiteSpace(request.ApiKey))
            {
                return BadRequest(new { message = "Name, ApiFormat, BaseUrl, Model, and ApiKey are all required." });
            }

            if (!AiApiFormats.All.Contains(request.ApiFormat))
            {
                return BadRequest(new { message = $"Unknown ApiFormat '{request.ApiFormat}'. Valid values: {string.Join(", ", AiApiFormats.All)}" });
            }

            var entity = new AiTool
            {
                UserId = CurrentUserId(),
                Name = request.Name,
                ApiFormat = request.ApiFormat,
                BaseUrl = request.BaseUrl,
                Model = request.Model,
                EncryptedApiKey = _encryption.Encrypt(request.ApiKey),
                Priority = request.Priority,
                IsEnabled = true
            };

            await _repo.AddAsync(entity, cancellationToken);
            return Ok(ToDto(entity));
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAiToolRequest request, CancellationToken cancellationToken)
        {
            var existing = await _repo.GetAsync(CurrentUserId(), id, cancellationToken);
            if (existing == null) return NotFound();

            existing.Name = request.Name;
            existing.BaseUrl = request.BaseUrl;
            existing.Model = request.Model;
            existing.Priority = request.Priority;
            existing.IsEnabled = request.IsEnabled;
            if (!string.IsNullOrWhiteSpace(request.ApiKey))
            {
                existing.EncryptedApiKey = _encryption.Encrypt(request.ApiKey);
            }

            await _repo.UpdateAsync(existing, cancellationToken);
            return Ok(ToDto(existing));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var deleted = await _repo.DeleteAsync(CurrentUserId(), id, cancellationToken);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
