using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;

namespace Connects.Models
{
    [Table("Session")]
    [Index(nameof(UserId), nameof(ComputerId), nameof(IsConnect), nameof(IsManage), IsUnique = true)]
    public class Session
    {
        
        [Key]
        public long Id { get; set; }
        
        public long? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public string ComputerId { get; set; }
        public bool Status { get; set; }
        public bool IsConnect { get; set; }
        public bool IsManage { get; set; }
    }
}
