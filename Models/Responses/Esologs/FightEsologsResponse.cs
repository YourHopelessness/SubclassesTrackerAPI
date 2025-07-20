using Newtonsoft.Json;

namespace SubclassesTracker.Api.Models.Responses.Esologs
{
    /// <summary>
    /// Represents a fight in the game.
    /// </summary>
    public sealed record FightEsologsResponse
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

        /// <summary>
        /// Is boss killed in this fight.
        /// </summary>
        [JsonProperty("kill")]
        public bool Killed { get; set; }
    }
}
