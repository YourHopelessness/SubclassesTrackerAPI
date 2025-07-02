using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SubclassesTracker.Database.Entity
{
    /// <summary>
    /// Represents a difficulty level for zones in the game, such as Normal or Veteran Hard mode.
    /// </summary>
    [Table("difficulty")]
    public class Difficulty
    {
        /// <summary>
        /// Unique identifier for the difficulty level.
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Name of the difficulty level, such as "Normal" or "Veteran Hard mode".
        /// </summary>
        [Required]
        [Column("name")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Collection of zone difficulties associated with this difficulty level.
        /// </summary>
        public ICollection<ZoneDifficulty> ZoneDifficulties { get; set; } = new HashSet<ZoneDifficulty>();
    }
}
