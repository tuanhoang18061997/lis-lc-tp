using Management.Models;
using Microsoft.EntityFrameworkCore;

namespace Management.BL
{
    public class ResultEditUnlockBL
    {
        public const string ToolAdminFunctionCode = "ToolAdmin";
        public const string XnScope = "XN_RUN";
        public const string CdhaScope = "CDHA_SERVICE";

        private static readonly HashSet<string> CdhaModules = new(StringComparer.OrdinalIgnoreCase)
        {
            "SA", "SAT", "DDT", "NS", "NSCTC", "XQ", "TDCN"
        };

        private readonly LABContext _db;

        public ResultEditUnlockBL(LABContext db)
        {
            _db = db;
        }

        public async Task<bool> UserHasToolAdminAsync(long userId)
        {
            return await _db.UserFunctions.AsNoTracking().AnyAsync(x =>
                x.UserId == userId && x.Function.Code == ToolAdminFunctionCode);
        }

        public static bool IsLockedByDate(DateTime? returnResultTime, DateTime now)
        {
            return returnResultTime.HasValue && now.Date > returnResultTime.Value.Date;
        }

        /// <summary>
        /// XN đã từng Valid nhưng tại thời điểm Valid vẫn chưa đủ kết quả.
        /// Trạng thái này được giữ qua lần Invalid/Process tiếp theo cho đến khi
        /// Valid lại với đầy đủ kết quả (NotFullResultXN = false).
        /// </summary>
        private static bool IsXNIncompleteResult(Patient patient)
        {
            return patient.NotFullResultXN;
        }

        /// <summary>
        /// Chỉ áp dụng khóa qua ngày cho XN không còn ở trạng thái Valid chưa đủ.
        /// Giữ nguyên behavior legacy cho các ca NotFullResultXN = false.
        /// </summary>
        private static bool IsXNLockedByDate(Patient patient, DateTime now)
        {
            return !IsXNIncompleteResult(patient)
                && IsLockedByDate(patient.ReturnResultTimeXN, now);
        }

        public async Task<bool> CanInvalidXNAsync(long patientId, DateTime now)
        {
            var patient = await _db.Patients.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Active && x.Id == patientId);

            if (patient == null || !patient.ValidXN)
                return false;

            // Valid chưa đủ kết quả: luôn cho phép Invalid để quay lại Process,
            // kể cả đã qua ngày hoặc lần làm này từng có lịch sử Admin Unlock.
            if (IsXNIncompleteResult(patient))
                return true;

            var lockedByDate = IsXNLockedByDate(patient, now);
            var hasAdminUnlockHistory = await HasXNAdminUnlockHistoryAsync(patientId);

            // Chỉ cho Invalid tự do khi kết quả vẫn trong ngày VÀ lần làm XN này
            // chưa từng phải nhờ Admin mở khóa.
            //
            // Sau khi Admin đã từng mở khóa, user sửa rồi Valid lại thì
            // ReturnResultTimeXN có thể được cập nhật thành ngày giờ hiện tại.
            // Lịch sử ADMIN trong ResultEditUnlock được dùng để giữ trạng thái khóa,
            // tránh việc ReturnResultTimeXN mới làm user tự Invalid lại trong ngày.
            if (!lockedByDate && !hasAdminUnlockHistory)
                return true;

            return await HasActivePermissionAsync(
                patientId,
                "XN",
                null,
                now,
                requireUsed: false);
        }

