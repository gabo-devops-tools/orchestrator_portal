using Microsoft.AspNetCore.Mvc;

namespace orchestrator_portal.Controllers
{
    [ApiController]
    public class EndpointsController : ControllerBase
    {
        [HttpGet("/api/health")]
        public IActionResult Health()
        {
            return Ok("API is alive");
        }
    }
}
