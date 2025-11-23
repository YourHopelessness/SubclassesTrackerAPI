using Newtonsoft.Json;

namespace SubclassesTracker.Models.Responses.Esologs
{
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
}
