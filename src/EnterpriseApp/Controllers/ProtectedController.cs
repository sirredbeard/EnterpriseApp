using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseApp.Controllers;

[ApiController]
[Route("protected")]
public class ProtectedController : ControllerBase
{
    // Create a protected endpoint that returns a secret message
    [HttpGet("secret")]
    [Authorize]
    public IActionResult GetSecret()
    {
        return Ok(new { secret = "HeroDevs", now = DateTime.UtcNow });
    }
}
