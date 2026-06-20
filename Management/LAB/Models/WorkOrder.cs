using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Management.Models
{
    [Table("WorkOrder")]
    [Index(nameof(PatientId), nameof(InsertTime), nameof(Status))]
    public class WorkOrder
    {
        
        [Key]
        public long Id { get; set; }

        public long? PatientId { get; set; }
        [ForeignKey("PatientId")]
        public virtual Patient? Patient { get; set; }

        public long? DeviceId { get; set; }
        [ForeignKey("DeviceId")]
        public virtual Device? Device { get; set; }

        public long? UserInsertId { get; set; }
        [ForeignKey("UserInsertId")]
        public virtual User? UserInsert { get; set; }

        public string TestCode { get; set; }
        public string TestCodeIn { get; set; }
        public string TestCodeIn2 { get; set; }
        public DateTime InsertTime { get; set; }
        public int Status { get; set; } // 0 : Đang chờ, 1 : Đang chạy, 2 : Đã có KQ, 3 : Chạy lại
    }
}
