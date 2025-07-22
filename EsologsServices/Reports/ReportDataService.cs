using SubclassesTracker.Api.Models;
using SubclassesTracker.Api.Models.Dto;
using SubclassesTracker.Api.Models.Responses.Api;
using SubclassesTracker.Api.Models.Responses.Esologs;
using SubclassesTracker.Database.Entity;
using SubclassesTracker.Database.Repository;

namespace SubclassesTracker.Api.EsologsServices.Reports
{
    public interface IReportDataService
    {
        /// <summary>
        /// Retrieves skill lines for a specific zone (trial) based on the provided zone ID.
        /// </summary>
        /// <param name="zoneId">Id of zone</param>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        Task<List<SkillLineReportEsologsResponse>> GetSkillLinesAsync(
            int zoneId,
            int difficulty,
            bool useScoreCense = false,
            CancellationToken token = default);

        /// <summary>
        /// Get skill lines that players used in specific logs and fights
        /// </summary>
        /// <param name="logId"></param>
        /// <param name="fightId"></param>
        /// <param name="bossId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<List<PlayerSkilllinesApiResponse>> GetSkillLinesByReportAsync(
            string logId,
            string? fightId = null,
            int? bossId = null,
            int? wipes = null,
            CancellationToken token = default);
    }
    public partial class ReportDataService(
        IGetDataService dataService,
        IBaseRepository<SkillTreeEntry> skillsRepository,
        IBaseRepository<Encounter> encounterRepository) : IReportDataService
    {
        private sealed record PlayerKey(string Name, string TrialName, string Display, string Spec);
        private Dictionary<int, SkillInfo> skillsDict = null!;
        private sealed record SkillInfo(string SkillName, string SkillLine, string SkillType, string? UrlIcon);

        public async Task<List<SkillLineReportEsologsResponse>> GetSkillLinesAsync(
                int zoneId,
                int difficulty,
                bool useScoreCense = false,
                CancellationToken token = default)
        {
            skillsDict ??= await LoadSkillsAsync(token);

            // Get zones and fight paralelly
            var reportsTask = dataService.GetAllReportsAndFightsAsync(zoneId, difficulty, token);
            var zonesTask = LoadTrialZonesAsync(token);

            await Task.WhenAll(reportsTask, zonesTask);

            var reports = reportsTask.Result;
            var zones = zonesTask.Result;

            var filteredReports = FilterReports(reports, zones, useScoreCense);

            if (filteredReports.Count == 0)
                return [];

            var playersByLog = await LoadPlayersForReportsAsync(filteredReports, token);

            var playerRows = BuildDistinctBestFightRows(filteredReports, playersByLog);

            await AddMissingBuffRowsAsync(playerRows, token);

            // Get all roles
            var roleBuckets = playerRows.GroupBy(r => r.Role)
                                        .ToDictionary(g => g.Key, g => g.ToList());

            var result = new List<SkillLineReportEsologsResponse>();

            // Filter by the trial id
            foreach (var group in playerRows.Where(r => r.TrialId == zoneId)
                                            .GroupBy(r => new { r.TrialId, r.TrialName }))
            {
                roleBuckets.TryGetValue(PlayerRole.Dps.ToString(), out var dpsList);
                roleBuckets.TryGetValue(PlayerRole.Healer.ToString(), out var healList);
                roleBuckets.TryGetValue(PlayerRole.Tank.ToString(), out var tankList);

                result.Add(new SkillLineReportEsologsResponse
                {
                    TrialId = group.Key.TrialId,
                    TrialName = group.Key.TrialName,
                    DdLinesModels = BuildLines(dpsList ?? Enumerable.Empty<PlayerRow>(), skillsDict),
                    HealersLinesModels = BuildLines(healList ?? Enumerable.Empty<PlayerRow>(), skillsDict),
                    TanksLinesModels = BuildLines(tankList ?? Enumerable.Empty<PlayerRow>(), skillsDict)
                });
            }

            return result;
        }

        public async Task<List<PlayerSkilllinesApiResponse>> GetSkillLinesByReportAsync(
            string logId,
            string? fightId = null,
            int? bossId = null,
            int? wipes = null,
            CancellationToken token = default)
        {
            skillsDict ??= await LoadSkillsAsync(token);

            var fightIdList = fightId?.Split('.').Select(int.Parse).ToList();
            var fights = await dataService.GetFigthsAsync(logId, token);
            var reports = new List<FilteredReport>()
            {
                new(logId, 0, logId, [.. fights
                    .Where(x => fightIdList?.Contains(x.Id) ?? true)
                    .Where(x => wipes == null || (wipes == 1 && (!x.Killed ?? true) || wipes == 2 && (x?.Killed ?? false)))
                    .Where(x => bossId == null || !(bossId > -1) || x.EncounterId == bossId)])
            };

            var playersByLog = await LoadPlayersForReportsAsync(reports, token);
            var rows = BuildDistinctBestFightRows(reports, playersByLog, false);

            await AddMissingBuffRowsAsync(rows, token);

            // Grouping by player char name
            var result = rows
                .GroupBy(r => new { r.PlayerId, r.TrialId })
                .Select(g =>
                {
                    // PlayerEsoId prefers
                    var sample = g.First();
                    var allTalents = g
                        .SelectMany(x => x.Talents)
                        .Where(t => skillsDict.ContainsKey(t.Id))
                        .Select(t => new PlayerSkillLine(
                            skillsDict[t.Id].SkillLine,
                            skillsDict[t.Id]?.UrlIcon ?? string.Empty))
                        .DistinctBy(t => t.LineName)
                        .ToList();

                    return new PlayerSkilllinesApiResponse
                    {
                        PlayerCharacterName = GetPlayerCharacterName(sample.PlayerId, playersByLog[sample.LogId]),
                        PlayerEsoId = GetPlayerEsoId(sample.PlayerId, playersByLog[sample.LogId]),
                        PlayerSkillLines = allTalents
                    };
                })
                .ToList();

            return result;
        }
    }
}