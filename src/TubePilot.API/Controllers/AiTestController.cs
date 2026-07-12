using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TubePilot.Application.DTOs;
using TubePilot.Application.Interfaces;

namespace TubePilot.API.Controllers
{
    // Throwaway endpoint proving the IAiProvider round trip works end-to-end.
    // Superseded by the OutputSpec-driven /api/generate endpoint in a later sprint.
    [ApiController]
    [Route("api/ai")]
    [Authorize]
    public class AiTestController : ControllerBase
    {
        private readonly IUserApiKeyRepository _apiKeyRepo;
        private readonly IApiKeyEncryptionService _encryption;
        private readonly IAiProviderFactory _providerFactory;

        public AiTestController(
            IUserApiKeyRepository apiKeyRepo,
            IApiKeyEncryptionService encryption,
            IAiProviderFactory providerFactory)
        {
            _apiKeyRepo = apiKeyRepo;
            _encryption = encryption;
            _providerFactory = providerFactory;
        }

        [HttpPost("test-generate")]
        public async Task<IActionResult> TestGenerate([FromBody] TestGenerateRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest(new { message = "Prompt is required." });
            }

            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(idClaim, out var userId)) return Unauthorized();

            var savedKey = await _apiKeyRepo.GetAsync(userId, request.ProviderName, cancellationToken);
            if (savedKey == null)
            {
                return BadRequest(new { message = $"No saved API key for provider '{request.ProviderName}'. Save one via POST /api/user/api-keys first." });
            }

            var apiKey = _encryption.Decrypt(savedKey.EncryptedKey);
            var provider = _providerFactory.GetProvider(request.ProviderName);

            var systemPrompt = string.IsNullOrWhiteSpace(request.Title)
                ? null
                : $"You are generating content for a piece titled \"{request.Title}\". Keep the output aligned with this title.";

            var aiRequest = new AiRequest(
                SystemPrompt: systemPrompt,
                UserPrompt: request.Prompt,
                Model: request.Model ?? string.Empty,
                Temperature: 0.7f,
                MaxTokens: 1024
            );

            try
            {
                var result = await provider.GenerateAsync(apiKey, aiRequest, cancellationToken);
                await _apiKeyRepo.TouchLastUsedAsync(userId, request.ProviderName, cancellationToken);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
            }
        }
    }
}
