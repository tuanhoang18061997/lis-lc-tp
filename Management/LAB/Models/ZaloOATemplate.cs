using System;

namespace Management.Models
{
    public class ZaloOATemplate
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

        public int ZaloOAConfigId { get; set; }
        public int? HospitalId { get; set; }

        public string TemplateZaloId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }

        public string Notes { get; set; }

        public DateTime CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int? UpdatedBy { get; set; }
        public string TemplateParams { get; set; }
    }
}