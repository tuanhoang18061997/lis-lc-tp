using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Management.Models
{
    [Table("Device")]
    [Index(nameof(Name))]
    [Index(nameof(Code), IsUnique = true)]
    public class Device
    {
        
        [Key]
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Protocol { get; set; }
        public bool ProcessQc { get; set; }
        public string? CodeBHYT { get; set; }
        public bool Active { get; set; }
        public virtual List<Map> Maps { get; set; }
    }
}
