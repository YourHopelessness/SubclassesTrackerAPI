using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace SubclassesTrackerExtension.EsologsServices
{
    public class TokenStorage
    {
        private Token _token;
        private readonly LinesConfig config;
        public TokenStorage(IOptions<LinesConfig> opt)
        {
            config = opt.Value;
            if (File.Exists(config.TokenFilePath))
            {
                var json = File.ReadAllText(config.TokenFilePath);
                _token = JsonConvert.DeserializeObject<Token>(json) 
                    ?? throw new ArgumentNullException("Token file is empty");
            }
            
        }

        public void UpdateToken(Token token)
        {
            _token = token;
            if (File.Exists(config.TokenFilePath))
            {
                File.WriteAllText(config.TokenFilePath, JsonConvert.SerializeObject(token, Formatting.Indented));
            }
        }

        public bool IsTokenValid()
        {
            return !string.IsNullOrEmpty(_token.AccessToken) && DateTime.UtcNow.AddSeconds(5) < _token.ExpiresAt;
        }

        public async Task<string> GetToken()
        {
            if (!IsTokenValid())
            {
                _token = await RefreshAccessTokenAsync(_token.RefreshToken) 
                    ?? throw new HttpRequestException("Failed to get new token");
            }

            return _token.AccessToken;
        }

        private async Task<Token?> RefreshAccessTokenAsync(string refreshToken)
        {
            using var client = new HttpClient();

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "client_id", config.ClientId },
                { "refresh_token", refreshToken }
            });

            var response = await client.PostAsync("https://www.esologs.com/oauth/token", content);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var tokenData = JsonConvert.DeserializeObject<Token>(json);

            return tokenData;
        }
    }

    public class Token
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; } = "";

        [JsonProperty("token_type")]
        public string TokenType { get; set; } = "";

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; } = "";

        public DateTime ExpiresAt => DateTime.UtcNow.AddSeconds(ExpiresIn - 60); // с запасом
    }
}
