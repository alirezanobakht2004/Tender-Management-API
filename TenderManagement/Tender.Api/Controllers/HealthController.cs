using Microsoft.AspNetCore.Mvc;

namespace Tender.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet] public IActionResult Get() => Ok("OK");
}

