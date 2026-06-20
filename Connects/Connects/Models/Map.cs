using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;

namespace Connects.Models
{
    [Table("Map")]
    [Index(nameof(DeviceId), nameof(TestCodeId), nameof(TestcodeIn), IsUnique = true)]
    public class Map
    {
        
        [Key]
        public long Id { get; set; }

        public long? DeviceId { get; set; }
        [ForeignKey("DeviceId")]
        public virtual Device Device { get; set; }

        public long? TestCodeId { get; set; }
        [ForeignKey("TestCodeId")]
        public virtual TestCode? TestCode { get; set; }

        public string TestcodeIn { get; set; }
        public string? TestcodeIn2 { get; set; }
        public string? TestcodeIn3 { get; set; }
        public string? Note { get; set; }
        public string? Note2 { get; set; }
        public string? Note3 { get; set; }
        public string? FormatNumber { get; set; }
    }
}
