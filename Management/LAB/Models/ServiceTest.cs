using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Management.Models
{
    [Table("ServiceTest")]
    [Index(nameof(ServiceId), nameof(TestCodeId), IsUnique = true)]
    public class ServiceTest
    {
        
        [Key]
        public long Id { get; set; }

        public long? ServiceId { get; set; }
        [ForeignKey("ServiceId")]
        public virtual Service? Service { get; set; }

        public long? TestCodeId { get; set; }
        [ForeignKey("TestCodeId")]
        public virtual TestCode? TestCode { get; set; }
    }
}
