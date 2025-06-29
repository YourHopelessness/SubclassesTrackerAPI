using SubclassesTrackerExtension;
using SubclassesTrackerExtension.EsologsServices;
using SubclassesTrackerExtension.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IGetDataService, GetDataService>();
builder.Services.AddScoped<IReportDataService, ReportDataService>();
builder.Services.AddSingleton<TokenStorage>();
builder.Services.AddHttpClient();
builder.Services.Configure<LinesConfig>(builder.Configuration.GetSection(nameof(LinesConfig)));
builder.Services.AddHostedService<BackgroundDataCollector>();
builder.Services.AddGraphQLClient();
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.MapOAuthEndpoints();

app.Run();
