using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using SubclassesTracker.Api.EsologsServices;
using SubclassesTracker.Api.GraphQLClient;
using SubclassesTracker.Api.Models.Dto;
using System.Collections;
using System.Text;
using static SubclassesTracker.Api.GraphQLClient.GraphQLQueries;

namespace SubclassesTracker.Api.Extensions
{
    public static class GraphQLExtensions
    {
        /// <summary>
        /// Execute safe graphQL request
        /// </summary>
        /// <typeparam name="TResult">Result model from query</typeparam>
        /// <typeparam name="TVars">Type of the value params</typeparam>
        /// <param name="queryName">Name of the query</param>
        /// <param name="variables">Variables for the request</param>
        /// <returns></returns>
        public static async Task<TResult> QueryAsync<TResult, TVars>(
            this QraphQlExecutor executeParams,
            GraphQlQueryEnum queryName,
            TVars variables)
                where TResult : class, new()
                where TVars : class
        {
            return await ExecuteQueryWithFallbackAsync<TVars, TResult>(
                "GraphQLClient/Queries/" + queryName.ToString() + ".graphql",
                variables,
                GraphQLQueries.QueryRootPathResponse[queryName],
                variables.FlatAnonToHashString("Saves/" + queryName.ToString() + "/" + queryName.ToString(), includeDate: false) + ".json",
                executeParams.apiUrl,
                executeParams.logger,
                executeParams.factory);
        }

        private static async Task<TResult> ExecuteQueryWithFallbackAsync<TVars, TResult>(
            string queryFilePath,
            TVars variables,
            string resultJsonPath,
            string cacheName,
            string apiUrl,
            ILogger logger,
            IHttpClientFactory factory)
                where TResult : class, new()
                where TVars : class
        {
            var report = await CacheService.LoadReportsFromCacheAsync<TResult>(cacheName, logger);
            if (report == null || report is IList && (report as IList).Count == 0)
            {
                try
                {
                    string queryText = await File.ReadAllTextAsync(queryFilePath);

                    using var http = factory.CreateClient("Esologs");

                    var body = new GraphQlRequest<TVars>(queryText, variables);

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
                        logger.LogInformation("Rate limit reached, wait 5 minute");
                        await Task.Delay(TimeSpan.FromMinutes(5));

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
