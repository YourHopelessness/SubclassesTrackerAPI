namespace SubclassesTracker.Models.Responses.Api
{
    public class RacialReportApiResponse
    {
        /// <summary>
        /// Unique identifier for the report.
        /// </summary>
        public string TrialName { get; set; } = string.Empty;

        /// <summary>
        /// Dict of races representing the races used by dds in the trial.
        /// </summary>
        public Dictionary<string, int> DdRacesQuantity { get; set; } = [];

        /// <summary>
        /// Dict of races representing the races used by healers in the trial.
        /// </summary>
        public Dictionary<string, int> HealerRacesQuantity { get; set; } = [];

        /// <summary>
        /// Dict of races representing the races used by tanks in the trial.
        /// </summary>
        public Dictionary<string, int> TankRacesQuantity { get; set; } = [];
    }
}
