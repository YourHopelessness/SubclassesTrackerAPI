using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SubclassesTracker.Models;
using SubclassesTracker.Models.Dto;
using SubclassesTracker.Models.Enums;
using SubclassesTracker.Models.Responses.Api;
using SubclassesTracker.Models.Responses.Esologs;
using static SubclassesTracker.GraphQL.GraphQLClient.GraphQLQueries;

namespace SubclassesTracker.GraphQL.Services
{
    public interface IGraphQLGetService
    {
        /// <summary>
        /// Retrieves the list of fights for a given log ID.
        /// </summary>
        Task<List<FightEsologsResponse>> GetFigthsAsync(string logId, CancellationToken token = default);

        /// <summary>
        /// Retrieves the list of players for a given log ID and fight IDs.
        /// </summary>
        Task<PlayerListResponse> GetPlayersAsync(string logId, List<int> fightsIds, CancellationToken token = default);

        /// <summary>
        /// Retrieves the buffs for a specific player in a given log and fight IDs.
        /// </summary>
        Task<List<BuffEsologsResponse>> GetPlayerBuffsAsync(string logId, int playerId, List<int> fightId, CancellationToken token = default);

        /// <summary>
        /// Retrieves all reports and their associated fights for a specific zone and difficulty.
        /// </summary>
        Task<List<ReportEsologsResponse>> GetAllReportsAndFightsAsync(
            int zoneId = 1, int difficulty = 122, long startTime = 0, long endTime = 0, CancellationToken token = default);

        /// <summary>
        /// Retrieves all zones and their encounters from the ESO Logs API.
        /// </summary>
        Task<List<ZoneApiResponse>> GetAllZonesAndEncountersAsync(CancellationToken token = default);

        /// <summary>
        /// Retrieves all events buffs with combantant info
        /// </summary>
        Task<BuffsEventsWithPlayersEsologsResponse> GetBuffsEventsAsync(
            string logId, List<int> fightId, CancellationToken token = default);
    }
    public class GraphQLGetService(
        IOptions<LinesConfig> options,
        IHttpClientFactory httpClientFactory,
        ILogger<GraphQLGetService> logger) : IGraphQLGetService
    {
        private readonly QraphQlExecutor qlExecutor =
            new(options.Value.EsoLogsApiUrl, logger, httpClientFactory);

        public async Task<List<FightEsologsResponse>> GetFigthsAsync(string logId, CancellationToken token = default)
        {
            var fights = await qlExecutor.QueryAsync<List<FightEsologsResponse>, GetFightsVars>(
                GraphQlQueryEnum.GetFights, new GetFightsVars(logId), token: token);

            return fights;
        }

        public async Task<PlayerListResponse> GetPlayersAsync(string logId, List<int> fightsIds, CancellationToken token = default)
        {
            var players = await qlExecutor.QueryAsync<PlayerListResponse, GetPlayersVars>(
                GraphQlQueryEnum.GetPlayers, new GetPlayersVars(logId, [.. fightsIds]), token: token);

            return players;
        }

        public async Task<List<BuffEsologsResponse>> GetPlayerBuffsAsync(
            string logId, int playerId, List<int> fightId, CancellationToken token = default)
        {
            var buffs = await qlExecutor.QueryAsync<List<BuffEsologsResponse>, GetBuffsVars>(
                GraphQlQueryEnum.GetBuffs, new GetBuffsVars(logId, playerId, [.. fightId]), token: token);

            return buffs;
        }

        public async Task<List<ReportEsologsResponse>> GetAllReportsAndFightsAsync(
            int zoneId = 1, int difficulty = 122, long startTime = 0, long endTime = 0,
            CancellationToken token = default)
        {
            var resultList = new List<ReportEsologsResponse>();

            var endSlice = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() <= endTime
                ? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                : endTime;
            var oneDay = 60480000; // 1 week slice

            for (double t = startTime; t < endSlice; t += oneDay)
            {
                var sliceStart = t;
                var sliceEnd = t + oneDay;

                resultList.AddRange(await GraphQLExtensions.QueryAllPagesAsync(async page =>
                {
                    if (page > 24)
                    {
                        return (new List<ReportEsologsResponse>(), false);
                    }

                    var variables = new GetReportsWithFightsVars(
                        zoneId,
                        (long)sliceStart,
                        (long)sliceEnd,
                        page,
                        KillType.Kills,
                        100,
                        difficulty);

                    var result = await qlExecutor.QueryAsync<ReportRequestEsologsResponse, GetReportsWithFightsVars>(
                            GraphQlQueryEnum.GetReportsWithFights,
                            variables,
                            token);

                    return (result.Data, result.HasMorePages);
                },
                x => x.Fights.Count > 0 && x.Fights.Any(f => f.TrialScore != null)));
            }

            return [.. resultList.DistinctBy(x => x.Code)];
        }

        public async Task<List<ZoneApiResponse>> GetAllZonesAndEncountersAsync(CancellationToken token = default)
        {
            var zones = await qlExecutor.QueryAsync<List<ZoneApiResponse>, GetAllEncountersVars>(
                GraphQlQueryEnum.GetAllEncounters, new GetAllEncountersVars(), token: token);

            return zones;
        }

        public async Task<BuffsEventsWithPlayersEsologsResponse> GetBuffsEventsAsync(
            string logId, List<int> fightId, CancellationToken token = default)
        {
            var buffAndEvents = await qlExecutor.QueryAsync<BuffsEventsWithPlayersEsologsResponse, GetBuffsEventsVars>(
                GraphQlQueryEnum.GetBuffsEvents, new GetBuffsEventsVars(logId, [.. fightId]), token);

            return buffAndEvents;
        }
    }
}
