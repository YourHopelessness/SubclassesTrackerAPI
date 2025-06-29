using GraphQLClientNS;
using Microsoft.Extensions.Options;
using SubclassesTrackerExtension.Extensions;
using SubclassesTrackerExtension.Models;

namespace SubclassesTrackerExtension.EsologsServices
{
    public interface IGetDataService
    {
        Task<IReadOnlyList<int>> GetFigthsAsync(string logId);

        Task<PlayerListModel> GetPlayersAsync(string logId, IReadOnlyList<int> fightsIds);

        Task<IEnumerable<BuffModel>> GetPlayerBuffsAsync(string logId, int playerId, IReadOnlyList<int> fightId);

        Task<List<ReportModel>> GetAllReportsAndFights(int zoneId = 1);

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

        public async Task<List<ReportModel>> GetAllReportsAndFights(int zoneId = 1)
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
                        page = page,
                        limit = 100,
                        difficulty = 122,
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

            return resultList;
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
