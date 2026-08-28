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

        public async Task<bool> CanInvalidXNAsync(long patientId, DateTime now)
        {
            var patient = await _db.Patients.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Active && x.Id == patientId);

            if (patient == null || !patient.ValidXN)
                return false;

            var lockedByDate = IsLockedByDate(patient.ReturnResultTimeXN, now);
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
            if (!CdhaModules.Contains(moduleCode) || resultIds.Count == 0)
                return false;

            var ids = resultIds.Distinct().ToList();
            var results = await _db.ResultCDHAs.AsNoTracking()
                .Where(x => x.Active
                    && x.PatientId == patientId
                    && ids.Contains(x.Id)
                    && x.Service.Category.Code == moduleCode)
                .Select(x => new { x.Id, x.Patient })
                .ToListAsync();

            if (results.Count != ids.Count)
                return false;

            var patient = results[0].Patient;
            if (patient == null || !GetModuleValid(patient, moduleCode))
                return false;

            var returnTime = GetReturnResultTime(patient, moduleCode);
            if (!IsLockedByDate(returnTime, now))
                return true;

            var activeCount = await _db.ResultEditUnlocks.AsNoTracking().CountAsync(x =>
                x.IsActive
                && x.Scope == CdhaScope
                && x.Source == "ADMIN"
                && x.PatientId == patientId
                && x.ModuleCode == moduleCode
                && x.ResultCDHAId.HasValue
                && ids.Contains(x.ResultCDHAId.Value)
                && x.RevokedAt == null
                && x.ExpireAt > now);

            return activeCount == ids.Count;
        }

        public async Task<bool> CanEditXNAsync(long patientId, DateTime now)
        {
            var patient = await _db.Patients.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Active && x.Id == patientId);

            if (patient == null)
                return false;

            var lockedByDate = IsLockedByDate(patient.ReturnResultTimeXN, now);
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

        public async Task<bool> CanEditCDHAAsync(long resultCDHAId, DateTime now)
        {
            var result = await _db.ResultCDHAs.AsNoTracking()
                .Where(x => x.Active && x.Id == resultCDHAId)
                .Select(x => new
                {
                    Result = x,
                    ModuleCode = x.Service.Category.Code,
                    Patient = x.Patient
                })
                .FirstOrDefaultAsync();

            if (result == null || result.Patient == null)
                return false;

            var moduleCode = NormalizeModule(result.ModuleCode);
            var returnTime = GetReturnResultTime(result.Patient, moduleCode);

            // Kết quả chưa từng Valid: đây là luồng thực hiện ban đầu, không cần quyền mở khóa.
            if (!returnTime.HasValue)
                return true;

            // Dịch vụ mới được chỉ định sau lần trả kết quả trước là luồng thực hiện mới,
            // không phải sửa lại dịch vụ cũ.
            if (result.Result.InsertTime.HasValue && result.Result.InsertTime > returnTime)
                return true;

            // Khi đã từng trả kết quả, CDHA luôn cần đúng phạm vi dịch vụ đã Invalid.
            return await HasActivePermissionAsync(
                result.Patient.Id,
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

            var lockedByDate = IsLockedByDate(patient.ReturnResultTimeXN, now);
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
            var ids = resultIds.Distinct().ToList();
            var results = await _db.ResultCDHAs.AsNoTracking()
                .Where(x => x.Active
                    && x.PatientId == patientId
                    && ids.Contains(x.Id)
                    && x.Service.Category.Code == moduleCode)
                .Select(x => new { x.Id, x.ServiceId, Patient = x.Patient })
                .ToListAsync();

            if (results.Count != ids.Count || results[0].Patient == null)
                return false;

            var returnTime = GetReturnResultTime(results[0].Patient!, moduleCode);
            var lockedByDate = IsLockedByDate(returnTime, now);

            foreach (var result in results)
            {
                var permission = await FindActivePermissionAsync(patientId, moduleCode, result.Id, now);

                if (lockedByDate)
                {
                    if (permission == null || permission.Source != "ADMIN")
                        return false;

                    permission.UsedAt ??= now;
                    permission.UsedByUserId ??= userId;
                    continue;
                }

                if (permission == null)
                {
                    // Bản ghi hết hạn vẫn còn IsActive=1 sẽ vướng unique filtered index.
                    await RevokeTargetPermissionsAsync(
                        patientId,
                        moduleCode,
                        result.Id,
                        userId,
                        now,
                        "Quyền cũ đã hết hạn");

                    permission = new ResultEditUnlock
                    {
                        PatientId = patientId,
                        ModuleCode = moduleCode,
                        Scope = CdhaScope,
                        ResultCDHAId = result.Id,
                        ServiceId = result.ServiceId,
                        Source = "SAME_DAY",
                        UnlockReason = "Phạm vi sửa được tạo khi Invalid kết quả trong ngày",
                        UnlockedAt = now,
                        ExpireAt = now.Date.AddDays(1),
                        UnlockedByUserId = userId,
                        UsedAt = now,
                        UsedByUserId = userId,
                        IsActive = true
                    };
                    await _db.ResultEditUnlocks.AddAsync(permission);
                }
                else
                {
                    permission.UsedAt ??= now;
                    permission.UsedByUserId ??= userId;
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

                returnTime = patient.ReturnResultTimeXN;
                alreadyInProcess = patient.ProcessXN;
                hasXNAdminUnlockHistory = await HasXNAdminUnlockHistoryAsync(request.PatientId);
            }
            else if (targetType == "CDHA" && CdhaModules.Contains(moduleCode))
            {
                var result = await _db.ResultCDHAs.AsNoTracking()
                    .Where(x => x.Active
                        && x.Id == request.ResultCDHAId
                        && x.PatientId == request.PatientId
                        && x.Service.Category.Code == moduleCode)
                    .Select(x => new { x.Id, x.ServiceId, Patient = x.Patient })
                    .FirstOrDefaultAsync();
                if (result == null || result.Patient == null)
                    return (false, "Không tìm thấy kết quả dịch vụ cần mở khóa.", false);

                resultCDHAId = result.Id;
                serviceId = result.ServiceId;
                returnTime = GetReturnResultTime(result.Patient, moduleCode);
                alreadyInProcess = GetModuleProcess(result.Patient, moduleCode);
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
                            IsLockedByDate(x.ReturnResultTimeXN, now)
                            || xnAdminHistoryIds.Contains(x.Id)
                    };
                }));
            }

            if (module == "ALL" || CdhaModules.Contains(module))
            {
                var resultQuery = _db.ResultCDHAs.AsNoTracking()
                    .Where(x => x.Active && x.Patient.Active);

                if (module != "ALL")
                    resultQuery = resultQuery.Where(x => x.Service.Category.Code == module);

                resultQuery = ApplyReturnTimeFilter(resultQuery, from, toExclusive);

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
                    })
                    .ToListAsync();

                rows.AddRange(cdhaResults.Select(x =>
                {
                    var code = NormalizeModule(x.ModuleCode);

                    DateTime? returnTime = code switch
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

                    return new ToolAdminResultRow
                    {
                        TargetType = "CDHA",
                        PatientTableId = x.PatientTableId,
                        ResultCDHAId = x.ResultCDHAId,
                        ServiceId = x.ServiceId,
                        ModuleCode = code,
                        PatientCode = x.PatientCode ?? string.Empty,
                        MedicalRecordCode = x.MedicalRecordCode ?? x.Sid ?? string.Empty,
                        PatientName = x.PatientName ?? string.Empty,
                        ServiceName = x.ServiceName ?? string.Empty,
                        ServiceNames = string.IsNullOrWhiteSpace(x.ServiceName)
                            ? new List<string>()
                            : new List<string> { x.ServiceName },
                        ReturnResultTime = returnTime!.Value,
                        IsLockedByDate = IsLockedByDate(returnTime, now)
                    };
                }));
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

        private static IQueryable<ResultCDHA> ApplyReturnTimeFilter(
            IQueryable<ResultCDHA> query,
            DateTime from,
            DateTime toExclusive)
        {
            return query.Where(x =>
                (x.Service.Category.Code == "SA" && x.Patient.ReturnResultTimeSA >= from && x.Patient.ReturnResultTimeSA < toExclusive)
                || (x.Service.Category.Code == "SAT" && x.Patient.ReturnResultTimeSAT >= from && x.Patient.ReturnResultTimeSAT < toExclusive)
                || (x.Service.Category.Code == "DDT" && x.Patient.ReturnResultTimeDDT >= from && x.Patient.ReturnResultTimeDDT < toExclusive)
                || (x.Service.Category.Code == "NS" && x.Patient.ReturnResultTimeNS >= from && x.Patient.ReturnResultTimeNS < toExclusive)
                || (x.Service.Category.Code == "NSCTC" && x.Patient.ReturnResultTimeNSCTC >= from && x.Patient.ReturnResultTimeNSCTC < toExclusive)
                || (x.Service.Category.Code == "XQ" && x.Patient.ReturnResultTimeXQ >= from && x.Patient.ReturnResultTimeXQ < toExclusive)
                || (x.Service.Category.Code == "TDCN" && x.Patient.ReturnResultTimeTDCN >= from && x.Patient.ReturnResultTimeTDCN < toExclusive));
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
