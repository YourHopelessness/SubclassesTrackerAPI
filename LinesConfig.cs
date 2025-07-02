namespace SubclassesTrackerExtension
{
    /// <summary>
    /// Configuration class for service settings.
    /// </summary>
    public class LinesConfig
    {
        /// <summary> The client ID for the service. </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary> The tokenFile for the service. </summary>
        public string TokenFilePath { get; set; } = string.Empty;

        /// <summary> The time slice for the trial start time. </summary>
        public long TrialStartTimeSlice { get; set; }

        /// <summary> The URL for the ESO Logs API. </summary>
        public string EsoLogsApiUrl { get; set; } = string.Empty;

        /// <summary> The local callback URI for OAuth authentication. </summary>
        public string LocalCallBackOAuthUri { get; set; } = string.Empty;

        /// <summary> The endpoint for OAuth authentication. </summary>
        public string AuthEndpoint { get; set; } = string.Empty;

        /// <summary> The endpoint for obtaining access tokens. </summary>
        public string TokenEndpoint { get; set; } = string.Empty;

        /// <summary> The path to the Db file containing skill lines data. </summary>
        public string SkillLinesDb { get; set; } = string.Empty;
    }
}
