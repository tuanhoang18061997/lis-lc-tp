using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Management.Models
{
    [Table("Group")]
    [Index(nameof(Name))]
    [Index(nameof(Code), IsUnique = true)]
    public class Group
    {
        
        [Key]
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public virtual List<Category> Categories { get; set; }
    }
}
