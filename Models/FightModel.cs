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
        /// Name of the fight.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Score of the trial, if applicable.
        /// </summary>
        public int? TrialScore { get; set; }
    }
}
