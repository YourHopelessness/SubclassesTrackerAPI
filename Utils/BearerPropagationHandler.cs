using System.Net.Http.Headers;

namespace SubclassesTracker.Api.Utils
{
    public sealed class BearerPropagationHandler(IHttpContextAccessor httpContext) : DelegatingHandler
    {
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
        {
            var ctx = httpContext.HttpContext;
            var auth = ctx?.Request.Headers.Authorization.FirstOrDefault();
            if (auth?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true)
            {
                req.Headers.Authorization = AuthenticationHeaderValue.Parse(auth);
            }

            return await base.SendAsync(req, ct);
        }
    }
}
