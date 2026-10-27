using System;
using System.Collections.Generic;

namespace Management.Models
{
    public class XNServiceStatisticsFilter
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? MaDotKham { get; set; }
        public string? CategoryCode { get; set; }
        public long? ServiceId { get; set; }
        public bool OnlyCompleted { get; set; } = true;
        public string TimeMode { get; set; } = "GETSAMPLE";
    }

    public class XNServiceStatisticsReport
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? MaDotKham { get; set; }
        public string? CategoryCode { get; set; }
        public long? ServiceId { get; set; }
        public bool OnlyCompleted { get; set; }
        public string TimeMode { get; set; } = "GETSAMPLE";
        public long TotalRows { get; set; }
        public List<XNServiceStatisticsRow> Rows { get; set; } = new List<XNServiceStatisticsRow>();
    }

    public class XNServiceStatisticsRow
    {
        // 15 cot A:O theo template [LIS]_Thong ke Dich vu XN.xlsx.
        public long STT { get; set; }
        public string? ID { get; set; }
        public string? SID { get; set; }
        public int? NamSinh { get; set; }
        public string? GioiTinh { get; set; }
        public string LoaiMau { get; set; } = "";
        public string? TenBenhNhan { get; set; }
        public string? NguoiNhap { get; set; }
        public string? NguoiDuyet { get; set; }
        public DateTime? InTime { get; set; }
        public string TrangThai { get; set; } = "";
        public string? TenXN { get; set; }
        public string? NhomXN { get; set; }
        public string TenBacSiKTV { get; set; } = "";
        public string? MaHopDong { get; set; }

        // Truong ky thuat de doi chieu, KHONG thuoc 15 cot Excel.
        public long PatientDbId { get; set; }
        public long ServiceId { get; set; }
        public string? TicketItemId { get; set; }
        public bool MissingTicketItemId => string.IsNullOrWhiteSpace(TicketItemId);
    }
}
