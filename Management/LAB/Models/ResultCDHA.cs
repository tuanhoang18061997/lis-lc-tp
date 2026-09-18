using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Management.Models
{
    [Table("ResultCDHA")]
    [Index(nameof(InsertTime), nameof(PatientId), nameof(ServiceId))]
    
    public class ResultCDHA
    {
        
        [Key]
        public long Id { get; set; }

        public long? PatientId { get; set; }
        [ForeignKey("PatientId")]
        public virtual Patient? Patient { get; set; }

        public long? ServiceId { get; set; }
        [ForeignKey("ServiceId")]
        public virtual Service? Service { get; set; }

        public long? DoctorId { get; set; }
        [ForeignKey("DoctorId")]
        public virtual Doctor? Doctor { get; set; }

        public long? UserInsertId { get; set; }
        [ForeignKey("UserInsertId")]
        public virtual User? UserInsert { get; set; }

        public long? UserUpdateId { get; set; }
        [ForeignKey("UserUpdateId")]
        public virtual User? UserUpdate { get; set; }

        public string? Result { get; set; }
        public string? Description { get; set; }
        public string? Suggest { get; set; }
        public string? SieuAmTim { get; set; }

        public virtual List<ImageCDHA> ImageCDHAs { get; set; }

        public DateTime? InsertTime { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string? TicketItemId { get; set; }   
        public string? DeviceCodeBHYT { get; set; }
        public bool PushBHYT { get; set; }
        public string? KeyResultForHis { get; set; }
        public bool Active { get; set; }
        public long? SignStoreId { get; set; }
        public int? SignStatus { get; set; }
        public string? TypeBenhAn { get; set; } = null;
        public bool? IsValidated { get; set; }
        public DateTime? LastValidatedAt { get; set; }
        public long? LastValidatedByUserId { get; set; }
    }
}
