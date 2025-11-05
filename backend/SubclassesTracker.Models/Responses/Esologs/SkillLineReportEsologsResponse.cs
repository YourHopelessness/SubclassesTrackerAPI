using SubclassesTracker.Models.Responses.Api;

namespace SubclassesTracker.Models.Responses.Esologs
{
    /// <summary>
    /// Represents a report model for skills of the players.
    /// </summary>
    public sealed record SkillLineReportEsologsResponse
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
        public List<SkillLinesApiResponse> DdLinesModels { get; set; } = [];

        /// <summary>
        /// List of skill lines models representing the skills used by healers in the trial.
        /// </summary>
        public List<SkillLinesApiResponse> HealersLinesModels { get; set; } = [];

        /// <summary>
        /// List of skill lines models representing the skills used by tanks in the trial.
        /// </summary>
        public List<SkillLinesApiResponse> TanksLinesModels { get; set; } = [];
    }

    public sealed record SkillLineReportResults
    {
        /// <summary>
        /// Without score cense applied
        /// </summary>
        public SkillLineReportEsologsResponse WithoutCense { get; set; }

        /// <summary>
        /// With score cense applied
        /// </summary>
        public SkillLineReportEsologsResponse WithCense { get; set; }
    }
}
