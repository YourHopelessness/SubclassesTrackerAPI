namespace SubclassesTracker.Api.Models.Responses.Api
{
    /// <summary>
    /// Represents the result of data collection for skill lines in trials.
    /// </summary>
    public sealed record DataCollectionResultApiResponse
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
