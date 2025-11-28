using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SubclassesTracker.AspireHost.Config;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("subclassestracker");

// PgSQL container
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile(
        $"appsettings.{builder.Environment.EnvironmentName}.json",
        optional: true,
        reloadOnChange: true);

PgConfig config = new();
builder.Configuration.Bind(PgConfig.SectionName, config);

var initUser = builder.AddParameter("defaultUser", config.InitUser, secret: true);
var initPass = builder.AddParameter("defaultPass", config.InitUserPassword, secret: true);

var pg = builder
    .AddPostgres("postgres", port: config.Port)
    .WithUserName(initUser)
    .WithPassword(initPass)
    .WithDataVolume();

// Default Connection
var defaultStr =
    new Npgsql.NpgsqlConnectionStringBuilder()
    {
        Host = config.Host,
        Port = config.Port,
        Username = config.InitUser,
        Password = config.InitUserPassword,
        Database = config.InitDatabase
    }
    .ToString();
var defaultConn = builder.AddParameter("defaultConnString", defaultStr);

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
var connString = builder.AddParameter("connString", newConnectionString);

// API Service
CacheSettings cacheSettings = new();
builder.Configuration.Bind(CacheSettings.SectionName, cacheSettings);

var api = builder.AddProject<Projects.SubclassesTracker_Api>("api")
    .WithEnvironment("ConnectionStrings__ParquetCache", connString)
    .WithEnvironment("ConnectionStrings__DefaultDbConn", defaultConn)
    .WithEnvironment("CachingSettings__CacheRootPath", cacheSettings.CachePath)
    .WaitFor(pg)
    .PublishAsDockerComposeService((res, ser) =>
    {
        ser.Ports.Add("7192:7192");
        ser.Restart = "always";
    });

api.WithEndpoint("api-http", e =>
{
    e.UriScheme = "https";
    e.Port = 0;
});
api.WithHttpHealthCheck("/api/HealthCheck/health");

// Cleaner Service
var cleaner = builder.AddProject<Projects.SubclassesTracker_CleanupService>("cleaner")
    .WithEnvironment("ConnectionStrings__ParquetCache", connString)
    .WithEnvironment("CachingSettings__CacheRootPath", cacheSettings.CachePath)
    .WaitFor(pg)
    .WaitFor(api);

// Build Aspire Host
builder.Build().Run();