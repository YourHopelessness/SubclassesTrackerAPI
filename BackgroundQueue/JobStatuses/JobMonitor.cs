using System.Collections.Concurrent;

namespace SubclassesTrackerExtension.BackgroundQueue.JobStatuses
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
    }

    /// <summary>
    /// Implementation of IJobMonitor that uses a ConcurrentDictionary to track job statuses.
    /// </summary>
    public sealed class JobMonitor : IJobMonitor
    {
        /// <summary>
        /// Thread-safe dictionary to store job statuses by their unique identifier.
        /// </summary>
        private readonly ConcurrentDictionary<Guid, IJobInfo> _map = new();

        public IJobInfo Get(Guid id) => _map[id];
        public void Set(IJobInfo info) => _map[info.Id] = info;
        public bool TryUpdate(Guid id, Func<IJobInfo, IJobInfo> upd)
            => _map.TryGetValue(id, out var cur) && _map.TryUpdate(id, upd(cur), cur);

        public IReadOnlyCollection<IJobInfo> GetAll() => (IReadOnlyCollection<IJobInfo>)_map.Values;

        public bool TryGet(Guid id, out IJobInfo jobInfo)
            => _map.TryGetValue(id, out jobInfo);
    }
}