using Microsoft.EntityFrameworkCore;
using SubclassesTracker.Database.Entity;
using SubclassesTracker.Database.Repository;
using SubclassesTrackerExtension.ExcelServices;
using System.Text.Json;

namespace SubclassesTrackerExtension.EsologsServices
{
    public class BackgroundDataCollector(
        ILogger<BackgroundDataCollector> logger, 
        IServiceProvider serviceProvider) : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () => await RunDataCollect(cancellationToken), cancellationToken);

            return Task.CompletedTask;
        }

        private async Task RunDataCollect(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var dataService = scope.ServiceProvider.GetRequiredService<IReportDataService>();
                var repository = scope.ServiceProvider.GetRequiredService<IBaseRepository<Zone>>();

                logger.LogInformation("Data collect begin");
            
                var zones = await repository.GetList(x => x.Type.Name == "Trial")
                    .Include(x => x.ZoneDifficulties)
                    .ToListAsync();

                foreach (var zone in zones)
                {
                    try
                    {
                        var difficulty = zone.ZoneDifficulties.Where(x => x.IsHardMode == 1).FirstOrDefault();

                        var linesStats = await dataService.GetSkillLinesAsync(zone.Id, difficulty?.DifficultyId ?? 0, token: cancellationToken);
                        var linesStatsWithScore = await dataService.GetSkillLinesAsync(zone.Id, difficulty?.DifficultyId ?? 0, true, token: cancellationToken);

                        File.WriteAllText($"Save/Lines_{zone.Name}.json", JsonSerializer.Serialize(linesStats));
                        File.WriteAllText($"Save/LinesWithScore_{zone.Name}.json", JsonSerializer.Serialize(linesStatsWithScore));

                        ExcelParserService.ExportToExcel(linesStats, "Save/lines.xlsx");
                        ExcelParserService.ExportToExcel(linesStatsWithScore, "Save/decent-score-lines.xlsx");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error during data collection for zone {ZoneId}", zone.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during data collection");
            }
            finally
            {
                logger.LogInformation("Data collect ended");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
