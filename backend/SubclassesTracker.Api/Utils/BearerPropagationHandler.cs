using System.Net.Http.Headers;

namespace SubclassesTracker.Api.Utils
{
    public sealed class BearerPropagationHandler(IHttpContextAccessor httpContext) : DelegatingHandler
    {
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
        {
            var ctx = httpContext.HttpContext;

            var auth = GetAuthFromContext(ctx);
            if (!string.IsNullOrEmpty(auth))
            {
                req.Headers.Authorization = AuthenticationHeaderValue.Parse(auth);
            }
            else if (!string.IsNullOrEmpty(TaskExecutionContext.AccessToken))
            {
                req.Headers.Authorization = AuthenticationHeaderValue.Parse(TaskExecutionContext.AccessToken);
            }

            return await base.SendAsync(req, ct);
        }

        public static string GetAuthFromContext(HttpContext? ctx)
        {
            var auth = ctx?.Request.Headers.Authorization.FirstOrDefault();
            if (auth?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true)
            {
                return auth;
            }

            return string.Empty;
        }
    }
}
