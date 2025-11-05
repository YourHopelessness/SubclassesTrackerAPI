using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubclassesTracker.Database.Context;

namespace SubclassesTracker.Api.Controllers
{
    /// <summary>
    /// Controller for health checks.
    /// </summary>
    [Route("api/[controller]")]
    public class HealthCheckController : ControllerBase
    {
        [HttpGet("health")]
        [AllowAnonymous]
        public async Task<IActionResult> HealthCheck(
            [FromServices] EsoContext db,
            [FromServices] ParquetCacheContext parquetDb,
            CancellationToken token)
        {
            try
            {
                await db.Database.ExecuteSqlRawAsync("SELECT 1", token);
                await parquetDb.Database.ExecuteSqlRawAsync("SELECT 1", token);

                return Ok("Healthy");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Unhealthy: {ex.Message}");
            }
        }
    }
}
