namespace SubclassesTracker.Models.Enums
{
    /// <summary>
    /// Enumeration representing the possible statuses of a job in the background queue.
    /// </summary>
    public enum JobStatusEnum
    {
        Queued,
        Running,
        Succeeded,
        SucceededWithErrors,
        Failed,
        Cancelled
    }
}
