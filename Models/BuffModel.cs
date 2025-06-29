using Newtonsoft.Json;

namespace SubclassesTrackerExtension.Models
{
    /// <summary>
    /// Represents a buff in the game, which can be applied to characters or encounters.
    /// </summary>
    public class BuffModel
    {
        /// <summary>
        /// Unique identifier for the buff.
        /// </summary>
        [JsonProperty("guid")]
        public int Id { get; set; }

        /// <summary>
        /// Name of the buff.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the buff.
        /// </summary>
        public string ClassSkillLine { get; set; } = string.Empty;
    }
}
