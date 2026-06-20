using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Management.Models
{
    [Table("DigitalSign")]
    public class DigitalSign
    {
        public long Id { get; set; } // Auto Increament
        public string? ReferenceType { get; set; } // loại mẫu đang in kết quả: SA | SATIM | XQUANG | NOISOI | DDT | XETNGHIEM
        public string? ReferenceKeyResult { get; set; } // id của bảng ResultCDHA | ResultXN
        public long? SignId { get; set; } // signStoreId trả về từ server ký số
        public string? SignUserId { get; set; } // CCCD người thực hiện
        public string? TaxCode { get; set; } // MST công ty 
        public string? TargetText { get; set; } // Tên người ký đúng từng chữ viết hoa thường 
        public string? RequestUrl { get; set; } // default null 
        public string? ResponseData { get; set; } // json resp trả về từ server ký
        public int? Status { get; set; } // 1 | 0  1: success | 0: failed
        public long? CreatorId { get; set; } // UserId người ký
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool Deleted { get; set; } = false;
    }

}
