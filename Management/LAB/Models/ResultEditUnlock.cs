using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Management.Models
{
    [Table("ResultEditUnlock")]
    [Index(nameof(PatientId), nameof(ModuleCode), nameof(IsActive))]
    [Index(nameof(ResultCDHAId), nameof(IsActive))]
    public class ResultEditUnlock
    {
        [Key]
        public long Id { get; set; }

        public long PatientId { get; set; }

        [Required, MaxLength(20)]
        public string ModuleCode { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string Scope { get; set; } = string.Empty;

        public long? ResultCDHAId { get; set; }
        public long? ServiceId { get; set; }

        [Required, MaxLength(20)]
        public string Source { get; set; } = "ADMIN";

        [Required, MaxLength(1000)]
        public string UnlockReason { get; set; } = string.Empty;

        public DateTime UnlockedAt { get; set; }
        public DateTime ExpireAt { get; set; }
        public long UnlockedByUserId { get; set; }
        public DateTime? UsedAt { get; set; }
        public long? UsedByUserId { get; set; }
        public DateTime? RevokedAt { get; set; }
        public long? RevokedByUserId { get; set; }

        [MaxLength(500)]
        public string? RevokeReason { get; set; }

        public bool IsActive { get; set; }
    }
}
