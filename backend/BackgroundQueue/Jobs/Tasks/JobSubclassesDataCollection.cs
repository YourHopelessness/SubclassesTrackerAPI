using Microsoft.EntityFrameworkCore;
using SubclassesTracker.Api.BackgroundQueue.Jobs.JobParameters;
using SubclassesTracker.Api.BackgroundQueue.JobStatuses;
using SubclassesTracker.Api.EsologsServices.Reports;
using SubclassesTracker.Api.ExcelServices;
using SubclassesTracker.Api.Models.Responses.Api;
using SubclassesTracker.Api.Models.Responses.Esologs;
using SubclassesTracker.Database.Entity;
using SubclassesTracker.Database.Repository;

namespace SubclassesTracker.Api.BackgroundQueue.Jobs.Tasks
{
    public class JobSubclassesDataCollection(
        IReportSubclassesDataService dataService,
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
            var trialStats = new List<SkillLineReportEsologsResponse>();
            var trialStatsWithScore = new List<SkillLineReportEsologsResponse>();
            var errors = new List<string>();

            foreach (var zone in zones)
            {
                try
                {
                    var difficulty = zone.ZoneDifficulties.Where(x => x.IsHardMode == 1).FirstOrDefault();

                    result.ZoneNames.Add(zone.Name);

                    trialStats.AddRange(await dataService.GetSkillLinesAsync(
                        zone.Id, difficulty?.DifficultyId ?? 0, startTime: parameters.StartSliceTime, endTime: parameters.EndSliceTime, token: ct));
                    trialStatsWithScore.AddRange(await dataService.GetSkillLinesAsync(
                        zone.Id, difficulty?.DifficultyId ?? 0, parameters.StartSliceTime, parameters.EndSliceTime, true, token: ct));

                    jobMonitor.TryUpdate(parameters.JobId, prev => ((JobInfo<SubclassesDataCollectionApiResponse, EsologsParams>)prev) with
                    {
                        State = JobStatusEnum.Running,
                        Progress = (int)Math.Floor((double)zones.IndexOf(zone) / zones.Count * 100),
                        Result = new SubclassesDataCollectionApiResponse
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

                    result.ZoneNames.Remove(zone.Name);
                }
            }

            trialStats.Add(SummarizeTrialStats(trialStats));
            trialStatsWithScore.Add(SummarizeTrialStats(trialStatsWithScore));

            result.LinesStats = ExcelExporterService.ExportSubclassesDataToExcel(trialStats);
            result.LinesStatsWithScore = ExcelExporterService.ExportSubclassesDataToExcel(trialStatsWithScore);

            if (errors.Count > 0)
            {
                var errorMessage = "Data collection completed with errors:\n" + string.Join("\n", errors);
                throw new PartialSuccessException<SubclassesDataCollectionApiResponse>(result, errorMessage);
            }

            logger.LogInformation("Data collect ended");

            return result;
        }

        /// <summary>
        /// Make sum of all trial stats
        /// </summary>
        private static SkillLineReportEsologsResponse SummarizeTrialStats(
            List<SkillLineReportEsologsResponse> trialStats)
        {
            static List<SkillLinesApiResponse> SumLines(IEnumerable<SkillLinesApiResponse> lines)
            {
                return [.. lines
                    .GroupBy(x => x.LineName)
                    .Select(x => new SkillLinesApiResponse
                    {
                        LineName = x.Key,
                        PlayersUsingThisLine = x.Sum(y => y.PlayersUsingThisLine),
                        UniqueSkillsCount = x.Max(y => y.UniqueSkillsCount),
                        Skills = [.. x.SelectMany(y => y.Skills).Distinct()]
                    })];
            }

            return new SkillLineReportEsologsResponse
            {
                TrialName = "All Zones",
                DdLinesModels = SumLines(trialStats.SelectMany(x => x.DdLinesModels)),
                HealersLinesModels = SumLines(trialStats.SelectMany(x => x.HealersLinesModels)),
                TanksLinesModels = SumLines(trialStats.SelectMany(x => x.TanksLinesModels))
            };
        }
    }
}
