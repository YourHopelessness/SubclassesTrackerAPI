namespace SubclassesTracker.Api.Middleware
{

    using System.Diagnostics;
    using System.Net;
    using System.Text.Json;

    public class OtelExceptionHandlerMiddleware(RequestDelegate next,
        ILogger<OtelExceptionHandlerMiddleware> logger)
    {
        public async Task Invoke(HttpContext ctx)
        {
            try
            {
                await next(ctx);
            }
            catch (Exception ex)
            {
                var activity = Activity.Current;

                if (activity != null)
                {
                    activity.AddEvent(
                        new ActivityEvent(
                             "exception",
                             tags: new ActivityTagsCollection
                             {
                                 { "exception.type", ex.GetType().FullName },
                                 { "exception.message", ex.Message },
                                 { "exception.stacktrace", ex.StackTrace ?? "" }
                             }));

                    activity.SetTag("otel.status_code", "ERROR");
                    activity.SetTag("otel.status_description", ex.Message);
                }

                logger.LogError(ex, "Unhandled exception");

                if (!ctx.Response.HasStarted)
                {
                    ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    ctx.Response.ContentType = "application/json";

                    var payload = JsonSerializer.Serialize(new
                    {
                        error = "Something went wrong..."
                    });

                    await ctx.Response.WriteAsync(payload);
                }
            }
        }
    }
}