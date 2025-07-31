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
    }
}
