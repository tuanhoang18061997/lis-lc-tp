using System;
using System.Globalization;
using System.IO;
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
        public async Task<IActionResult> ExportXNServiceStatisticsExcel(
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
                return BadRequest(new { success = false, message = "Model binding không hợp lệ.", fields });
            }
            if (!DateTime.TryParseExact(fromText?.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fromDate))
                return BadRequest(new { success = false, field = "from", message = "from phải có định dạng yyyy-MM-dd." });
            if (!DateTime.TryParseExact(toText?.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var toDate))
                return BadRequest(new { success = false, field = "to", message = "to phải có định dạng yyyy-MM-dd." });
            if (fromDate.Date > toDate.Date || toDate.Date == DateTime.MaxValue.Date)
                return BadRequest(new { success = false, field = "to", message = "Khoảng thời gian không hợp lệ." });

            maDotKham = string.IsNullOrWhiteSpace(maDotKham) ? null : maDotKham.Trim();
            if (maDotKham == null)
                return BadRequest(new { success = false, field = "maDotKham", message = "Phải chọn Mã Đợt Khám trước khi xuất Excel." });
            if (maDotKham.Length > 250)
                return BadRequest(new { success = false, field = "maDotKham", message = "maDotKham không được vượt quá 250 ký tự." });

            categoryCode = string.IsNullOrWhiteSpace(categoryCode) ? null : categoryCode.Trim();
            if (categoryCode?.Length > 100)
                return BadRequest(new { success = false, field = "categoryCode", message = "categoryCode không được vượt quá 100 ký tự." });

            long? serviceId = null;
            if (!string.IsNullOrWhiteSpace(serviceIdText))
            {
                if (!long.TryParse(serviceIdText.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var parsedServiceId) || parsedServiceId <= 0)
                    return BadRequest(new { success = false, field = "serviceId", message = "serviceId phải là số nguyên dương." });
                serviceId = parsedServiceId;
            }
            var onlyCompleted = true;
            if (!string.IsNullOrWhiteSpace(onlyCompletedText) && !bool.TryParse(onlyCompletedText.Trim(), out onlyCompleted))
                return BadRequest(new { success = false, field = "onlyCompleted", message = "onlyCompleted chỉ nhận true hoặc false." });
            timeMode = string.IsNullOrWhiteSpace(timeMode) ? "GETSAMPLE" : timeMode.Trim().ToUpperInvariant();
            if (timeMode != "GETSAMPLE" && timeMode != "INSERT")
                return BadRequest(new { success = false, field = "timeMode", message = "timeMode chỉ nhận GETSAMPLE hoặc INSERT." });

            var filter = new XNServiceStatisticsFilter
            {
                FromDate = fromDate.Date, ToDate = toDate.Date, MaDotKham = maDotKham,
                CategoryCode = categoryCode, ServiceId = serviceId, OnlyCompleted = onlyCompleted, TimeMode = timeMode
            };
            try
            {
                var templatePath = Path.Combine(_environment.ContentRootPath, "ReportTemplates", "XNServiceStatistics.xlsx");
                if (!System.IO.File.Exists(templatePath))
                {
                    _logger.LogError("REPORT-06 Excel template missing: {Path}", templatePath);
                    return StatusCode(500, new { success = false, message = "Không tìm thấy template Excel trên server." });
                }
                var report = await _reportBL.LC_GetXNServiceStatisticsAsync(filter, HttpContext.RequestAborted);
                if (report.Rows.Count == 0) return NotFound(new { success = false, message = "Không có dữ liệu phù hợp để xuất Excel." });
                var content = _reportBL.LC_ExportXNServiceStatisticsExcel(report, templatePath);
                var fileName = $"ThongKeDichVuXN_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}_{DateTime.Now:HHmmss}.xlsx";
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (OperationCanceledException) when (HttpContext.RequestAborted.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "REPORT-06: ExportXNServiceStatisticsExcel failed");
                return StatusCode(500, new { success = false, message = "Không thể xuất Excel. Vui lòng kiểm tra log ứng dụng." });
            }
        }
    }
}
