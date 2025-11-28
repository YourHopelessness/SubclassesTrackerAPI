using Microsoft.EntityFrameworkCore;
using SubclassesTracker.Database.CacheEntities;
using SubclassesTracker.Database.Context;
using SubclassesTracker.GraphQL.GraphQLClient;

namespace SubclassesTracker.Api.Utils.Seed
{
    public static class DbInitializer
    {
        public static async Task SeedDatasetsAsync(
            IServiceProvider services, CancellationToken token)
        {
            using var scope = services.CreateAsyncScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
                .CreateLogger("DbInitializer");

            logger.LogInformation("Create user");
            var scriptInit = scope.ServiceProvider.GetRequiredService<PgScriptInit>();
            await scriptInit.InitDatabase(token);

            logger.LogInformation("Migrate the database");
            var context = scope.ServiceProvider.GetRequiredService<ParquetCacheContext>();
            await context.Database.MigrateAsync(token);

            logger.LogInformation("Seed datasets");
            var seedData = GraphQLQueries.QueryDatasets;
            foreach (var kvp in seedData)
            {
                var name = kvp.Key.ToString();
                var id = kvp.Value;

                var existing = await context.Datasets.FirstOrDefaultAsync(x => x.Name == name, token);

                if (existing is null)
                {
                    // Insert new record
                    var ds = new Dataset
                    {
                        Id = id,
                        Name = name,
                        Version = 1,
                        RootPath = $"datasets/{name}",
                        CreatedAt = DateTimeOffset.UtcNow
                    };

                    context.Datasets.Add(ds);
                    logger.LogInformation("Added new dataset {Name} (v{Version})", name, ds.Version);
                }
                else if (existing.Id != id)
                {
                    // Id changed, update record
                    existing.Id = id;
                    existing.Version += 1;
                    existing.CreatedAt = DateTimeOffset.UtcNow;

                    context.Datasets.Update(existing);
                    logger.LogInformation("Updated dataset {Name} to v{Version}", name, existing.Version);
                }
                else
                {
                    logger.LogDebug("Dataset {Name} is up to date (v{Version})", name, existing.Version);
                }
            }

            await context.SaveChangesAsync(token);
        }
    }
}
