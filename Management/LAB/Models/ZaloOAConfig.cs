using System;

namespace Management.Models
{
    public class ZaloOAConfig
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int? HospitalId { get; set; }
        public string ApiUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ZaloOaId { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int? UpdatedBy { get; set; }
    }
}