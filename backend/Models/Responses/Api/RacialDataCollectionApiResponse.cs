namespace SubclassesTracker.Api.Models.Responses.Api
{
    public class RacialDataCollectionApiResponse
    {
        /// <summary>
        /// Unique identifier for the report.
        /// </summary>
        public List<string> ZoneNames { get; set; } = [];
        /// <summary>
        /// List of skill lines models representing the skills used by dds in the trial.
        /// </summary>
        public byte[] RacesData { get; set; } = [];
    }
}
