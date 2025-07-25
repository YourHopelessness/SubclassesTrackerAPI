﻿namespace SubclassesTracker.Api
{
    /// <summary>
    /// Configuration class for service settings.
    /// </summary>
    public class LinesConfig
    {
        /// <summary> The time slice for the trial start time. </summary>
        public long TrialStartTimeSlice { get; set; }

        /// <summary> The URL for the ESO Logs API. </summary>
        public string EsoLogsApiUrl { get; set; } = string.Empty;

        /// <summary> The endpoint for OAuth authentication. </summary>
        public string AuthEndpoint { get; set; } = string.Empty;

        /// <summary> The endpoint for obtaining access tokens. </summary>
        public string TokenEndpoint { get; set; } = string.Empty;
    }
}
