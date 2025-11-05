using SubclassesTracker.Models.Enums;

namespace SubclassesTracker.Api.BackgroundQueue.JobStatuses
{
    /// <summary>
    /// Interface representing the information about a job in the background queue.
    /// </summary>
    public interface IJobInfo
    {
        object Parameters { get; }
        JobStatusEnum State { get; }
        int Progress { get; }
        object? ResultObj { get; }
        Exception? Error { get; }
    }

    /// <summary>
    /// Represents the information about a job in the background queue, including its status, progress, result, and any error that occurred.
    /// </summary>
    /// <typeparam name="TResult">Returning type</typeparam>
    public sealed record JobInfo<TResult, TParams>(
        TParams Parameters,
        JobStatusEnum State,
        int Progress,
        TResult? Result,
        Exception? Error) : IJobInfo
    {
        object? IJobInfo.ResultObj => Result;
        object IJobInfo.Parameters => Parameters;
    }
}
