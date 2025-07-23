namespace SubclassesTracker.Api.Models.Responses.Api
{
    /// <summary>
    /// Represents a skill line in the game.
    /// </summary>
    public sealed record SkillLinesApiResponse
    {
        /// <summary>
        /// Unique identifier for the skill line.
        /// </summary>
        public string LineName { get; set; } = string.Empty;

        /// <summary>
        /// Count of unique skills that used by players.
        /// </summary>
        public int UniqueSkillsCount { get; set; }

        /// <summary>
        /// How many Players used this line
        /// </summary>
        public int PlayersUsingThisLine { get; set; }

        /// <summary>
        /// List of skills in the skill line.
        /// </summary>
        public List<SkillApiResponse> Skills { get; set; } = [];
    }

    
}
