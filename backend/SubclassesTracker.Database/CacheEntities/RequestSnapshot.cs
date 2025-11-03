using SubclassesTracker.Database.Entity;

namespace SubclassesTracker.Database.CacheEntities
{
    public class RequestSnapshot : IHaveIdentifier
    {
        /// <summary>
        /// Id of the request snapshot
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Timestamp when the snapshot was created
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Query name associated with the snapshot
        /// </summary>
        public string QueryName { get; set; } = null!;

        /// <summary>
        /// Hash of the variables used in the request
        /// </summary>
        public string VarsHash { get; set; } = null!;

        /// <summary>
        /// Canonical JSON representation of the variables used in the request
        /// </summary>
        public string VarsJson { get; set; } = null!;

        /// <summary>
        /// File entries associated with this request snapshot
        /// </summary>
        public ICollection<FileEntry> FileEntries { get; set; } = [];
    }
}
