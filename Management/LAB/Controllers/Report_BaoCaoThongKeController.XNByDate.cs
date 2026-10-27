using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
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
        public async Task<IActionResult> GetXNByDateReport([FromQuery] string from, [FromQuery] string to, CancellationToken cancellationToken)
        {
            if (!DateTime.TryParseExact(from, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fromDate))
                return BadRequest(new { success = false, field = "from", message = "Từ ngày phải có định dạng yyyy-MM-dd." });
            if (!DateTime.TryParseExact(to, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var toDate))
                return BadRequest(new { success = false, field = "to", message = "Đến ngày phải có định dạng yyyy-MM-dd." });
            if (toDate < fromDate || toDate.Date >= DateTime.MaxValue.Date)
                return BadRequest(new { success = false, field = "to", message = "Khoảng ngày không hợp lệ." });

            var stopwatch = Stopwatch.StartNew();
            try
            {
                var model = await _reportBL.LC_GetPatientServicesByDateXN(fromDate, toDate, cancellationToken);
                var response = new XNByDateApiResponse
                {
                    From = model.FromDate,
                    To = model.ToDate,
                    TotalRows = model.DateGroups.Sum(g => g.Rows.Count),
                    DateGroups = model.DateGroups
                };
                _logger.LogInformation("XNByDate loaded {RowCount} rows, {GroupCount} days in {Ms} ms", response.TotalRows, response.DateGroups.Count, stopwatch.ElapsedMilliseconds);
                return Json(response);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return new EmptyResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "XNByDate failed after {Ms} ms", stopwatch.ElapsedMilliseconds);
                return StatusCode(500, new { success = false, message = "Không thể tải báo cáo xét nghiệm theo ngày. Vui lòng kiểm tra log máy chủ." });
            }
        }

        // Giữ nguyên URL và tham số export Excel đang dùng trên frontend cũ.
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ExportPatientServicesByDateXN(DateTime fromtime, DateTime totime)
        {
            if (fromtime == default || totime < fromtime || totime.Date >= DateTime.MaxValue.Date)
                return BadRequest(new { success = false, message = "Khoảng ngày không hợp lệ." });
            var watch = Stopwatch.StartNew();
            try
            {
                var model = await _reportBL.LC_GetPatientServicesByDateXN(fromtime, totime, HttpContext.RequestAborted);
                var bytes = _reportBL.ExportPatientServicesByDateToExcel(model);
                _logger.LogInformation("XNByDate Excel exported in {Ms} ms", watch.ElapsedMilliseconds);
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BaoCaoXN_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            }
            catch (OperationCanceledException) when (HttpContext.RequestAborted.IsCancellationRequested)
            {
                return new EmptyResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "XNByDate export Excel failed after {Ms} ms", watch.ElapsedMilliseconds);
                return StatusCode(500, new { success = false, message = "Không thể xuất Excel báo cáo xét nghiệm theo ngày." });
            }
        }
    }
}
