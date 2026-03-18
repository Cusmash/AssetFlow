using Microsoft.AspNetCore.Mvc;

namespace AssetFlow.Api.Controllers
{
    [ApiController]
    [Route("api/health")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "Healthy",
                service = "AssetFlow.Api",
                utc = DateTime.UtcNow
            });
        }
    }
}
