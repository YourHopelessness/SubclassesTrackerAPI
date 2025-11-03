using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SubclassesTracker.Models.Responses.Rows;

namespace SubclassesTracker.Models.Responses.Esologs
{
    /// <summary>
    /// GetEventsBuffs response model
    /// </summary>
    public sealed record BuffsEventsWithPlayersEsologsResponse : IFlattener<FlatBuffWithSkillsRow>
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

        public IEnumerable<FlatBuffWithSkillsRow> ToFlatRows()
        {
            var playersById = PlayerDetails
                .Healers.Concat(PlayerDetails.Tanks).Concat(PlayerDetails.Dps)
                .ToDictionary(p => p.Id);


            foreach (var ev in EventsBuffs)
            {
                if (!playersById.TryGetValue(ev.PlayerId, out var player))
                    continue;


                foreach (var buff in ev.PlayerBuffs)
                {
                    if (player.CombatInfo?.Talents is { Count: > 0 })
                    {
                        foreach (var skill in player.CombatInfo.Talents)
                        {
                            yield return new FlatBuffWithSkillsRow(
                                PlayerDetails.LogId,
                                ev.PlayerId,
                                player.Name,
                                player.PlayerEsoId,
                                GetPlayerRole(player.Id),
                                string.Join(", ", player.Specs),
                                buff.BuffId,
                                buff.BuffName,
                                skill.Id,
                                skill.Name);
                        }
                    }
                    else
                    {
                        yield return new FlatBuffWithSkillsRow(
                            PlayerDetails.LogId,
                            ev.PlayerId,
                            player.Name,
                            player.PlayerEsoId,
                            GetPlayerRole(player.Id),
                            string.Join(", ", player.Specs),
                            buff.BuffId,
                            buff.BuffName,
                            null,
                            null);
                    }
                }
            }


            string GetPlayerRole(int playerId)
            {
                if (PlayerDetails.Healers.Any(p => p.Id == playerId)) return "healer";
                if (PlayerDetails.Tanks.Any(p => p.Id == playerId)) return "tank";
                if (PlayerDetails.Dps.Any(p => p.Id == playerId)) return "dps";
                return "unknown";
            }
        }
    }
}
