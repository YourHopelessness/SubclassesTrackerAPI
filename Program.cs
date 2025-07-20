using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using SubclassesTracker.Api;
using SubclassesTracker.Api.BackgroundQueue;
using SubclassesTracker.Api.BackgroundQueue.HostedService;
using SubclassesTracker.Api.BackgroundQueue.Jobs;
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
const string CorsPolicy = "ExtensionPolicy";
builder.Services.AddCors(opts =>
{
    opts.AddPolicy(CorsPolicy, b => b
        .WithOrigins("chrome-extension://mpldcmnhbilholjaopjhcjfhcimgehjf")
        .WithMethods("GET", "POST")
        .AllowAnyHeader()
        .AllowCredentials()
    );
});

// Register services
builder.Services.AddScoped<IGetDataService, GetDataService>();
builder.Services.AddScoped<IReportDataService, ReportDataService>();

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
builder.Services.AddScoped<JobDataCollection>();

// Add graphql
builder.Services.AddGraphQLClient();

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

app.UseCors(CorsPolicy);

app.UseRouting();
app.UseMiddleware<EnsureFreshTokenMiddleware>();
app.MapControllers();

app.Run();
