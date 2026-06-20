using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Management.Models
{

    [Table("UserType")]
    [Index(nameof(UserId), nameof(TypeId), IsUnique = true)]
    public class UserType
    {
        
        [Key]
        public long Id { get; set; }

        public long? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public long? TypeId { get; set; }
        [ForeignKey("TypeId")]
        public virtual Type? Type { get; set; }
    }
}
