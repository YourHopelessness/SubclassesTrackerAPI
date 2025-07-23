namespace SubclassesTracker.Api.Models.Dto
{
    public record QraphQlExecutor(
        string apiUrl,
        ILogger logger,
        IHttpClientFactory factory);
}
