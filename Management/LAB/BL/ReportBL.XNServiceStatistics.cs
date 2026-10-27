using Management.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Management.BL
{
    public partial class ReportBL
    {
        // REPORT-04C: chi READ; bat buoc MaDotKham, tra tat ca luot dich vu trong dot kham.
        // Chon ADO.NET parameterized vi can gom TicketItemId nvarchar(max) va STRING_AGG DISTINCT.
        public async Task<XNServiceStatisticsReport> LC_GetXNServiceStatisticsAsync(XNServiceStatisticsFilter filter, CancellationToken cancellationToken = default)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));
            if (filter.FromDate == default || filter.ToDate == default || filter.FromDate.Date > filter.ToDate.Date)
                throw new ArgumentException("Khoảng ngày không hợp lệ.", nameof(filter));
            if (filter.ServiceId.HasValue && filter.ServiceId.Value <= 0) throw new ArgumentOutOfRangeException(nameof(filter.ServiceId));

            var timeMode = (filter.TimeMode ?? "GETSAMPLE").Trim().ToUpperInvariant();
            if (timeMode != "GETSAMPLE" && timeMode != "INSERT") throw new ArgumentException("TimeMode chỉ nhận GETSAMPLE hoặc INSERT.", nameof(filter));
            var maDotKham = string.IsNullOrWhiteSpace(filter.MaDotKham) ? null : filter.MaDotKham.Trim();
            var categoryCode = string.IsNullOrWhiteSpace(filter.CategoryCode) ? null : filter.CategoryCode.Trim();
            if (maDotKham == null) throw new ArgumentException("Phải chọn Mã Đợt Khám trước khi xem báo cáo.", nameof(filter));
            if (maDotKham.Length > 250) throw new ArgumentException("MaDotKham vượt quá 250 ký tự.", nameof(filter));
            if (categoryCode?.Length > 100) throw new ArgumentException("CategoryCode vượt quá 100 ký tự.", nameof(filter));

            var report = new XNServiceStatisticsReport
            {
                FromDate = filter.FromDate.Date, ToDate = filter.ToDate.Date,
                MaDotKham = maDotKham, CategoryCode = categoryCode, ServiceId = filter.ServiceId,
                OnlyCompleted = filter.OnlyCompleted, TimeMode = timeMode
            };

            var connection = _db.Database.GetDbConnection();
            var openedHere = connection.State != ConnectionState.Open;
            if (openedHere) await connection.OpenAsync(cancellationToken);
            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = XNServiceStatisticsSql;
                command.CommandType = CommandType.Text;
                command.CommandTimeout = _db.Database.GetCommandTimeout() ?? 60;
                if (_db.Database.CurrentTransaction != null) command.Transaction = _db.Database.CurrentTransaction.GetDbTransaction();
                AddParameter(command, "@FromDate", DbType.DateTime2, report.FromDate);
                AddParameter(command, "@EndExclusive", DbType.DateTime2, report.ToDate.AddDays(1));
                AddParameter(command, "@MaDotKham", DbType.String, maDotKham ?? (object)DBNull.Value, 250);
                AddParameter(command, "@CategoryCode", DbType.String, categoryCode ?? (object)DBNull.Value, 100);
                AddParameter(command, "@ServiceId", DbType.Int64, filter.ServiceId.HasValue ? (object)filter.ServiceId.Value : DBNull.Value);
                AddParameter(command, "@OnlyCompleted", DbType.Boolean, filter.OnlyCompleted);
                AddParameter(command, "@TimeMode", DbType.AnsiString, timeMode, 12);

                using var reader = await command.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken)) report.TotalRows = reader.GetInt64(0);
                if (!await reader.NextResultAsync(cancellationToken)) throw new DataException("Query không trả về result set chi tiết.");
                while (await reader.ReadAsync(cancellationToken))
                {
                    report.Rows.Add(new XNServiceStatisticsRow
                    {
                        STT = reader.GetInt64(0), ID = NullableText(reader, 1), SID = NullableText(reader, 2),
                        NamSinh = reader.IsDBNull(3) ? null : reader.GetInt32(3), GioiTinh = NullableText(reader, 4),
                        LoaiMau = NullableText(reader, 5) ?? "", TenBenhNhan = NullableText(reader, 6),
                        NguoiNhap = NullableText(reader, 7), NguoiDuyet = NullableText(reader, 8),
                        InTime = reader.IsDBNull(9) ? null : reader.GetDateTime(9), TrangThai = NullableText(reader, 10) ?? "",
                        TenXN = NullableText(reader, 11), NhomXN = NullableText(reader, 12), TenBacSiKTV = NullableText(reader, 13) ?? "",
                        MaHopDong = NullableText(reader, 14), PatientDbId = reader.GetInt64(15), ServiceId = reader.GetInt64(16),
                        TicketItemId = NullableText(reader, 17)
                    });
                }
                return report;
            }
            finally
            {
                if (openedHere) await connection.CloseAsync();
            }
        }

        private static string? NullableText(DbDataReader reader, int index) => reader.IsDBNull(index) ? null : reader.GetString(index);

        private static void AddParameter(DbCommand command, string name, DbType type, object value, int size = 0)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.DbType = type;
            if (size > 0) parameter.Size = size;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }

        private const string XNServiceStatisticsSql = @"
SET NOCOUNT ON;
DROP TABLE IF EXISTS #XNReportRaw;
DROP TABLE IF EXISTS #XNReportKeys;
DROP TABLE IF EXISTS #XNReportSpecimen;
DROP TABLE IF EXISTS #XNReportTechnician;

