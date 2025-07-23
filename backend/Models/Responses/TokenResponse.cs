using Newtonsoft.Json;

namespace SubclassesTracker.Api.Models.Responses
{
    /// <summary>
    /// Token response
    /// </summary>
    public sealed record TokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; } = "";

        [JsonProperty("token_type")]
        public string TokenType { get; set; } = "";

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; } = "";

        [JsonProperty("expires_at")]
        public DateTime ExpiresAt { get; set; }
    }
}
