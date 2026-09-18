using Management.Models;
using Microsoft.EntityFrameworkCore;

namespace Management.BL
{
    public class ResultInvalidBL
    {
        private readonly LABContext _db;
        private readonly PatientXNBL _patientXNBL;
        private readonly PatientCDHABL _patientCDHABL;
        private readonly ResultEditUnlockBL _unlockBL;
        private readonly ResultCDHABL _resultCDHABL;

        public ResultInvalidBL(
            LABContext db,
            PatientXNBL patientXNBL,
            PatientCDHABL patientCDHABL,
            ResultEditUnlockBL unlockBL,
            ResultCDHABL resultCDHABL)
        {
            _db = db;
            _patientXNBL = patientXNBL;
            _patientCDHABL = patientCDHABL;
            _unlockBL = unlockBL;
            _resultCDHABL = resultCDHABL;
        }

        public async Task<ResultInvalidOutcome> InvalidXNAsync(long patientId, long userId)
        {
            var now = ToolBL.Get_DateNow();
            if (!await _unlockBL.CanInvalidXNAsync(patientId, now))
                return ResultInvalidOutcome.Fail("Kết quả đã khóa. Admin phải mở khóa lần làm xét nghiệm này trước.");

            if (!await _unlockBL.ActivateXNPermissionForInvalidAsync(patientId, userId, now))
                return ResultInvalidOutcome.Fail("Quyền mở khóa đã hết hạn hoặc đã bị thu hồi.");

            var changed = await _patientXNBL.GetSample_ProcessResult_ReturnResult(
                patientId, false, true, false, userId);

            return changed
                ? ResultInvalidOutcome.Ok()
                : ResultInvalidOutcome.Fail("Không thể chuyển xét nghiệm về trạng thái đang thực hiện.");
        }

