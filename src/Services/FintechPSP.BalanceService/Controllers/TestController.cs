using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FintechPSP.BalanceService.Controllers;

[ApiController]
[Route("test")]
[AllowAnonymous]
public class TestController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", controller = "TestController", timestamp = DateTime.UtcNow });
    }

    [HttpPost("post")]
    public IActionResult Post([FromBody] dynamic data)
    {
        return Ok(new { message = "POST funcionando", data = data, timestamp = DateTime.UtcNow });
    }

    [HttpPost("pix")]
    public IActionResult Pix([FromBody] dynamic data)
    {
        return Ok(new { message = "PIX endpoint funcionando", data = data, timestamp = DateTime.UtcNow });
    }
}
