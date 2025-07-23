using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SubclassesTracker.Database.Entity
{
    /// <summary>
    /// Represents icons of the skills
    /// </summary>
    [Table("icon")]
    public class Icon
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Url 
        /// </summary>
        [Column("url")]
        public string Url { get; set; } = null!;

        /// <summary>
        /// Collection of skill lines
        /// </summary>
        public ICollection<SkillLine> SkillLines { get; set; }
    }
}