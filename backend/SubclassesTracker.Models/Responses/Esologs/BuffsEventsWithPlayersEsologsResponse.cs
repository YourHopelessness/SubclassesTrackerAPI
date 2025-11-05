using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SubclassesTracker.Models.Responses.Esologs
{
    /// <summary>
    /// GetEventsBuffs response model
    /// </summary>
    public sealed record BuffsEventsWithPlayersEsologsResponse
    {
        /// <summary>
        /// Players details with combantant info
        /// </summary>
        [JsonProperty("playerDetails")]
        public JToken PlayerDetailsRaw { get; init; } = new JObject();

        /// <summary>
        /// Flatten wrapped player's details
        /// </summary>
        [JsonIgnore]
        public PlayerListResponse PlayerDetails
        {
            get
            {
                if (PlayerDetailsRaw is not { Type: not (JTokenType.Null or JTokenType.Undefined) })
                    return new();

                var inner = PlayerDetailsRaw?["data"]?["playerDetails"];
                return inner?.ToObject<PlayerListResponse>() ?? new();
            }
        }

        /// <summary>
        /// Events list
        /// </summary>
        [JsonProperty("events")]
        public JToken EventsRaw { get; init; } = new JObject();

        /// <summary>
        /// Flatten wrapped player's buff events
        /// </summary>
        [JsonIgnore]
        public List<EventsBuffsData> EventsBuffs
        {
            get
            {
                if (EventsRaw is not { Type: not (JTokenType.Null or JTokenType.Undefined) })
                    return [];

                var list = EventsRaw?["data"];
                return list?.ToObject<List<EventsBuffsData>>() ?? [];
            }
        }
    }
}
