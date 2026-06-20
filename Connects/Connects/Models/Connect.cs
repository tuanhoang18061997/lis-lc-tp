using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;

namespace Connects.Models
{
    [Table("Connect")]
    [Index(nameof(ComputerId), nameof(PortId), nameof(DeviceId), IsUnique = true)]
    public class Connect
    {
        [Key]
        public long Id { get; set; }
        public string ComputerId { get; set; }
        public int PortId { get; set; }

        public long? DeviceId { get; set; }
        [ForeignKey("DeviceId")]
        public virtual Device? Device { get; set; }
       
        public string? Com { get; set; }
        public double? BaudRate { get; set; }
        public double? DataBit { get; set; }
        public string? Parity { get; set; }
        public double? StopBit { get; set; }
        public bool Rts { get; set; }
        public string? Description { get; set; }
        public bool Status { get; set; }       
    }
}