        public async Task<bool> CanInvalidCDHAAsync(
            long patientId,
            IReadOnlyCollection<long> resultIds,
            string moduleCode,
            DateTime now)
        {
            moduleCode = NormalizeModule(moduleCode);

            if (!CdhaModules.Contains(moduleCode) ||
                resultIds == null ||
                resultIds.Count == 0)
            {
                return false;
            }

            var ids = resultIds
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            if (ids.Count == 0)
                return false;

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
                    x.IsValidated,
                    x.LastValidatedAt,
                    Patient = x.Patient
                })
                .ToListAsync();

            if (results.Count != ids.Count)
                return false;

            var lockedResultIds = new List<long>();

            foreach (var result in results)
            {
                if (result.Patient == null)
                    return false;

                // =====================================================
                // RECORD MỚI
                // =====================================================
                if (result.IsValidated.HasValue)
                {
                    // Chỉ service đang Valid mới được Invalid.
                    //
                    // false + NULL:
                    // chưa từng Valid.
                    //
                    // false + LastValidatedAt:
                    // đã Invalid rồi / đang Process.
                    if (!result.IsValidated.Value)
                        return false;
                }
                else
                {
                    // =================================================
                    // LEGACY
                    // =================================================
                    if (!GetModuleValid(
                        result.Patient,
                        moduleCode))
                    {
                        return false;
                    }
                }

                var validationTime =
                    GetCDHAValidationTime(
                        result.IsValidated,
                        result.LastValidatedAt,
                        result.Patient,
                        moduleCode);

                // Fail-safe:
                // service được đánh dấu Valid nhưng không có timestamp.
                if (result.IsValidated == true &&
                    !validationTime.HasValue)
                {
                    return false;
                }

                if (IsLockedByDate(
                    validationTime,
                    now))
                {
                    lockedResultIds.Add(
                        result.Id);
                }
            }

            // Không có service nào qua ngày.
            // Cho phép Invalid cùng ngày.
            if (lockedResultIds.Count == 0)
                return true;

            // =========================================================
            // Qua ngày:
            // từng ResultCDHA phải có ADMIN permission riêng.
            // =========================================================
            var activeAdminResultIds =
                await _db.ResultEditUnlocks
                    .AsNoTracking()
                    .Where(x =>
                        x.IsActive &&
                        x.Scope == CdhaScope &&
                        x.Source == "ADMIN" &&
                        x.PatientId == patientId &&
                        x.ModuleCode == moduleCode &&
                        x.ResultCDHAId.HasValue &&
                        lockedResultIds.Contains(
                            x.ResultCDHAId.Value) &&
                        x.RevokedAt == null &&
                        x.ExpireAt > now)
                    .Select(x =>
                        x.ResultCDHAId!.Value)
                    .Distinct()
                    .ToListAsync();

            return activeAdminResultIds.Count ==
                   lockedResultIds.Count;
        }

        public async Task<bool> CanEditXNAsync(long patientId, DateTime now)
        {
            var patient = await _db.Patients.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Active && x.Id == patientId);

            if (patient == null)
                return false;

            // Sau partial Valid, NotFullResultXN vẫn giữ true khi Invalid về Process.
            // Vì vậy user tiếp tục được sửa/nhập kết quả ở các ngày sau cho đến khi
            // Valid hoàn tất và NotFullResultXN được đưa về false.
            if (IsXNIncompleteResult(patient))
                return true;

            var lockedByDate = IsXNLockedByDate(patient, now);
            var hasAdminUnlockHistory = await HasXNAdminUnlockHistoryAsync(patientId);

            // Lần làm mới / kết quả trong ngày chưa từng qua Admin Unlock
            // vẫn được sửa theo workflow bình thường.
            if (!lockedByDate && !hasAdminUnlockHistory)
                return true;

            // Đã từng bị khóa và được Admin mở thì chỉ cho sửa khi quyền mới
            // đang Active và đã thực sự được dùng qua thao tác Invalid.
            return await HasActivePermissionAsync(
                patientId,
                "XN",
                null,
                now,
                requireUsed: true);
        }

        //public async Task<bool> CanEditCDHAAsync(long resultCDHAId, DateTime now)
        //{
        //    var result = await _db.ResultCDHAs.AsNoTracking()
        //        .Where(x => x.Active && x.Id == resultCDHAId)
        //        .Select(x => new
        //        {
        //            Result = x,
        //            ModuleCode = x.Service.Category.Code,
        //            Patient = x.Patient
        //        })
        //        .FirstOrDefaultAsync();

        //    if (result == null || result.Patient == null)
        //        return false;

        //    var moduleCode = NormalizeModule(result.ModuleCode);
        //    var returnTime = GetReturnResultTime(result.Patient, moduleCode);

        //    // Kết quả chưa từng Valid: đây là luồng thực hiện ban đầu, không cần quyền mở khóa.
        //    if (!returnTime.HasValue)
        //        return true;

        //    // Dịch vụ mới được chỉ định sau lần trả kết quả trước là luồng thực hiện mới,
        //    // không phải sửa lại dịch vụ cũ.
        //    if (result.Result.InsertTime.HasValue && result.Result.InsertTime > returnTime)
        //        return true;

        //    // Khi đã từng trả kết quả, CDHA luôn cần đúng phạm vi dịch vụ đã Invalid.
        //    return await HasActivePermissionAsync(
        //        result.Patient.Id,
        //        moduleCode,
        //        resultCDHAId,
        //        now,
        //        requireUsed: true);
        //}
        public async Task<bool> CanEditCDHAAsync(long resultCDHAId, DateTime now)
        {
            var result = await _db.ResultCDHAs
                .AsNoTracking()
                .Where(x =>
                    x.Active &&
                    x.Id == resultCDHAId)
                .Select(x => new
                {
                    x.Id,
                    x.PatientId,
                    x.InsertTime,

                    x.IsValidated,
                    x.LastValidatedAt,

                    ModuleCode = x.Service.Category.Code,
                    Patient = x.Patient
                })
                .FirstOrDefaultAsync();

            if (result == null ||
                result.Patient == null ||
                !result.PatientId.HasValue)
            {
                return false;
            }

            var moduleCode = NormalizeModule(result.ModuleCode);

            if (!CdhaModules.Contains(moduleCode))
                return false;

            // =========================================================
            // RECORD THEO CƠ CHẾ MỚI
            // =========================================================
            if (result.IsValidated.HasValue)
            {
                // =========================================================
                // CASE 1:
                // Service hiện vẫn đang Valid.
                //
                // Dù có permission hay không cũng KHÔNG được sửa.
                // Phải hoàn thành Invalid trước để IsValidated chuyển false.
                // =========================================================
                if (result.IsValidated.Value)
                {
                    return false;
                }

                // =========================================================
                // CASE 2:
                // false + LastValidatedAt = NULL
                //
                // Dịch vụ mới, chưa từng Valid.
                // Cho phép nhập/sửa bình thường.
                // =========================================================
                if (!result.LastValidatedAt.HasValue)
                {
                    return true;
                }

                // =========================================================
                // CASE 3:
                // false + LastValidatedAt != NULL
                //
                // Dịch vụ đã từng Valid và hiện đã Invalid.
                // Chỉ được sửa nếu permission của CHÍNH service này
                // đang Active và đã được sử dụng qua thao tác Invalid.
                // =========================================================
                return await HasActivePermissionAsync(
                    result.PatientId.Value,
                    moduleCode,
                    resultCDHAId,
                    now,
                    requireUsed: true);
            }

            // =========================================================
            // LEGACY RECORD
            // =========================================================
            // Không có IsValidated => giữ logic cũ để không phá
            // các kết quả được tạo trước thời điểm deploy cơ chế mới.
            var returnTime = GetReturnResultTime(
                result.Patient,
                moduleCode);

            // Chưa từng trả kết quả theo dữ liệu legacy.
            if (!returnTime.HasValue)
                return true;

            // Dịch vụ được chỉ định sau lần trả kết quả cũ.
            if (result.InsertTime.HasValue &&
                result.InsertTime.Value > returnTime.Value)
            {
                return true;
            }

            // Dịch vụ legacy đã thuộc lần trả kết quả trước.
            return await HasActivePermissionAsync(
                result.PatientId.Value,
                moduleCode,
                resultCDHAId,
                now,
                requireUsed: true);
        }

        public async Task<bool> ActivateXNPermissionForInvalidAsync(long patientId, long userId, DateTime now)
        {
            var patient = await _db.Patients.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Active && x.Id == patientId);

            if (patient == null)
                return false;

            // Valid chưa đủ kết quả không cần consume Admin permission.
            // User được phép Invalid để tiếp tục Process theo workflow bình thường.
            if (IsXNIncompleteResult(patient))
                return true;

            var lockedByDate = IsXNLockedByDate(patient, now);
            var hasAdminUnlockHistory = await HasXNAdminUnlockHistoryAsync(patientId);

            // XN trong ngày và chưa từng qua Admin Unlock không cần permission.
            if (!lockedByDate && !hasAdminUnlockHistory)
                return true;

            // Nếu đã từng qua Admin Unlock thì kể cả ReturnResultTimeXN vừa được
            // cập nhật về hôm nay, lần Invalid tiếp theo vẫn phải dùng permission ADMIN.
            var permission = await FindActivePermissionAsync(patientId, "XN", null, now);
            if (permission == null || permission.Source != "ADMIN")
                return false;

            permission.UsedAt ??= now;
            permission.UsedByUserId ??= userId;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivateCDHAPermissionsForInvalidAsync(
            long patientId,
            IReadOnlyCollection<long> resultIds,
            string moduleCode,
            long userId,
            DateTime now)
        {
            moduleCode = NormalizeModule(moduleCode);

            if (!CdhaModules.Contains(moduleCode))
                return false;

            var ids = resultIds
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            if (ids.Count == 0)
                return false;

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
                    x.ServiceId,
                    x.IsValidated,
                    x.LastValidatedAt,
                    Patient = x.Patient
                })
                .ToListAsync();

            if (results.Count != ids.Count)
                return false;

            // =========================================================
            // PHASE 1:
            // Kiểm tra TOÀN BỘ trước khi thay đổi permission.
            //
            // Tránh trường hợp xử lý dịch vụ đầu tiên xong rồi mới phát
            // hiện dịch vụ thứ hai không có quyền ADMIN.
            // =========================================================

            var prepared = new List<(
                long ResultId,
                long? ServiceId,
                bool LockedByDate,
                ResultEditUnlock? Permission)>();

            foreach (var result in results)
            {
                if (result.Patient == null)
                    return false;

                // Service-level record
                if (result.IsValidated.HasValue)
                {
                    // Chỉ service đang Valid mới được bắt đầu Invalid.
                    if (!result.IsValidated.Value)
                        return false;
                }
                else
                {
                    // Legacy fallback.
                    if (!GetModuleValid(result.Patient, moduleCode))
                        return false;
                }

                var validationTime = GetCDHAValidationTime(
                    result.IsValidated,
                    result.LastValidatedAt,
                    result.Patient,
                    moduleCode);

                if (result.IsValidated == true &&
                    !validationTime.HasValue)
                {
                    return false;
                }

                var lockedByDate =
                    IsLockedByDate(validationTime, now);

                var permission =
                    await FindActivePermissionAsync(
                        patientId,
                        moduleCode,
                        result.Id,
                        now);

                // Qua ngày bắt buộc permission ADMIN.
                if (lockedByDate)
                {
                    if (permission == null ||
                        !string.Equals(
                            permission.Source,
                            "ADMIN",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }

                prepared.Add((
                    result.Id,
                    result.ServiceId,
                    lockedByDate,
                    permission));
            }

            // =========================================================
            // PHASE 2:
            // Tất cả đã hợp lệ, lúc này mới activate permission.
            // =========================================================

            foreach (var item in prepared)
            {
                if (item.LockedByDate)
                {
                    // Permission ADMIN đã được kiểm tra ở Phase 1.
                    item.Permission!.UsedAt ??= now;
                    item.Permission.UsedByUserId ??= userId;

                    continue;
                }

                // -----------------------------------------------------
                // Invalid trong cùng ngày.
                // -----------------------------------------------------
                if (item.Permission == null)
                {
                    // Dọn permission cũ/hết hạn để tránh unique index.
                    await RevokeTargetPermissionsAsync(
                        patientId,
                        moduleCode,
                        item.ResultId,
                        userId,
                        now,
                        "Quyền cũ đã hết hạn");

                    var permission = new ResultEditUnlock
                    {
                        PatientId = patientId,
                        ModuleCode = moduleCode,
                        Scope = CdhaScope,

                        ResultCDHAId = item.ResultId,
                        ServiceId = item.ServiceId,

                        Source = "SAME_DAY",

                        UnlockReason =
                            "Phạm vi sửa được tạo khi Invalid kết quả trong ngày",

                        UnlockedAt = now,

                        // SAME_DAY hết hiệu lực lúc 00:00 ngày kế tiếp.
                        ExpireAt = now.Date.AddDays(1),

                        UnlockedByUserId = userId,

                        // Invalid đang diễn ra nên permission được đánh dấu
                        // là đã dùng ngay.
                        UsedAt = now,
                        UsedByUserId = userId,

                        IsActive = true
                    };

                    await _db.ResultEditUnlocks.AddAsync(permission);
                }
                else
                {
                    item.Permission.UsedAt ??= now;
                    item.Permission.UsedByUserId ??= userId;
                }
            }

            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<(bool Success, string Message, bool ShouldInvalid)> UnlockAsync(
            ToolAdminUnlockRequest request,
            long adminUserId,
            DateTime now)
        {
            var targetType = NormalizeTargetType(request.TargetType);
            var moduleCode = targetType == "XN" ? "XN" : NormalizeModule(request.ModuleCode);
            var reason = (request.Reason ?? string.Empty).Trim();

            if (request.PatientId <= 0 || reason.Length < 5)
                return (false, "Vui lòng nhập lý do mở khóa ít nhất 5 ký tự.", false);

            if (request.DurationMinutes < 5 || request.DurationMinutes > 1440)
                return (false, "Thời gian mở khóa phải từ 5 đến 1440 phút.", false);

            long? resultCDHAId = null;
            long? serviceId = null;
            DateTime? returnTime;
            var alreadyInProcess = false;
            var hasXNAdminUnlockHistory = false;

            if (targetType == "XN")
            {
                var patient = await _db.Patients.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Active && x.Id == request.PatientId);
                if (patient == null)
                    return (false, "Không tìm thấy lần làm xét nghiệm.", false);

                // XN Valid chưa đủ kết quả không thuộc diện khóa qua ngày.
                // User được phép Invalid/Process tiếp mà không cần Tool Admin mở khóa.
                if (IsXNIncompleteResult(patient))
                {
                    return (
                        false,
                        "Kết quả xét nghiệm đang Valid chưa đủ; user có thể tiếp tục xử lý mà không cần mở khóa.",
                        false);
                }

                returnTime = patient.ReturnResultTimeXN;
                alreadyInProcess = patient.ProcessXN;
                hasXNAdminUnlockHistory = await HasXNAdminUnlockHistoryAsync(request.PatientId);
            }
            else if (targetType == "CDHA" && CdhaModules.Contains(moduleCode))
            {
                var result = await _db.ResultCDHAs
                    .AsNoTracking()
                    .Where(x =>
                        x.Active &&
                        x.Id == request.ResultCDHAId &&
                        x.PatientId == request.PatientId &&
                        x.Service.Category.Code == moduleCode)
                    .Select(x => new
                    {
                        x.Id,
                        x.ServiceId,

                        x.IsValidated,
                        x.LastValidatedAt,

                        Patient = x.Patient
                    })
                    .FirstOrDefaultAsync();

                if (result == null || result.Patient == null)
                {
                    return (
                        false,
                        "Không tìm thấy kết quả dịch vụ cần mở khóa.",
                        false);
                }

                resultCDHAId = result.Id;
                serviceId = result.ServiceId;

                // =========================================================
                // Thời điểm Valid theo từng service.
                // Legacy sẽ fallback về Patient.ReturnResultTime*
                // =========================================================
                returnTime = GetCDHAValidationTime(
                    result.IsValidated,
                    result.LastValidatedAt,
                    result.Patient,
                    moduleCode);

                // =========================================================
                // Xác định service đang ở trạng thái Process.
                //
                // Cơ chế mới:
                // false + LastValidatedAt != null
                // => đã từng Valid và hiện đang Invalid/Process.
                //
                // Legacy:
                // fallback về Patient.Process*
                // =========================================================
                if (result.IsValidated.HasValue)
                {
                    alreadyInProcess = result.IsValidated.Value == false && result.LastValidatedAt.HasValue;
                }
                else
                {
                    alreadyInProcess = GetModuleProcess(result.Patient, moduleCode);
                }
            }
            else
            {
                return (false, "Module không hợp lệ.", false);
            }

            if (!returnTime.HasValue)
                return (false, "Kết quả chưa từng được trả nên không cần mở khóa.", false);

            var requiresAdminUnlock = IsLockedByDate(returnTime, now)
                || (targetType == "XN" && hasXNAdminUnlockHistory);

            if (!requiresAdminUnlock)
                return (false, "Kết quả chưa thuộc diện khóa; user có thể Invalid theo workflow trong ngày.", false);

            await RevokeTargetPermissionsAsync(
                request.PatientId,
                moduleCode,
                resultCDHAId,
                adminUserId,
                now,
                "Thay thế bằng lần mở khóa mới");

            var permission = new ResultEditUnlock
            {
                PatientId = request.PatientId,
                ModuleCode = moduleCode,
                Scope = targetType == "XN" ? XnScope : CdhaScope,
                ResultCDHAId = resultCDHAId,
                ServiceId = serviceId,
                Source = "ADMIN",
                UnlockReason = reason,
                UnlockedAt = now,
                ExpireAt = now.AddMinutes(request.DurationMinutes),
                UnlockedByUserId = adminUserId,
                UsedAt = alreadyInProcess ? now : null,
                UsedByUserId = alreadyInProcess ? adminUserId : null,
                IsActive = true
            };

            await _db.ResultEditUnlocks.AddAsync(permission);
            await _db.SaveChangesAsync();
            return alreadyInProcess
                ? (true, "Đã mở lại quyền sửa cho kết quả đang ở trạng thái Process.", false)
                : (true, "Đã tạo quyền mở khóa. Hệ thống sẽ Invalid kết quả ngay.", true);
        }

        public async Task<(bool Success, string Message)> LockAsync(
            ToolAdminLockRequest request,
            long adminUserId,
            DateTime now)
        {
            var targetType = NormalizeTargetType(request.TargetType);
            var moduleCode = targetType == "XN" ? "XN" : NormalizeModule(request.ModuleCode);
            var resultId = targetType == "CDHA" ? request.ResultCDHAId : null;

            if (request.PatientId <= 0
                || (targetType != "XN" && targetType != "CDHA")
                || (targetType == "CDHA" && (!resultId.HasValue || !CdhaModules.Contains(moduleCode))))
            {
                return (false, "Dữ liệu khóa không hợp lệ.");
            }

            var count = await RevokeTargetPermissionsAsync(
                request.PatientId,
                moduleCode,
                resultId,
                adminUserId,
                now,
                string.IsNullOrWhiteSpace(request.Reason) ? "Admin khóa lại" : request.Reason!.Trim());

            return count > 0
                ? (true, "Đã khóa quyền Invalid/sửa kết quả.")
                : (false, "Không có quyền mở khóa nào đang hoạt động.");
        }

        // Dùng cho XN
        public async Task RevokeAfterValidAsync(
            long patientId,
            string moduleCode,
            long userId,
            DateTime now)
        {
            await RevokeTargetPermissionsAsync(
                patientId,
                NormalizeModule(moduleCode),
                null,
                userId,
                now,
                "Tự động khóa lại sau khi Valid kết quả");
        }

        // Dùng cho CDHA
        public async Task RevokeAfterValidCDHAAsync(
            long patientId,
            string moduleCode,
            long resultCDHAId,
            long userId,
            DateTime now)
        {
            moduleCode = NormalizeModule(moduleCode);

            if (patientId <= 0 ||
                resultCDHAId <= 0 ||
                !CdhaModules.Contains(moduleCode))
            {
                return;
            }

            await RevokeTargetPermissionsAsync(
                patientId,
                moduleCode,
                resultCDHAId,
                userId,
                now,
                "Tự động khóa lại sau khi Valid kết quả CDHA");
        }

        public async Task<List<ToolAdminResultRow>> SearchAsync(
            string? keyword,
            string? moduleCode,
            DateTime fromDate,
            DateTime toDate,
            DateTime now)
        {
            var from = fromDate.Date;
            var toExclusive = toDate.Date.AddDays(1);
            var module = string.IsNullOrWhiteSpace(moduleCode) ? "ALL" : NormalizeModule(moduleCode);
            var term = (keyword ?? string.Empty).Trim();
            var rows = new List<ToolAdminResultRow>();

            if (module is "ALL" or "XN")
            {
                var patientQuery = _db.Patients.AsNoTracking().Where(x =>
                    x.Active
                    && x.ReturnResultTimeXN.HasValue
                    && x.ReturnResultTimeXN >= from
                    && x.ReturnResultTimeXN < toExclusive);

                if (term.Length > 0)
                {
                    patientQuery = patientQuery.Where(x =>
                        (x.PatientId ?? string.Empty).Contains(term)
                        || (x.MaBenhAn ?? string.Empty).Contains(term)
                        || (x.Sid ?? string.Empty).Contains(term)
                        || (x.PatientName ?? string.Empty).Contains(term));
                }

                var patients = await patientQuery
                    .OrderByDescending(x => x.ReturnResultTimeXN)
                    .Take(1000)
                    .ToListAsync();

                var patientIds = patients.Select(x => x.Id).ToList();

                var xnAdminHistoryIds = (await _db.ResultEditUnlocks.AsNoTracking()
                    .Where(x => patientIds.Contains(x.PatientId)
                        && x.ModuleCode == "XN"
                        && x.Scope == XnScope
                        && x.Source == "ADMIN")
                    .Select(x => x.PatientId)
                    .Distinct()
                    .ToListAsync())
                    .ToHashSet();

                var servicePairs = await _db.ResultXNs.AsNoTracking()
                    .Where(x => x.Active && x.PatientId.HasValue && patientIds.Contains(x.PatientId.Value))
                    .Select(x => new { PatientId = x.PatientId!.Value, ServiceName = x.Service.Name })
                    .Distinct()
                    .ToListAsync();

                var servicesByPatient = servicePairs
                .Where(x => !string.IsNullOrWhiteSpace(x.ServiceName))
                .GroupBy(x => x.PatientId)
                .ToDictionary(
                    g => g.Key,
                    g => g
                        .Select(x => x.ServiceName!.Trim())
                        .Distinct()
                        .OrderBy(x => x)
                        .ToList()
                );

                rows.AddRange(patients.Select(x =>
                {
                    var serviceNames = servicesByPatient.TryGetValue(x.Id, out var services)
                        && services.Count > 0
                            ? services
                            : new List<string> { "Xét nghiệm" };

                    return new ToolAdminResultRow
                    {
                        TargetType = "XN",
                        PatientTableId = x.Id,
                        ModuleCode = "XN",

                        PatientCode = x.PatientId ?? string.Empty,
                        MedicalRecordCode = x.MaBenhAn ?? x.Sid ?? string.Empty,
                        PatientName = x.PatientName ?? string.Empty,
                        MaDotKham = x.MaDotKham ?? string.Empty,

                        // Giữ lại field cũ nếu chỗ khác còn đang sử dụng
                        ServiceName = string.Join(", ", serviceNames),

                        // Field mới dùng để render UI
                        ServiceNames = serviceNames,

                        ReturnResultTime = x.ReturnResultTimeXN!.Value,

                        IsLockedByDate =
                            !IsXNIncompleteResult(x)
                            && (IsLockedByDate(x.ReturnResultTimeXN, now)
                                || xnAdminHistoryIds.Contains(x.Id))
                    };
                }));
            }

            if (module == "ALL" || CdhaModules.Contains(module))
            {
                var resultQuery = _db.ResultCDHAs.AsNoTracking()
                    .Where(x => x.Active && x.Patient.Active);

                if (module != "ALL")
                    resultQuery = resultQuery.Where(x => x.Service.Category.Code == module);

                resultQuery = ApplyCDHAValidationTimeFilter(resultQuery, from, toExclusive);

                if (term.Length > 0)
                {
                    resultQuery = resultQuery.Where(x =>
                        (x.Patient.PatientId ?? string.Empty).Contains(term)
                        || (x.Patient.MaBenhAn ?? string.Empty).Contains(term)
                        || (x.Patient.Sid ?? string.Empty).Contains(term)
                        || (x.Patient.PatientName ?? string.Empty).Contains(term)
                        || (x.Service.Name ?? string.Empty).Contains(term));
                }

                // Project dùng LazyLoadingProxy + AsNoTracking(). Vì vậy không materialize
                // ResultCDHA rồi mới truy cập navigation x.Service/x.Patient ở phía client.
                // Projection toàn bộ dữ liệu cần dùng ngay trong SQL để tránh
                // DetachedLazyLoadingWarning.
                var cdhaResults = await resultQuery
                    .OrderByDescending(x => x.UpdateTime)
                    .Take(2000)
                    .Select(x => new
                    {
                        ResultCDHAId = x.Id,
                        x.ServiceId,
                        ModuleCode = x.Service.Category.Code,
                        ServiceName = x.Service.Name,

                        // Service-level state
                        x.IsValidated,
                        x.LastValidatedAt,

                        PatientTableId = x.Patient.Id,
                        PatientCode = x.Patient.PatientId,
                        MedicalRecordCode = x.Patient.MaBenhAn,
                        Sid = x.Patient.Sid,
                        PatientName = x.Patient.PatientName,

                        ReturnResultTimeSA = x.Patient.ReturnResultTimeSA,
                        ReturnResultTimeSAT = x.Patient.ReturnResultTimeSAT,
                        ReturnResultTimeDDT = x.Patient.ReturnResultTimeDDT,
                        ReturnResultTimeNS = x.Patient.ReturnResultTimeNS,
                        ReturnResultTimeNSCTC = x.Patient.ReturnResultTimeNSCTC,
                        ReturnResultTimeXQ = x.Patient.ReturnResultTimeXQ,
                        ReturnResultTimeTDCN = x.Patient.ReturnResultTimeTDCN
                    }).ToListAsync();

                foreach (var x in cdhaResults)
                {
                    var code =
                        NormalizeModule(x.ModuleCode);

                    // =========================================================
                    // Timestamp module cũ chỉ dùng fallback.
                    // =========================================================
                    DateTime? legacyReturnTime = code switch
                    {
                        "SA" => x.ReturnResultTimeSA,
                        "SAT" => x.ReturnResultTimeSAT,
                        "DDT" => x.ReturnResultTimeDDT,
                        "NS" => x.ReturnResultTimeNS,
                        "NSCTC" => x.ReturnResultTimeNSCTC,
                        "XQ" => x.ReturnResultTimeXQ,
                        "TDCN" => x.ReturnResultTimeTDCN,
                        _ => null
                    };

                    DateTime? effectiveReturnTime;

                    // =========================================================
                    // RECORD MỚI
                    // =========================================================
                    if (x.IsValidated.HasValue)
                    {
                        if (x.LastValidatedAt.HasValue)
                        {
                            effectiveReturnTime =
                                x.LastValidatedAt;
                        }
                        else if (x.IsValidated.Value)
                        {
                            // Fail-safe transitional record
                            effectiveReturnTime =
                                legacyReturnTime;
                        }
                        else
                        {
                            // false + NULL
                            // => chưa từng Valid.
                            effectiveReturnTime = null;
                        }
                    }
                    else
                    {
                        // =====================================================
                        // LEGACY
                        // =====================================================
                        effectiveReturnTime =
                            legacyReturnTime;
                    }

                    // Dịch vụ chưa từng Valid không thuộc Tools Admin.
                    if (!effectiveReturnTime.HasValue)
                        continue;

                    rows.Add(new ToolAdminResultRow
                    {
                        TargetType = "CDHA",

                        PatientTableId =
                            x.PatientTableId,

                        ResultCDHAId =
                            x.ResultCDHAId,

                        ServiceId =
                            x.ServiceId,

                        ModuleCode =
                            code,

                        PatientCode =
                            x.PatientCode ?? string.Empty,

                        MedicalRecordCode =
                            x.MedicalRecordCode
                            ?? x.Sid
                            ?? string.Empty,

                        PatientName =
                            x.PatientName ?? string.Empty,

                        ServiceName =
                            x.ServiceName ?? string.Empty,

                        ServiceNames =
                            string.IsNullOrWhiteSpace(
                                x.ServiceName)
                            ? new List<string>()
                            : new List<string>
                            {
                x.ServiceName
                            },

                        // Timestamp RIÊNG của service
                        ReturnResultTime =
                            effectiveReturnTime.Value,

                        IsLockedByDate =
                            IsLockedByDate(
                                effectiveReturnTime,
                                now)
                    });
                }
            }

            var patientKeys = rows.Select(x => x.PatientTableId).Distinct().ToList();
            var resultKeys = rows.Where(x => x.ResultCDHAId.HasValue)
                .Select(x => x.ResultCDHAId!.Value).Distinct().ToList();
            var activePermissions = await _db.ResultEditUnlocks.AsNoTracking()
                .Where(x => x.IsActive
                    && x.RevokedAt == null
                    && x.ExpireAt > now
                    && patientKeys.Contains(x.PatientId)
                    && (!x.ResultCDHAId.HasValue || resultKeys.Contains(x.ResultCDHAId.Value)))
                .ToListAsync();

            var userIds = activePermissions.Select(x => x.UnlockedByUserId).Distinct().ToList();
            var users = await _db.Users.AsNoTracking()
                .Where(x => userIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name ?? string.Empty);

            foreach (var row in rows)
            {
                var permission = activePermissions
                    .Where(x => x.PatientId == row.PatientTableId
                        && x.ModuleCode == row.ModuleCode
                        && x.ResultCDHAId == row.ResultCDHAId)
                    .OrderByDescending(x => x.UnlockedAt)
                    .FirstOrDefault();

                if (permission == null)
                    continue;

                row.HasActiveUnlock = true;
                row.UnlockWasUsed = permission.UsedAt.HasValue;
                row.UnlockExpireAt = permission.ExpireAt;
                row.UnlockReason = permission.UnlockReason;
                row.UnlockedByName = users.GetValueOrDefault(permission.UnlockedByUserId, permission.UnlockedByUserId.ToString());
            }

            return rows.OrderByDescending(x => x.ReturnResultTime).ToList();
        }

        private async Task<bool> HasXNAdminUnlockHistoryAsync(long patientId)
        {
            // Không lọc IsActive/ExpireAt/RevokedAt vì đây là kiểm tra LỊCH SỬ.
            // Chỉ cần lần làm XN này đã từng được Admin mở khóa thì sau đó nó
            // không được quay lại cơ chế "Invalid tự do trong ngày" nữa.
            return await _db.ResultEditUnlocks.AsNoTracking().AnyAsync(x =>
                x.PatientId == patientId
                && x.ModuleCode == "XN"
                && x.Scope == XnScope
                && x.Source == "ADMIN");
        }

        private async Task<bool> HasActivePermissionAsync(
            long patientId,
            string moduleCode,
            long? resultCDHAId,
            DateTime now,
            bool requireUsed)
        {
            return await _db.ResultEditUnlocks.AsNoTracking().AnyAsync(x =>
                x.IsActive
                && x.PatientId == patientId
                && x.ModuleCode == moduleCode
                && x.ResultCDHAId == resultCDHAId
                && x.RevokedAt == null
                && x.ExpireAt > now
                && (!requireUsed || x.UsedAt.HasValue));
        }

        private async Task<ResultEditUnlock?> FindActivePermissionAsync(
            long patientId,
            string moduleCode,
            long? resultCDHAId,
            DateTime now)
        {
            return await _db.ResultEditUnlocks
                .Where(x => x.IsActive
                    && x.PatientId == patientId
                    && x.ModuleCode == moduleCode
                    && x.ResultCDHAId == resultCDHAId
                    && x.RevokedAt == null
                    && x.ExpireAt > now)
                .OrderByDescending(x => x.UnlockedAt)
                .FirstOrDefaultAsync();
        }

        private async Task<int> RevokeTargetPermissionsAsync(
            long patientId,
            string moduleCode,
            long? resultCDHAId,
            long userId,
            DateTime now,
            string reason)
        {
            var query = _db.ResultEditUnlocks.Where(x =>
                x.IsActive
                && x.PatientId == patientId
                && x.ModuleCode == moduleCode
                && x.RevokedAt == null);

            if (resultCDHAId.HasValue)
                query = query.Where(x => x.ResultCDHAId == resultCDHAId);

            var permissions = await query.ToListAsync();
            foreach (var permission in permissions)
            {
                permission.IsActive = false;
                permission.RevokedAt = now;
                permission.RevokedByUserId = userId;
                permission.RevokeReason = reason;
            }

            if (permissions.Count > 0)
                await _db.SaveChangesAsync();

            return permissions.Count;
        }

        private static IQueryable<ResultCDHA> ApplyCDHAValidationTimeFilter(IQueryable<ResultCDHA> query, DateTime from, DateTime toExclusive)
        {
            return query.Where(x =>

                // =====================================================
                // 1. RECORD MỚI
                //
                // Nếu đã có LastValidatedAt thì dùng timestamp riêng
                // của ResultCDHA, bất kể hiện tại IsValidated=true
                // hay false.
                //
                // false + LastValidatedAt != null nghĩa là đã Invalid,
                // nhưng vẫn cần xuất hiện trong Tools Admin.
                // =====================================================
                (
                    x.IsValidated.HasValue &&
                    x.LastValidatedAt.HasValue &&
                    x.LastValidatedAt >= from &&
                    x.LastValidatedAt < toExclusive
                )

                ||

                // =====================================================
                // 2. FAIL-SAFE
                //
                // IsValidated=true nhưng LastValidatedAt=NULL.
                // Trạng thái này không nên xảy ra sau khi flow mới hoàn
                // thiện, nhưng trong giai đoạn chuyển tiếp vẫn fallback
                // timestamp Patient để tránh làm mất dữ liệu Tools Admin.
                // =====================================================
                (
                    x.IsValidated == true &&
                    !x.LastValidatedAt.HasValue &&

                    (
                        (
                            x.Service.Category.Code == "SA" &&
                            x.Patient.ReturnResultTimeSA >= from &&
                            x.Patient.ReturnResultTimeSA < toExclusive
                        )
                        ||
                        (
                            x.Service.Category.Code == "SAT" &&
                            x.Patient.ReturnResultTimeSAT >= from &&
                            x.Patient.ReturnResultTimeSAT < toExclusive
                        )
                        ||
                        (
                            x.Service.Category.Code == "DDT" &&
                            x.Patient.ReturnResultTimeDDT >= from &&
                            x.Patient.ReturnResultTimeDDT < toExclusive
                        )
                        ||
                        (
                            x.Service.Category.Code == "NS" &&
                            x.Patient.ReturnResultTimeNS >= from &&
                            x.Patient.ReturnResultTimeNS < toExclusive
                        )
                        ||
                        (
                            x.Service.Category.Code == "NSCTC" &&
                            x.Patient.ReturnResultTimeNSCTC >= from &&
                            x.Patient.ReturnResultTimeNSCTC < toExclusive
                        )
                        ||
                        (
                            x.Service.Category.Code == "XQ" &&
                            x.Patient.ReturnResultTimeXQ >= from &&
                            x.Patient.ReturnResultTimeXQ < toExclusive
                        )
                        ||
                        (
                            x.Service.Category.Code == "TDCN" &&
                            x.Patient.ReturnResultTimeTDCN >= from &&
                            x.Patient.ReturnResultTimeTDCN < toExclusive
                        )
                    )
                )

                ||

                // =====================================================
                // 3. LEGACY
                // =====================================================
                (
                    !x.IsValidated.HasValue &&

                    (
                        (
                            x.Service.Category.Code == "SA" &&
                            x.Patient.ReturnResultTimeSA >= from &&
                            x.Patient.ReturnResultTimeSA < toExclusive
                        )
                        ||
                        (
                            x.Service.Category.Code == "SAT" &&
                            x.Patient.ReturnResultTimeSAT >= from &&
                            x.Patient.ReturnResultTimeSAT < toExclusive
                        )
                        ||
                        (
                            x.Service.Category.Code == "DDT" &&
                            x.Patient.ReturnResultTimeDDT >= from &&
                            x.Patient.ReturnResultTimeDDT < toExclusive
                        )
                        ||
                        (
                            x.Service.Category.Code == "NS" &&
                            x.Patient.ReturnResultTimeNS >= from &&
                            x.Patient.ReturnResultTimeNS < toExclusive
                        )
                        ||
                        (
                            x.Service.Category.Code == "NSCTC" &&
                            x.Patient.ReturnResultTimeNSCTC >= from &&
                            x.Patient.ReturnResultTimeNSCTC < toExclusive
                        )
                        ||
                        (
                            x.Service.Category.Code == "XQ" &&
                            x.Patient.ReturnResultTimeXQ >= from &&
                            x.Patient.ReturnResultTimeXQ < toExclusive
                        )
                        ||
                        (
                            x.Service.Category.Code == "TDCN" &&
                            x.Patient.ReturnResultTimeTDCN >= from &&
                            x.Patient.ReturnResultTimeTDCN < toExclusive
                        )
                    )
                )
            );
        }

        private static DateTime? GetCDHAValidationTime(
            bool? isValidated,
            DateTime? lastValidatedAt,
            Patient patient,
            string moduleCode)
        {
            // Record đã đi theo cơ chế service-level mới.
            if (isValidated.HasValue)
            {
                // Đã từng Valid, kể cả hiện tại đang Invalid.
                if (lastValidatedAt.HasValue)
                    return lastValidatedAt;

                // Safety fallback:
                // nếu IsValidated=true nhưng LastValidatedAt chưa được ghi
                // thì dùng timestamp Patient để tránh mở khóa nhầm.
                if (isValidated.Value)
                    return GetReturnResultTime(patient, moduleCode);

                // IsValidated=false + LastValidatedAt=NULL
                // => dịch vụ mới, chưa từng Valid.
                return null;
            }

            // Legacy record:
            // chưa có state riêng, giữ logic Patient cũ.
            return GetReturnResultTime(patient, moduleCode);
        }

        public static DateTime? GetReturnResultTime(Patient patient, string moduleCode)
        {
            return NormalizeModule(moduleCode) switch
            {
                "XN" => patient.ReturnResultTimeXN,
                "SA" => patient.ReturnResultTimeSA,
                "SAT" => patient.ReturnResultTimeSAT,
                "DDT" => patient.ReturnResultTimeDDT,
                "NS" => patient.ReturnResultTimeNS,
                "NSCTC" => patient.ReturnResultTimeNSCTC,
                "XQ" => patient.ReturnResultTimeXQ,
                "TDCN" => patient.ReturnResultTimeTDCN,
                _ => null
            };
        }

        private static bool GetModuleValid(Patient patient, string moduleCode)
        {
            return NormalizeModule(moduleCode) switch
            {
                "SA" => patient.ValidSA,
                "SAT" => patient.ValidSAT,
                "DDT" => patient.ValidDDT,
                "NS" => patient.ValidNS,
                "NSCTC" => patient.ValidNSCTC,
                "XQ" => patient.ValidXQ,
                "TDCN" => patient.ValidTDCN,
                _ => false
            };
        }

        private static bool GetModuleProcess(Patient patient, string moduleCode)
        {
            return NormalizeModule(moduleCode) switch
            {
                "XN" => patient.ProcessXN,
                "SA" => patient.ProcessSA,
                "SAT" => patient.ProcessSAT,
                "DDT" => patient.ProcessDDT,
                "NS" => patient.ProcessNS,
                "NSCTC" => patient.ProcessNSCTC,
                "XQ" => patient.ProcessXQ,
                "TDCN" => patient.ProcessTDCN,
                _ => false
            };
        }

        private static string NormalizeModule(string? value) => (value ?? string.Empty).Trim().ToUpperInvariant();
        private static string NormalizeTargetType(string? value) => NormalizeModule(value);
    }
}
