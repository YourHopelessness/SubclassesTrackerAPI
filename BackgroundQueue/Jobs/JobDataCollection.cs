using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.EntityFrameworkCore;
using SubclassesTracker.Database.Entity;
using SubclassesTracker.Database.Repository;
using SubclassesTrackerExtension.BackgroundQueue.JobStatuses;
using SubclassesTrackerExtension.EsologsServices;
using SubclassesTrackerExtension.ExcelServices;
using SubclassesTrackerExtension.Models;
using System.Text.Json;
using System.Threading;

namespace SubclassesTrackerExtension.BackgroundQueue.Jobs
{
    public class JobDataCollection(
        IReportDataService dataService,
        IBaseRepository<Zone> repository,
        ILogger<JobDataCollection> logger,
        IJobMonitor jobMonitor) : IJob<DataCollectionResultModel>
    {
        public async Task<DataCollectionResultModel> RunAsync(Guid id, CancellationToken ct)
        {
            logger.LogInformation("Data collect begin");

            var zones = await repository.GetList(x => x.Type.Name == "Trial")
                .Include(x => x.ZoneDifficulties)
                .ToListAsync(cancellationToken: ct);

            var result = new DataCollectionResultModel();
            var trialStats = new List<SkillLineReportModel>();
            var trialStatsWithScore = new List<SkillLineReportModel>();
            var errors = new List<string>();

            foreach (var zone in zones)
            {
                try
                {
                    var difficulty = zone.ZoneDifficulties.Where(x => x.IsHardMode == 1).FirstOrDefault();

                    result.ZoneNames.Add(zone.Name);

                    trialStats.AddRange(await dataService.GetSkillLinesAsync(zone.Id, difficulty?.DifficultyId ?? 0, token: ct));
                    trialStatsWithScore.AddRange(await dataService.GetSkillLinesAsync(zone.Id, difficulty?.DifficultyId ?? 0, true, token: ct));

                    jobMonitor.TryUpdate(id, prev => ((JobInfo<DataCollectionResultModel>)prev) with
                    {
                        State = JobStatusEnum.Running,
                        Progress = (int)Math.Floor((double)zones.IndexOf(zone) / zones.Count * 100),
                        Result = new DataCollectionResultModel
                        {
                            ZoneNames = result.ZoneNames
                        }
                    });
                }
                catch (Exception ex)
                {
                    var errorMessage = $"Error collecting data for zone {zone.Name}: {ex.Message}";
                    errors.Add(errorMessage);
                    logger.LogError(ex, errorMessage);
                }
            }

            result.LinesStats = ExcelParserService.ExportToExcel(trialStats);
            result.LinesStatsWithScore = ExcelParserService.ExportToExcel(trialStatsWithScore);

            if (errors.Count > 0)
            {
                var errorMessage = "Data collection completed with errors:\n" + string.Join("\n", errors);
                throw new PartialSuccessException<DataCollectionResultModel>(result, errorMessage);
            }

            logger.LogInformation("Data collect ended");
           
            return result;
        }
    }
}
