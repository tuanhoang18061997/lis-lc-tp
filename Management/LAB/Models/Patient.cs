using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Management.Models
{
    [Table("Patient")]
    [Index(nameof(Sid), IsUnique = true)]
    [Index(nameof(Seq), nameof(PatientId), nameof(PatientName), nameof(InsertTime))]
    public class Patient
    {
        
        [Key]
        public long Id { get; set; }
        public string? Seq { get; set; }
        public string? Sid { get; set; }       
        public string? PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? Sex { get; set; }
        public string? Address { get; set; }
        public DateTime? Age { get; set; }
        public bool IsYear { get; set; }
        public string? Diagnostic { get; set; }
        public bool FullResultXN { get; set; }
        public bool NotFullResultXN { get; set; }
        public string? MaBenhAn { get; set; }
        public string? MaDotKham { get; set; }
        public string? Phone { get; set; }


        public long? UserReturnResultXN { get; set; }
        [ForeignKey("UserReturnResultXN")]
        public virtual User? UserXN { get; set; }

        public long? UserReturnResultSA { get; set; }
        [ForeignKey("UserReturnResultSA")]
        public virtual User? UserSA { get; set; }

        public long? UserReturnResultSAT { get; set; }
        [ForeignKey("UserReturnResultSAT")]
        public virtual User? UserSAT { get; set; }

        public long? UserReturnResultDDT { get; set; }
        [ForeignKey("UserReturnResultDDT")]
        public virtual User? UserDDT { get; set; }

        public long? UserReturnResultNS { get; set; }
        [ForeignKey("UserReturnResultNS")]
        public virtual User? UserNS { get; set; }

        public long? UserReturnResultNSCTC { get; set; }
        [ForeignKey("UserReturnResultNSCTC")]
        public virtual User? UserNSCTC { get; set; }

        public long? UserReturnResultXQ { get; set; } // Bác sĩ trả KQ
        [ForeignKey("UserReturnResultXQ")]
        public virtual User? UserReturnXQ { get; set; }

        public long? UserProcessResultXQ { get; set; } // Kỹ thuật viên chụp, xử lý hình ảnh
        [ForeignKey("UserProcessResultXQ")]
        public virtual User? UserProcessXQ { get; set; }

        public long? UserReturnResultTDCN { get; set; }

        [ForeignKey("UserReturnResultTDCN")]
        public virtual User? UserTDCN { get; set; }

        public long? UserInsertId { get; set; }
        [ForeignKey("UserInsertId")]
        public virtual User? UserInsert { get; set; }

        public long? UserUpdateId { get; set; }
        [ForeignKey("UserUpdateId")]
        public virtual User? UserUpdate { get; set; }

        public long? DoctorId { get; set; }
        [ForeignKey("DoctorId")]
        public virtual Doctor? Doctor { get; set; }

        public long? ObjectId { get; set; }
        [ForeignKey("ObjectId")]
        public virtual Object? Object { get; set; }

        public long? LocationId { get; set; }
        [ForeignKey("LocationId")]
        public virtual Location? Location { get; set; }

        public long? HospitalId { get; set; }
        [ForeignKey("HospitalId")]
        public virtual Hospital? Hospital { get; set; }

        public DateTime? InsertTime { get; set; }
        public DateTime? UpdateTime { get; set; }
        public DateTime? GetSampleTimeXN { get; set; }
        public DateTime? GetSampleTimeSA { get; set; }
        public DateTime? GetSampleTimeSAT { get; set; }
        public DateTime? GetSampleTimeNS { get; set; }
        public DateTime? GetSampleTimeNSCTC { get; set; }
        public DateTime? GetSampleTimeXQ { get; set; }
        public DateTime? GetSampleTimeDDT { get; set; }
        public DateTime? GetSampleTimeTDCN { get; set; }

        public DateTime? ReturnResultTimeXN { get; set; }
        public DateTime? ReturnResultTimeSA { get; set; }
        public DateTime? ReturnResultTimeSAT { get; set; }
        public DateTime? ReturnResultTimeDDT { get; set; }
        public DateTime? ReturnResultTimeNS { get; set; }
        public DateTime? ReturnResultTimeNSCTC { get; set; }
        public DateTime? ReturnResultTimeXQ { get; set; }
        public DateTime? ReturnResultTimeTDCN { get; set; }


        public bool WaitXN { get; set; }
        public bool WaitSA { get; set; }
        public bool WaitSAT { get; set; }
        public bool WaitDDT { get; set; }
        public bool WaitNS { get; set; }
        public bool WaitNSCTC { get; set; }
        public bool WaitXQ { get; set; }
        public bool WaitTDCN { get; set; }

        public bool ProcessXN { get; set; }
        public bool ProcessSA { get; set; }
        public bool ProcessSAT { get; set; }
        public bool ProcessDDT { get; set; }
        public bool ProcessNS { get; set; }
        public bool ProcessNSCTC { get; set; }
        public bool ProcessXQ { get; set; }
        public bool ProcessTDCN { get; set; }

        public bool ValidXN { get; set; }
        public bool ValidSA { get; set; }
        public bool ValidSAT { get; set; }
        public bool ValidDDT { get; set; }
        public bool ValidNS { get; set; }
        public bool ValidNSCTC { get; set; }
        public bool ValidXQ { get; set; }
        public bool ValidTDCN { get; set; }

        public string? TicketId { get; set; }
        public bool Active { get; set; }       
        public string? BenhAn { get; set; }

        public virtual List<ResultXN> ResultXNs { get; set; }
        public virtual List<ResultCDHA> ResultCDHAs { get; set; }
        public virtual List<WorkOrder> WorkOrders { get; set; }
    }
}
