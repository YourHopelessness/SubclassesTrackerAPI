using SubclassesTracker.Database.Entity;
using System.ComponentModel.DataAnnotations;

namespace SubclassesTracker.Database.CacheEntities
{
    public class Dataset : IHaveIdentifier
    {
        /// <summary>
        /// Dataset ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the dataset
        /// </summary>
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Schema version of the dataset
        /// </summary>
        public int Version { get; set; } = 1; // schema version (v1)

        /// <summary>
        /// Path to the root directory where Parquet files are stored
        /// </summary>
        [MaxLength(1024)]
        public string RootPath { get; set; } = null!;

        /// <summary>
        /// Creation timestamp of the dataset
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        public ICollection<Partition> Partitions { get; set; } = [];
        public ICollection<FileEntry> Files { get; set; } = [];
    }
}
