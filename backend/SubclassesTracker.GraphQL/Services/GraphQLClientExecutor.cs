using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using SubclassesTracker.Caching;
using SubclassesTracker.Caching.Services;
using SubclassesTracker.Models;
using SubclassesTracker.Models.Enums;
using System.Text;
using static SubclassesTracker.GraphQL.GraphQLClient.GraphQLQueries;

namespace SubclassesTracker.GraphQL.Services
{
    public interface IGraphQLClientExecutor
    {
        /// <summary>
        /// Query graphQL endpoint safely
        /// </summary>
        /// <typeparam name="TResult">Type of the result model</typeparam>
        /// <typeparam name="TVars">Variables type</typeparam>
        /// <param name="queryName">Name of the query</param>
        /// <param name="variables">Variables for the request</param>
        /// <param name="partitionPath">Partition path for caching</param>
        /// <param name="forceRefresh">Force refresh cache</param>
        /// <param name="token">Token for cancellation</param>
        /// <returns>Result model from the query</returns>
        Task<TResult> QueryAsync<TResult, TVars>(
            GraphQlQueryEnum queryName,
            TVars variables,
            string? partitionPath = null,
            bool forceRefresh = false,
            CancellationToken token = default)
                where TResult : class, new()
                where TVars : class;
    }

    public class GraphQLClientExecutor(
        IOptions<LinesConfig> config,
        ILogger<GraphQLClientExecutor> logger,
        HttpClient client,
        IParquetCacheService cache,
        IQueryLoader queries) : IGraphQLClientExecutor
    {
        private readonly string apiUrl = config.Value.EsoLogsApiUrl;

        /// <summary>
        /// Execute safe graphQL request
        /// </summary>
        /// <typeparam name="TResult">Result model from query</typeparam>
        /// <typeparam name="TVars">Type of the value params</typeparam>
        /// <param name="queryName">Name of the query</param>
        /// <param name="variables">Variables for the request</param>
        /// <returns></returns>
        public async Task<TResult> QueryAsync<TResult, TVars>(
            GraphQlQueryEnum queryName,
            TVars variables,
            string? partitionPath = null,
            bool forceRefresh = false,
            CancellationToken token = default)
                where TResult : class, new()
                where TVars : class
        {
            var cacheKey = VarsHashHelper.ComputeHash(variables);

            var cached = await cache.LoadFromCacheAsync<TResult>(
                queryName.ToString(), cacheKey, token);

            if (cached != null)
                return cached;

            var query = await queries.LoadAsync(queryName.ToString(), token);
            var result = await ExecuteInternalAsync<TResult, TVars>(query, variables, QueryRootPathResponse[queryName], token);

            if (result != null)
            {
                await cache.SaveToCacheAsync(
                    result,
                    QueryDatasets[queryName],
                    queryName.ToString(),
                    partitionPath,
                    cacheKey,
                    System.Text.Json.JsonSerializer.Serialize(variables),
                    forceRefresh,
                    token);
            }

            return result ?? new();
        }

        private async Task<TResult?> ExecuteInternalAsync<TResult, TVars>(
            string queryText,
            TVars vars,
            string jsonPath,
            CancellationToken token)
                where TResult : class, new()
                where TVars : class
        {
            try
            {
                var body = new GraphQlRequest<TVars>(queryText, vars);
                var content = new StringContent(
                    JsonConvert.SerializeObject(body, new JsonSerializerSettings
                    {
                        Converters = { new StringEnumConverter() },
                        NullValueHandling = NullValueHandling.Ignore
                    }),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync(apiUrl, content, token);

                var raw = await response.Content.ReadAsStringAsync(token);

                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException($"Bad status: {response.StatusCode}");

                var parsed = JsonConvert.DeserializeObject<JToken>(raw);
                var data = parsed?.SelectToken(jsonPath);

                return data?.ToObject<TResult>() ?? new();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GraphQL error");

                return null;
            }
        }

        /// <summary>
        /// Query all pages from a paginated graphQL endpoint
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="pageFetcher">Function to fetch a page</param>
        /// <param name="predicate">Condition to filter items</param>
        /// <param name="startPage">Starting page number</param>
        /// <returns>List of resulting models</returns>
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