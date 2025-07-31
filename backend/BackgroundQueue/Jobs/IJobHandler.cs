using SubclassesTracker.Api.BackgroundQueue.Jobs.JobParameters;

namespace SubclassesTracker.Api.BackgroundQueue.Jobs
{
    /// <summary>
    /// Interface for a job that can be executed asynchronously and returns a result.
    /// </summary>
    public interface IJob<TResult, TParams>
        where TParams : BaseParams
    {
        /// <summary>
        /// Runs the job asynchronously and returns a result.
        /// </summary>
        /// <param name="parameters">The job parameters.</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TResult> RunAsync(TParams parameters, CancellationToken ct);
    }
}
