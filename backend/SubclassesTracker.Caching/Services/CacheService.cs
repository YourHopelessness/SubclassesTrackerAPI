using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SubclassesTracker.Caching.Services
{
    public class CacheService
    {
        public static async Task SaveReportsToCacheAsync<T>(
           T data,
           string filePath,
           ILogger logger,
           bool forceRefresh = false,
           CancellationToken token = default)
        {
            lock (filePath)
            {
                if (token.IsCancellationRequested)
                {
                    logger.LogWarning("Operation cancelled while trying to save cache file: {FilePath}", filePath);
                    return;
                }
                if (!forceRefresh && File.Exists(filePath))
                {
                    logger.LogInformation("Cache file {FilePath} already exists, skipping save.", filePath);

                    return;
                }
            }

            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            logger.LogInformation("Saving data to cache file: {FilePath}", filePath);
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);

            await File.WriteAllTextAsync(filePath, json, token);
        }

        public static async Task<T?> LoadReportsFromCacheAsync<T>(
            string filePath,
            ILogger logger,
            CancellationToken token = default) where T : new()
        {
            lock (filePath)
            {
                if (!File.Exists(filePath))
                {
                    return default;
                }
            }

            logger.LogInformation("Loading data from cache file: {FilePath}", filePath);
            var json = await File.ReadAllTextAsync(filePath, token);
            var data = JsonConvert.DeserializeObject<T>(json) ?? new();

            return data;
        }
    }
}
