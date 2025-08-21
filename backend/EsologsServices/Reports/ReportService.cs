using SubclassesTracker.Api.Models.Dto;
using SubclassesTracker.Api.Models.Responses.Api;
using SubclassesTracker.Api.Models.Responses.Esologs;

namespace SubclassesTracker.Api.EsologsServices.Reports
{
    public interface IReportService
    {
        /// <summary>
        /// Get report with score and by difficulty
        /// </summary>
        /// <param name="zoneId">Trial zone Id</param>
        /// <param name="difficulty">Difficult of the closed trial</param>
        /// <param name="useScoreCense">Additional filter, use the score cense</param>
        /// <returns>List of filtered reports</returns>
        Task<List<FilteredReport>> GetAllFilteredReportAsync(
            int zoneId,
            int difficulty,
            long startTime,
            long endTime,
            bool useScoreCense = false,
            CancellationToken token = default);
    }
    public class ReportService(
        IGraphQLGetService dataService,
        ILoaderService loaderService) : IReportService
    {
        /// <summary>
        /// Filter reports
        /// </summary>
        private static List<FilteredReport> FilterReports(
            List<ReportEsologsResponse> reports,
            List<ZoneApiResponse> zones,
            bool useScoreCense)
        {
            var zoneDict = zones.ToDictionary(z => z.Id);

            var filtered = new List<FilteredReport>();

            foreach (var report in reports)
            {
                if (!zoneDict.TryGetValue(report.Zone.Id, out var zone))
                    continue;

                var fights = report.Fights
                    .Where(f =>
                    {
                        // Filter by only existying bosses in fights
                        var enc = zone.Encounters.FirstOrDefault(e => e.Id == f.EncounterId);
                        if (enc is null) return false;

                        // Filter by score
                        return !useScoreCense || (!f.TrialScore.HasValue || f.TrialScore.Value >= enc.ScoreCense);
                    })
                    .ToList();

                // Filter only 'closed' reports
                if (fights.Count == 0 || !fights.Any(x => x.TrialScore != null))
                    continue;

                filtered.Add(new FilteredReport(report.Code, zone.Id, zone.Name, fights));
            }

            return filtered;
        }

        public async Task<List<FilteredReport>> GetAllFilteredReportAsync(
            int zoneId,
            int difficulty,
            long startTime,
            long endTime,
            bool useScoreCense = false,
            CancellationToken token = default)
        {
            // Get zones and fight paralelly
            var reportsTask = dataService.GetAllReportsAndFightsAsync(zoneId, difficulty, startTime, endTime, token);
            var zonesTask = loaderService.LoadTrialZonesAsync(token);

            await Task.WhenAll(reportsTask, zonesTask);

            var reports = reportsTask.Result;
            var zones = zonesTask.Result;

            var filteredReports = FilterReports(reports, zones, useScoreCense)
                .OrderByDescending(fr =>
                    fr.Fights
                      .Where(f => f.TrialScore.HasValue)
                      .Max(f => f.TrialScore) ?? int.MinValue)
                .ToList();

            return filteredReports;
        }
    }
}
