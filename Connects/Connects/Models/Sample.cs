using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;

namespace Connects.Models
{
    [Table("Sample")]
    [Index(nameof(Name))]
    [Index(nameof(Code), IsUnique = true)]
    public class Sample
    {
        
        [Key]
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }      
        public string? Description { get; set; }
        public string? Result { get; set; }
        public string? Suggest { get; set; }
        public bool Active { get; set; }

        public long? CategorieId { get; set; }
        [ForeignKey("CategorieId")]
        public virtual Category? Category { get; set; }
        
    }
}
