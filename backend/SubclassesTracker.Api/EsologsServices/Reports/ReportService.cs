using SubclassesTracker.GraphQL.Services;
using SubclassesTracker.Models.Dto;
using SubclassesTracker.Models.Responses.Esologs;

namespace SubclassesTracker.Api.EsologsServices.Reports
{
    public interface IReportService
    {
        /// <summary>
        /// Load players dor specific report
        /// </summary>
        /// <param name="filteredReports">filtered reports</param>
        /// <returns></returns>
        Task<Dictionary<string, PlayerListResponse>> LoadPlayersForReportsAsync(
            List<FilteredReport> filteredReports,
            CancellationToken token);
        /// <summary>
        /// Get report with score and by difficulty
        /// </summary>
        /// <param name="zoneId">Trial zone Id</param>
        /// <param name="difficulty">Difficult of the closed trial</param>
        /// <param name="useScoreCense">Additional filter, use the score cense</param>
        /// <returns>List of filtered reports</returns>
        Task<FilterReportsResult> GetAllFilteredReportAsync(
            int zoneId,
            int difficulty,
            long startTime,
            long endTime,
            CancellationToken token = default);
    }
    public class ReportService(
        IGraphQLGetService dataService,
        ILoaderService loaderService) : IReportService
    {
        public async Task<FilterReportsResult> GetAllFilteredReportAsync(
            int zoneId,
            int difficulty,
            long startTime,
            long endTime,
            CancellationToken token = default)
        {
            // Get zones and fight paralelly
            var reports = await dataService.GetAllReportsAndFightsAsync(zoneId, difficulty, startTime, endTime, token);
            var zoneDict = await loaderService.LoadTrialZonesAsync(token);

            var withoutCense = new List<FilteredReport>();
            var withCense = new List<FilteredReport>();

            // Filter reports
            foreach (var report in reports)
            {
                if (!zoneDict.ToDictionary(z => z.Id).TryGetValue(report.Zone.Id, out var zone))
                    continue;

                bool filteredFunc(FightEsologsResponse fight, bool useScoreCense)
                {
                    // Filter by only existying bosses in fights
                    var enc = zone.Encounters.FirstOrDefault(e => e.Id == fight.EncounterId);
                    if (enc is null) return false;

                    // Filter by score
                    return !useScoreCense || (!fight.TrialScore.HasValue || fight.TrialScore.Value >= enc.ScoreCense);
                }

                var reportsWithCense = report.Fights
                    .Where(f => filteredFunc(f, true) && f.TrialScore.HasValue)
                    .OrderByDescending(f => f.TrialScore);

                var reportsWithoutCense = report.Fights
                    .Where(f => filteredFunc(f, false) && f.TrialScore.HasValue)
                    .OrderByDescending(f => f.TrialScore);

                if (reportsWithCense.Any())
                    withCense.Add(new(report.Code,
                        zone.Id,
                        zone.Name,
                        [.. reportsWithCense]));

                if (reportsWithoutCense.Any())
                    withoutCense.Add(new(report.Code,
                        zone.Id,
                        zone.Name,
                        [.. reportsWithoutCense]));
            }

            return new FilterReportsResult(withoutCense, withCense);
        }

        public async Task<Dictionary<string, PlayerListResponse>> LoadPlayersForReportsAsync(
            List<FilteredReport> filteredReports,
            CancellationToken token)
        {
            var result = new Dictionary<string, PlayerListResponse>();

            foreach (var r in filteredReports)
            {
                var fightIds = r.Fights.Select(f => f.Id).ToList();
                var players = await dataService.GetPlayersAsync(r.LogId, fightIds, token);
                players.LogId = r.LogId;

                result[r.LogId] = players;
            }

            return result;
        }
    }
}
