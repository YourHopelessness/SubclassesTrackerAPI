using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using SQLitePCL;
using SubclassesTracker.Api;
using SubclassesTracker.Api.BackgroundQueue;
using SubclassesTracker.Api.BackgroundQueue.HostedService;
using SubclassesTracker.Api.BackgroundQueue.Jobs.Tasks;
using SubclassesTracker.Api.BackgroundQueue.JobStatuses;
using SubclassesTracker.Api.EsologsServices;
using SubclassesTracker.Api.EsologsServices.Reports;
using SubclassesTracker.Api.Middleware;
using SubclassesTracker.Api.Utils;
using SubclassesTracker.Database.Context;
using SubclassesTracker.Database.Entity;
using SubclassesTracker.Database.Repository;

// For SQLite
Batteries.Init();

var builder = WebApplication.CreateBuilder(args);

// Configire the entire application conf
builder.Services.Configure<LinesConfig>(builder.Configuration.GetSection(nameof(LinesConfig)));

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

// Register services
builder.Services.AddScoped<IGraphQLGetService, GraphQLGetService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IReportSubclassesDataService, ReportSubclassesDataService>();
builder.Services.AddScoped<IReportRacialDataService, ReportRacialDataService>();
builder.Services.AddScoped<ILoaderService, LoaderService>();

// Register the controller Validation
builder.Services.AddValidatorsFromAssemblyContaining<RequiredZonesForSpecificJobTypesAttribute>();
builder.Services.AddFluentValidationAutoValidation();

// Register token validator
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("Esologs")
       .AddHttpMessageHandler<BearerPropagationHandler>();
builder.Services.AddTransient<BearerPropagationHandler>();

// Register database dependencies
builder.Services.AddDbContext<EsoContext>(options =>
    options.UseSqlite("Data Source=" +
        builder.Configuration.GetSection("LinesConfig:SkillLinesDb").Value));
// Register the repositories
builder.Services.AddScoped<IBaseRepository<SkillLine>, BaseRepository<SkillLine>>();
builder.Services.AddScoped<IBaseRepository<SkillTreeEntry>, BaseRepository<SkillTreeEntry>>();
builder.Services.AddScoped<IBaseRepository<Zone>, BaseRepository<Zone>>();
builder.Services.AddScoped<IBaseRepository<Encounter>, BaseRepository<Encounter>>();

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

app.UseCors(ExtensionCorsPolicy);
app.UseCors(EsologsCorsPolicy);

app.UseRouting();
app.UseMiddleware<EnsureFreshTokenMiddleware>();
app.MapControllers();

app.Run();
