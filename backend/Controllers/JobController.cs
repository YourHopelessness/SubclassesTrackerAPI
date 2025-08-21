using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SubclassesTracker.Api.BackgroundQueue;
using SubclassesTracker.Api.BackgroundQueue.Jobs;
using SubclassesTracker.Api.BackgroundQueue.Jobs.JobParameters;
using SubclassesTracker.Api.BackgroundQueue.Jobs.Tasks;
using SubclassesTracker.Api.BackgroundQueue.JobStatuses;
using SubclassesTracker.Api.Extensions;
using SubclassesTracker.Api.Models.Requests.Api;
using SubclassesTracker.Api.Models.Responses.Api;
using SubclassesTracker.Api.Utils;
using System.ComponentModel.DataAnnotations;

namespace SubclassesTracker.Api.Controllers
{
    [Route("api/[controller]")]
    public class JobController(
        IJobMonitor monitor,
        IBackgroundTaskQueue queue) : ControllerBase
    {
        /// <summary>
        /// Retrieves the status of a job by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:guid}")]
        public IActionResult Status([FromRoute] Guid id)
        {
            if (!monitor.TryGet(id, out var jobinfo))
                return NotFound("Job with id not found");

            return Ok(jobinfo);
        }

        /// <summary>
        /// Retrieves the result of a job by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:guid}/result")]
        public IActionResult Result([FromRoute] Guid id)
        {
            if (!monitor.TryGet(id, out var jobinfo))
                return NotFound("Job with id not found");

            return jobinfo.State switch
            {
                JobStatusEnum.Succeeded or JobStatusEnum.SucceededWithErrors 
                    => jobinfo.ResultObj is SubclassesDataCollectionApiResponse response ? 
                           File(ZipHelper.GenerateDataCollectionZipArchive(
                                response),
                                "application/zip",
                                $"subclasses_stats_{DateTime.UtcNow}.zip") :
                       jobinfo.ResultObj is RacialDataCollectionApiResponse racesResponse ?
                           File(racesResponse.RacesData,
                                "application/octet-stream",
                                $"races_stats_{DateTime.UtcNow}.xlsx") :
                       Ok(jobinfo.ResultObj),
                JobStatusEnum.Failed => Problem(detail: jobinfo.Error?.Message),
                _ => Accepted(jobinfo)
            };
        }

        /// <summary>
        /// Retrieves all jobs currently being monitored.
        /// </summary>
        /// <returns>List of jobs</returns>
        [HttpGet("getAll")]
        public IActionResult GetAllJobs()
        {
            var jobs = monitor.GetAll();

            if (jobs == null || jobs.Count == 0)
                return NotFound("No jobs found.");

            return Ok(jobs);
        }

        /// <summary>
        /// Creates a new job based on the specified job name.
        /// </summary>
        /// <param name="createNewJobModel">job model</param>
        /// <returns>Id of the job</returns>
        [HttpPost("create")]
        public IActionResult CreateNewJob([FromBody] CreateNewJobApiRequest createNewJobModel)
        {
            Guid guid = Guid.NewGuid();
            var auth = BearerPropagationHandler.GetAuthFromContext(HttpContext);

            switch (createNewJobModel.JobType)
            {
                case JobsEnum.CollectDataForClassLines:
                case JobsEnum.CollecctDataForRaces:
                    var param = new EsologsParams
                    {
                        JobId = guid,
                        Token = auth,
                        StartSliceTime = createNewJobModel.StartSliceTime?.ToUnix() ?? 0,
                        EndSliceTime = createNewJobModel.EndSliceTime?.ToUnix() ?? 0,
                        ZonesList = createNewJobModel.CollectedZoneIds
                    };
                    if (JobsEnum.CollectDataForClassLines == createNewJobModel.JobType)
                    {
                        queue.Enqueue<JobSubclassesDataCollection,
                            SubclassesDataCollectionApiResponse,
                            EsologsParams>(param);
                    }
                    else
                    {
                        queue.Enqueue<JobRacesDataCollection,
                            RacialDataCollectionApiResponse,
                            EsologsParams>(param);
                    }
                    break;
                default:
                    return BadRequest("Invalid job type.");
            }

            return new CreatedResult() { Value = guid };
        }

        /// <summary>
        /// Cancel the job
        /// </summary>
        /// <param name="jobType">Job type</param>
        /// <returns>Id of the job</returns>
        [HttpDelete("cancel")]
        public IActionResult CancelJob([FromQuery] Guid jobId)
        {
            if (!monitor.TryGet(jobId, out var jobinfo))
                return NotFound("Job with id not found");

            if (jobinfo.State == JobStatusEnum.Running
                || jobinfo.State == JobStatusEnum.Queued)
            {
                if (monitor.TryCancel(jobId, ref jobinfo))
                    return Ok();
            }

            return BadRequest("Job is not running");
        }
    }
}
