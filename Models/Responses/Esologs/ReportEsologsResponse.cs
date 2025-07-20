using Newtonsoft.Json;
using SubclassesTracker.Api.Models.Responses.Api;

namespace SubclassesTracker.Api.Models.Responses.Esologs
{
    /// <summary>
    /// Represents a report containing multiple fights within a specific zone.
    /// </summary>
    public sealed record ReportEsologsResponse
    {
        /// <summary>
        /// Log Identifier for the report.
        /// </summary>
        public string Code { get; set; } = "";

        /// <summary>
        /// Zone of the report.
        /// </summary>
        public ZoneApiResponse Zone { get; set; } = new();

        /// <summary>
        /// List of fights within the report, each representing a specific encounter.
        /// </summary>
        [JsonProperty("fights")]
        public List<FightEsologsResponse> Fights { get; set; } = [];
    }
}
