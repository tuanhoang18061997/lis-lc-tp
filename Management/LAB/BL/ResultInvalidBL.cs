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

        public ResultInvalidBL(
            LABContext db,
            PatientXNBL patientXNBL,
            PatientCDHABL patientCDHABL,
            ResultEditUnlockBL unlockBL)
        {
            _db = db;
            _patientXNBL = patientXNBL;
            _patientCDHABL = patientCDHABL;
            _unlockBL = unlockBL;
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
            var ids = resultIds?.Where(x => x > 0).Distinct().ToList() ?? new List<long>();
            if (patientId <= 0 || ids.Count == 0)
                return ResultInvalidOutcome.Fail("Vui lòng chọn ít nhất một dịch vụ.");

            // Module và KeyResultForHis đều được lấy lại từ DB, không tin dữ liệu client.
            var results = await _db.ResultCDHAs.AsNoTracking()
                .Where(x => x.Active
                    && x.PatientId == patientId
                    && ids.Contains(x.Id)
                    && x.Service.Category.Code == moduleCode)
                .Select(x => new { x.Id, x.KeyResultForHis })
                .ToListAsync();

            if (results.Count != ids.Count)
                return ResultInvalidOutcome.Fail("Danh sách dịch vụ không thuộc bệnh nhân/module hiện tại.");

            if (!await _unlockBL.CanInvalidCDHAAsync(patientId, ids, moduleCode, now))
                return ResultInvalidOutcome.Fail("Kết quả đã khóa. Admin phải mở đúng dịch vụ này trước.");

            if (!await _unlockBL.ActivateCDHAPermissionsForInvalidAsync(patientId, ids, moduleCode, userId, now))
                return ResultInvalidOutcome.Fail("Quyền mở khóa đã hết hạn hoặc đã bị thu hồi.");

            // Vẫn chuyển cả module Valid -> Process theo luồng hiện hữu.
            var changed = await _patientCDHABL.GetSample_ProcessResult_ReturnResult(
                patientId, false, true, false, userId, moduleCode);

            if (!changed)
                return ResultInvalidOutcome.Fail("Không thể chuyển module về trạng thái đang thực hiện.");

            var keys = results.Select(x => x.KeyResultForHis)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!)
                .Distinct()
                .ToList();

            if (keys.Count > 0)
                _patientCDHABL.Remove_Result_PDF(keys, moduleCode);

            return ResultInvalidOutcome.Ok();
        }
    }
}
