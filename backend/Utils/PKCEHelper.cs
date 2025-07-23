using System.Security.Cryptography;
using System.Text;

namespace SubclassesTracker.Api.Utils
{
    /// <summary>
    /// Helper for pkce auth
    /// </summary>
    public class PKCEHelper
    {
        /// <summary>
        /// Generate the code verifier for OAuth PKCE auth
        /// </summary>
        /// <returns>base64 fof the code verifier</returns>
        public static string GenerateCodeVerifier()
        {
            Span<byte> bytes = stackalloc byte[32];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        /// <summary>
        /// Generate the code challanger for OAuth PKCE auth
        /// </summary>
        /// <param name="verifier">code verifier</param>
        /// <returns>base64 string of the code challanger</returns>
        public static string GenerateCodeChallenge(string verifier)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(verifier));
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
}
