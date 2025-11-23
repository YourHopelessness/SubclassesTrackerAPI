using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SubclassesTracker.Database.Entity
{
    /// <summary>
    /// Represents an encounter in the game, such as a boss fight or a dungeon run.
    /// </summary>
    [Table("encounter")]
    public class Encounter : IHaveIdentifier
    {
        /// <summary>
        /// Unique identifier for the encounter.
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Name of the encounter, such as "Ansuul the Tormentor".
        /// </summary>
        [Required]
        [Column("name")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Identifier for the type of encounter, indicating its category (e.g., Trial, Dungeon).
        /// </summary>
        [Column("typeId")]
        public int TypeId { get; set; }
        public EncounterType Type { get; set; } = null!;

        /// <summary>
        /// Identifier for "good" score in the trial.
        /// </summary>
        [Column("scoreCense")]
        public int? ScoreCense { get; set; }

        /// <summary>
        /// Indicates whether the encounter is a last boss fight.
        /// </summary>
        [Column("lastBoss")]
        public bool? LastBoss { get; set; }

        /// <summary>
        /// Identifier for the zone where the encounter takes place.
        /// </summary>
        [Column("zoneId")]
        public int ZoneId { get; set; }
        public Zone Zone { get; set; } = null!;
    }

}
