using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitupProject.Controllers
{
    [ApiController]
    [Route("")]
    public class HealthController : ControllerBase
    {
        [AllowAnonymous]
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok("ok");
        }
    }
}
