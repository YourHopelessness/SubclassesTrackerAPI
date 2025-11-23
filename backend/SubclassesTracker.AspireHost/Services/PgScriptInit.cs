using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Npgsql;
using SubclassesTracker.AspireHost.Config;

namespace SubclassesTracker.AspireHost.Services
{
    public class PgScriptInit(IServiceProvider sp) : IHostedService
    {
        public async Task StartAsync(CancellationToken ct)
        {
            using var scope = sp.CreateScope();

            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            PgConfig cfg = new();
            configuration.Bind("Postgres", cfg);

            var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

            var rootConnectionString = new NpgsqlConnectionStringBuilder()
            {
                Host = cfg.Host,
                Port = cfg.Port,
                Database = cfg.InitDatabase,
                Username = cfg.InitUser,
                Password = cfg.InitUserPassword
            }.ToString();

            var appUser = cfg.User;
            var appPassword = cfg.UserPassword;
            var appDb = cfg.Database;

            var sqlPath = Path.Combine(env.ContentRootPath, "Services", "Scripts");

            using var conn = new NpgsqlConnection(rootConnectionString);
            await conn.OpenAsync(ct);

            foreach (var file in Directory.GetFiles(sqlPath, "*.sql").OrderBy(x => x))
            {
                var sql = await File.ReadAllTextAsync(file, ct);

                sql = sql
                    .Replace("${UserName}", appUser)
                    .Replace("${UserPassword}", appPassword)
                    .Replace("${DatabaseName}", appDb);

                using var cmd = new NpgsqlCommand(sql, conn);
                await cmd.ExecuteNonQueryAsync(ct);
            }
        }

        public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
    }
}
