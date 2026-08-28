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

            var now = ToolBL.Get_DateNow();
            var unlockResult = await _unlockBL.UnlockAsync(request, userId.Value, now);
            if (!unlockResult.Success)
                return BadRequest(new { success = false, message = unlockResult.Message });

            // Nếu module đã ở Process thì chỉ cần mở lại quyền sửa.
            // Nếu còn Valid thì Admin bấm Unlock sẽ Invalid ngay, user không cần bấm Invalid lần nữa.
            if (!unlockResult.ShouldInvalid)
            {
                return Json(new
                {
                    success = true,
                    message = unlockResult.Message,
                    invalidated = false
                });
            }

            ResultInvalidOutcome invalidResult;

            if (string.Equals(request.TargetType, "XN", StringComparison.OrdinalIgnoreCase))
            {
                invalidResult = await _resultInvalidBL.InvalidXNAsync(
                    request.PatientId,
                    userId.Value);
            }
            else
            {
                if (!request.ResultCDHAId.HasValue || request.ResultCDHAId.Value <= 0)
                {
                    await RollbackUnlockAsync(request, userId.Value, now,
                        "Tự động thu hồi vì thiếu ResultCDHAId khi Invalid từ Tools Admin");

                    return BadRequest(new
                    {
                        success = false,
                        message = "Thiếu dịch vụ CDHA cần Invalid. Quyền mở khóa đã được thu hồi."
                    });
                }

                invalidResult = await _resultInvalidBL.InvalidCDHAAsync(
                    request.PatientId,
                    new[] { request.ResultCDHAId.Value },
                    request.ModuleCode,
                    userId.Value);
            }

            if (!invalidResult.Success)
            {
                // LABContext của project hiện đang đăng ký Transient nên các BL không chắc dùng cùng DbContext.
                // Không tạo transaction giả giữa nhiều DbContext; thay vào đó thu hồi permission vừa mở
                // nếu thao tác Invalid thất bại để không để sót quyền sửa đang active.
                await RollbackUnlockAsync(
                    request,
                    userId.Value,
                    now,
                    "Tự động thu hồi vì Invalid thất bại từ Tools Admin");

                return BadRequest(new
                {
                    success = false,
                    message = $"Không thể Invalid kết quả: {invalidResult.Message}. Quyền mở khóa vừa tạo đã được thu hồi."
                });
            }

            return Json(new
            {
                success = true,
                message = "Đã mở khóa và chuyển kết quả về trạng thái Process. Người dùng có thể chỉnh sửa ngay trong thời gian được cấp quyền.",
                invalidated = true
            });
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


        private async Task RollbackUnlockAsync(
            ToolAdminUnlockRequest request,
            long adminUserId,
            DateTime now,
            string reason)
        {
            await _unlockBL.LockAsync(
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
        }

        private long? GetUserLogin()
        {
            var value = User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
            return long.TryParse(value, out var userId) ? userId : null;
        }
    }
}
