
namespace SubclassesTrackerExtension.Models
{
    /// <summary>
    /// Represents the result of data collection for skill lines in trials.
    /// </summary>
    public class DataCollectionResultModel
    {
        /// <summary>
        /// Unique identifier for the report.
        /// </summary>
        public List<string> ZoneNames { get; set; } = [];
        /// <summary>
        /// List of skill lines models representing the skills used by dds in the trial.
        /// </summary>
        public byte[] LinesStats { get; set; } = [];
        /// <summary>
        /// List of skill lines models representing the skills used by dds in the trial with cense.
        /// </summary>
        public byte[] LinesStatsWithScore { get; set; } = [];
    }
}
