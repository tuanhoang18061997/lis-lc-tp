using System;
using System.Collections.Generic;

namespace Management.Models
{
    #region Request Models

    /// <summary>
    /// Request để cập nhật thông tin bệnh nhân
    /// CHÚ Ý: Chỉ PatientId là bắt buộc
    /// Các field khác là optional - chỉ truyền field nào cần update
    /// Hệ thống sẽ tự động cập nhật TẤT CẢ các bản ghi liên quan đến PatientId
    /// </summary>
    public class UpdatePatientRequest
    {
        /// <summary>
        /// Mã bệnh nhân (REQUIRED)
        /// </summary>
        public string PatientId { get; set; }

        /// <summary>
        /// Mã phiếu khám (OPTIONAL - nếu không truyền, sẽ cập nhật tất cả phiếu khám của bệnh nhân)
        /// </summary>
        public string? TicketId { get; set; }

        /// <summary>
        /// Tên bệnh nhân (OPTIONAL - chỉ truyền khi cần update)
        /// </summary>
        public string? PatientName { get; set; }

        /// <summary>
        /// Giới tính: "male" hoặc "female" (OPTIONAL)
        /// </summary>
        public string? Sex { get; set; }

        /// <summary>
        /// Ngày sinh format: "yyyy-MM-dd" hoặc "dd/MM/yyyy" (OPTIONAL)
        /// Ví dụ: "1990-05-15"
        /// </summary>
        public string? Age { get; set; }

        /// <summary>
        /// Địa chỉ (OPTIONAL)
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Chẩn đoán (OPTIONAL)
        /// </summary>
        public string? Diagnostic { get; set; }

        /// <summary>
        /// Tên bác sĩ (OPTIONAL)
        /// </summary>
        public string? DoctorName { get; set; }

        /// <summary>
        /// Tên đối tượng (OPTIONAL)
        /// Ví dụ: "BHYT", "Thu phí", "Viện phí"
        /// </summary>
        public string? ObjectName { get; set; }

        /// <summary>
        /// Tên phòng/khoa (OPTIONAL)
        /// </summary>
        public string? LocationName { get; set; }

        /// <summary>
        /// Mã bệnh án (OPTIONAL)
        /// </summary>
        public string? MaBenhAn { get; set; }

        /// <summary>
        /// Mã đợt khám (OPTIONAL)
        /// </summary>
        public string? MaDotKham { get; set; }

        /// <summary>
        /// Phone (OPTIONAL)
        /// </summary>
        public string? Phone { get; set; }
    }

    /// <summary>
    /// Request để cập nhật dịch vụ không thực hiện
    /// </summary>
    public class UpdateServiceNotPerformedRequest
    {
        /// <summary>
        /// Mã TicketItemId (REQUIRED)
        /// </summary>
        public string TicketItemId { get; set; }

        /// <summary>
        /// Mã dịch vụ ServiceCode (REQUIRED)
        /// </summary>
        public string ServiceCode { get; set; }

        /// <summary>
        /// Tên bác sĩ xác nhận không thực hiện (OPTIONAL)
        /// </summary>
        public string? DoctorName { get; set; }

        /// <summary>
        /// Loại bệnh án: "out", "in", "package" (OPTIONAL)
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Lý do không thực hiện (OPTIONAL)
        /// </summary>
        public string? Reason { get; set; }
    }

    #endregion

    #region Response Models

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ErrorCode { get; set; }
        public object Data { get; set; }
    }

    #endregion
}
