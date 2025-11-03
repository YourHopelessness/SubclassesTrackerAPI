using Microsoft.Extensions.Logging;

namespace SubclassesTracker.Models.Dto
{
    public record QraphQlExecutor(
        string ApiUrl,
        ILogger Logger,
        IHttpClientFactory Factory);
}
