using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SubclassesTracker.Database.Entity
{
    /// <summary>
    /// Represents an entry in the skill tree, linking abilities to skill lines.
    /// </summary>
    [Table("skillTree")]
    public class SkillTreeEntry
    {
        /// <summary>
        /// Unique identifier for the skill tree entry.
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Identifier for the ability associated with this skill tree entry.
        /// </summary>
        [Column("abilityId")]
        public int AbilityId { get; set; }

        /// <summary>
        /// Identifier for the skill line this entry belongs to.
        /// </summary>
        [Column("skillLineId")]
        public int SkillLineId { get; set; }
        public SkillLine SkillLine { get; set; } = null!;

        /// <summary>
        /// Name of the skill line associated with this entry.
        /// </summary>
        [Column("name")]
        public string SkillName { get; set; } = null!;

        /// <summary>
        /// Type of the skill line, indicating its category (like Active or Passive).
        /// </summary>
        [Column("type")]
        public string SkillType { get; set; } = null!;
    }
}
