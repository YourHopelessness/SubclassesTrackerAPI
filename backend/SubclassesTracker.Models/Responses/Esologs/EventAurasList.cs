using Newtonsoft.Json;

namespace SubclassesTracker.Models.Responses.Esologs
{
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
