using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using StrawberryShake;
using SubclassesTrackerExtension.EsologsServices;
using System.Collections;
using System.Net.Http.Headers;
using System.Text;

namespace SubclassesTrackerExtension.Extensions
{
    public static class GraphQLExtensions
    {
        /// <summary>
        /// Execute safe graphQL request
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="clientMethod"></param>
        /// <param name="strawberryCall"></param>
        /// <param name="accessToken"></param>
        /// <param name="queryFilePath"></param>
        /// <param name="variables"></param>
        /// <param name="resultJsonPath"></param>
        /// <returns></returns>
        public static async Task<TResult> ExecuteQueryWithFallbackAsync<TResult>(
            this IOperationRequestFactory clientMethod,
            string accessToken,
            string queryFilePath,
            object variables,
            string resultJsonPath,
            string cacheName,
            string apiUrl,
            ILogger logger,
            IHttpClientFactory factory)
                where TResult : class, new()
        {
            var report = await CacheService.LoadReportsFromCacheAsync<TResult>(cacheName, logger);
            if (report == null || (report is IList && (report as IList).Count == 0))
            {
                try
                {
                    string queryText = await File.ReadAllTextAsync(queryFilePath);

                    using var http = factory.CreateClient();
                    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    var body = new
                    {
                        query = queryText,
                        variables
                    };

                    var jsonBody = JsonConvert.SerializeObject(body, new JsonSerializerSettings
                    {
                        Converters = { new StringEnumConverter() },
                        NullValueHandling = NullValueHandling.Ignore
                    });
                    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                    var response = await http.PostAsync(apiUrl, content);
                    while (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        // If we hit rate limit, wait and retry
                        logger.LogInformation("Rate limit reached, wait 1 minute");
                        await Task.Delay(TimeSpan.FromMinutes(2));

                        response = await http.PostAsync(apiUrl, content);
                    }
                    var raw = await response.Content.ReadAsStringAsync();

                    var parsed = JsonConvert.DeserializeObject<JToken>(raw);
                    var entries = parsed?.SelectToken(resultJsonPath);

                    var result = entries?.ToObject<TResult>();
                    if (result != null)
                    {
                        // If we got a result, save it to cache
                        await CacheService.SaveReportsToCacheAsync(result, cacheName, logger, true);
                    }

                    return result ?? new();
                }
                catch (ArgumentException ex) when (ex.Message.Contains("0x00"))
                {
                    logger.LogWarning("GraphQL query file {QueryFile} contains invalid characters.", queryFilePath);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to execute GraphQL query from {QueryFile} with variables {@Variables}.", queryFilePath, variables);
                }
            }

            return report ?? new();
        }

        public static async Task<List<T>> QueryAllPagesAsync<T>(
           Func<int, Task<(List<T> items, bool hasMore)>> pageFetcher,
           Func<T, bool> predicate,
           int startPage = 1)
        {
            var allItems = new List<T>();
            int page = startPage;
            bool hasMore;

            do
            {
                var (items, more) = await pageFetcher(page);
                if (items != null)
                {
                    allItems.AddRange(items.Where(predicate));
                }
                hasMore = more;
                page++;
            }
            while (hasMore);

            return allItems;
        }
    }
}
