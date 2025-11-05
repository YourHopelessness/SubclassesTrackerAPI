using Newtonsoft.Json;

namespace SubclassesTracker.Models.Responses.Esologs
{
    /// <summary>
    /// Represents a request model for fetching reports, containing a list of reports and pagination information.
    /// </summary>
    public sealed record ReportRequestEsologsResponse
    {
        /// <summary>
        /// List of reports retrieved by the request.
        /// </summary>
        public List<ReportEsologsResponse> Data { get; set; } = [];

        /// <summary>
        /// Indicates whether there are more pages of reports available for retrieval.
        /// </summary>
        [JsonProperty("has_more_pages")]
        public bool HasMorePages { get; set; } = false;
    }
}
