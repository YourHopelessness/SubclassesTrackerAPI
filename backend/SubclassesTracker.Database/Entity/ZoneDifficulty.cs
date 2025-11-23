using System.ComponentModel.DataAnnotations.Schema;

namespace SubclassesTracker.Database.Entity
{
    /// <summary>
    /// Represents the difficulty level of a zone in the game, linking zones to their respective difficulty levels and indicating if they are in hard mode.
    /// </summary>
    [Table("zoneDifficulty")]
    public class ZoneDifficulty : IHaveIdentifier
    {
        /// <summary>
        /// Id of the zone difficulties
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Unique identifier for the zone difficulty.
        /// </summary>
        [Column("zoneId")]
        public int ZoneId { get; set; }
        public Zone Zone { get; set; } = null!;

        /// <summary>
        /// Unique identifier for the difficulty level of the zone.
        /// </summary>
        [Column("difficultyId")]
        public int DifficultyId { get; set; }
        public Difficulty Difficulty { get; set; } = null!;

        /// <summary>
        /// Indicates whether the zone is in hard mode.
        /// </summary>
        [Column("isHardMode")]
        public int IsHardMode { get; set; } = 0;
    }
}
