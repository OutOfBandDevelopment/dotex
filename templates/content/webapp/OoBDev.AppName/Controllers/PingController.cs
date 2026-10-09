using Microsoft.AspNetCore.Mvc;

namespace OoBDev.AppName.Controllers;

/// <summary>
/// Minimal endpoint to prove the composition root works.
/// </summary>
[ApiController]
[Route("[controller]")]
public class PingController : ControllerBase
{
    /// <summary>
    /// Returns a static response.
    /// </summary>
    /// <returns>The text "pong".</returns>
    [HttpGet]
    public IActionResult Get() => Ok("pong");
}
