namespace SubclassesTracker.Api.BackgroundQueue.HostedService
{
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using SubclassesTracker.Api.BackgroundQueue;

    public sealed class QueuedHostedService(
            IBackgroundTaskQueue queue,
            IServiceScopeFactory scopeFactory,
            ILogger<QueuedHostedService> log) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Remove from the queue and get the next job
                var (id, work, tcs) = await queue.Dequeue(stoppingToken);

                // Start a new thread to run the job
                _ = Task.Run(async () =>
                {
                    try
                    {
                        // Create scope for the job
                        await using var scope = scopeFactory.CreateAsyncScope();
                        // Box the new job
                        await work(scope.ServiceProvider, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        // Set the exception on the task completion source
                        tcs.TrySetException(ex);
                        log.LogError(ex, "Job {JobId} crashed inside worker", id);
                    }
                }, stoppingToken);
            }
        }
    }
}
