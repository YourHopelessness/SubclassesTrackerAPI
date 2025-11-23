using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Timeout;
using SubclassesTracker.GraphQL.Services;
using System.Net;

namespace SubclassesTracker.GraphQL
{
    /// <summary>
    /// Provides an extension method to register the Esologs HttpClient
    /// with built-in resilience using Polly (retry + circuit breaker).
    /// </summary>
    public static class HttpClientBuilderExtensions
    {
        private readonly static int[] retries = [1, 2, 5, 5, 10];
        /// <summary>
        /// Registers a named HttpClient ("Esologs") with exponential retry,
        /// 429 (TooManyRequests) handling, and a circuit breaker.
        /// </summary>
        public static IHttpClientBuilder AddEsologsHttpClient(
            this IServiceCollection services,
            IConfiguration? configuration = null)
        {
            return services.AddHttpClient<IGraphQLClientExecutor, GraphQLClientExecutor>((sp, client) =>
            {
                var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("EsologsHttpClient");

                // Base URL
                var apiUrl = configuration?["LinesConfig:EsoLogsApiUrl"];

                client.BaseAddress = new Uri(apiUrl);
                client.Timeout = TimeSpan.FromMinutes(retries.Sum());
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                logger.LogInformation("Esologs HttpClient configured with base address: {Url}", apiUrl);
            })
            // Add our custom Polly resilience policy
            .AddPolicyHandler((sp, _) =>
            {
                var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("EsologsPollyPolicy");
                return CreateResiliencePolicy(logger);
            });
        }

        /// <summary>
        /// Creates a composite Polly policy: exponential retry (including 429)
        /// and a circuit breaker to prevent hammering an unstable API.
        /// </summary>
        private static IAsyncPolicy<HttpResponseMessage> CreateResiliencePolicy(ILogger logger)
        {
            // --- Retry policy ---
            var retryPolicy = Policy<HttpResponseMessage>
                 .Handle<HttpRequestException>()
                 .OrResult(r => r.StatusCode == HttpStatusCode.RequestTimeout ||
                                (int)r.StatusCode >= 500 ||
                                r.StatusCode == (HttpStatusCode)429)
                 .WaitAndRetryAsync(
                     retryCount: 5,
                     sleepDurationProvider: attempt => TimeSpan.FromMinutes(retries[attempt - 1]),
                     onRetryAsync: async (outcome, delay, attempt, _) =>
                     {
                         var status = outcome.Result?.StatusCode.ToString() ?? "exception";
                         logger.LogWarning(
                             "Retry {Attempt} due to {Status}. Waiting {Delay:F1}s before next attempt.",
                             attempt, status, delay.TotalSeconds);
                         await Task.CompletedTask;
                     });

            // --- Circuit breaker policy ---
            var circuitBreakerPolicy = Policy<HttpResponseMessage>
                  .Handle<HttpRequestException>()
                  .OrResult(r => r.StatusCode == (HttpStatusCode)429 || (int)r.StatusCode >= 500)
                  .CircuitBreakerAsync(
                      handledEventsAllowedBeforeBreaking: 5,
                      durationOfBreak: TimeSpan.FromMinutes(1),
                      onBreak: (outcome, breakDelay) =>
                      {
                          var status = outcome.Result?.StatusCode.ToString() ?? "exception";
                          logger.LogError(
                              "Circuit opened for {Delay:F0}s due to {Status}. Further requests will be blocked.",
                              breakDelay.TotalSeconds, status);
                      },
                      onReset: () => logger.LogInformation("Circuit closed. Normal operation resumed."),
                      onHalfOpen: () => logger.LogInformation("Circuit half-open. Testing API availability...")
                  );

            // --- Timeout policy ---
            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(
               TimeSpan.FromMinutes(60), // 1 hour timeout
               TimeoutStrategy.Pessimistic,
               onTimeoutAsync: (context, timespan, task) =>
               {
                   logger.LogWarning("Timeout after {Seconds}s", timespan.TotalSeconds);
                   return Task.CompletedTask;
               });

            // Combine retry + breaker
            return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy);
        }
    }
}
