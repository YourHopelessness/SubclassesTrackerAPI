using Microsoft.AspNetCore.Mvc;
using SubclassesTracker.Api.BackgroundQueue;
using SubclassesTracker.Api.BackgroundQueue.Jobs;
using SubclassesTracker.Api.BackgroundQueue.JobStatuses;
using SubclassesTracker.Api.Models.Responses.Api;
using SubclassesTracker.Api.Utils;

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
                    => jobinfo.ResultObj is DataCollectionResultApiResponse ? 
                       File(ZipHelper.GenerateDataCollectionZipArchive(
                                        (DataCollectionResultApiResponse)jobinfo.ResultObj),
                            "application/zip",
                            $"stats_{DateTime.UtcNow}.zip")
                       : Ok(jobinfo.ResultObj),
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
        /// <param name="jobType">Job type</param>
        /// <returns>Id of the job</returns>
        [HttpPost("create")]
        public IActionResult CreateNewJob([FromQuery] JobsEnum jobType)
        {
            Guid guid = Guid.NewGuid();
            switch (jobType)
            {
                case JobsEnum.CollectDataForClassLines:
                    queue.Enqueue<JobDataCollection, DataCollectionResultApiResponse>(guid);
                    break;
                default:
                    return BadRequest("Invalid job type.");
            }

            return new CreatedResult() { Value = guid };
        }
    }
}
