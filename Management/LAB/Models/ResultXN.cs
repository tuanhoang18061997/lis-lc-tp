using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Management.Models
{
    [Table("ResultXN")]
    [Index(nameof(PatientId), nameof(TestCodeId), nameof(InsertTime), nameof(ValidTime))]
   
    public class ResultXN
    {      
        [Key]
        public long Id { get; set; }

        public long? PatientId { get; set; }
        [ForeignKey("PatientId")]
        public virtual Patient? Patient { get; set; }

        public long? TestCodeId { get; set; }
        [ForeignKey("TestCodeId")]
        public virtual TestCode? TestCode { get; set; }

        public long? UserInsertId { get; set; }
        [ForeignKey("UserInsertId")]
        public virtual User? UserInsert { get; set; }

        public long? UserUpdateId { get; set; }
        [ForeignKey("UserUpdateId")]
        public virtual User? UserUpdate { get; set; }

        public long? ServiceId { get; set; }
        [ForeignKey("ServiceId")]
        public virtual Service? Service { get; set; }

        public long? DoctorId { get; set; }
        [ForeignKey("DoctorId")]
        public virtual Doctor? Doctor { get; set; }

        public string? Result { get; set; }
        public string? PosNeg { get; set; }
        public bool ValidPrint { get; set; }
        public int? Status { get; set; } // 0 => Bình thường, 1 => Nhỏ hơn CSBT, In đậm, Màu xanh => 2 => Lớn hơn CSBT, In đậm, Màu đỏ
        public int? StatusResult { get; set; } // 0 => NotResult, 1 => HaveResult, 2 => Sended, 3 => Error
        public string? TicketItemId { get; set; }
        public string? DeviceCodeBHYT { get; set; }
        public bool PushBHYT { get; set; }
        public string? KeyResultForHis { get; set; }
        public bool Active { get; set; }

        public DateTime? InsertTime { get; set; }
        public DateTime? UpdateTime { get; set; }
        public DateTime? ValidTime { get; set; }
        public long? SignStoreId { get; set; }
        public int? SignStatus { get; set; }
        public string? Note { get; set; }
        public string? TypeBenhAn { get; set; } = null;
    }
}
