using SubclassesTracker.Api.BackgroundQueue.Jobs.JobParameters;
using SubclassesTracker.Models.Enums;
using System.Collections.Concurrent;

namespace SubclassesTracker.Api.BackgroundQueue.JobStatuses
{
    /// <summary>
    /// Interface for monitoring job statuses.
    /// </summary>
    public interface IJobMonitor
    {
        /// <summary>
        /// Retrieves the job information for a given job ID.
        /// </summary>
        /// <param name="id">Id of job</param>
        /// <returns></returns>
        IJobInfo Get(Guid id);

        /// <summary>
        /// Try to get the job information for a given job ID.
        /// </summary>
        /// <param name="id">Id of job</param>
        /// <returns></returns>
        bool TryGet(Guid id, out IJobInfo jobInfo);

        /// <summary>
        /// Sets the job information for a given job ID.
        /// </summary>
        /// <param name="info">Current job information</param>
        void Set(IJobInfo info);

        /// <summary>
        /// Attempts to update the job information for a given job ID using a provided updater function.
        /// </summary>
        /// <param name="id">Job Id</param>
        /// <param name="upd">New job info</param>
        /// <returns></returns>
        bool TryUpdate(Guid id, Func<IJobInfo, IJobInfo> updater);

        /// <summary>
        /// Retrieves all job information currently being monitored.
        /// </summary>
        /// <returns>List of all current jobs</returns>
        IReadOnlyCollection<IJobInfo> GetAll();

        /// <summary>
        /// Add the new task
        /// </summary>
        /// <param name="info">New job info</param>
        void Add(IJobInfo info, CancellationTokenSource cts);

        /// <summary>
        /// Try cancel the task by Id
        /// </summary>
        /// <param name="id">Job Id</param>
        /// <param name="jobInfo">New job info</param>
        /// <returns>result of cancelling</returns>
        bool TryCancel(Guid id, ref IJobInfo jobInfo);
    }

    /// <summary>
    /// Implementation of IJobMonitor that uses a ConcurrentDictionary to track job statuses.
    /// </summary>
    public sealed class JobMonitor : IJobMonitor
    {
        /// <summary>
        /// Thread-safe dictionary to store job statuses by their unique identifier.
        /// </summary>
        private readonly ConcurrentDictionary<Guid, IJobInfo> jobMap = new();
        private readonly ConcurrentDictionary<Guid, CancellationTokenSource> tokens = new();

        public IJobInfo Get(Guid id)
             => (IJobInfo)jobMap[id];

        public void Set(IJobInfo info)
            => jobMap[((BaseParams)info.Parameters).JobId] = info;

        public void Add(IJobInfo info, CancellationTokenSource cts)
        {
            jobMap.TryAdd(((BaseParams)info.Parameters).JobId, info);
            tokens.TryAdd(((BaseParams)info.Parameters).JobId, cts);
        }

        public bool TryUpdate(Guid id, Func<IJobInfo, IJobInfo> upd)
            => jobMap.TryGetValue(id, out var cur) && jobMap.TryUpdate(id, upd(cur), cur);

        public IReadOnlyCollection<IJobInfo> GetAll()
            => (IReadOnlyCollection<IJobInfo>)jobMap.Values;

        public bool TryGet(Guid id, out IJobInfo jobInfo)
            => jobMap.TryGetValue(id, out jobInfo);

        public bool TryCancel(Guid id, ref IJobInfo jobInfo)
        {
            if (!jobMap.TryGetValue(id, out var existing))
                return false;

            if (!tokens.TryGetValue(id, out var tokenSource))
                return false;

            var cancelled = existing switch
            {
                JobInfo<object, object> untyped => untyped with { State = JobStatusEnum.Cancelled },
                _ => new JobInfo<object, object>(existing, JobStatusEnum.Cancelled, ((IJobInfo)existing).Progress, null, null)
            };

            var updated = jobMap.TryUpdate(id, cancelled, existing);

            if (updated)
            {
                jobInfo = cancelled;
                tokenSource.Cancel();
            }

            return updated;
        }
    }
}