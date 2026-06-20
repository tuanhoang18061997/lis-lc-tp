using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;

namespace Connects.Models
{
    [Table("PrintSample")]
    [Index(nameof(Name))]
    [Index(nameof(Code), IsUnique = true)]
    public class PrintSample
    {
        
        [Key]
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Note { get; set; }
        public bool Active { get; set; }
        public virtual List<Service> Services { get; set; }
    }
}
