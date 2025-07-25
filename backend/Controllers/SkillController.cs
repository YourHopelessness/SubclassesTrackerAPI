﻿using Microsoft.AspNetCore.Mvc;
using SubclassesTracker.Api.EsologsServices;
using SubclassesTracker.Api.EsologsServices.Reports;

namespace SubclassesTracker.Api.Controllers
{
    [Route("api/[controller]")]
    public class SkillController(
        IGetDataService getDataService,
        IReportDataService reportDataService) : ControllerBase
    {
        /// <summary>
        /// Retrieves fights for a specific log ID.
        /// </summary>
        /// <param name="logId"></param>
        /// <returns></returns>
        [HttpGet("getFights")]
        public async Task<IActionResult> GetFigthsAsync(
            [FromQuery] string logId,
            CancellationToken token = default)
        {
            return Ok(await getDataService.GetFigthsAsync(logId, token));
        }

        /// <summary>
        /// Retrieves players for a specific log ID and their associated fights.
        /// </summary>
        /// <param name="logId"></param>
        /// <returns></returns>
        [HttpGet("getPlayers")]
        public async Task<IActionResult> GetPlayersAsync(
            [FromQuery] string logId,
            CancellationToken token = default)
        {
            var fightsIds = await getDataService.GetFigthsAsync(logId);
            if (fightsIds == null || fightsIds.Count == 0)
            {
                return NotFound("No fights found for the provided log ID.");
            }
            var players = await getDataService.GetPlayersAsync(
                logId, [.. fightsIds.Select(x => x.Id)], token);

            return Ok(players);
        }

        /// <summary>
        /// Retrieves buffs for a specific player in a given log ID and fight IDs.
        /// </summary>
        /// <param name="logId"></param>
        /// <param name="playerId"></param>
        /// <returns></returns>
        [HttpGet("getPlayerBuffs")]
        public async Task<IActionResult> GetPlayerBuffsAsync(
            [FromQuery] string logId, 
            [FromQuery] int playerId,
            CancellationToken token = default)
        {
            var fightsIds = await getDataService.GetFigthsAsync(logId, token);
            if (fightsIds == null || fightsIds.Count == 0)
            {
                return NotFound("No fights found for the provided log ID.");
            }
            var buffs = await getDataService.GetPlayerBuffsAsync(
                logId, playerId, [.. fightsIds.Select(x => x.Id)], token);

            return Ok(buffs);
        }

        /// <summary>
        /// Get all players skill lines by specified logId
        /// </summary>
        /// <param name="logId">Specified log</param>
        /// <param name="fightId">Fight id</param>
        /// <param name="bossId">Boss id</param>
        /// <returns>List of all players lines</returns>
        [HttpGet("getPlayersLines")]
        public async Task<IActionResult> GetPlayersLinesAsync(
            [FromQuery] string logId,
            [FromQuery] string? fightId = null,
            [FromQuery] int? bossId = null,
            [FromQuery] int? wipes = null,
            CancellationToken token = default)
        {
            if (fightId == null && bossId == null && wipes == null)
                return NoContent();

            return Ok(await reportDataService.GetSkillLinesByReportAsync(
                logId, fightId, bossId, wipes, token));
        }


        /// <summary>
        /// Retrieves all reports and their associated fights for a specific zone and difficulty.
        /// </summary>
        /// <returns></returns>
        [HttpGet("getAllReports")]
        public async Task<IActionResult> GetAllReports(
            CancellationToken token = default)
        {
            var allReports = await getDataService.GetAllReportsAndFightsAsync(token: token);

            return Ok(allReports);
        }
    }
}
