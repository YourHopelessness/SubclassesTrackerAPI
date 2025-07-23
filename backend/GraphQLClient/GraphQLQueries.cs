using SubclassesTracker.Api.Models.Enums;

namespace SubclassesTracker.Api.GraphQLClient
{
    /// <summary>
    /// GraphQl query enum name
    /// </summary>
    public enum GraphQlQueryEnum
    {
        GetPlayers,
        GetFights,
        GetAllEncounters,
        GetReportsWithFights,
        GetBuffs
    }
    public static class GraphQLQueries
    {
        /// <summary>
        /// Path to the data in the query response for each GraphQL query.
        /// </summary>
        public readonly static Dictionary<GraphQlQueryEnum, string> QueryRootPathResponse =
            new()
            {
                { GraphQlQueryEnum.GetPlayers, "data.reportData.report.table.data.playerDetails" },
                { GraphQlQueryEnum.GetFights, "data.reportData.report.fights" },
                { GraphQlQueryEnum.GetAllEncounters, "data.worldData.zones" },
                { GraphQlQueryEnum.GetReportsWithFights, "data.reportData.reports" },
                { GraphQlQueryEnum.GetBuffs, "data.reportData.report.table.data.auras" },
            };

        /// <summary>
        /// Variables for GraphQL request
        /// </summary>
        public sealed record GraphQlRequest<TVars>(string Query, TVars Variables);

        /// <summary>
        /// Represents the input variables required to retrieve fight data.
        /// </summary>
        /// <param name="Code">The unique code identifying the fight or set of fights to retrieve. 
        /// This value cannot be null or empty.
        /// </param>
        public sealed record GetFightsVars(string Code);

        /// <summary>
        /// Represents a request to retrieve player variables associated with a specific code and a list of fight IDs.
        /// </summary>
        /// <param name="Code">The unique code identifying the player or group of players. Cannot be null or empty.</param>
        /// <param name="FightsIds">A list of fight IDs for which the player variables are to be retrieved. Cannot be null.</param>
        public sealed record GetPlayersVars(string Code, int[] FightsIds);

        /// <summary>
        /// Represents the input parameters required to retrieve buffs for a specific player and fights.
        /// </summary>
        /// <param name="Code">The unique code identifying the context or session for the request. Cannot be null or empty.</param>
        /// <param name="PlayerId">The identifier of the player for whom buffs are being retrieved.</param>
        /// <param name="FightsIds">A list of fight identifiers for which buffs are being retrieved. Cannot be null.</param>
        public sealed record GetBuffsVars(string Code, int PlayerId, int[] FightIds);

        /// <summary>
        /// Represents the parameters required to retrieve all encounter variables for a specific zone and difficulty.
        /// </summary>
        public sealed record GetAllEncountersVars();

        /// <summary>
        /// Represents the input parameters required to retrieve reports with fights.
        /// </summary>
        /// <remarks>This record encapsulates the zone identifier, difficulty level, and an optional flag
        /// to determine  whether to use score cense when retrieving the reports.</remarks>
        /// <param name="ZoneId">The identifier of the zone for which reports are to be retrieved. Must be a valid zone ID.</param>
        /// <param name="Difficulty">The difficulty level of the fights to filter the reports. Defaults to 122 if not specified.</param>
        /// <param name="UseScoreCense">A flag indicating whether to use score cense in the report retrieval process. Defaults to <see
        /// langword="false"/>.</param>
        public sealed record GetReportsWithFightsVars(
            int ZoneId,
            long StartTime,
            long EndTime,
            int Page,
            KillType KillType = KillType.All,
            int Limit = 100,
            int Difficulty = 122,
            bool UseScoreCense = false);

    }
}
