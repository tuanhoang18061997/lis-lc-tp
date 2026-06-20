using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;

namespace Connects.Models
{
    [Table("TestCode")]
    [Index(nameof(Code),IsUnique = true)]
    [Index(nameof(Name), nameof(CategoryId))]
    public class TestCode
    {
        
        [Key]
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

        public long? TestTypeId { get; set; }
        [ForeignKey("TestTypeId")]
        public virtual TestType? TestType { get; set; }

        public long? CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        public string? NormalRangeM { get; set; }
        public string? NormalRangeF { get; set; }
        public string? NormalResult { get; set; }
        public double? LowerLimit { get; set; }
        public double? HigherLimit { get; set; }
        public string? Unit { get; set; }
        public bool IsTestHead { get; set; }
        public bool IsTestChild { get; set; }
        public int? PrintOrder { get; set; }        
        public string? CodeBHYT { get; set; }
        public string? NameBHYT { get; set; }
        public bool Active { get; set; }

        public virtual List<Map> Maps { get; set; }
    }
}
