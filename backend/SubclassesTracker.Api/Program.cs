using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using FluentValidation;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using SQLitePCL;
using SubclassesTracker.Api.BackgroundQueue;
using SubclassesTracker.Api.BackgroundQueue.HostedService;
using SubclassesTracker.Api.BackgroundQueue.Jobs.Tasks;
using SubclassesTracker.Api.BackgroundQueue.JobStatuses;
using SubclassesTracker.Api.EsologsServices;
using SubclassesTracker.Api.EsologsServices.Reports;
using SubclassesTracker.Api.Middleware;
using SubclassesTracker.Api.Utils;
using SubclassesTracker.Caching;
using SubclassesTracker.Caching.Parquet;
using SubclassesTracker.Caching.Services;
using SubclassesTracker.Caching.Services.ObjectSerilization;
using SubclassesTracker.Database.Context;
using SubclassesTracker.Database.Entity;
using SubclassesTracker.Database.Repository;
using SubclassesTracker.GraphQL;
using SubclassesTracker.GraphQL.Services;
using SubclassesTracker.Models;

// For SQLite
Batteries.Init();

var builder = WebApplication.CreateBuilder(args);

// Configire the entire application conf
builder.Services.Configure<LinesConfig>(builder.Configuration.GetSection(nameof(LinesConfig)));
builder.Services.Configure<CachingSettings>(builder.Configuration.GetSection(nameof(CachingSettings)));

// Add CORS policy for the Chrome extension
const string ExtensionCorsPolicy = "ExtensionPolicy";
const string EsologsCorsPolicy = "EsologsCorsPolicy";
builder.Services.AddCors(opts =>
{
    opts.AddPolicy(ExtensionCorsPolicy, b => b
        .WithOrigins(builder.Configuration.GetSection("LinesConfig:ChromeExtensionID").Value!)
        .WithMethods("GET", "POST")
        .AllowAnyHeader()
        .AllowCredentials()
    );
});
builder.Services.AddCors(opts =>
{
    opts.AddPolicy("EsologsCorsPolicy", b => b
        .SetIsOriginAllowed(origin =>
        {
            if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                return false;

            return uri.Scheme == Uri.UriSchemeHttps &&
                   (uri.Host.Equals("esologs.com", StringComparison.OrdinalIgnoreCase) ||
                    uri.Host.EndsWith(".esologs.com", StringComparison.OrdinalIgnoreCase));
        })
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});

// Register graphql services
builder.Services.AddScoped<IGraphQLClientExecutor, GraphQLClientExecutor>();
builder.Services.AddScoped<IQueryLoader, EmbeddedQueryLoader>();
builder.Services.AddScoped<IGraphQLGetService, GraphQLGetService>();

// Register services
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IReportSubclassesDataService, ReportSubclassesDataService>();
builder.Services.AddScoped<IReportRacialDataService, ReportRacialDataService>();
builder.Services.AddScoped<ILoaderService, LoaderService>();

// Register the controller Validation
builder.Services.AddValidatorsFromAssemblyContaining<RequiredZonesForSpecificJobTypesAttribute>();
builder.Services.AddFluentValidationAutoValidation();

// Register caching services
builder.Services.AddObjectFlattening();
builder.Services.AddTransient<IParquetCacheService, ParquetCacheService>();
builder.Services.AddTransient<IDynamicParquetWriter, DynamicParquetWriter>();
builder.Services.AddTransient<IDynamicParquetReader, DynamicParquetReader>();
builder.Services.AddDbContextPool<ParquetCacheContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("ParquetCache"));
});

// Register token validator
builder.Services.AddHttpContextAccessor();
builder.Services.AddEsologsHttpClient(builder.Configuration)
       .AddHttpMessageHandler<BearerPropagationHandler>();
builder.Services.AddTransient<BearerPropagationHandler>();

// Register database dependencies
builder.Services.AddDbContext<EsoContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SkillLinesDb")));
builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

// Register the background queue and hosted service
builder.Services.AddHostedService<QueuedHostedService>();
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddSingleton<IJobMonitor, JobMonitor>();
// Register the job
builder.Services.AddScoped<JobSubclassesDataCollection>();
builder.Services.AddScoped<JobRacesDataCollection>();

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

// Apply migrations automatically
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var parquetDb = services.GetRequiredService<ParquetCacheContext>();
        await parquetDb.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating databases");
    }
}

app.UseCors(ExtensionCorsPolicy);
app.UseCors(EsologsCorsPolicy);

if (!app.Environment.IsDevelopment())
{
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });

    app.UseHsts();
}

app.UseRouting();
app.UseMiddleware<EnsureFreshTokenMiddleware>();
app.MapControllers();

app.Run();