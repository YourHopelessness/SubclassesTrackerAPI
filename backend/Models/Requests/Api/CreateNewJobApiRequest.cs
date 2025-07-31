using SubclassesTracker.Api.BackgroundQueue.Jobs;
using System.ComponentModel.DataAnnotations;

namespace SubclassesTracker.Api.Models.Requests.Api
{
    /// <summary>
    /// Request model for create new job
    /// </summary>
    public class CreateNewJobApiRequest
    {
        /// <summary>
        /// Job Type
        /// </summary>
        [Required]
        public JobsEnum JobType { get; set; }

        /// <summary>
        /// The number of zones for job type = CollectDataForClassLines and CollecctDataForRaces
        /// </summary>
        public List<int>? CollectedZoneIds { get; set; }
    }
}
