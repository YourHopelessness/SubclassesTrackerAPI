using DocumentFormat.OpenXml.InkML;
using SubclassesTracker.Api.BackgroundQueue.Jobs;
using SubclassesTracker.Api.BackgroundQueue.JobStatuses;
using System.Threading;
using System.Threading.Channels;

namespace SubclassesTracker.Api.BackgroundQueue
{
    /// <summary>
    /// Task queue for background tasks
    /// </summary>
    public sealed class BackgroundTaskQueue(IJobMonitor monitor) : IBackgroundTaskQueue
    {
        /// <summary>
        /// Queue
        /// </summary>
        private readonly Channel<(Guid,
                             Func<IServiceProvider, CancellationToken, Task>,
                             TaskCompletionSource<object?>)> channel
            = System.Threading.Channels.Channel.CreateUnbounded<(Guid,
                                  Func<IServiceProvider, CancellationToken, Task>,
                                  TaskCompletionSource<object?>)>();

        public JobHandle<TResult> Enqueue<TResult>(
                Func<IServiceProvider, CancellationToken, Task<TResult>> work,
                Guid? jobId)
        {
            var id = jobId ?? Guid.NewGuid();
            var tcs = new TaskCompletionSource<object?>(
                          TaskCreationOptions.RunContinuationsAsynchronously);

            // Job is queued
            monitor.Set(new JobInfo<TResult>(id, JobStatusEnum.Queued, 0, default, null));

            async Task Boxed(IServiceProvider sp, CancellationToken ct)
            {
                try
                {
                    // Update the job status to Running
                    monitor.TryUpdate(id, prev => ((JobInfo<TResult>)prev) with
                    {
                        State = JobStatusEnum.Running
                    });

                    // Execute the job
                    var result = await work(sp, ct);

                    // Update the job status to Succeeded
                    monitor.Set(new JobInfo<TResult>(id, JobStatusEnum.Succeeded, 100, result, null));
                    tcs.SetResult(result);
                }
                catch (PartialSuccessException<TResult> ex)
                {
                    // Update the job status to SucceededWithErrors
                    monitor.Set(new JobInfo<TResult>(id, JobStatusEnum.SucceededWithErrors, 100, ex.PartialResult, ex));
                    tcs.SetResult(ex.PartialResult);
                }
                catch (Exception ex)
                {
                    // Update the job status to Failed
                    monitor.Set(new JobInfo<TResult>(id, JobStatusEnum.Failed, 100, default, ex));
                    tcs.SetException(ex);
                }
            }

            // Enqueue the job
            channel.Writer.TryWrite((id, Boxed, tcs));

            return new JobHandle<TResult>(id, tcs.Task.ContinueWith(t => (TResult)t.Result!));
        }

        public JobHandle<TResult> Enqueue<TJob, TResult>(Guid Id)
            where TJob : IJob<TResult> =>
                Enqueue(async (sp, ct) =>
                {
                    var job = sp.GetRequiredService<TJob>();
                    return await job.RunAsync(Id, ct);
                }, Id);

        public async ValueTask<(Guid, Func<IServiceProvider, CancellationToken, Task>, TaskCompletionSource<object?>)> Dequeue(CancellationToken ct)
            => await channel.Reader.ReadAsync(ct);
    }
}
