using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Management.Models;
using Microsoft.EntityFrameworkCore;

namespace Management.BL
{
    public partial class ReportBL
    {
        // Tương thích với LC_Search(type=5) và ExportPatientServicesByDateXN đang chạy.
        // Một dòng = một Patient.Id + ServiceId trong ngày Patient.InsertTime, giống báo cáo cũ.
        public Task<XNServiceByDateReport> LC_GetPatientServicesByDateXN(DateTime fromtime, DateTime totime)
        {
            return LC_GetPatientServicesByDateXN(fromtime, totime, CancellationToken.None);
        }

        public async Task<XNServiceByDateReport> LC_GetPatientServicesByDateXN(DateTime fromtime, DateTime totime, CancellationToken cancellationToken)
        {
            var start = fromtime.Date;
            var endExclusive = totime.Date.AddDays(1);
            if (start >= endExclusive) throw new ArgumentException("Khoảng ngày không hợp lệ.");

            var report = new XNServiceByDateReport { FromDate = start, ToDate = totime.Date, DateGroups = new List<XNServiceByDateGroup>() };
            var connection = _db.Database.GetDbConnection();
            var shouldClose = connection.State != ConnectionState.Open;
            if (shouldClose) await connection.OpenAsync(cancellationToken);
            try
            {
                using var command = connection.CreateCommand();
                command.CommandTimeout = 45;
                command.CommandText = @"
SELECT p.InsertTime AS ReportTime, p.Seq, p.PatientName, p.Sex, p.Age, p.BenhAn, p.Address,
       s.Name AS ServiceName, u.Name AS ReaderName
FROM dbo.Patient AS p
CROSS APPLY (
    SELECT r.ServiceId
    FROM dbo.ResultXN AS r
    WHERE r.PatientId = p.Id AND r.Active = 1 AND r.ServiceId IS NOT NULL
    GROUP BY r.ServiceId
) AS services
INNER JOIN dbo.Service AS s ON s.Id = services.ServiceId
LEFT JOIN dbo.[User] AS u ON u.Id = p.UserReturnResultXN
WHERE p.Active = 1 AND (p.ValidXN = 1 OR p.ProcessXN = 1)
  AND p.InsertTime >= @FromInclusive AND p.InsertTime < @ToExclusive
ORDER BY CONVERT(date, p.InsertTime), p.Seq, p.PatientName, s.Name, p.Id, s.Id
OPTION (RECOMPILE);";
                var fromParam = command.CreateParameter();
                fromParam.ParameterName = "@FromInclusive";
                fromParam.DbType = DbType.DateTime2;
                fromParam.Value = start;
                command.Parameters.Add(fromParam);
                var toParam = command.CreateParameter();
                toParam.ParameterName = "@ToExclusive";
                toParam.DbType = DbType.DateTime2;
                toParam.Value = endExclusive;
                command.Parameters.Add(toParam);

                using var reader = await command.ExecuteReaderAsync(cancellationToken);
                XNServiceByDateGroup currentGroup = null;
                var currentYear = DateTime.Now.Year;
                while (await reader.ReadAsync(cancellationToken))
                {
                    var day = reader.GetDateTime(0).Date;
                    if (currentGroup == null || currentGroup.Date != day)
                    {
                        currentGroup = new XNServiceByDateGroup { Date = day, Rows = new List<XNServiceByDateRow>() };
                        report.DateGroups.Add(currentGroup);
                    }
                    DateTime? birthDate = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4);
                    var benhAn = reader.IsDBNull(5) ? "" : reader.GetString(5);
                    currentGroup.Rows.Add(new XNServiceByDateRow
                    {
                        STT = currentGroup.Rows.Count + 1,
                        Date = day,
                        PatientName = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        Gender = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        AgeValue = birthDate.HasValue ? currentYear - birthDate.Value.Year : (int?)null,
                        ObjectName = XNByDateMapObjectName(benhAn),
                        Address = reader.IsDBNull(6) ? "" : reader.GetString(6),
                        ServiceName = reader.IsDBNull(7) ? "" : reader.GetString(7),
                        ReaderName = reader.IsDBNull(8) ? "" : reader.GetString(8)
                    });
                }
                return report;
            }
            finally
            {
                // Connection do EF Core quản lý: chỉ đóng nếu chính phương thức này mở nó.
                if (shouldClose) await connection.CloseAsync();
            }
        }

