using GraphQLClientNS;
using Microsoft.Extensions.Options;
using SubclassesTracker.Api.Extensions;
using SubclassesTracker.Api.GraphQLClient;
using SubclassesTracker.Api.Models.Dto;
using SubclassesTracker.Api.Models.Responses.Api;
using SubclassesTracker.Api.Models.Responses.Esologs;
using static SubclassesTracker.Api.GraphQLClient.GraphQLQueries;

namespace SubclassesTracker.Api.EsologsServices
{
    public interface IGetDataService
    {
        /// <summary>
        /// Retrieves the list of fights for a given log ID.
        /// </summary>
        /// <param name="logId"></param>
        /// <returns></returns>
        Task<List<FightEsologsResponse>> GetFigthsAsync(string logId);

        /// <summary>
        /// Retrieves the list of players for a given log ID and fight IDs.
        /// </summary>
        /// <param name="logId"></param>
        /// <param name="fightsIds"></param>
        /// <returns></returns>
        Task<PlayerListResponse> GetPlayersAsync(string logId, List<int> fightsIds);

        /// <summary>
        /// Retrieves the buffs for a specific player in a given log and fight IDs.
        /// </summary>
        /// <param name="logId"></param>
        /// <param name="playerId"></param>
        /// <param name="fightId"></param>
        /// <returns></returns>
        Task<List<BuffEsologsResponse>> GetPlayerBuffsAsync(string logId, int playerId, List<int> fightId);

        /// <summary>
        /// Retrieves all reports and their associated fights for a specific zone and difficulty.
        /// </summary>
        /// <param name="zoneId"></param>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        Task<List<ReportEsologsResponse>> GetAllReportsAndFights(int zoneId = 1, int difficulty = 122);

        /// <summary>
        /// Retrieves all zones and their encounters from the ESO Logs API.
        /// </summary>
        /// <returns></returns>
        Task<List<ZoneApiResponse>> GetAllZonesAndEncountersAsync();
    }
    public class GetDataService(
        IOptions<LinesConfig> options,
        IHttpClientFactory httpClientFactory,
        ILogger<GetDataService> logger) : IGetDataService
    {
        private readonly QraphQlExecutor qlExecutor = 
            new(options.Value.EsoLogsApiUrl, logger, httpClientFactory);

        public async Task<List<FightEsologsResponse>> GetFigthsAsync(string logId)
        {
            var fights = await qlExecutor.QueryAsync<List<FightEsologsResponse>, GetFightsVars>(
                GraphQlQueryEnum.GetFights,
                new GetFightsVars(logId));

            return fights;
        }

        public async Task<PlayerListResponse> GetPlayersAsync(string logId, List<int> fightsIds)
        {
            var players = await qlExecutor.QueryAsync<PlayerListResponse, GetPlayersVars>(
                GraphQlQueryEnum.GetPlayers,
                new GetPlayersVars(logId, [.. fightsIds]));

            return players;
        }

        public async Task<List<BuffEsologsResponse>> GetPlayerBuffsAsync(string logId, int playerId, List<int> fightId)
        {
            var buffs = await qlExecutor.QueryAsync<List<BuffEsologsResponse>, GetBuffsVars>(
                GraphQlQueryEnum.GetBuffs,
                new GetBuffsVars(logId, playerId, [.. fightId]));

            return buffs;
        }

        public async Task<List<ReportEsologsResponse>> GetAllReportsAndFights(
            int zoneId = 1, int difficulty = 122)
        {
            var resultList = new List<ReportEsologsResponse>();

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
                            variables);

                    return (result.Data, result.HasMorePages);
                },
                x => x.Fights.Count > 0 && x.Fights.Any(f => f.TrialScore != null)));
            }

            return [.. resultList.DistinctBy(x => x.Code)];
        }

        public async Task<List<ZoneApiResponse>> GetAllZonesAndEncountersAsync()
        {
            var zones = await qlExecutor.QueryAsync<List<ZoneApiResponse>, GetAllEncountersVars>(
                GraphQlQueryEnum.GetAllEncounters,
                new GetAllEncountersVars());

            return zones;
        }
    }
}
