using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SubclassesTracker.Database.Entity
{
    [Table("class")]
    public class Class : IHaveIdentifier
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Class name.
        /// </summary>
        [Column("name")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Class short names, separated by commas.
        /// </summary>
        [Column("shortName")]
        public string? ShortName { get; set; }

        /// <summary>
        /// Collection of skill lines associated with this line type.
        /// </summary>
        public ICollection<SkillLine> SkillLines { get; set; } = null!;
    }
}
