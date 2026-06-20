namespace Management.Models
{
    public class PatientResultPortalModel
    {
        public long Id { get; set; }
        public string Type { get; set; } // X-Quang, Siêu âm, etc.
        public string TypeCode { get; set; } // XQ, SA, SAT, DDT, NS
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public string Result { get; set; }
        public string Suggest { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ReturnResultDate { get; set; }
        public string DoctorName { get; set; }
        public string KeyResult { get; set; }
        public bool HasImages { get; set; }
        public int ImageCount { get; set; }
        
        // Thêm thông tin ?nh ?? hi?n th? tr?c ti?p
        public List<PatientResultImageModel> Images { get; set; } = new List<PatientResultImageModel>();
    }

    public class PatientResultImageModel
    {
        public long Id { get; set; }
        public string ImagePath { get; set; }
        public string FullImageUrl { get; set; }
        public string ThumbnailUrl { get; set; }
    }
}