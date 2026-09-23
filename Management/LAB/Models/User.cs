using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Management.Models
{

    [Table("User")]
    [Index(nameof(Code), nameof(Name))]
    public class User
    {

        [Key]
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string MaBHYT { get; set; }
        public string Password { get; set; }
        public string Cccd { get; set; }

        public int? HisEmployeeId { get; set; }

        public bool Active { get; set; }
        public string Cks { get; set; }

        public virtual List<UserType> UserTypes { get; set; }
        public virtual List<UserFunction> UserFunctions { get; set; }
    }
}