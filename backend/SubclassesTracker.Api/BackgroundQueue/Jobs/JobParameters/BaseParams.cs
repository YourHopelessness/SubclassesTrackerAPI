namespace SubclassesTracker.Api.BackgroundQueue.Jobs.JobParameters
{
    /// <summary>
    /// Base params of the jobs
    /// </summary>
    public abstract class BaseParams
    {
        /// <summary>
        /// The unique job id.
        /// </summary>
        public Guid JobId { get; set; }
    }
}
