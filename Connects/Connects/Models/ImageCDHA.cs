using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;

namespace Connects.Models
{
    [Table("ImageCDHA")]
    [Index(nameof(ResultCDHAId))]
    public class ImageCDHA
    {
        [Key]
        public long Id { get; set; }
        public string ImagePath { get; set; }
        public string ImagePath1 { get; set; }

        public long? ResultCDHAId { get; set; }
        [ForeignKey("ResultCDHAId")]
        public virtual ResultCDHA? ResultCDHA { get; set; }
    }
}
