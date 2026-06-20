using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;

namespace Connects.Models
{
    [Table("Type")]
    [Index(nameof(Name))]
    [Index(nameof(Code), IsUnique = true)]
    public class Type
    {
        
        [Key]
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

        public virtual List<UserType> UserTypes { get; set; }
    }
}
