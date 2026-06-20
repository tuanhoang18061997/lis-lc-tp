namespace Management.Models.SignServer
{
    public class SignContext
    {
        public SignFormType FormType { get; set; }
        public string PatientCode { get; set; } = "";
        public string PatientMaBenhAn { get; set; } = "";
        public string? PatientName { get; set; }
        public string? ServiceCode { get; set; }
        public string? DoctorName { get; set; }          // dùng cho targetText
        public DateTime? PerformedAt { get; set; }       // thời điểm hoàn tất kết quả
        public string? Extra { get; set; }               // tuỳ biến thêm (ví dụ số phiếu)
        public string? ResultId { get; set; } = "";
    }
}
