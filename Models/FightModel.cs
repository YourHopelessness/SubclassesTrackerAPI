using Newtonsoft.Json;

namespace SubclassesTrackerExtension.Models
{
    /// <summary>
    /// Represents a fight in the game.
    /// </summary>
    public class FightModel
    {
        /// <summary>
        /// Unique identifier for the fight.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Identifier for the encounter associated with the fight.
        /// </summary>
        [JsonProperty("encounterID")]
        public int EncounterId { get; set; }

        /// <summary>
        /// Name of the fight.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Score of the trial, if applicable.
        /// </summary>
        public int? TrialScore { get; set; }
    }
}
