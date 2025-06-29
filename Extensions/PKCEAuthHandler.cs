using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SubclassesTrackerExtension.EsologsServices;
using System.Security.Cryptography;
using System.Text;

namespace SubclassesTrackerExtension.Extensions
{
    public static class OAuthRoutes
    {
        static readonly string codeVerifier = GenerateCodeVerifier();
        static readonly string codeChallenge = GenerateCodeChallenge(codeVerifier);
        public static void MapOAuthEndpoints(this WebApplication app)
        {
            app.MapGet("/", async context =>
            {
                using var scope = app.Services.CreateScope();
                var opts = scope.ServiceProvider.GetService<IOptions<LinesConfig>>();

                var query = new QueryString()
                    .Add("response_type", "code")
                    .Add("client_id", opts.Value.ClientId)
                    .Add("redirect_uri", opts.Value.LocalCallBackOAuthUri)
                    .Add("code_challenge", codeChallenge)
                    .Add("code_challenge_method", "S256")
                    .Add("scope", "view-user-profile view-private-reports");

                var authUrl = $"{opts.Value.AuthEndpoint}{query}";

                Console.WriteLine("Redirecting to: " + authUrl);

                context.Response.Redirect(authUrl);
            });

            app.MapGet("/auth/callback", async context =>
            {
                using var scope = app.Services.CreateScope();
                var opts = scope.ServiceProvider.GetService<IOptions<LinesConfig>>();

                var code = context.Request.Query["code"];
                if (string.IsNullOrEmpty(code))
                {
                    await context.Response.WriteAsync("No code received.");
                    return;
                }

                var client = new HttpClient();
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "authorization_code" },
                    { "client_id", opts.Value.ClientId },
                    { "redirect_uri", opts.Value.LocalCallBackOAuthUri },
                    { "code", code },
                    { "code_verifier", codeVerifier }
                });

                var response = await client.PostAsync(opts.Value.TokenEndpoint, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(responseBody);

                scope.ServiceProvider
                    .GetRequiredService<TokenStorage>()
                    .UpdateToken(JsonConvert.DeserializeObject<Token>(responseBody) ?? new Token());
            });
        }

        static string GenerateCodeVerifier()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes)
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        static string GenerateCodeChallenge(string codeVerifier)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(codeVerifier));
            return Convert.ToBase64String(bytes)
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
    }

}
