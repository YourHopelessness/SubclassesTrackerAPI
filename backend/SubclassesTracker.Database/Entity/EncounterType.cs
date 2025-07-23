using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SubclassesTracker.Database.Entity
{
    /// <summary>
    /// Represents a type of encounter in the game, categorizing encounters into different types such as Trials or Dungeons.
    /// </summary>
    [Table("type")]
    public class EncounterType
    {
        /// <summary>
        /// Unique identifier for the encounter type.
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Name of the encounter type, such as "Trial" or "Dungeon".
        /// </summary>
        [Required]
        [Column("name")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Collection of zones associated with this encounter type.
        /// </summary>
        public ICollection<Zone> Zones { get; set; } = new HashSet<Zone>();

        /// <summary>
        /// Collection of encounters associated with this encounter type.
        /// </summary>
        public ICollection<Encounter> Encounters { get; set; } = new HashSet<Encounter>();
    }
}
