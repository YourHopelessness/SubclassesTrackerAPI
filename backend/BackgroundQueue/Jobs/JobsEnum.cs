namespace SubclassesTracker.Api.BackgroundQueue.Jobs
{
    /// <summary>
    /// Enumerate for jobs type
    /// </summary>
    public enum JobsEnum
    {
        /// <summary>
        /// Collect all subclasses data
        /// </summary>
        CollectDataForClassLines,
        /// <summary>
        /// Collect races usage data
        /// </summary>
        CollecctDataForRaces
    }
}
