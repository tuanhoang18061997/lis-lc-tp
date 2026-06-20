using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;

namespace Connects.Models
{
    [Table("Category")]
    [Index(nameof(Name))]
    [Index(nameof(Code), IsUnique = true)]
    public class Category
    {
        [Key]
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int? PrintOrder { get; set; }

        public long? GroupId { get; set; }
        [ForeignKey("GroupId")]
        public virtual Group? Group { get; set; }
        public bool Active { get; set; }

        public virtual List<Sample> Samples { get; set; }
        public virtual List<Service> Services { get; set; }
        public virtual List<TestCode> TestCodes { get; set; }
    }
}
