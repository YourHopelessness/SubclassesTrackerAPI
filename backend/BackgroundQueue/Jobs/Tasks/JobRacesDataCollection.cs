using Microsoft.EntityFrameworkCore;
using SubclassesTracker.Api.BackgroundQueue.Jobs.JobParameters;
using SubclassesTracker.Api.BackgroundQueue.JobStatuses;
using SubclassesTracker.Api.EsologsServices.Reports;
using SubclassesTracker.Api.ExcelServices;
using SubclassesTracker.Api.Extensions;
using SubclassesTracker.Api.Models.Responses.Api;
using SubclassesTracker.Database.Entity;
using SubclassesTracker.Database.Repository;

namespace SubclassesTracker.Api.BackgroundQueue.Jobs.Tasks
{
    public class JobRacesDataCollection(
        IBaseRepository<Zone> repository,
        IReportRacialDataService reportRacialDataService,
        ILogger<JobRacesDataCollection> logger,
        IJobMonitor jobMonitor) : IJob<RacialDataCollectionApiResponse, EsologsParams>
    {
        public async Task<RacialDataCollectionApiResponse> RunAsync(EsologsParams jobParameters, CancellationToken ct)
        {
            logger.LogInformation("Races data collection begin");

            var zones = await repository.GetList(x => x.Type.Name == "Trial")
                .Include(x => x.ZoneDifficulties)
                .ToListAsync(cancellationToken: ct);

            var result = new RacialDataCollectionApiResponse();
            var trialStats = new List<RacialReportApiResponse>();
            var errors = new List<string>();

            foreach (var zone in zones)
            {
                try
                {
                    var difficulty = zone.ZoneDifficulties.Where(x => x.IsHardMode == 1).FirstOrDefault();

                    result.ZoneNames.Add(zone.Name);

                    trialStats.AddRange(await reportRacialDataService.GetRacesDataAsync(zone.Id, difficulty?.DifficultyId ?? 0, token: ct));

                    jobMonitor.TryUpdate(jobParameters.JobId, prev => ((JobInfo<RacialDataCollectionApiResponse, EsologsParams>)prev) with
                    {
                        State = JobStatusEnum.Running,
                        Progress = (int)Math.Floor(((double)zones.IndexOf(zone) + 1) / zones.Count * 100),
                        Result = new RacialDataCollectionApiResponse
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

            trialStats.Add(new RacialReportApiResponse
            {
                TrialName = "All Zones",
                DdRacesQuantity = trialStats.Select(x => x.DdRacesQuantity).MergeDictionaries(),
                HealerRacesQuantity = trialStats.Select(x => x.HealerRacesQuantity).MergeDictionaries(),
                TankRacesQuantity = trialStats.Select(x => x.TankRacesQuantity).MergeDictionaries()
            });
            result.RacesData = ExcelExporterService.ExportRacesDataToExcel(trialStats);

            return result;
        }
    }
}
