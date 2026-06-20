using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;

namespace Connects.Models
{
    [Table("TestType")]
    [Index(nameof(Name))]
    [Index(nameof(Code), IsUnique = true)]
    public class TestType
    {
        
        [Key]
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public virtual List<TestCode> TestCodes { get; set; }
    }
}
