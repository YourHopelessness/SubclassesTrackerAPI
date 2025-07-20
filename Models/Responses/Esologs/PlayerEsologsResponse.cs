using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace SubclassesTracker.Api.Models.Responses.Esologs
{
    public sealed record PlayerEsologsResponse
    {
        /// <summary> The unique identifier for the player. </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary> The name of the player. </summary>
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary> The base class of the player. </summary>
        [JsonProperty("type")]
        public string BaseClass { get; set; } = string.Empty;

        /// <summary> ESO ID of the player. </summary>
        [JsonProperty("displayName")]
        public string PlayerEsoId { get; set; } = string.Empty;

        [JsonProperty("specs")]
        public List<string> Specs { get; set; } = [];

        /// <summary> Skills and gear of the player </summary>
        [JsonProperty("combatantInfo")]
        private JToken CombatantInfoRaw { get; init; } = new JObject();

        [JsonIgnore]
        public CombatInfoEsologsResponse CombatInfo
        {
            get
            {
                if (CombatantInfoRaw is null ||
                    CombatantInfoRaw.Type is JTokenType.Null or JTokenType.Undefined)
                    return new CombatInfoEsologsResponse();

                if (CombatantInfoRaw.Type == JTokenType.Array)
                {
                    var first = CombatantInfoRaw.FirstOrDefault();

                    return first is null ||
                           first.Type is JTokenType.Null or JTokenType.Undefined
                           ? new CombatInfoEsologsResponse()
                           : first.ToObject<CombatInfoEsologsResponse>() ?? new CombatInfoEsologsResponse();
                }

                return CombatantInfoRaw.ToObject<CombatInfoEsologsResponse>() ?? new CombatInfoEsologsResponse();
            }
        }
    }
}
