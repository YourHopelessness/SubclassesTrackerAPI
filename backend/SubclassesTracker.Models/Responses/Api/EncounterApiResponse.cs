namespace SubclassesTracker.Models.Responses.Api
{
    /// <summary>
    /// Represents an encounter in the game.
    /// </summary>
    public sealed record EncounterApiResponse
    {
        /// <summary>
        /// Unique identifier for the encounter.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the encounter.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// The "good" score.
        /// </summary>
        public int ScoreCense { get; set; }
    }
}