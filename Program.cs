using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using SubclassesTracker.Database.Context;
using SubclassesTracker.Database.Entity;
using SubclassesTracker.Database.Repository;
using SubclassesTrackerExtension;
using SubclassesTrackerExtension.BackgroundQueue;
using SubclassesTrackerExtension.BackgroundQueue.HostedService;
using SubclassesTrackerExtension.BackgroundQueue.Jobs;
using SubclassesTrackerExtension.BackgroundQueue.JobStatuses;
using SubclassesTrackerExtension.EsologsServices;
using SubclassesTrackerExtension.Extensions;

Batteries.Init();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IGetDataService, GetDataService>();
builder.Services.AddScoped<IReportDataService, ReportDataService>();
builder.Services.AddSingleton<TokenStorage>();
builder.Services.AddScoped<IBaseRepository<SkillLine>, BaseRepository<SkillLine>>();
builder.Services.AddScoped<IBaseRepository<SkillTreeEntry>, BaseRepository<SkillTreeEntry>>();
builder.Services.AddScoped<IBaseRepository<Zone>, BaseRepository<Zone>>();
builder.Services.AddScoped<IBaseRepository<Encounter>, BaseRepository<Encounter>>();
builder.Services.AddHttpClient();
builder.Services.Configure<LinesConfig>(builder.Configuration.GetSection(nameof(LinesConfig)));
builder.Services.AddDbContext<EsoContext>(options =>
    options.UseSqlite("Data Source=" +
        builder.Configuration.GetSection("LinesConfig:SkillLinesDb").Value));

// Register the background queue and hosted service
builder.Services.AddHostedService<QueuedHostedService>();
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddSingleton<IJobMonitor, JobMonitor>();
// Register the job
builder.Services.AddScoped<JobDataCollection>();

builder.Services.AddGraphQLClient();
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.MapOAuthEndpoints();

app.Run();
