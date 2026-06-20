using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Management.Models
{
    [Table("Doctor")]
    [Index(nameof(Name))]
    [Index(nameof(Code), IsUnique = true)]
    public class Doctor
    {
        
        [Key]
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public virtual List<ResultXN> ResultXNs { get; set; }
        public virtual List<ResultCDHA> ResultCDHAs { get; set; }
    }
}
