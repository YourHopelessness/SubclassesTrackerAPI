using Newtonsoft.Json;

namespace SubclassesTrackerExtension.Models
{
    /// <summary>
    /// Represents a report containing multiple fights within a specific zone.
    /// </summary>
    public class ReportModel
    {
        /// <summary>
        /// Log Identifier for the report.
        /// </summary>
        public string Code { get; set; } = "";

        /// <summary>
        /// Zone of the report.
        /// </summary>
        public ZoneModel Zone { get; set; } = new();

        /// <summary>
        /// List of fights within the report, each representing a specific encounter.
        /// </summary>
        [JsonProperty("fights")]
        public List<FightModel> Fights { get; set; } = [];
    }
}
