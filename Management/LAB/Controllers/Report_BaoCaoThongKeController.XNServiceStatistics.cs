using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Management.Controllers
{
    public partial class Report_BaoCaoThongKeController
    {
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetXNServiceStatistics(
            [FromQuery(Name = "from")] string? fromText,
            [FromQuery(Name = "to")] string? toText,
            [FromQuery] string? maDotKham = null,
            [FromQuery] string? categoryCode = null,
            [FromQuery(Name = "serviceId")] string? serviceIdText = null,
            [FromQuery(Name = "onlyCompleted")] string? onlyCompletedText = null,
            [FromQuery] string? timeMode = null)
        {
            if (!ModelState.IsValid)
            {
                var fields = ModelState.Where(x => x.Value != null && x.Value.Errors.Count > 0).Select(x => x.Key).ToArray();
                _logger.LogWarning("REPORT-04C invalid ModelState fields: {Fields}", string.Join(", ", fields));
                return BadRequest(new { success = false, message = "Model binding không hợp lệ.", fields });
            }

            if (!DateTime.TryParseExact(fromText?.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fromDate))
                return BadRequest(new { success = false, field = "from", message = "from phải có định dạng yyyy-MM-dd." });
            if (!DateTime.TryParseExact(toText?.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var toDate))
                return BadRequest(new { success = false, field = "to", message = "to phải có định dạng yyyy-MM-dd." });
            if (fromDate.Date > toDate.Date || toDate.Date == DateTime.MaxValue.Date)
                return BadRequest(new { success = false, field = "to", message = "Khoảng thời gian không hợp lệ." });

            long? serviceId = null;
            if (!string.IsNullOrWhiteSpace(serviceIdText))
            {
                if (!long.TryParse(serviceIdText.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var parsedServiceId) || parsedServiceId <= 0)
                    return BadRequest(new { success = false, field = "serviceId", message = "serviceId phải là số nguyên dương." });
                serviceId = parsedServiceId;
            }

            bool onlyCompleted = true;
            if (!string.IsNullOrWhiteSpace(onlyCompletedText) && !bool.TryParse(onlyCompletedText.Trim(), out onlyCompleted))
                return BadRequest(new { success = false, field = "onlyCompleted", message = "onlyCompleted chỉ nhận true hoặc false." });

            maDotKham = string.IsNullOrWhiteSpace(maDotKham) ? null : maDotKham.Trim();
            if (maDotKham == null)
                return BadRequest(new { success = false, field = "maDotKham", message = "Phải chọn Mã Đợt Khám trước khi xem báo cáo." });
            categoryCode = string.IsNullOrWhiteSpace(categoryCode) ? null : categoryCode.Trim();
            if (maDotKham.Length > 250)
                return BadRequest(new { success = false, field = "maDotKham", message = "maDotKham không được vượt quá 250 ký tự." });
            if (categoryCode?.Length > 100)
                return BadRequest(new { success = false, field = "categoryCode", message = "categoryCode không được vượt quá 100 ký tự." });

            timeMode = string.IsNullOrWhiteSpace(timeMode) ? "GETSAMPLE" : timeMode.Trim().ToUpperInvariant();
            if (timeMode != "GETSAMPLE" && timeMode != "INSERT")
                return BadRequest(new { success = false, field = "timeMode", message = "timeMode chỉ nhận GETSAMPLE hoặc INSERT." });

            var filter = new XNServiceStatisticsFilter
            {
                FromDate = fromDate.Date, ToDate = toDate.Date, MaDotKham = maDotKham, CategoryCode = categoryCode,
                ServiceId = serviceId, OnlyCompleted = onlyCompleted, TimeMode = timeMode
            };

            try
            {
                var report = await _reportBL.LC_GetXNServiceStatisticsAsync(filter, HttpContext.RequestAborted);
                return Json(new
                {
                    success = true,
                    filters = new
                    {
                        from = report.FromDate.ToString("yyyy-MM-dd"), to = report.ToDate.ToString("yyyy-MM-dd"),
                        maDotKham = report.MaDotKham, categoryCode = report.CategoryCode, serviceId = report.ServiceId,
                        onlyCompleted = report.OnlyCompleted, timeMode = report.TimeMode
                    },
                    totalRows = report.TotalRows,
                    rows = report.Rows.Select(r => new
                    {
                        stt = r.STT, id = r.ID, sid = r.SID, namSinh = r.NamSinh,
                        gioiTinh = r.GioiTinh, loaiMau = r.LoaiMau, tenBenhNhan = r.TenBenhNhan,
                        nguoiNhap = r.NguoiNhap, nguoiDuyet = r.NguoiDuyet, inTime = r.InTime,
                        trangThai = r.TrangThai, tenXN = r.TenXN, nhomXN = r.NhomXN,
                        tenBacSiKTV = r.TenBacSiKTV, maHopDong = r.MaHopDong,
                        patientDbId = r.PatientDbId, serviceId = r.ServiceId,
                        ticketItemId = r.TicketItemId, missingTicketItemId = r.MissingTicketItemId
                    }).ToList()
                });
            }
            catch (OperationCanceledException) when (HttpContext.RequestAborted.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "REPORT-04C: GetXNServiceStatistics failed");
                return StatusCode(500, new { success = false, message = "Không thể tải báo cáo. Vui lòng kiểm tra log ứng dụng." });
            }
        }
    }
}
