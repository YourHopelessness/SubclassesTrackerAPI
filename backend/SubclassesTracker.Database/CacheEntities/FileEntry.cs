using SubclassesTracker.Database.Entity;

namespace SubclassesTracker.Database.CacheEntities
{
    /// <summary>
    /// File entry in the caching system.
    /// </summary>
    public class FileEntry : IHaveIdentifier
    {
        public int Id { get; set; }

        /// <summary>
        /// File name with extension (e.g. part-HASH.parquet)
        /// </summary>
        public string FileName { get; set; } = null!;

        /// <summary>
        /// Hash of variables
        /// </summary>
        public string Hash { get; set; } = null!;

        /// <summary>
        /// File size in bytes
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Date and time when the file was cached
        /// </summary>
        public DateTimeOffset CachedAt { get; set; }

        /// <summary>
        /// Time to live in seconds
        /// </summary>
        public long Ttl { get; set; } = 15768000; // 6 months by default

        /// <summary>
        /// Partition ID where the file is located
        /// if null, the file is considered to be in the root (e.g. dataset) folder
        /// </summary>
        public long? PartitionId { get; set; }
        public Partition? Partition { get; set; } = null!;

        /// <summary>
        /// Dataset ID where the file belongs
        /// </summary>
        public int DatasetId { get; set; }
        public Dataset Dataset { get; set; } = null!;

        /// <summary>
        /// Full path to the file
        /// </summary>
        public string FullPath => System.IO.Path.Combine(Partition?.Path ?? Dataset.RootPath, FileName);

        /// <summary>
        /// Request snapshots associated with this file entry.
        /// </summary>
        public ICollection<RequestSnapshot> RequestSnapshots { get; set; } = [];
    }
}
