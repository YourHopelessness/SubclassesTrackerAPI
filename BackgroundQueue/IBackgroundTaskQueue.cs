using SubclassesTrackerExtension.BackgroundQueue.Jobs;

namespace SubclassesTrackerExtension.BackgroundQueue
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
        JobHandle<TResult> Enqueue<TResult>(
                Func<IServiceProvider, CancellationToken, Task<TResult>> work,
                Guid? id);

        /// <summary>
        /// Enqueue a job that returns a result
        /// </summary>
        /// <typeparam name="TJob"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="id">job Id</param>
        /// <returns></returns>
        JobHandle<TResult> Enqueue<TJob, TResult>(Guid ids)
            where TJob : IJob<TResult>;

        /// <summary>
        /// Remove from the queue
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        ValueTask<(
         Guid, 
         Func<IServiceProvider, 
         CancellationToken, Task>, 
         TaskCompletionSource<object?>)> 
            Dequeue(CancellationToken ct);
    }
}
