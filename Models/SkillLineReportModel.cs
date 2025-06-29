namespace SubclassesTrackerExtension.Models
{
    /// <summary>
    /// Represents a report model for skills of the players.
    /// </summary>
    public class SkillLineReportModel
    {
        /// <summary>
        /// Unique identifier for the report.
        /// </summary>
        public string TrialName { get; set; } = string.Empty;

        /// <summary>
        /// Unique identifier for the trial.
        /// </summary>
        public int TrialId { get; set; }

        /// <summary>
        /// List of skill lines models representing the skills used by dds in the trial.
        /// </summary>
        public List<SkillLinesModel> DdLinesModels { get; set; } = [];

        /// <summary>
        /// List of skill lines models representing the skills used by healers in the trial.
        /// </summary>
        public List<SkillLinesModel> HealersLinesModels { get; set; } = [];

        /// <summary>
        /// List of skill lines models representing the skills used by tanks in the trial.
        /// </summary>
        public List<SkillLinesModel> TanksLinesModels { get; set; } = [];
    }
}
