using GraphQLClientNS;
using Microsoft.Extensions.Options;
using SubclassesTrackerExtension.Extensions;
using SubclassesTrackerExtension.Models;

namespace SubclassesTrackerExtension.EsologsServices
{
    public interface IGetDataService
    {
        /// <summary>
        /// Retrieves the list of fight IDs for a given log ID.
        /// </summary>
        /// <param name="logId"></param>
        /// <returns></returns>
        Task<IReadOnlyList<int>> GetFigthsAsync(string logId);

        /// <summary>
        /// Retrieves the list of players for a given log ID and fight IDs.
        /// </summary>
        /// <param name="logId"></param>
        /// <param name="fightsIds"></param>
        /// <returns></returns>
        Task<PlayerListModel> GetPlayersAsync(string logId, IReadOnlyList<int> fightsIds);

        /// <summary>
        /// Retrieves the buffs for a specific player in a given log and fight IDs.
        /// </summary>
        /// <param name="logId"></param>
        /// <param name="playerId"></param>
        /// <param name="fightId"></param>
        /// <returns></returns>
        Task<IEnumerable<BuffModel>> GetPlayerBuffsAsync(string logId, int playerId, IReadOnlyList<int> fightId);

        /// <summary>
        /// Retrieves all reports and their associated fights for a specific zone and difficulty.
        /// </summary>
        /// <param name="zoneId"></param>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        Task<List<ReportModel>> GetAllReportsAndFights(int zoneId = 1, int difficulty = 122);

        /// <summary>
        /// Retrieves all zones and their encounters from the ESO Logs API.
        /// </summary>
        /// <returns></returns>
        Task<List<ZoneModel>> GetAllZonesAndEncountersAsync();
    }
    public class GetDataService(
        GraphQLClient graphQLClient,
        IOptions<LinesConfig> options,
        TokenStorage tokenStorage,
        IHttpClientFactory httpClientFactory,
        ILogger<GetDataService> logger) : IGetDataService
    {
        public async Task<IReadOnlyList<int>> GetFigthsAsync(string logId)
        {
            var fights = await graphQLClient.GetFights.ExecuteAsync(logId);

            return fights?.Data?.ReportData?.Report?.Fights?
                    .Where(f => f != null)
                    .Select(f => f.Id)
                    .ToList() ?? [];
        }

        public async Task<PlayerListModel> GetPlayersAsync(string logId, IReadOnlyList<int> fightsIds)
        {
            var players = await graphQLClient.GetPlayers.ExecuteQueryWithFallbackAsync<PlayerListModel>(
                await tokenStorage.GetToken(),
                "GraphQLClient/GetPlayers.graphql",
                new { code = logId, fightsIds = fightsIds.ToArray() },
                "data.reportData.report.table.data.playerDetails",
                $"Saves/GetPlayers/GetPlayers{logId}_{string.Join(',', fightsIds)}.json",
                options.Value.EsoLogsApiUrl,
                logger,
                httpClientFactory);

            return players;
        }

        public async Task<IEnumerable<BuffModel>> GetPlayerBuffsAsync(string logId, int playerId, IReadOnlyList<int> fightId)
        {
            var buffs = await graphQLClient.GetBuffs.ExecuteQueryWithFallbackAsync<List<BuffModel>>(
                await tokenStorage.GetToken(),
                "GraphQLClient/GetBuffs.graphql",
                new { code = logId, fightIds = fightId, playerId },
                "data.reportData.report.table.data.auras",
                $"Saves/GetBuffs/GetBuffs_{logId}_{playerId}.json",
                options.Value.EsoLogsApiUrl,
                logger,
                httpClientFactory
            );

            return buffs;
        }

        public async Task<List<ReportModel>> GetAllReportsAndFights(
            int zoneId = 1, int difficulty = 122)
        {
            var resultList = new List<ReportModel>();

            var startTime = options.Value.TrialStartTimeSlice;
            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var oneDay = 60480000; // 1 week slice

            for (double t = startTime; t < currentTime; t += oneDay)
            {
                var sliceStart = t;
                var sliceEnd = t + oneDay;

                resultList.AddRange(await GraphQLExtensions.QueryAllPagesAsync(async page =>
                {
                    if (page > 24)
                    {
                        return (new List<ReportModel>(), false);
                    }

                    var variables = new
                    {
                        zoneID = zoneId,
                        startTime = (long)sliceStart,
                        endTime = (long)sliceEnd,
                        page,
                        limit = 100,
                        difficulty,
                        killType = KillType.Kills
                    };

                    var result = await graphQLClient.GetReportsWithFights
                        .ExecuteQueryWithFallbackAsync<ReportRequestModel>(
                            await tokenStorage.GetToken(),
                            "GraphQLClient/GetReportsWithFights.graphql",
                            variables,
                            "data.reportData.reports",
                            $"Saves/GetReportsWithFights/GetReportsWithFights{variables.zoneID}_{variables.page}_{variables.limit}_{sliceStart}_{sliceEnd}.json",
                            options.Value.EsoLogsApiUrl,
                            logger,
                            httpClientFactory
                        );

                    return (result.Data, result.HasMorePages);
                },
                x => x.Fights.Count > 0 && x.Fights.Any(f => f.TrialScore != null)));
            }

            return [.. resultList.DistinctBy(x => x.Code)];
        }

        public async Task<List<ZoneModel>> GetAllZonesAndEncountersAsync()
        {
            var zones = await graphQLClient.GetBuffs.ExecuteQueryWithFallbackAsync<List<ZoneModel>>(
                await tokenStorage.GetToken(),
                "GraphQLClient/GetAllEncounters.graphql",
                new { },
                "data.worldData.zones",
                "Saves/GetAllEncounters/GetAllEncounters.json",
                options.Value.EsoLogsApiUrl,
                logger,
                httpClientFactory
            );

            return zones;
        }
    }
}
