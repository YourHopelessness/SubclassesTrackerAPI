using SubclassesTracker.Api.BackgroundQueue.Jobs;
using SubclassesTracker.Api.BackgroundQueue.Jobs.JobParameters;
using SubclassesTracker.Api.BackgroundQueue.JobStatuses;
using SubclassesTracker.Models.Enums;
using System.Threading.Channels;

namespace SubclassesTracker.Api.BackgroundQueue
{
    /// <summary>
    /// Task queue for background tasks
    /// </summary>
    public sealed class BackgroundTaskQueue
        (IJobMonitor monitor) : IBackgroundTaskQueue
    {
        /// <summary>
        /// Queue
        /// </summary>
        private readonly Channel<(Guid,
                             Func<IServiceProvider, Task>,
                             TaskCompletionSource<object?>)> channel
            = Channel.CreateUnbounded<(Guid,
                    Func<IServiceProvider, Task>,
                    TaskCompletionSource<object?>)>();

        public JobHandle<TResult> Enqueue<TResult, TParams>(
                Func<IServiceProvider, CancellationToken, Task<TResult>> work,
                TParams jobParams)
             where TParams : BaseParams
        {
            var id = jobParams.JobId;
            var tcs = new TaskCompletionSource<object?>(
                          TaskCreationOptions.RunContinuationsAsynchronously);

            // Job is queued
            var cts = new CancellationTokenSource();
            monitor.Add(new JobInfo<TResult, TParams>(jobParams, JobStatusEnum.Queued, 0, default, null), cts);

            async Task Boxed(IServiceProvider sp)
            {
                try
                {
                    // Update the job status to Running
                    monitor.TryUpdate(id, prev => ((JobInfo<TResult, TParams>)prev) with
                    {
                        State = JobStatusEnum.Running
                    });

                    // Execute the job
                    var result = await work(sp, cts.Token);

                    // Update the job status to Succeeded
                    monitor.Set(new JobInfo<TResult, TParams>(jobParams, JobStatusEnum.Succeeded, 100, result, null));
                    tcs.SetResult(result);
                }
                catch (PartialSuccessException<TResult> ex)
                {
                    // Update the job status to SucceededWithErrors
                    monitor.Set(new JobInfo<TResult, TParams>(jobParams, JobStatusEnum.SucceededWithErrors, 100, ex.PartialResult, ex));
                    tcs.SetResult(ex.PartialResult);
                }
                catch (Exception ex)
                {
                    // Update the job status to Failed
                    monitor.Set(new JobInfo<TResult, TParams>(jobParams, JobStatusEnum.Failed, 100, default, ex));
                    tcs.SetException(ex);
                }
            }

            // Enqueue the job
            channel.Writer.TryWrite((id, Boxed, tcs));

            return new JobHandle<TResult>(id, tcs.Task.ContinueWith(t => (TResult)t.Result!));
        }

        public JobHandle<TResult> Enqueue<TJob, TResult, TParams>(TParams jobParametrs)
            where TParams : BaseParams
            where TJob : IJob<TResult, TParams> =>
                Enqueue(async (sp, ct) =>
                {
                    var job = sp.GetRequiredService<TJob>();

                    return await job.RunAsync(jobParametrs, ct);
                }, jobParametrs);

        public async ValueTask<(Guid, Func<IServiceProvider, Task>, TaskCompletionSource<object?>)> Dequeue(CancellationToken ct)
            => await channel.Reader.ReadAsync(ct);
    }
}
