using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TubePilot.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SampleController : ControllerBase
    {
        [HttpGet("public")]
        public IActionResult Public() => Ok(new { message = "Public endpoint" });

        [HttpGet("protected")]
        [Authorize]
        public IActionResult Protected() => Ok(new { message = "Protected endpoint - authenticated" });

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminOnly() => Ok(new { message = "Admin only endpoint" });
    }
}
