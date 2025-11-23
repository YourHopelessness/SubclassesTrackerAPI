using SubclassesTracker.Api.BackgroundQueue.Jobs;
using SubclassesTracker.Api.BackgroundQueue.Jobs.JobParameters;

namespace SubclassesTracker.Api.BackgroundQueue
{
    /// <summary>
    /// Delegate for Job
    /// </summary>
    /// <typeparam name="TResult">Returning type from the job</typeparam>
    /// <param name="Id">job Id</param>
    /// <param name="Completion">result</param>
    public sealed record JobHandle<TResult>(Guid Id, Task<TResult> Completion);

    /// <summary>
    /// Backgound task queue
    /// </summary>
    public interface IBackgroundTaskQueue
    {
        /// <summary>
        /// Add to the queue
        /// </summary>
        /// <param name="work">task</param>
        /// <param name="id">job Id</param>
        /// <returns></returns>
        JobHandle<TResult> Enqueue<TResult, TParams>(
                Func<IServiceProvider, CancellationToken, Task<TResult>> work,
                TParams jobParametrs)
            where TParams : BaseParams;

        /// <summary>
        /// Enqueue a job that returns a result
        /// </summary>
        /// <typeparam name="TJob"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="id">job Id</param>
        /// <returns></returns>
        JobHandle<TResult> Enqueue<TJob, TResult, TParams>(TParams jobParametrs)
            where TParams : BaseParams
            where TJob : IJob<TResult, TParams>;

        /// <summary>
        /// Remove from the queue
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        ValueTask<(
         Guid,
         Func<IServiceProvider, Task>,
         TaskCompletionSource<object?>)>
            Dequeue(CancellationToken ct);
    }
}
