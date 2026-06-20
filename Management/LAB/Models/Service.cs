using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Management.Models
{
    [Table("Service")]
    [Index(nameof(Name))]
    [Index(nameof(Code), IsUnique = true)]
    [Index(nameof(CategoryId), nameof(PrintSampleId))]  
    public class Service
    {
        
        [Key]
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

        public long? CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        public long? PrintSampleId { get; set; }
        [ForeignKey("PrintSampleId")]
        public virtual PrintSample? PrintSample { get; set; }

        public bool ProcessType { get; set; } // True => Covid
        public int? PrintOrder { get; set; }
        public bool Active { get; set; }

        public virtual List<ServiceTest> ServiceTests { get; set; }
    }
}
