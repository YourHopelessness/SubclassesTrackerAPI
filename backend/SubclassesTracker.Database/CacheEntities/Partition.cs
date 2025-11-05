using SubclassesTracker.Database.Entity;
using System.ComponentModel.DataAnnotations;

namespace SubclassesTracker.Database.CacheEntities
{
    /// <summary>
    /// Represents a partition in the caching system.
    /// </summary>
    public class Partition : IHaveIdentifier
    {
        /// <summary>
        /// Identifier for the partition.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Dataset this partition belongs to.
        /// </summary>
        public int DatasetId { get; set; }
        public Dataset Dataset { get; set; } = null!;

        /// <summary>
        /// Path of the partition (e.g logId or time).
        /// </summary>
        [MaxLength(1024)]
        public string Path { get; set; } = null!;

        public ICollection<FileEntry> Files { get; set; } = [];
    }
}
