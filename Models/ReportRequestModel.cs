using Newtonsoft.Json;

namespace SubclassesTrackerExtension.Models
{
    /// <summary>
    /// Represents a request model for fetching reports, containing a list of reports and pagination information.
    /// </summary>
    public class ReportRequestModel
    {
        /// <summary>
        /// List of reports retrieved by the request.
        /// </summary>
        public List<ReportModel> Data { get; set; } = [];

        /// <summary>
        /// Indicates whether there are more pages of reports available for retrieval.
        /// </summary>
        [JsonProperty("has_more_pages")]
        public bool HasMorePages { get; set; } = false;
    }
}
