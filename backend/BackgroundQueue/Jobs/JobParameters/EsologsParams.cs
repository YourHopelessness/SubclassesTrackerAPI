using System.ComponentModel;

namespace SubclassesTracker.Api.BackgroundQueue.Jobs.JobParameters
{
    public class EsologsParams : BaseParams
    {
        /// <summary>
        /// Bearer token 
        /// </summary>
        public required string Token { get; set; }

        /// <summary>
        /// List of the zones
        /// </summary>
        public List<int>? ZonesList { get; set; }

        /// <summary>
        /// Start time of the reports slice
        /// </summary>
        public long StartSliceTime { get; set; }

        /// <summary>
        /// End time of the reports slice
        /// </summary>
        public long EndSliceTime { get; set; }
    }
}
