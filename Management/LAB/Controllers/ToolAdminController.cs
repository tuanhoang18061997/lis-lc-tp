using Management.BL;
using Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Management.Controllers
{
    [Authorize]
    public class ToolAdminController : Controller
    {
        private readonly ResultEditUnlockBL _unlockBL;
        private readonly ResultInvalidBL _resultInvalidBL;

        public ToolAdminController(
            ResultEditUnlockBL unlockBL,
            ResultInvalidBL resultInvalidBL)
        {
            _unlockBL = unlockBL;
            _resultInvalidBL = resultInvalidBL;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetUserLogin();
            if (!userId.HasValue || !await _unlockBL.UserHasToolAdminAsync(userId.Value))
                return Forbid();

            var now = ToolBL.Get_DateNow();
            ViewData["FromDate"] = now.Date.AddDays(-7).ToString("yyyy-MM-dd");
            ViewData["ToDate"] = now.Date.ToString("yyyy-MM-dd");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Search(
            string? keyword,
            string? moduleCode,
            DateTime fromDate,
            DateTime toDate)
        {
            var userId = GetUserLogin();
            if (!userId.HasValue || !await _unlockBL.UserHasToolAdminAsync(userId.Value))
                return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = "Bạn không có quyền Tool Admin." });

            if (toDate.Date < fromDate.Date || (toDate.Date - fromDate.Date).TotalDays > 366)
                return BadRequest(new { success = false, message = "Khoảng ngày không hợp lệ hoặc lớn hơn 366 ngày." });

            var rows = await _unlockBL.SearchAsync(
                keyword,
                moduleCode,
                fromDate,
                toDate,
                ToolBL.Get_DateNow());

            return Json(new { success = true, data = rows });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock([FromBody] ToolAdminUnlockRequest request)
        {
            var userId = GetUserLogin();
            if (!userId.HasValue || !await _unlockBL.UserHasToolAdminAsync(userId.Value))
                return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = "Bạn không có quyền Tool Admin." });

            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Dữ liệu mở khóa không hợp lệ." });

            var result = await ExecuteUnlockAsync(request, userId.Value);
            return result.Success
                ? Json(new { success = true, message = result.Message, invalidated = result.Invalidated })
                : BadRequest(new { success = false, message = result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BatchUnlock([FromBody] ToolAdminBatchUnlockRequest request)
        {
            var userId = GetUserLogin();
            if (!userId.HasValue || !await _unlockBL.UserHasToolAdminAsync(userId.Value))
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { success = false, message = "Bạn không có quyền Tool Admin." });

            if (request == null || !ModelState.IsValid ||
                request.Items == null || request.Items.Count == 0 || request.Items.Count > 50 ||
                string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Trim().Length < 5 ||
                request.DurationMinutes < 5 || request.DurationMinutes > 1440)
            {
                return BadRequest(new { success = false,
                    message = "Danh sách mở khóa không hợp lệ (tối đa 50 dòng/lượt)." });
            }

            var results = new List<object>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int successCount = 0, failCount = 0, skippedCount = 0;

            for (var index = 0; index < request.Items.Count; index++)
            {
                var target = request.Items[index];
                if (target == null)
                {
                    failCount++;
                    results.Add(new { index, success = false, skipped = false,
                        message = "Dòng dữ liệu không hợp lệ." });
                    continue;
                }

                var itemRequest = new ToolAdminUnlockRequest
                {
                    TargetType = target.TargetType,
                    PatientId = target.PatientId,
                    ResultCDHAId = target.ResultCDHAId,
                    ModuleCode = target.ModuleCode,
                    Reason = request.Reason.Trim(),
                    DurationMinutes = request.DurationMinutes
                };

                var normalizedType = (target.TargetType ?? "").Trim().ToUpperInvariant();
                var key = normalizedType == "XN"
                    ? $"XN|{target.PatientId}"
                    : $"{normalizedType}|{target.PatientId}|{target.ResultCDHAId}|" +
                      $"{(target.ModuleCode ?? "").Trim().ToUpperInvariant()}";
                if (!seen.Add(key))
                {
                    skippedCount++;
                    results.Add(new { index, success = false, skipped = true,
                        message = "Dòng trùng trong danh sách chọn." });
                    continue;
                }

                try
                {
                    // Rà soát quyền tại DB; dữ liệu client có thể đã cũ sau khi Search.
                    if (await _unlockBL.HasActiveUnlockForBatchAsync(itemRequest, ToolBL.Get_DateNow()))
                    {
                        skippedCount++;
                        results.Add(new { index, success = false, skipped = true,
                            message = "Dòng này đang được mở khóa; không cấp chồng quyền." });
                        continue;
                    }

                    // Cùng một nghiệp vụ với nút Mở khóa từng dòng (kể cả Auto Invalid).
                    var result = await ExecuteUnlockAsync(itemRequest, userId.Value);
                    if (result.Success) successCount++;
                    else failCount++;

                    results.Add(new { index, success = result.Success, skipped = false,
                        invalidated = result.Invalidated, message = result.Message });
                }
                catch
                {
                    // Thất bại của một dòng không được chặn các dòng khác.
                    failCount++;
                    results.Add(new { index, success = false, skipped = false,
                        message = "Có lỗi khi xử lý dòng này. Vui lòng tải lại để kiểm tra trạng thái." });
                }
            }

            // HTTP 200 thể hiện batch đã xử lý; phải đọc successCount/failCount,
            // không được hiểu đây là 100% dòng đều mở thành công.
            return Json(new
            {
                success = true,
                total = request.Items.Count,
                successCount,
                failCount,
                skippedCount,
                results
            });
        }

        private async Task<(bool Success, string Message, bool Invalidated)> ExecuteUnlockAsync(
            ToolAdminUnlockRequest request, long userId)
        {
            var now = ToolBL.Get_DateNow();
            var unlockResult = await _unlockBL.UnlockAsync(request, userId, now);
            if (!unlockResult.Success)
                return (false, unlockResult.Message, false);

            if (!unlockResult.ShouldInvalid)
                return (true, unlockResult.Message, false);

            ResultInvalidOutcome invalidResult;
            try
            {
                if (string.Equals(request.TargetType, "XN", StringComparison.OrdinalIgnoreCase))
                {
                    invalidResult = await _resultInvalidBL.InvalidXNAsync(request.PatientId, userId);
                }
                else if (request.ResultCDHAId.HasValue && request.ResultCDHAId.Value > 0)
                {
                    invalidResult = await _resultInvalidBL.InvalidCDHAAsync(
                        request.PatientId,
                        new[] { request.ResultCDHAId.Value },
                        request.ModuleCode,
                        userId);
                }
                else
                {
                    invalidResult = ResultInvalidOutcome.Fail("Thiếu ResultCDHAId khi Invalid.");
                }
            }
            catch
            {
                invalidResult = ResultInvalidOutcome.Fail("Lỗi khi tự động Invalid kết quả.");
            }

            if (!invalidResult.Success)
            {
                // Các BL dùng DbContext khác nhau, không tạo transaction giả.
                // Cố gắng thu hồi permission vừa tạo nếu Invalid thất bại.
                bool rollbackOk;
                try
                {
                    rollbackOk = await RollbackUnlockAsync(
                        request, userId, ToolBL.Get_DateNow(),
                        "Tự động thu hồi vì Invalid thất bại từ Tools Admin");
                }
                catch
                {
                    rollbackOk = false;
                }
                var suffix = rollbackOk
                    ? "Quyền mở khóa vừa tạo đã được thu hồi."
                    : "CẢNH BÁO: Chưa xác nhận được việc thu hồi quyền; cần kiểm tra lại ToolAdmin.";
                return (false, $"Không thể Invalid kết quả: {invalidResult.Message}. {suffix}", false);
            }

            return (true,
                "Đã mở khóa và chuyển kết quả về trạng thái Process. Người dùng có thể chỉnh sửa trong thời gian được cấp quyền.",
                true);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Lock([FromBody] ToolAdminLockRequest request)
        {
            var userId = GetUserLogin();
            if (!userId.HasValue || !await _unlockBL.UserHasToolAdminAsync(userId.Value))
                return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = "Bạn không có quyền Tool Admin." });

            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Dữ liệu khóa không hợp lệ." });

            var result = await _unlockBL.LockAsync(request, userId.Value, ToolBL.Get_DateNow());
            return result.Success
                ? Json(new { success = true, message = result.Message })
                : BadRequest(new { success = false, message = result.Message });
        }


        private async Task<bool> RollbackUnlockAsync(
            ToolAdminUnlockRequest request,
            long adminUserId,
            DateTime now,
            string reason)
        {
            var result = await _unlockBL.LockAsync(
                new ToolAdminLockRequest
                {
                    TargetType = request.TargetType,
                    PatientId = request.PatientId,
                    ResultCDHAId = request.ResultCDHAId,
                    ModuleCode = request.ModuleCode,
                    Reason = reason
                },
                adminUserId,
                now);
            return result.Success;
        }

        private long? GetUserLogin()
        {
            var value = User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
            return long.TryParse(value, out var userId) ? userId : null;
        }
    }
}
