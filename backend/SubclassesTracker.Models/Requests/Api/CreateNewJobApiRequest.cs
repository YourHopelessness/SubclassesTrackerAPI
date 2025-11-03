using SubclassesTracker.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace SubclassesTracker.Models.Requests.Api
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

        /// <summary>
        /// Start time of the reports slice for job type = CollectDataForClassLines and CollecctDataForRaces
        /// </summary>
        public DateTime? StartSliceTime { get; set; }

        /// <summary>
        /// End time of the reports slice for job type = CollectDataForClassLines and CollecctDataForRaces
        /// </summary>
        public DateTime? EndSliceTime { get; set; }
    }
}
