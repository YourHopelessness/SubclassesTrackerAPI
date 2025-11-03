using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SubclassesTracker.Caching.Parquet;
using SubclassesTracker.Caching.Services.ObjectSerilization;
using SubclassesTracker.Database.CacheEntities;
using SubclassesTracker.Database.Context;
using System.Collections;
using System.Text.Json;

namespace SubclassesTracker.Caching.Services
{
    public interface IParquetCacheService
    {
        /// <summary>
        /// Saves data to Parquet cache
        /// </summary>
        Task SaveToCacheAsync<T>(T data,
           int datasetId,
           string queryName,
           string? partitionPath,
           string hash,
           string varsJson,
           bool forceRefresh = false,
           CancellationToken token = default);

        /// <summary>
        /// Reads data from Parquet cache
        /// </summary>
        Task<T?> LoadFromCacheAsync<T>(
           string queryName,
           string hash,
           CancellationToken token = default) where T : class, new();
    }

    public class ParquetCacheService(
        ILogger<ParquetCacheService> logger,
        IObjectFlattener objectFlattener,
        IDynamicParquetWriter dynamicParquetWriter,
        IDynamicParquetReader dynamicParquetReader,
        IOptions<CachingSettings> options,
        ParquetCacheContext context) : IParquetCacheService
    {
        private readonly CachingSettings cachingSettings = options.Value;

        public async Task<T?> LoadFromCacheAsync<T>(string queryName, string hash, CancellationToken token = default)
            where T : class, new()
        {
            var snapshot = await context.RequestSnapshots
                .Include(r => r.FileEntries)
                .ThenInclude(f => f.Partition)
                .Include(r => r.FileEntries)
                .ThenInclude(f => f.Dataset)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.QueryName == queryName && r.VarsHash == hash, token);

            if (snapshot is null)
                return null;

            var file = snapshot.FileEntries
                .OrderByDescending(f => f.CachedAt)
                .FirstOrDefault(f => f.CachedAt + TimeSpan.FromSeconds(f.Ttl) > DateTimeOffset.UtcNow);

            if (file == null)
                return null;

            // Determine if T is a collection type
            var tType = typeof(T);
            var isCollection = tType.IsGenericType && tType.GetGenericTypeDefinition() == typeof(List<>);
            var elementType = isCollection ? tType.GetGenericArguments()[0] : tType;

            var readerType = typeof(DynamicParquetReader);
            var readMethod = readerType.GetMethod(nameof(DynamicParquetReader.ReadTypedAsync))!
                .MakeGenericMethod(elementType);

            var task = (Task)readMethod.Invoke(dynamicParquetReader, [Path.Combine(cachingSettings.CacheRootPath, file.FullPath), token])!;
            await task.ConfigureAwait(false);

            var resultProperty = task.GetType().GetProperty("Result")!;
            var list = (IList)resultProperty.GetValue(task)!;

            // Return as collection
            if (isCollection)
                return list as T;

            // Return first item or null
            if (list.Count > 0)
                return list[0] as T;

            return null;
        }

        public async Task SaveToCacheAsync<T>(
            T data,
            int datasetId,
            string queryName,
            string? partitionPath,
            string hash,
            string varsJson,
            bool forceRefresh = false,
            CancellationToken token = default)
        {
            var existing = await context.RequestSnapshots
                .Where(x => x.QueryName == queryName && x.VarsHash == hash)
                .AsNoTracking()
                .FirstOrDefaultAsync(token);

            if (existing is not null && !forceRefresh)
                return;

            var dataset = await context.Datasets
                .Include(d => d.Partitions)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == datasetId, token)
                ?? throw new InvalidOperationException("Dataset not found");

            Partition? partition = null;
            if (partitionPath != null)
            {
                partition = dataset.Partitions.FirstOrDefault(p => p.Path.EndsWith(partitionPath ?? ""))
                               ?? new Partition
                               {
                                   DatasetId = dataset.Id,
                                   Path = partitionPath ?? string.Empty
                               };

                if (partition.Id == 0)
                    context.Partitions.Add(partition);
            }

            var rows = data switch
            {
                null => [],
                IList => ((IList)data).Cast<object?>()
                    .Select(item => objectFlattener.Flatten(item))
                    .ToList(),
                _ => [objectFlattener.Flatten(data)]
            };

            bool isEmpty = rows.Count == 0 || rows.All(r => r.Count == 0);

            var snapshot = new RequestSnapshot
            {
                CreatedAt = DateTimeOffset.UtcNow,
                QueryName = queryName,
                VarsHash = hash,
                VarsJson = varsJson,
                FileEntries = []
            };

            if (!isEmpty)
            {
                // Write Parquet file
                var filename = $"part-{Guid.NewGuid():N}.parquet";
                var fullPath = Path.Combine(cachingSettings.CacheRootPath, dataset.RootPath, 
                    partition is not null 
                        ? Path.Combine(partition!.Path, filename) 
                        : filename);

                await dynamicParquetWriter.WriteAsync(rows, fullPath, ct: token);

                var fileEntry = new FileEntry
                {
                    FileName = filename,
                    Hash = hash,
                    Size = new FileInfo(fullPath).Length,
                    CachedAt = DateTimeOffset.UtcNow,
                    DatasetId = dataset.Id,
                    Partition = partition
                };

                snapshot.FileEntries.Add(fileEntry);
            }
            else
            {
                logger.LogInformation("Skipping file write: empty snapshot for query {QueryName}", queryName);
            }

            context.RequestSnapshots.Add(snapshot);
            await context.SaveChangesAsync(token);
        }
    }
}
