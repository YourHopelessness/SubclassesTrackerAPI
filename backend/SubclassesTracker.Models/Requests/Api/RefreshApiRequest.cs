namespace SubclassesTracker.Models.Requests.Api
{
    public sealed record RefreshApiRequest
    {
        /// <summary>
        /// The refresh token
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// The esologs client id (https://www.esologs.com/api/clients)
        /// </summary>
        public string ClientId { get; set; } = string.Empty;
    }
}
