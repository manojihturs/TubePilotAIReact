using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TubePilot.Application.DTOs;
using TubePilot.Application.Interfaces;
using TubePilot.Domain.Entities;

namespace TubePilot.API.Controllers
{
    [ApiController]
    [Route("api/user/api-keys")]
    [Authorize]
    public class UserApiKeysController : ControllerBase
    {
        private readonly IUserApiKeyRepository _repo;
        private readonly IApiKeyEncryptionService _encryption;

        public UserApiKeysController(IUserApiKeyRepository repo, IApiKeyEncryptionService encryption)
        {
            _repo = repo;
            _encryption = encryption;
        }

        private Guid CurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(idClaim, out var id) ? id : Guid.Empty;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = CurrentUserId();
            var keys = await _repo.GetAllForUserAsync(userId);

            var dtos = keys.Select(k => new UserApiKeyDto
            {
                ProviderName = k.ProviderName,
                Saved = true,
                LastUsedAt = k.LastUsedAt
            });

            return Ok(dtos);
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] SaveApiKeyRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ProviderName) || string.IsNullOrWhiteSpace(request.ApiKey))
            {
                return BadRequest(new { message = "ProviderName and ApiKey are required." });
            }

            var userId = CurrentUserId();
            var entity = new UserApiKey
            {
                UserId = userId,
                ProviderName = request.ProviderName,
                EncryptedKey = _encryption.Encrypt(request.ApiKey)
            };

            await _repo.UpsertAsync(entity);

            return Ok(new UserApiKeyDto { ProviderName = request.ProviderName, Saved = true });
        }

        [HttpDelete("{provider}")]
        public async Task<IActionResult> Delete(string provider)
        {
            var userId = CurrentUserId();
            var deleted = await _repo.DeleteAsync(userId, provider);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
