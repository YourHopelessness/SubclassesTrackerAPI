using SubclassesTracker.GraphQL.Services;
using SubclassesTracker.Models.Dto;
using SubclassesTracker.Models.Enums;
using SubclassesTracker.Models.Responses.Api;
using SubclassesTracker.Models.Responses.Esologs;

namespace SubclassesTracker.Api.EsologsServices.Reports
{
    public interface IReportSubclassesDataService
    {
        /// <summary>
        /// Retrieves skill lines for a specific zone (trial) based on the provided zone ID.
        /// </summary>
        /// <param name="zoneId">Id of zone</param>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        Task<SkillLineReportResults> GetSkillLinesAsync(
            int zoneId,
            int difficulty,
            long startTime,
            long endTime,
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
    public partial class ReportSubclassesDataService(
        IGraphQLGetService dataService,
        ILoaderService loaderService,
        IReportService reportService) : IReportSubclassesDataService
    {
        private Dictionary<int, SkillInfo> skillsDict = null!;

        public async Task<SkillLineReportResults> GetSkillLinesAsync(
                int zoneId,
                int difficulty,
                long startTime,
                long endTime,
                CancellationToken token = default)
        {
            skillsDict ??= await loaderService.LoadSkillsAsync(token);

            var reports = await reportService.GetAllFilteredReportAsync(zoneId, difficulty, startTime, endTime, token);

            var results = new SkillLineReportResults();

            // Load players for all reports at once
            var playersByLog = await reportService.LoadPlayersForReportsAsync(reports.WithoutCense, token);

            // Build all player rows for best fights
            var allPlayerRows = BuildDistinctBestFightRows(reports.WithoutCense, playersByLog);
            await AddMissingBuffRowsAsync(allPlayerRows, token); // Add missing buff rows

            // Build results without cense filter
            results.WithoutCense = BuildResult(allPlayerRows, zoneId);

            // Filter player rows for cense reports only
            var allowedReportCodes = reports.WithCense.Select(r => r.LogId).ToHashSet();
            var censePlayerRows = allPlayerRows
                .Where(p => allowedReportCodes.Contains(p.LogId))
                .ToList();

            results.WithCense = BuildResult(censePlayerRows, zoneId);

            return results;
        }

        public async Task<List<PlayerSkilllinesApiResponse>> GetSkillLinesByReportAsync(
            string logId,
            string? fightId = null,
            int? bossId = null,
            int? wipes = null,
            CancellationToken token = default)
        {
            skillsDict ??= await loaderService.LoadSkillsAsync(token);

            var fightIdList = fightId?.Split('.').Select(int.Parse).ToList();
            var fights = await dataService.GetFigthsAsync(logId, token);
            var reports = new List<FilteredReport>()
            {
                new(logId, 0, logId, [.. fights
                    .Where(x => fightIdList?.Contains(x.Id) ?? true)
                    .Where(x => wipes == null || (wipes == 1 && (!x.Killed ?? true) || wipes == 2 && (x?.Killed ?? false)))
                    .Where(x => bossId == null || !(bossId > -1) || x.EncounterId == bossId)])
            };

            var playersByLog = await reportService.LoadPlayersForReportsAsync(reports, token);
            var rows = BuildDistinctBestFightRows(reports, playersByLog, false);

            await AddMissingBuffRowsAsync(rows, token); // Less than 3 lines

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
                            skillsDict[t.Id]?.UrlIcon ?? string.Empty,
                            skillsDict[t.Id]?.ClassName ?? string.Empty))
                        .DistinctBy(t => t.LineName)
                        .OrderBy(t => t.ClassName)
                        .ToList();

                    return new PlayerSkilllinesApiResponse
                    {
                        PlayerCharacterName = GetPlayerCharacterName(sample.PlayerId, playersByLog[sample.LogId]),
                        PlayerEsoId = GetPlayerEsoId(sample.PlayerId, playersByLog[sample.LogId]),
                        BaseClass = sample.BaseClass,
                        PlayerSkillLines = [.. allTalents.OrderByDescending(t => t.ClassName == sample.BaseClass)]
                    };
                })
                .ToList();

            return result;
        }

        private SkillLineReportEsologsResponse BuildResult(IEnumerable<PlayerRow> playerRows, int zoneId)
        {
            var roleBuckets = playerRows
                .GroupBy(r => r.Role)
                .ToDictionary(g => g.Key, g => g.ToList());

            var result = new List<SkillLineReportEsologsResponse>();

            foreach (var group in playerRows
                         .Where(r => r.TrialId == zoneId)
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

            return new SkillLineReportEsologsResponse
            {
                TrialId = zoneId,
                TrialName = result.First().TrialName,
                DdLinesModels = [.. result.SelectMany(l => l.DdLinesModels)],
                HealersLinesModels = [.. result.SelectMany(l => l.HealersLinesModels)],
                TanksLinesModels = [.. result.SelectMany(l => l.TanksLinesModels)]
            };
        }
    }
}