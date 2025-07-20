namespace SubclassesTracker.Api.Models.Requests.Api
{
    public sealed record ExchangeApiRequest
    {
        /// <summary>
        /// Client code, recieved from the esologs api
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Redirect-callback url
        /// </summary>
        public string RedirectUri { get; set; } = string.Empty;

        /// <summary>
        /// The Client Id (https://www.esologs.com/api/clients)
        /// </summary>
        public string ClientId { get; set; } = string.Empty;
    }
}
