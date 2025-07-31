using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SubclassesTracker.Api.Models.Responses.Esologs
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

    /// <summary>
    /// Player's events buffs
    /// </summary>
    public sealed record EventsBuffsData
    {
        /// <summary>
        /// Player id
        /// </summary>
        [JsonProperty("sourceID")]
        public int PlayerId { get; set; }

        /// <summary>
        /// Player's buffs
        /// </summary>
        [JsonProperty("auras")]
        public List<EventAurasList> PlayerBuffs { get; set; }
    }

    /// <summary>
    /// List of buffs
    /// </summary>
    public sealed record EventAurasList
    {
        /// <summary>
        /// Buff id
        /// </summary>
        [JsonProperty("ability")]
        public int BuffId { get; set; }

        /// <summary>
        /// Buff Name
        /// </summary>
        [JsonProperty("name")]
        public string BuffName { get; set; }
    }
}
