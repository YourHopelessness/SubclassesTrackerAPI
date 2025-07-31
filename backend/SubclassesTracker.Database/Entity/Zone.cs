using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SubclassesTracker.Database.Entity
{
    /// <summary>
    /// Represents a zone in the game, which can contain multiple encounters and difficulties.
    /// </summary>
    [Table("zone")]
    public class Zone : IHaveIdentifier
    {
        /// <summary>
        /// Unique identifier for the zone.
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Name of the zone.
        /// </summary>
        [Required]
        [Column("name")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Type of the zone, indicating its category (e.g., Trial, Dungeon).
        /// </summary>
        [Column("typeId")]
        public int TypeId { get; set; }
        public EncounterType Type { get; set; } = null!;

        /// <summary>
        /// Collection of zone difficulties associated with this zone.
        /// </summary>
        public ICollection<ZoneDifficulty> ZoneDifficulties { get; set; } = null!;

        /// <summary>
        /// Collection of encounters associated with this zone.
        /// </summary>
        public ICollection<Encounter> Encounters { get; set; } = null!;
    }
}
