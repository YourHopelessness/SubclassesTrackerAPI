using Npgsql;

namespace SubclassesTracker.Api.Utils.Seed
{
    public class PgScriptInit(IServiceProvider sp)
    {
        public async Task InitDatabase(CancellationToken ct)
        {
            using var scope = sp.CreateScope();
            var configuration = scope.ServiceProvider
                .GetRequiredService<IConfiguration>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<PgScriptInit>>();

            var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

            var sqlPath = Path.Combine(env.ContentRootPath, "Utils", "Seed", "Scripts");

            var defaultConn = configuration.GetConnectionString("DefaultDbConn");
            using var conn = new NpgsqlConnection(defaultConn);
            await conn.OpenAsync(ct);

            var appConn = configuration.GetConnectionString("ParquetCache");
            var builder = new NpgsqlConnectionStringBuilder(appConn);
            var appDb = builder.Database;
            var appUser = builder.Username;
            var appPassword = builder.Password;

            var files = Directory.GetFiles(sqlPath, "*.sql").OrderBy(x => x);
            logger.LogInformation("Found {Count} SQL scripts to execute", files.Count());

            foreach (var file in files)
            {
                var sql = await File.ReadAllTextAsync(file, ct);

                sql = sql
                    .Replace("${UserName}", appUser)
                    .Replace("${UserPassword}", appPassword)
                    .Replace("${DatabaseName}", appDb);

                using var cmd = new NpgsqlCommand(sql, conn);
                var rows = await cmd.ExecuteNonQueryAsync(ct);
            }
        }
    }
}
