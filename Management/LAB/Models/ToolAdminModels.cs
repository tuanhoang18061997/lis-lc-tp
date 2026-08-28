using System.ComponentModel.DataAnnotations;

namespace Management.Models
{
    public class ToolAdminResultRow
    {
        public string TargetType { get; set; } = string.Empty;
        public long PatientTableId { get; set; }
        public long? ResultCDHAId { get; set; }
        public long? ServiceId { get; set; }
        public string ModuleCode { get; set; } = string.Empty;
        public string PatientCode { get; set; } = string.Empty;
        public string MedicalRecordCode { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string MaDotKham { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public List<string> ServiceNames { get; set; } = new();
        public DateTime ReturnResultTime { get; set; }
        public bool IsLockedByDate { get; set; }
        public bool HasActiveUnlock { get; set; }
        public bool UnlockWasUsed { get; set; }
        public DateTime? UnlockExpireAt { get; set; }
        public string UnlockReason { get; set; } = string.Empty;
        public string UnlockedByName { get; set; } = string.Empty;
    }

    public class ToolAdminUnlockRequest
    {
        [Required]
        public string TargetType { get; set; } = string.Empty;
        public long PatientId { get; set; }
        public long? ResultCDHAId { get; set; }
        public string ModuleCode { get; set; } = string.Empty;

        [Required, StringLength(1000, MinimumLength = 5)]
        public string Reason { get; set; } = string.Empty;

        [Range(5, 1440)]
        public int DurationMinutes { get; set; } = 60;
    }

    public class ToolAdminLockRequest
    {
        [Required]
        public string TargetType { get; set; } = string.Empty;
        public long PatientId { get; set; }
        public long? ResultCDHAId { get; set; }
        public string ModuleCode { get; set; } = string.Empty;
        public string? Reason { get; set; }
    }

    public class InvalidCDHARequest
    {
        public long PatientId { get; set; }
        public List<long> ResultIds { get; set; } = new();
    }

    public class ResultInvalidOutcome
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public static ResultInvalidOutcome Ok() => new() { Success = true };
        public static ResultInvalidOutcome Fail(string message) => new() { Success = false, Message = message };
    }
}
