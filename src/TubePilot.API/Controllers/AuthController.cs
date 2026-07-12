using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TubePilot.Application.DTOs;
using TubePilot.Application.Interfaces;
using TubePilot.Domain.Entities;
using TubePilot.API.Models;

namespace TubePilot.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtSettings _jwtSettings;

        public AuthController(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AuthRequest req)
        {
            var existing = await _userRepository.GetByEmailAsync(req.Email);
            if (existing != null)
            {
                return Conflict(new { message = "A user with this email already exists" });
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = req.Email,
                Password = req.Password,
                Role = "User"
            };
            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return CreatedAtAction(nameof(Me), null, new UserDto { Id = user.Id, Email = user.Email, Role = user.Role });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthRequest req)
        {
            var user = await _userRepository.GetByEmailAsync(req.Email);
            if (user == null || user.Password != req.Password)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
            };
            if (!string.IsNullOrEmpty(user.Role))
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);

            return Ok(new AuthResponse { Token = jwt, ExpiresAt = tokenDescriptor.Expires!.Value });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(idClaim, out var id)) return Unauthorized();
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(new UserDto { Id = user.Id, Email = user.Email, Role = user.Role });
        }
    }
}
