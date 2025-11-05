using Microsoft.EntityFrameworkCore;
using SubclassesTracker.Api.BackgroundQueue.Jobs.JobParameters;
using SubclassesTracker.Api.BackgroundQueue.JobStatuses;
using SubclassesTracker.Api.EsologsServices.Reports;
using SubclassesTracker.Api.ExcelServices;
using SubclassesTracker.Database.Entity;
using SubclassesTracker.Database.Repository;
using SubclassesTracker.Models.Enums;
using SubclassesTracker.Models.Responses.Api;
using System.Collections.Concurrent;

namespace SubclassesTracker.Api.BackgroundQueue.Jobs.Tasks
{
    public class JobSubclassesDataCollection(
        IServiceProvider provider,
        IBaseRepository<Zone> repository,
        ILogger<JobSubclassesDataCollection> logger,
        IJobMonitor jobMonitor) : IJob<SubclassesDataCollectionApiResponse, EsologsParams>
    {
        public async Task<SubclassesDataCollectionApiResponse> RunAsync(EsologsParams parameters, CancellationToken ct)
        {
            logger.LogInformation("Subclasses data collection begin");

            var zones = await repository.GetList(x => x.Type.Name == "Trial")
                .Include(x => x.ZoneDifficulties)
                .ToListAsync(cancellationToken: ct);

            var result = new SubclassesDataCollectionApiResponse();
            var errors = new ConcurrentBag<string>();

            // Create temp directories
            var tempDir = Path.Combine(Path.GetTempPath(), "subclasses-temp");
            Directory.CreateDirectory(tempDir);

            var tempDirScore = Path.Combine(Path.GetTempPath(), "subclasses-temp-score");
            Directory.CreateDirectory(tempDirScore);

            // No more tha 2 paeallel tasks to avoid overwhelming the CPU
            var parallelOptions = new ParallelOptions
            {
                CancellationToken = ct,
                MaxDegreeOfParallelism = 2
            };

            var accumulator = new SummaryAccumulator();
            var accumulatorWithScore = new SummaryAccumulator();

            var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

            // Execute data collection per zone in parallel
            await Parallel.ForEachAsync(zones, parallelOptions, async (zone, token) =>
            {
                try
                {
                    // Create a new scope for each parallel task
                    var scope = scopeFactory.CreateScope();
                    var dataService = scope.ServiceProvider.GetRequiredService<IReportSubclassesDataService>();

                    var difficulty = zone.ZoneDifficulties.FirstOrDefault(x => x.IsHardMode == 1);
                    var zoneNameSafe = SanitizeFileName(zone.Name);

                    var trialStats = await dataService.GetSkillLinesAsync(
                        zone.Id, difficulty?.DifficultyId ?? 0,
                        startTime: parameters.StartSliceTime,
                        endTime: parameters.EndSliceTime,
                        token: token);

                    // Export per-zone Excel
                    var singleFilePath = Path.Combine(tempDir, $"{zoneNameSafe}.xlsx");
                    ExcelExporterService.ExportTrialSheet(trialStats.WithoutCense, singleFilePath);

                    var singleFilePathScore = Path.Combine(tempDirScore, $"{zoneNameSafe}-score.xlsx");
                    ExcelExporterService.ExportTrialSheet(trialStats.WithCense, singleFilePathScore);

                    accumulator.AddTrial(trialStats.WithoutCense);
                    accumulatorWithScore.AddTrial(trialStats.WithCense);

                    logger.LogInformation("Zone {Zone} exported to {Path} and {PathScore}", zone.Name, singleFilePath, singleFilePathScore);

                    lock (result.ZoneNames)
                        result.ZoneNames.Add(zone.Name);

                    // Update job progress
                    jobMonitor.TryUpdate(parameters.JobId, prev =>
                        ((JobInfo<SubclassesDataCollectionApiResponse, EsologsParams>)prev) with
                        {
                            State = JobStatusEnum.Running,
                            Progress = (int)Math.Floor((double)(zones.IndexOf(zone) + 1) / zones.Count * 100),
                            Result = new SubclassesDataCollectionApiResponse
                            {
                                ZoneNames = result.ZoneNames
                            }
                        });
                }
                catch (Exception ex)
                {
                    var msg = $"Error collecting data for zone {zone.Name}: {ex.Message}";
                    errors.Add(msg);
                    logger.LogError(ex, msg);
                }
            });

            // Make All Zones summary sheets
            ExcelExporterService.ExportAllZonesSheet(
                accumulator.BuildSummary(),
                Path.Combine(tempDir, $"allZones.xlsx"));
            ExcelExporterService.ExportAllZonesSheet(
                accumulatorWithScore.BuildSummary(),
                Path.Combine(tempDirScore, $"allZones_score.xlsx"));

            // Merge all zone Excels into one
            var mergedPath = Path.Combine(tempDir, $"subclasses_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            ExcelExporterService.MergeExcels(tempDir, mergedPath);

            var mergedPathScore = Path.Combine(tempDirScore, $"subclasses_score_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            ExcelExporterService.MergeExcels(tempDirScore, mergedPathScore);

            result.LinesStats = await File.ReadAllBytesAsync(mergedPath, ct);
            result.LinesStatsWithScore = await File.ReadAllBytesAsync(mergedPathScore, ct);

            logger.LogInformation("Data collection complete, Excel files ready ({Bytes} bytes, and {BytesScore} bytes with score)",
                result.LinesStats.Length,
                result.LinesStatsWithScore.Length);

            if (!errors.IsEmpty)
            {
                var errMsg = "Data collection completed with errors:\n" + string.Join("\n", errors);
                throw new PartialSuccessException<SubclassesDataCollectionApiResponse>(result, errMsg);
            }

            logger.LogInformation("Data collection finished successfully, Excel saved to {Path}", mergedPath);

            return result;
        }

        /// <summary>
        /// Sanitize file name by replacing invalid characters
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string SanitizeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }
    }
}
