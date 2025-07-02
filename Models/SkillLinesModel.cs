namespace SubclassesTrackerExtension.Models
{
    /// <summary>
    /// Represents a skill line in the game.
    /// </summary>
    public class SkillLinesModel
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
        public List<SkillModel> Skills { get; set; } = [];
    }

    /// <summary>
    /// Represents a skill model for the ESO Logs API.
    /// </summary>
    public sealed class SkillModel
    {
        /// <summary>
        /// Unique identifier for the skill.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the skill.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Type of the skill, indicating its category (like Active or Passive).
        /// </summary>
        public string Type { get; set; } = string.Empty;
    }
}
