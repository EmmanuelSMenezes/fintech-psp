using Microsoft.AspNetCore.Mvc;

namespace FintechPSP.BalanceService.Controllers;

[ApiController]
[Route("simple")]
public class SimpleController : ControllerBase
{
    [HttpGet("get")]
    public IActionResult Get()
    {
        return Ok(new { message = "GET funcionando", timestamp = DateTime.UtcNow });
    }

    [HttpPost("post")]
    public IActionResult Post()
    {
        return Ok(new { message = "POST funcionando", timestamp = DateTime.UtcNow });
    }

    [HttpPut("put")]
    public IActionResult Put()
    {
        return Ok(new { message = "PUT funcionando", timestamp = DateTime.UtcNow });
    }

    [HttpDelete("delete")]
    public IActionResult Delete()
    {
        return Ok(new { message = "DELETE funcionando", timestamp = DateTime.UtcNow });
    }
}
