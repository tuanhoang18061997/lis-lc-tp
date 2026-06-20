namespace Management.Models
{
    public static class ExternalFileModule
    {
        public const int Lab = 1;     // Xét nghiệm
        public const int CDHA = 2;    // Chẩn đoán hình ảnh
    }
    public class ExternalFile
    {
        public long Id { get; set; }

        public long TablePatientId { get; set; }
        public string PId { get; set; }
        public string MaBenhAn { get; set; }
        public string? PatientName { get; set; }

        public int ModuleType { get; set; }

        public string StoredFileName { get; set; }
        public string RelativePath { get; set; }

        public bool IsVisibleToUser { get; set; }
        public bool IsDeleted { get; set; }

        public DateTime CreatedOn { get; set; }
        public long? CreatedBy { get; set; }
    }

}
