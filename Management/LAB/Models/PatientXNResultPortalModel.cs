using System.ComponentModel.DataAnnotations;

namespace Management.Models
{
    /// <summary>
    /// Model hiển thị kết quả xét nghiệm trên portal
    /// </summary>
    public class PatientXNResultPortalModel
    {
        public long Id { get; set; }
        
        public long TestCodeId { get; set; }
        
        [Display(Name = "Tên xét nghiệm")]
        public string TestCodeName { get; set; } = "";
        
        [Display(Name = "Mã xét nghiệm")]
        public string TestCodeCode { get; set; } = "";
        
        [Display(Name = "Tên dịch vụ")]
        public string ServiceName { get; set; } = "";
        
        [Display(Name = "Loại dịch vụ")]
        public string CategoryName { get; set; } = "";
        
        [Display(Name = "Mã loại dịch vụ")]
        public string CategoryCode { get; set; } = "";
        
        [Display(Name = "Kết quả định lượng")]
        public string Result { get; set; } = "";
        
        [Display(Name = "Kết quả định tính")]
        public string PosNeg { get; set; } = "";
        
        [Display(Name = "Đơn vị")]
        public string Unit { get; set; } = "";
        
        [Display(Name = "Giá trị bình thường")]
        public string NormalRange { get; set; } = "";
        
        [Display(Name = "Trạng thái")]
        public int Status { get; set; }
        
        [Display(Name = "Mô tả trạng thái")]
        public string StatusText { get; set; } = "";
        
        [Display(Name = "CSS Class")]
        public string StatusCssClass 
        { 
            get 
            {
                return Status switch
                {
                    0 => "text-success", // Bình thường - màu xanh
                    1 => "text-primary", // Thấp - màu xanh dương
                    2 => "text-danger",  // Cao - màu đỏ
                    _ => ""
                };
            } 
        }
        
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; }
        
        [Display(Name = "Thời gian xác nhận")]
        public DateTime? ValidTime { get; set; }
        
        [Display(Name = "Bác sĩ")]
        public string DoctorName { get; set; } = "";
        
        [Display(Name = "Thời gian trả kết quả")]
        public DateTime? ReturnResultDate { get; set; }
        
        [Display(Name = "Đã xác nhận")]
        public bool IsValidPrint { get; set; }
        
        [Display(Name = "Mã kết quả HIS")]
        public string KeyResult { get; set; } = "";
        
        /// <summary>
        /// Kết quả hiển thị (định lượng hoặc định tính)
        /// </summary>
        [Display(Name = "Kết quả")]
        public string DisplayResult 
        { 
            get 
            {
                if (!string.IsNullOrEmpty(PosNeg))
                {
                    // Nếu có kết quả định tính thì ưu tiên hiển thị
                    if (!string.IsNullOrEmpty(Result))
                    {
                        return $"{Result} ({PosNeg})";
                    }
                    return PosNeg;
                }
                return Result;
            } 
        }
        
        /// <summary>
        /// Kết quả với đơn vị
        /// </summary>
        [Display(Name = "Kết quả có đơn vị")]
        public string ResultWithUnit 
        { 
            get 
            {
                var result = DisplayResult;
                if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(Unit))
                {
                    // Chỉ thêm đơn vị cho kết quả định lượng (số)
                    if (!string.IsNullOrEmpty(Result) && string.IsNullOrEmpty(PosNeg))
                    {
                        return $"{result} {Unit}";
                    }
                }
                return result;
            } 
        }
        
        /// <summary>
        /// Kết quả không có đơn vị
        /// </summary>
        [Display(Name = "Kết quả không có đơn vị")]
        public string OnlyResult 
        { 
            get 
            {
                return DisplayResult;
            } 
        }
        
        /// <summary>
        /// Trạng thái Badge CSS cho Bootstrap
        /// </summary>
        public string StatusBadgeClass 
        { 
            get 
            {
                return Status switch
                {
                    0 => "badge bg-success", // Bình thường
                    1 => "badge bg-primary", // Thấp
                    2 => "badge bg-danger",  // Cao
                    _ => "badge bg-secondary"
                };
            } 
        }
        
        /// <summary>
        /// Icon cho trạng thái
        /// </summary>
        public string StatusIcon 
        { 
            get 
            {
                return Status switch
                {
                    0 => "fas fa-check-circle", // Bình thường
                    1 => "fas fa-arrow-down",   // Thấp
                    2 => "fas fa-arrow-up",     // Cao
                    _ => "fas fa-question-circle"
                };
            } 
        }
        
        /// <summary>
        /// CSS class cho category badge
        /// </summary>
        public string CategoryBadgeClass
        {
            get
            {
                return CategoryCode?.ToLower() switch
                {
                    "hs" => "badge bg-info",        // Hóa sinh - xanh dương nhạt
                    "hh" => "badge bg-danger",      // Huyết học - đỏ
                    "vs" => "badge bg-warning",     // Vi sinh - vàng
                    "md" => "badge bg-success",     // Miễn dịch - xanh lá
                    "ns" => "badge bg-purple",      // Nội tiết - tím
                    "nt" => "badge bg-dark",        // Nước tiểu - đen
                    "xn" => "badge bg-primary",     // Xét nghiệm tổng quát - xanh
                    _ => "badge bg-secondary"       // Mặc định - xám
                };
            }
        }
        
        /// <summary>
        /// Icon cho category
        /// </summary>
        public string CategoryIcon
        {
            get
            {
                return CategoryCode?.ToLower() switch
                {
                    "hs" => "fas fa-flask",         // Hóa sinh
                    "hh" => "fas fa-tint",          // Huyết học
                    "vs" => "fas fa-bacteria",      // Vi sinh
                    "md" => "fas fa-shield-alt",    // Miễn dịch
                    "ns" => "fas fa-heartbeat",     // Nội tiết
                    "nt" => "fas fa-vial",          // Nước tiểu
                    "xn" => "fas fa-microscope",    // Xét nghiệm tổng quát
                    _ => "fas fa-vials"             // Mặc định
                };
            }
        }
    }
}