using Microsoft.Extensions.Logging;
using SubclassesTracker.Caching.Services;

namespace SubclassesTracker.GraphQL.GraphQLClient
{
    public record QraphQlExecutor(
        string ApiUrl,
        ILogger Logger,
        IHttpClientFactory Factory,
        IParquetCacheService ParquetCacheService);
}
