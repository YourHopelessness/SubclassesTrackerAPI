using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SubclassesTracker.Database.Entity
{
    public interface IHaveIdentifier
    {
        /// <summary>
        /// Unique identifier of the enity
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }
    }
}
