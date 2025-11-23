using Microsoft.AspNetCore.Mvc;

namespace SubclassesTracker.Models.Requests.Api
{
    public sealed record AuthUriApiRequests
    {
        /// <summary>
        /// The esologs client Id, creates on https://www.esologs.com/api/clients
        /// </summary>
        [FromQuery(Name = "clientId")]
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// The callback url for complete the auth and get the token
        /// </summary>
        [FromQuery(Name = "redirectUri")]
        public string RedirectUrl { get; set; } = string.Empty;
    }
}
