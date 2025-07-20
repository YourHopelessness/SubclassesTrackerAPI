using Newtonsoft.Json;

namespace SubclassesTracker.Api.EsologsServices
{
    public class CacheService
    {
        public static async Task SaveReportsToCacheAsync<T>(
           T data,
           string filePath,
           ILogger logger,
           bool forceRefresh = false)
        {
            if (!forceRefresh && File.Exists(filePath))
            {
                logger.LogInformation("Cache file {FilePath} already exists, skipping save.", filePath);
                return;
            }

            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            logger.LogInformation("Saving data to cache file: {FilePath}", filePath);
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            await File.WriteAllTextAsync(filePath, json);
        }

        public static async Task<T?> LoadReportsFromCacheAsync<T>(
            string filePath,
            ILogger logger) where T : new()
        {
            if (!File.Exists(filePath))
            {
                logger.LogWarning("Cache file {FilePath} does not exist.", filePath);

                return default;
            }

            logger.LogInformation("Loading data from cache file: {FilePath}", filePath);
            var json = await File.ReadAllTextAsync(filePath);
            var data = JsonConvert.DeserializeObject<T>(json) ?? new();

            return data;
        }
    }
}
