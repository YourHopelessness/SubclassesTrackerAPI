namespace SubclassesTracker.Models.Configuration
{
    /// <summary>
    /// Configuration class for service settings.
    /// </summary>
    public class LinesConfig
    {
        /// <summary> The URL for the ESO Logs API. </summary>
        public string EsoLogsApiUrl { get; set; } = string.Empty;

        /// <summary> The endpoint for OAuth authentication. </summary>
        public string AuthEndpoint { get; set; } = string.Empty;

        /// <summary> The endpoint for obtaining access tokens. </summary>
        public string TokenEndpoint { get; set; } = string.Empty;
    }
}
