using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Management.Models
{
    [Table("ResultStandard")]
    [Index(nameof(Seq), nameof(TestCodeId), nameof(Status))]
    public class ResultStandard
    {
        
        [Key]
        public long Id { get; set; }
        public string Seq { get; set; }

        public long? TestCodeId { get; set; }
        [ForeignKey("TestCodeId")]
        public virtual TestCode? TestCode { get; set; }

        public long? DeviceId { get; set; }
        [ForeignKey("DeviceId")]
        public virtual Device? Device { get; set; }

        public string TestCodeIn { get; set; }
        public string? TestCodeIn2 { get; set; }
        public string? TestCodeIn3 { get; set; }
        public double? Result { get; set; }
        public string? PosNeg { get; set; }
        public string? Unit { get; set; }
        public string Status { get; set; }
        public DateTime InsertTime { get; set; } // Time insert vô DB
        public DateTime ReturnTime { get; set; } // // Time máy trả về cùng với KQ
        public string ComputerId { get; set; }
    }
}
