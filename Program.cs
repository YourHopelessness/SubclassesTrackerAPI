using Microsoft.EntityFrameworkCore;
using SubclassesTracker.Database.Context;
using SubclassesTracker.Database.Entity;
using SubclassesTracker.Database.Repository;
using SubclassesTrackerExtension;
using SubclassesTrackerExtension.EsologsServices;
using SubclassesTrackerExtension.Extensions;
using SQLitePCL;
using Microsoft.Extensions.Configuration;

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
builder.Services.AddHostedService<BackgroundDataCollector>();
builder.Services.AddGraphQLClient();
builder.Services.AddControllers();

var app = builder.Build();

Console.WriteLine(Path.GetFullPath(builder.Configuration["LinesConfig:SkillLinesDb"]));

app.MapControllers();

app.MapOAuthEndpoints();

app.Run();
