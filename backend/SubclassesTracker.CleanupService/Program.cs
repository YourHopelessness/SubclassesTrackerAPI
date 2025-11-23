using Microsoft.EntityFrameworkCore;
using SubclassesTracker.CleanupService;
using SubclassesTracker.Database.Context;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<ParquetCacheContext>(o =>
{
    o.UseNpgsql(builder.Configuration.GetConnectionString("ParquetCache"));
});

builder.Services.AddHostedService<ParquetCleaner>();

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();
builder.Logging.AddOpenTelemetry();

builder.Services.AddOpenTelemetry()
    .WithTracing(t =>
    {
        t.AddSource("ParquetCleaner");
        t.AddHttpClientInstrumentation();
    })
    .WithMetrics(m =>
    {
        m.AddMeter("ParquetCleaner");
        m.AddRuntimeInstrumentation();
    })
    .UseOtlpExporter();

var host = builder.Build();
host.Run();
