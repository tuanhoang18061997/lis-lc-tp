using System;
using System.IO;
using ClosedXML.Excel;
using Management.Models;

namespace Management.BL
{
    public partial class ReportBL
    {
        // REPORT-06: dung template Excel goc, nhan ket qua tu LC_GetXNServiceStatisticsAsync; KHONG query DB lan hai tai day.
        public byte[] LC_ExportXNServiceStatisticsExcel(XNServiceStatisticsReport report, string templatePath)
        {
            if (report == null) throw new ArgumentNullException(nameof(report));
            if (string.IsNullOrWhiteSpace(templatePath)) throw new ArgumentException("Thiếu đường dẫn template Excel.", nameof(templatePath));
            if (report.Rows == null || report.Rows.Count != report.TotalRows) throw new InvalidDataException("Số dòng báo cáo không khớp totalRows.");
            if (!File.Exists(templatePath)) throw new FileNotFoundException("Không tìm thấy template thống kê dịch vụ XN.", templatePath);
            if (report.Rows.Count > 1048574) throw new InvalidDataException("Số dòng vượt giới hạn một sheet Excel.");

            using var workbook = new XLWorkbook(templatePath);
            var ws = workbook.Worksheet("XN");
            var expectedHeaders = new[] { "STT", "ID", "SID", "Năm sinh", "Giới tính", "Loại mẫu", "Tên Bệnh Nhân", "Người Nhập", "Người Duyệt", "InTime", "Trạng thái", "Tên XN", "Nhóm XN", "Tên Bác Sĩ/KTV", "Mã Hợp Đồng" };
            for (var column = 1; column <= expectedHeaders.Length; column++)
            {
                if (!string.Equals(ws.Cell(2, column).GetString().Trim(), expectedHeaders[column - 1], StringComparison.Ordinal))
                    throw new InvalidDataException($"Template sai tiêu đề tại cột {column}: {expectedHeaders[column - 1]}.");
            }

            const int firstDataRow = 3;
            const int templateFormattedLastRow = 1221; // Template goc co san border den dong 1221.
            var lastDataRow = report.Rows.Count + firstDataRow - 1;
            if (lastDataRow > templateFormattedLastRow)
                ws.Range(templateFormattedLastRow + 1, 1, lastDataRow, 15).Style = ws.Range(3, 1, 3, 15).Style;
            else if (lastDataRow < templateFormattedLastRow)
                ws.Rows(lastDataRow + 1, templateFormattedLastRow).Delete();

            // ID, SID phai la TEXT de giu so 0 o dau, Năm sinh va STT la so, InTime la ngay gio Excel.
            if (report.Rows.Count > 0)
            {
                ws.Range(3, 2, lastDataRow, 3).Style.NumberFormat.Format = "@";
                ws.Range(3, 10, lastDataRow, 10).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
            }

            for (var index = 0; index < report.Rows.Count; index++)
            {
                var item = report.Rows[index];
                var row = firstDataRow + index;
                ws.Cell(row, 1).Value = index + 1; // Danh so lien tuc dung theo thu tu bao cao.
                ws.Cell(row, 2).Value = item.ID ?? string.Empty;
                ws.Cell(row, 3).Value = item.SID ?? string.Empty;
                if (item.NamSinh.HasValue) ws.Cell(row, 4).Value = item.NamSinh.Value;
                ws.Cell(row, 5).Value = item.GioiTinh ?? string.Empty;
                ws.Cell(row, 6).Value = item.LoaiMau ?? string.Empty;
                ws.Cell(row, 7).Value = item.TenBenhNhan ?? string.Empty;
                ws.Cell(row, 8).Value = item.NguoiNhap ?? string.Empty;
                ws.Cell(row, 9).Value = item.NguoiDuyet ?? string.Empty;
                if (item.InTime.HasValue) ws.Cell(row, 10).Value = item.InTime.Value;
                ws.Cell(row, 11).Value = item.TrangThai ?? string.Empty;
                ws.Cell(row, 12).Value = item.TenXN ?? string.Empty;
                ws.Cell(row, 13).Value = item.NhomXN ?? string.Empty;
                ws.Cell(row, 14).Value = item.TenBacSiKTV ?? string.Empty;
                ws.Cell(row, 15).Value = item.MaHopDong ?? string.Empty;
            }

            // Giu nguyen dinh dang header xanh, chieu rong, border va freeze 2 dong co san trong template.
            using var output = new MemoryStream();
            workbook.SaveAs(output);
            return output.ToArray();
        }
    }
}
