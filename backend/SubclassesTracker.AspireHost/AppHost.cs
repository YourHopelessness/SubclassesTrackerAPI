using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SubclassesTracker.AspireHost.Config;
using SubclassesTracker.AspireHost.Services;

var builder = DistributedApplication.CreateBuilder(args);

// PgSQL container
PgConfig config = new();
builder.Configuration.Bind(PgConfig.SectionName, config);

var initUser = builder.AddParameter("username", config.InitUser, secret: true);
var initPass = builder.AddParameter("password", config.InitUserPassword, secret: true);

var pg = builder.AddPostgres("postgres", initUser, initPass, config.Port);

if (config.Persist)
    pg.WithDataVolume();

//init db script
builder.Services.AddHostedService<PgScriptInit>();

// Connection string for other pgsdql
var newConnectionString =
    new Npgsql.NpgsqlConnectionStringBuilder()
    {
        Host = config.Host,
        Port = config.Port,
        Username = config.User,
        Password = config.UserPassword,
        Database = config.Database
    }
    .ToString();

// API Service
CacheSettings cacheSettings = new();
builder.Configuration.Bind(CacheSettings.SectionName, cacheSettings);

var api = builder.AddProject<Projects.SubclassesTracker_Api>("api")
    .WithEnvironment("ConnectionStrings__ParquetCache", newConnectionString)
    .WithEnvironment("CachingSettings__CacheRootPath", cacheSettings.CachePath)
    .WaitFor(pg);

api.WithEndpoint("api-http", e =>
{
    e.UriScheme = "http";
    e.Port = 0;
});
api.WithHttpHealthCheck("/api/HealthCheck/health");

// Cleaner Service
var cleaner = builder.AddProject<Projects.SubclassesTracker_CleanupService>("cleaner")
    .WithReference(pg)
    .WithEnvironment("ConnectionStrings__ParquetCache", newConnectionString)
    .WithEnvironment("CachingSettings__CacheRootPath", cacheSettings.CachePath)
    .WaitFor(pg);

// Build Aspire Host
builder.Build().Run();