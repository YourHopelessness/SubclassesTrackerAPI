using SubclassesTrackerExtension.ExcelServices;

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
            using var scope = serviceProvider.CreateScope();
            var dataService = scope.ServiceProvider.GetRequiredService<IReportDataService>();

            logger.LogInformation("Data collect begin");

            try
            {
                for (int i = 19; i > 0; i--)
                {
                    ExcelParserService.ExportToExcel(await dataService.GetSkillLinesAsync(i), "Save/lines.xlsx");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during data collection");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
