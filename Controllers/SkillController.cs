using Microsoft.AspNetCore.Mvc;
using SubclassesTrackerExtension.EsologsServices;

namespace SubclassesTrackerExtension.Controllers
{
    [Route("api/[controller]")]
    public class SkillController(
        IGetDataService getDataService,
        IReportDataService reportDataService) : ControllerBase
    {
        [HttpGet("getFights")]
        public async Task<IActionResult> GetFigthsAsync([FromQuery] string logId)
        {
            return Ok(await getDataService.GetFigthsAsync(logId));
        }

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

        [HttpGet("getAllReports")]
        public async Task<IActionResult> GetAllReports()
        {
            var allReports = await getDataService.GetAllReportsAndFights();

            return Ok(allReports);
        }

        [HttpGet("getSkillLines")]
        public async Task<IActionResult> GetSkillLines()
        {
            var skillLines = await reportDataService.GetSkillLinesAsync(19);

            return Ok(skillLines);
        }
    }
}
