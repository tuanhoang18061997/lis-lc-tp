using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;

namespace Connects.Models
{
    
    [Table("UserFunction")]
    [Index(nameof(UserId), nameof(FunctionId), IsUnique = true)]
    public class UserFunction
    {
        
        [Key]
        public long Id { get; set; }

        public long? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public long? FunctionId { get; set; }
        [ForeignKey("FunctionId")]
        public virtual Function? Function { get; set; }
    }
}
