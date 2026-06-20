using System;

namespace Management.Models.DTO
{
    public class ZnsResultXNSendModel
    {
        public long PatientId { get; set; }
        public string Sid { get; set; }
        public string PatientName { get; set; }
        public string MaBenhAn { get; set; }
        public string Phone { get; set; }
        public string Service { get; set; }
        public string UID { get; set; }
        public string Status { get; set; }
        public long HospitalId { get; set; }
        public int ZaloOAConfigId { get; set; }
        public int ZaloOATemplateId { get; set; }

        public string Title { get; set; }
        public string Content { get; set; }

        public string PdfUrl { get; set; }
    }
}