using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Management.Models
{
    [Table("Setting")]
    [Index(nameof(Name))]
    [Index(nameof(Code), IsUnique = true)]
    public class Setting
    {
        
        [Key]
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }     
    }
}
