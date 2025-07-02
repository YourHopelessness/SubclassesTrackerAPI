using Microsoft.AspNetCore.Mvc;
using SubclassesTrackerExtension.EsologsServices;

namespace SubclassesTrackerExtension.Controllers
{
    [Route("api/[controller]")]
    public class SkillController(
        IGetDataService getDataService) : ControllerBase
    {
        /// <summary>
        /// Retrieves fights for a specific log ID.
        /// </summary>
        /// <param name="logId"></param>
        /// <returns></returns>
        [HttpGet("getFights")]
        public async Task<IActionResult> GetFigthsAsync([FromQuery] string logId)
        {
            return Ok(await getDataService.GetFigthsAsync(logId));
        }

        /// <summary>
        /// Retrieves players for a specific log ID and their associated fights.
        /// </summary>
        /// <param name="logId"></param>
        /// <returns></returns>
        [HttpGet("getPlayers")]
        public async Task<IActionResult> GetPlayersAsync([FromQuery] string logId)
        {
            var fightsIds = await getDataService.GetFigthsAsync(logId);
            if (fightsIds == null || !fightsIds.Any())
            {
                return NotFound("No fights found for the provided log ID.");
            }
            var players = await getDataService.GetPlayersAsync(logId, fightsIds);
            return Ok(players);
        }

        /// <summary>
        /// Retrieves buffs for a specific player in a given log ID and fight IDs.
        /// </summary>
        /// <param name="logId"></param>
        /// <param name="playerId"></param>
        /// <returns></returns>
        [HttpGet("getPlayerBuffs")]
        public async Task<IActionResult> GetPlayerBuffsAsync([FromQuery] string logId, [FromQuery] int playerId)
        {
            var fightsIds = await getDataService.GetFigthsAsync(logId);
            if (fightsIds == null || !fightsIds.Any())
            {
                return NotFound("No fights found for the provided log ID.");
            }
            var buffs = await getDataService.GetPlayerBuffsAsync(logId, playerId, fightsIds);
            return Ok(buffs);
        }

        /// <summary>
        /// Retrieves all reports and their associated fights for a specific zone and difficulty.
        /// </summary>
        /// <returns></returns>
        [HttpGet("getAllReports")]
        public async Task<IActionResult> GetAllReports()
        {
            var allReports = await getDataService.GetAllReportsAndFights();

            return Ok(allReports);
        }
    }
}
