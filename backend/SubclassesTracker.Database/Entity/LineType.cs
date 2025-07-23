using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SubclassesTracker.Database.Entity
{
    /// <summary>
    /// Represents a type of skill line in the game, categorizing skill lines into different groups such as crafting or combat.
    /// </summary>
    [Table("lineType")]
    public class LineType
    {
        /// <summary>
        /// Unique identifier for the line type.
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Name of the line type, such as "Class" or "Guild".
        /// </summary>
        [Required]
        [Column("name")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Collection of skill lines associated with this line type.
        /// </summary>
        public ICollection<SkillLine> SkillLines { get; set; } = new HashSet<SkillLine>();
    }
}
