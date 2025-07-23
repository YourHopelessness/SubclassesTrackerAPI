using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SubclassesTracker.Api.Controllers
{
    /// <summary>
    /// Controller for health checks.
    /// </summary>
    [Route("api/[controller]")]
    public class HealthCheckController : ControllerBase
    {
        /// <summary>
        /// Returns a simple health check response.
        /// </summary>
        /// <returns>Health check response.</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Get()
        {
            return Ok(new { status = "Healthy" });
        }
    }
}
