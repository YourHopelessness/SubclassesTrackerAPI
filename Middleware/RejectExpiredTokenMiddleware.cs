using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;

namespace SubclassesTracker.Api.Middleware
{
    public sealed class EnsureFreshTokenMiddleware(RequestDelegate next)
    {
        private static readonly JwtSecurityTokenHandler Handler = new();

        public async Task InvokeAsync(HttpContext ctx)
        {
            try
            {
                if (IsAnonymous(ctx))
                {
                    await next(ctx);
                    return;
                }

                if (!TryGetBearerToken(ctx, out var token))
                {
                    WritePlain(ctx, 401, "missing bearer");
                    return;
                }

                if (!IsTokenFresh(token))
                {
                    WritePlain(ctx, 401, "token expired");
                    return;
                }

                await next(ctx);
            }
            catch (Exception ex)
            {
                ctx.RequestServices
                   .GetRequiredService<ILogger<EnsureFreshTokenMiddleware>>()
                   .LogError(ex, "Unhandled exception after token middleware.");
                throw;
            }
        }

        private static bool IsAnonymous(HttpContext ctx)
            => ctx.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() is not null;

        private static bool TryGetBearerToken(HttpContext ctx, out string token)
        {
            token = null!;
            var raw = ctx.Request.Headers.Authorization.FirstOrDefault();
            if (raw is null || !raw.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return false;
            token = raw[7..];
            return true;
        }

        private static bool IsTokenFresh(string token)
        {
            JwtSecurityToken jwt;
            try
            {
                jwt = Handler.ReadJwtToken(token);
            }
            catch
            {
                return false;
            }
            return jwt.ValidTo >= DateTime.UtcNow.AddMinutes(-1);
        }

        private static void WritePlain(HttpContext ctx, int status, string text)
        {
            ctx.Response.StatusCode = status;
            ctx.Response.ContentType = "text/plain; charset=utf-8";
            ctx.Response.WriteAsync(text);
        }
    }
}