SELECT p.Id AS PatientDbId, r.Id AS ResultXNId, r.ServiceId,
    NULLIF(LTRIM(RTRIM(r.TicketItemId)), N'') AS TicketItemId,
    CASE WHEN @TimeMode = 'GETSAMPLE' THEN p.GetSampleTimeXN ELSE p.InsertTime END AS InTime,
    tt.Name AS TestTypeName, resultDoctor.Name AS ResultDoctorName
INTO #XNReportRaw
FROM dbo.Patient AS p
INNER JOIN dbo.ResultXN AS r ON r.PatientId = p.Id AND r.Active = 1
INNER JOIN dbo.Service AS s ON s.Id = r.ServiceId
INNER JOIN dbo.Category AS c ON c.Id = s.CategoryId
INNER JOIN dbo.[Group] AS gr ON gr.Id = c.GroupId AND gr.Code = 'XN'
LEFT JOIN dbo.TestCode AS tc ON tc.Id = r.TestCodeId
LEFT JOIN dbo.TestType AS tt ON tt.Id = tc.TestTypeId
LEFT JOIN dbo.Doctor AS resultDoctor ON resultDoctor.Id = r.DoctorId
WHERE p.Active = 1
    AND (@OnlyCompleted = 0 OR p.ValidXN = 1)
    AND p.MaDotKham = @MaDotKham
    AND (@CategoryCode IS NULL OR c.Code = @CategoryCode)
    AND (@ServiceId IS NULL OR r.ServiceId = @ServiceId)
    AND (
        (@TimeMode = 'GETSAMPLE' AND p.GetSampleTimeXN >= @FromDate AND p.GetSampleTimeXN < @EndExclusive)
        OR (@TimeMode = 'INSERT' AND p.InsertTime >= @FromDate AND p.InsertTime < @EndExclusive)
    )
OPTION (RECOMPILE);

SELECT PatientDbId, ServiceId, TicketItemId, MIN(InTime) AS InTime
INTO #XNReportKeys
FROM #XNReportRaw
GROUP BY PatientDbId, ServiceId, TicketItemId;

SELECT COUNT_BIG(*) AS TotalRows FROM #XNReportKeys;

-- Gom loai mau va bac si/KTV mot lan tren toan bo dot kham.
-- Khong dung OUTER APPLY lap lai cho tung dong khi bo phan trang.
SELECT x.PatientDbId, x.ServiceId, x.TicketItemId,
    STRING_AGG(CONVERT(nvarchar(max), x.Name), N', ') WITHIN GROUP (ORDER BY x.Name) AS Names
INTO #XNReportSpecimen
FROM (
    SELECT DISTINCT PatientDbId, ServiceId, TicketItemId,
        NULLIF(LTRIM(RTRIM(TestTypeName)), N'') AS Name
    FROM #XNReportRaw
) AS x
WHERE x.Name IS NOT NULL
GROUP BY x.PatientDbId, x.ServiceId, x.TicketItemId;

SELECT x.PatientDbId, x.ServiceId, x.TicketItemId,
    STRING_AGG(CONVERT(nvarchar(max), x.Name), N', ') WITHIN GROUP (ORDER BY x.Name) AS Names
INTO #XNReportTechnician
FROM (
    SELECT DISTINCT PatientDbId, ServiceId, TicketItemId,
        NULLIF(LTRIM(RTRIM(ResultDoctorName)), N'') AS Name
    FROM #XNReportRaw
) AS x
WHERE x.Name IS NOT NULL
GROUP BY x.PatientDbId, x.ServiceId, x.TicketItemId;

;WITH Numbered AS (
    SELECT ROW_NUMBER() OVER (ORDER BY InTime DESC, PatientDbId DESC, ServiceId, TicketItemId) AS RowNo,
        PatientDbId, ServiceId, TicketItemId, InTime
    FROM #XNReportKeys
)
SELECT k.RowNo, p.PatientId AS ID, p.Seq AS SID, YEAR(p.Age) AS NamSinh,
    p.Sex AS GioiTinh, ISNULL(specimen.Names, N'') AS LoaiMau,
    p.PatientName AS TenBenhNhan, requestDoctor.Name AS NguoiNhap, approvedUser.Name AS NguoiDuyet,
    k.InTime,
    CASE WHEN p.ValidXN = 1 THEN N'Đã hoàn thành'
         WHEN p.ProcessXN = 1 THEN N'Đang thực hiện' ELSE N'Chưa hoàn thành' END AS TrangThai,
    s.Name AS TenXN, c.Name AS NhomXN, ISNULL(technician.Names, N'') AS TenBacSiKTV,
    p.MaDotKham AS MaHopDong, k.PatientDbId, k.ServiceId, k.TicketItemId
FROM Numbered AS k
INNER JOIN dbo.Patient AS p ON p.Id = k.PatientDbId
INNER JOIN dbo.Service AS s ON s.Id = k.ServiceId
INNER JOIN dbo.Category AS c ON c.Id = s.CategoryId
LEFT JOIN dbo.Doctor AS requestDoctor ON requestDoctor.Id = p.DoctorId
LEFT JOIN dbo.[User] AS approvedUser ON approvedUser.Id = p.UserReturnResultXN
LEFT JOIN #XNReportSpecimen AS specimen ON specimen.PatientDbId = k.PatientDbId
    AND specimen.ServiceId = k.ServiceId
    AND (specimen.TicketItemId = k.TicketItemId OR (specimen.TicketItemId IS NULL AND k.TicketItemId IS NULL))
LEFT JOIN #XNReportTechnician AS technician ON technician.PatientDbId = k.PatientDbId
    AND technician.ServiceId = k.ServiceId
    AND (technician.TicketItemId = k.TicketItemId OR (technician.TicketItemId IS NULL AND k.TicketItemId IS NULL))
ORDER BY k.RowNo;";
    }
}
