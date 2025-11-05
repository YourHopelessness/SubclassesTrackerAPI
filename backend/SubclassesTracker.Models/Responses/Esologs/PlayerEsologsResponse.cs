using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SubclassesTracker.Models.Responses.Esologs
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

        /// <summary>
        /// Specs of player
        /// </summary>
        [JsonProperty("specs")]
        public JToken SpecsRaw { get; init; } = new JArray();

        /// <summary>
        /// Flatten specs
        /// </summary>
        [JsonIgnore]
        public List<string> Specs
        {
            get
            {
                if (SpecsRaw == null || SpecsRaw.Type == JTokenType.Null)
                    return [];

                if (SpecsRaw.Type == JTokenType.Array)
                {
                    var array = (JArray)SpecsRaw;

                    // if specs is strings
                    if (array.FirstOrDefault()?.Type == JTokenType.String)
                    {
                        return array.Values<string>().ToList();
                    }

                    // if elements is object
                    if (array.FirstOrDefault()?.Type == JTokenType.Object)
                    {
                        return array
                            .Select(obj => obj?["spec"]?.ToString())
                            .Where(s => !string.IsNullOrWhiteSpace(s))
                            .ToList()!;
                    }
                }

                return [];
            }
        }

        /// <summary> Skills and gear of the player </summary>
        [JsonProperty("combatantInfo")]
        private JToken CombatantInfoRaw { get; init; } = new JObject();

        /// <summary>
        /// Flatten sjulls an gear of the players
        /// </summary>
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