        private static string XNByDateMapObjectName(string benhAn)
        {
            if (string.IsNullOrWhiteSpace(benhAn)) return "";
            switch (benhAn.Trim().ToLowerInvariant())
            {
                case "pakage": return "Khám sức khỏe";
                case "out": return "Dịch vụ";
                case "in": return "Nội trú";
                default: return benhAn;
            }
        }

        // Giữ 9 cột và cách nhóm ngày của export cũ, bỏ AdjustToContents toàn sheet (rất chậm khi nhiều dòng).
        public byte[] ExportPatientServicesByDateToExcel(XNServiceByDateReport model)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("BaoCaoXN");
            var widths = new[] { 17d, 9d, 32d, 12d, 10d, 19d, 39d, 49d, 28d };
            for (int i = 0; i < widths.Length; i++) ws.Column(i + 1).Width = widths[i];
            var currentRow = 1;
            ws.Cell(currentRow, 1).Value = "BÁO CÁO XÉT NGHIỆM";
            ws.Range(currentRow, 1, currentRow, 9).Merge().Style.Font.SetBold().Font.SetFontSize(16).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            currentRow++;
            ws.Cell(currentRow, 1).Value = $"Từ ngày {model.FromDate:dd/MM/yyyy} đến {model.ToDate:dd/MM/yyyy}";
            ws.Range(currentRow, 1, currentRow, 9).Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            currentRow += 2;
            foreach (var group in model.DateGroups)
            {
                ws.Cell(currentRow, 1).Value = $"NGÀY THỰC HIỆN: {group.Date:dd/MM/yyyy}";
                ws.Range(currentRow, 1, currentRow, 9).Merge().Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightGray);
                currentRow++;
                var headers = new[] { "NGÀY THỰC HIỆN", "STT", "HỌ TÊN", "GIỚI TÍNH", "TUỔI", "ĐỐI TƯỢNG", "ĐỊA CHỈ", "YÊU CẦU", "NGƯỜI ĐỌC" };
                for (int i = 0; i < headers.Length; i++) ws.Cell(currentRow, i + 1).Value = headers[i];
                ws.Range(currentRow, 1, currentRow, 9).Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center).Fill.SetBackgroundColor(XLColor.LightGray);
                currentRow++;
                foreach (var row in group.Rows)
                {
                    ws.Cell(currentRow, 1).Value = row.Date.ToString("yyyy-MM-dd");
                    ws.Cell(currentRow, 2).Value = row.STT;
                    ws.Cell(currentRow, 3).Value = row.PatientName ?? "";
                    ws.Cell(currentRow, 4).Value = row.Gender ?? "";
                    if (row.AgeValue.HasValue) ws.Cell(currentRow, 5).Value = row.AgeValue.Value;
                    ws.Cell(currentRow, 6).Value = row.ObjectName ?? "";
                    ws.Cell(currentRow, 7).Value = row.Address ?? "";
                    ws.Cell(currentRow, 8).Value = row.ServiceName ?? "";
                    ws.Cell(currentRow, 9).Value = row.ReaderName ?? "";
                    currentRow++;
                }
                ws.Cell(currentRow, 1).Value = $"TỔNG: {group.Total}";
                ws.Range(currentRow, 1, currentRow, 9).Merge().Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightGray);
                currentRow += 2;
            }
            var used = ws.RangeUsed();
            if (used != null)
            {
                used.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                used.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                used.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }
            using var output = new System.IO.MemoryStream();
            workbook.SaveAs(output);
            return output.ToArray();
        }
    }
}
