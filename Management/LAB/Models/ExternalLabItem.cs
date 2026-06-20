using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Management.Models
{
    public class ExternalLabItem
    {
        public string Code { get; set; }      // Mã nội bộ (nếu map được, tạm thời có thể = Name)
        public string Name { get; set; }      // Tên xét nghiệm đọc từ PDF (CA 125, HBsAg,…)
        public string Value { get; set; }     // 16.30 | 0.41 Negative | …
        public string Unit { get; set; }      // U/mL | ng/mL | COI | …
        public string RefRange { get; set; }  // "0 - 35" | "< 5.5" | "< 1.0 Negative" | …
    }
}
