using Newtonsoft.Json;

namespace SubclassesTracker.Api.Models.Responses.Esologs
{
    public sealed record PlayerListResponse
    {
        /// <summary>
        /// LogId
        /// </summary>
        public string LogId { get; set; } = string.Empty;
        /// <summary>
        /// List of healers in the player list.
        /// </summary>
        [JsonProperty("healers")]
        public List<PlayerEsologsResponse> Healers { get; set; } = [];

        /// <summary>
        /// List of tanks in the player list.
        /// </summary>
        [JsonProperty("tanks")]
        public List<PlayerEsologsResponse> Tanks { get; set; } = [];

        /// <summary>
        /// List of damage dealers (DPS) in the player list.
        /// </summary>
        [JsonProperty("dps")]
        public List<PlayerEsologsResponse> Dps { get; set; } = [];
    }
}
