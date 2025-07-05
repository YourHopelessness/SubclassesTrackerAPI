using SubclassesTrackerExtension.BackgroundQueue.JobStatuses;

namespace SubclassesTrackerExtension.BackgroundQueue.Jobs
{
    /// <summary>
    /// Interface for a job that can be executed asynchronously and returns a result.
    /// </summary>
    public interface IJob<TResult>
    {
        /// <summary>
        /// Runs the job asynchronously and returns a result.
        /// </summary>
        /// <param name="id">The unique identifier for the job.</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TResult> RunAsync(Guid id, CancellationToken ct);
    }
}
