using SubclassesTracker.Api.Extensions;
using SubclassesTracker.GraphQL.Services;
using SubclassesTracker.Models.Dto;
using SubclassesTracker.Models.Responses.Api;
using SubclassesTracker.Models.Responses.Esologs;

namespace SubclassesTracker.Api.EsologsServices.Reports
{
    public interface IReportRacialDataService
    {
        Task<RacialReportApiResponse> GetRacesDataAsync(
            int zoneId,
            int difficulty,
            long startTime = 0,
            long endTime = 0,
            CancellationToken token = default);
    }

    public class ReportRacialDataService(
        ILogger<ReportRacialDataService> logger,
        IGraphQLGetService graphQLGetService,
        IReportService reportService,
        ILoaderService loaderService) : IReportRacialDataService
    {
        private Dictionary<int, RacialPassivesInfo> racialSkillsDict = null!;

        public async Task<RacialReportApiResponse> GetRacesDataAsync(
            int zoneId,
            int difficulty,
            long startTime = 0,
            long endTime = 0,
            CancellationToken token = default)
        {
            racialSkillsDict ??= await loaderService.LoadRacialPassivesAsync(token);

            var filteredReports = await reportService.GetAllFilteredReportAsync(zoneId, difficulty, startTime: startTime, endTime: endTime, token: token);

            var playerMaxScore = new Dictionary<string, int>();

            var result = new RacialReportApiResponse
            {
                TrialName = filteredReports.First().ZoneName,
                TankRacesQuantity = racialSkillsDict.Values.DistinctBy(x => x.RacialName).ToDictionary(x => x.RacialName, v => 0),
                HealerRacesQuantity = racialSkillsDict.Values.DistinctBy(x => x.RacialName).ToDictionary(x => x.RacialName, v => 0),
                DdRacesQuantity = racialSkillsDict.Values.DistinctBy(x => x.RacialName).ToDictionary(x => x.RacialName, v => 0)
            };

            foreach (var report in filteredReports)
            {
                var eventsbuffs = await graphQLGetService.GetBuffsEventsAsync(
                    report.LogId, [.. report.Fights.Select(x => x.Id)], token);

                var playerRaces = eventsbuffs.EventsBuffs
                    .GroupBy(x => x.PlayerId)
                    .ToDictionary(
                        k => k.Key,
                        v => v.SelectMany(x => x.PlayerBuffs)
                              .Where(x => racialSkillsDict.ContainsKey(x.BuffId))
                              .Select(x => racialSkillsDict[x.BuffId].RacialName)
                              .FirstOrDefault());

                if (playerRaces == null
                    || playerRaces.Count < eventsbuffs.EventsBuffs.DistinctBy(x => x.PlayerId).Count())
                {
                    logger.LogWarning("In log {0} not all players have a race", report.LogId);
                }

                var trialMaxScore = report.Fights.Max(x => x.TrialScore) ?? 0;

                result.TankRacesQuantity.ChangeRacesQuantity(
                    CountRaces(playerRaces, eventsbuffs.PlayerDetails.Tanks, playerMaxScore, trialMaxScore));
                result.HealerRacesQuantity.ChangeRacesQuantity(
                    CountRaces(playerRaces, eventsbuffs.PlayerDetails.Healers, playerMaxScore, trialMaxScore));
                result.DdRacesQuantity.ChangeRacesQuantity(
                    CountRaces(playerRaces, eventsbuffs.PlayerDetails.Dps, playerMaxScore, trialMaxScore));
            }

            return result;
        }

        private static IEnumerable<string> CountRaces(
            Dictionary<int, string?>? racesQuantity,
            List<PlayerEsologsResponse> playerRaces,
            Dictionary<string, int> playerMaxScore,
            int maxScore)
        {
            playerRaces = [.. playerRaces.Where(t =>
                t.PlayerEsoId.Equals("nil", StringComparison.CurrentCultureIgnoreCase)
                || playerMaxScore.TryAdd(t.Name.ToLower(), maxScore))];

            return playerRaces.Select(t => racesQuantity?.GetValueOrDefault(t.Id) ?? string.Empty);
        }
    }
}
