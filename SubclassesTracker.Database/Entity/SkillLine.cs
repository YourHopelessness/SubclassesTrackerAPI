using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SubclassesTracker.Database.Entity
{
    /// <summary>
    /// Represents a skill line in the game, which groups related abilities and skills.
    /// </summary>
    [Table("skillLine")]
    public class SkillLine
    {
        /// <summary>
        /// Unique identifier for the skill line.
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Name of the skill line, such as "Alchemy" or "Blacksmithing".
        /// </summary>
        [Required]
        [Column("name")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Identifier for the type of line, indicating its category (e.g., crafting, combat).
        /// </summary>
        [Column("lineTypeId")]
        public int LineTypeId { get; set; }
        public LineType LineType { get; set; } = null!;

        /// <summary>
        /// Collection of skill tree entries associated with this skill line.
        /// </summary>
        public ICollection<SkillTreeEntry> SkillTreeEntries { get; set; } = new HashSet<SkillTreeEntry>();
    }

}