        public async Task<ResultInvalidOutcome> InvalidCDHAAsync(
            long patientId,
            IReadOnlyCollection<long> resultIds,
            string moduleCode,
            long userId)
        {
            var now = ToolBL.Get_DateNow();

            moduleCode = (moduleCode ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

            var ids = resultIds?
                .Where(x => x > 0)
                .Distinct()
                .ToList()
                ?? new List<long>();

            if (patientId <= 0 ||
                userId <= 0 ||
                ids.Count == 0 ||
                string.IsNullOrWhiteSpace(moduleCode))
            {
                return ResultInvalidOutcome.Fail(
                    "Vui lòng chọn ít nhất một dịch vụ.");
            }

            // =========================================================
            // STEP 1:
            // Xác minh toàn bộ ResultCDHA thực sự thuộc
            // patient + module đang thao tác.
            //
            // Không tin ResultId/module từ client.
            // =========================================================
            var results = await _db.ResultCDHAs
                .AsNoTracking()
                .Where(x =>
                    x.Active &&
                    x.PatientId == patientId &&
                    ids.Contains(x.Id) &&
                    x.Service.Category.Code == moduleCode)
                .Select(x => new
                {
                    x.Id,
                    x.KeyResultForHis,

                    // Lấy luôn state để phục vụ debug / kiểm soát
                    x.IsValidated,
                    x.LastValidatedAt
                })
                .ToListAsync();

            if (results.Count != ids.Count)
            {
                return ResultInvalidOutcome.Fail(
                    "Danh sách dịch vụ không thuộc bệnh nhân/module hiện tại.");
            }

            // =========================================================
            // STEP 2:
            // Kiểm tra quyền Invalid.
            //
            // Record mới:
            // IsValidated phải = true.
            //
            // Legacy:
            // fallback logic Patient cũ.
            //
            // Qua ngày:
            // phải có ADMIN permission đúng từng ResultCDHAId.
            // =========================================================
            if (!await _unlockBL.CanInvalidCDHAAsync(
                patientId,
                ids,
                moduleCode,
                now))
            {
                return ResultInvalidOutcome.Fail(
                    "Kết quả đã khóa. Admin phải mở đúng dịch vụ này trước.");
            }

            // =========================================================
            // STEP 3:
            // Activate permission sửa cho CHÍNH các service Invalid.
            //
            // Cùng ngày:
            // Source = SAME_DAY
            //
            // Qua ngày:
            // sử dụng ADMIN permission hiện hữu.
            // =========================================================
            if (!await _unlockBL.ActivateCDHAPermissionsForInvalidAsync(
                patientId,
                ids,
                moduleCode,
                userId,
                now))
            {
                return ResultInvalidOutcome.Fail(
                    "Quyền mở khóa đã hết hạn hoặc đã bị thu hồi.");
            }

            // =========================================================
            // STEP 5:
            // Hạ state CHÍNH XÁC các ResultCDHA vừa Invalid.
            // =========================================================
            var trackedResults = await _db.ResultCDHAs
                .Where(x =>
                    x.Active &&
                    x.PatientId == patientId &&
                    ids.Contains(x.Id) &&
                    x.Service.Category.Code == moduleCode)
                .ToListAsync();

            if (trackedResults.Count != ids.Count)
            {
                // Không được coi Invalid là thành công khi DB state
                // của service chưa xác nhận đầy đủ.
                return ResultInvalidOutcome.Fail(
                    "Không thể cập nhật trạng thái Invalid của dịch vụ.");
            }

            var patient = await _db.Patients
            .Where(x =>
                x.Active &&
                x.Id == patientId)
            .FirstOrDefaultAsync();

            if (patient == null)
            {
                return ResultInvalidOutcome.Fail(
                    "Không tìm thấy thông tin bệnh nhân.");
            }

            var legacyValidationTime =
                ResultEditUnlockBL.GetReturnResultTime(
                    patient,
                    moduleCode);

            foreach (var result in trackedResults)
            {
                // =====================================================
                // Record đã migrate.
                // =====================================================
                if (result.IsValidated.HasValue)
                {
                    result.IsValidated = false;
                    continue;
                }

                // =====================================================
                // LEGACY.
                //
                // Khi legacy lần đầu được Invalid, migrate luôn nó vào
                // cơ chế mới.
                //
                // Không được tạo false + NULL vì CanEdit sẽ hiểu nhầm là
                // "chưa từng Valid".
                // =====================================================
                if (!legacyValidationTime.HasValue)
                {
                    return ResultInvalidOutcome.Fail(
                        "Không xác định được thời điểm Valid trước đó của dịch vụ.");
                }

                result.IsValidated = false;

                result.LastValidatedAt =
                    legacyValidationTime.Value;

                // Không biết chính xác user đã Valid dữ liệu lịch sử,
                // nên KHÔNG giả mạo LastValidatedByUserId.
            }

            // Persist ResultCDHA vừa Invalid trước khi tính lại Patient aggregate.
            // Như vậy Recalculate luôn đọc được IsValidated = false.
            try
            {
                await _db.SaveChangesAsync();
            }
            catch
            {
                return ResultInvalidOutcome.Fail(
                    "Không thể lưu trạng thái Invalid của dịch vụ.");
            }

            try
            {
                var aggregateUpdated = await _resultCDHABL.RecalculatePatientModuleStateAsync(
                    patientId,
                    moduleCode,
                    userId);

                if (!aggregateUpdated)
                {
                    return ResultInvalidOutcome.Fail(
                        "Không thể cập nhật trạng thái tổng hợp của bệnh nhân.");
                }
            }
            catch
            {
                return ResultInvalidOutcome.Fail(
                    "Không thể cập nhật trạng thái tổng hợp của bệnh nhân.");
            }

            // =========================================================
            // STEP 6:
            // Xóa PDF cũ của chính các Result vừa Invalid.
            // =========================================================
            var keys = results
                .Select(x => x.KeyResultForHis)
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x))
                .Select(x => x!)
                .Distinct()
                .ToList();

            if (keys.Count > 0)
            {
                _patientCDHABL.Remove_Result_PDF(
                    keys,
                    moduleCode);
            }

            return ResultInvalidOutcome.Ok();
        }
    }
}
