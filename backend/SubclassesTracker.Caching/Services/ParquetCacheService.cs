using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SubclassesTracker.Database.CacheEntities;
using SubclassesTracker.Database.Context;
using SubclassesTracker.Database.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SubclassesTracker.Caching.Services
{
    public interface IParquetCacheService
    {
        Task SaveToCacheAsync<T>(T data,
           int datasetId,
           string queryName,
           string? partitionPath,
           string hash,
           bool forceRefresh = false,
           CancellationToken token = default);

        Task<T?> LoadFromCacheAsync<T>(
           string filePath,
           CancellationToken token = default) where T : new();
    }

    public class ParquetCacheService(
        IBaseRepository<RequestSnapshot> requestSnapshotsRepository) : IParquetCacheService
    {
        public Task<T?> LoadFromCacheAsync<T>(string filePath, CancellationToken token = default) where T : new()
        {
            throw new NotImplementedException();
        }

        public async Task SaveToCacheAsync<T>(
            T data, 
            int datasetId, 
            string queryName, 
            string? partitionPath, 
            string hash, 
            bool forceRefresh = false, 
            CancellationToken token = default)
        {
            var cache = await requestSnapshotsRepository.GetByParam(x => 
                x.QueryName == queryName
                && x.VarsHash == hash,
                token);

            if (cache != null && !forceRefresh)
            {
                
            }
        }
    }
}
