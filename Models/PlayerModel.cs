using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SubclassesTrackerExtension.Extensions;

namespace SubclassesTrackerExtension.Models
{
    /// <summary>
    /// Represents a player model for the ESO Logs API.
    /// </summary>
    public class PlayerListModel
    {
        public string LogId { get; set; } = string.Empty;
        /// <summary>
        /// List of healers in the player list.
        /// </summary>
        [JsonProperty("healers")]
        public List<PlayerModel> Healers { get; set; } = [];

        /// <summary>
        /// List of tanks in the player list.
        /// </summary>
        [JsonProperty("tanks")]
        public List<PlayerModel> Tanks { get; set; } = [];

        /// <summary>
        /// List of damage dealers (DPS) in the player list.
        /// </summary>
        [JsonProperty("dps")]
        public List<PlayerModel> Dps { get; set; } = [];
    }

    public class PlayerModel
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
        public CombatInfoModel CombatInfo
        {
            get
            {
                if (CombatantInfoRaw is null ||
                    CombatantInfoRaw.Type is JTokenType.Null or JTokenType.Undefined)
                    return new CombatInfoModel();

                if (CombatantInfoRaw.Type == JTokenType.Array)
                {
                    var first = CombatantInfoRaw.FirstOrDefault();

                    return first is null ||
                           first.Type is JTokenType.Null or JTokenType.Undefined
                           ? new CombatInfoModel()
                           : first.ToObject<CombatInfoModel>() ?? new CombatInfoModel();
                }

                return CombatantInfoRaw.ToObject<CombatInfoModel>() ?? new CombatInfoModel();
            }
        }
    }

    public class CombatInfoModel
    {
        [JsonProperty("talents")]
        public List<PlayerSkillsModel> Talents { get; set; } = [];
    }

    public class PlayerSkillsModel
    {
        [JsonProperty("guid")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;


    }
}
