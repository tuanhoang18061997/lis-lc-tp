using Management.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using Object = Management.Models.Object;

namespace Management.BL
{
    public class ResultCDHABL
    {
        private readonly LABContext _db;
        private readonly ServiceTestBL _serviceTestBL;
        private readonly ResultEditUnlockBL _resultEditUnlockBL;
        private static readonly HashSet<string> CdhaValidationModules =
            new(StringComparer.OrdinalIgnoreCase)
        {
            "SA",
            "SAT",
            "DDT",
            "NS",
            "NSCTC",
            "XQ",
            "TDCN"
        };
        public ResultCDHABL(
            LABContext db,
            ServiceTestBL serviceTestBL,
            ResultEditUnlockBL resultEditUnlockBL)
        {
            _db = db;
            _serviceTestBL = serviceTestBL;
            _resultEditUnlockBL = resultEditUnlockBL;
        }

        public async Task<bool> DeleteByPatientId(long patientId, long? userInsertOrUpdate)
        {
            try
            {
                var _lstResultCDHA = await _db.ResultCDHAs.Where(p => p.Active == true && p.PatientId == patientId).ToListAsync();
                if (_lstResultCDHA != null)
                {
                    foreach (var _item in _lstResultCDHA)
                    {
                        _item.Active = false;
                        _item.UserUpdateId = userInsertOrUpdate;
                    }
                    await _db.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteById(long id, long? userInsertOrUpdate)
        {
            try
            {
                var _resultCDHA = await _db.ResultCDHAs.Where(p => p.Active == true && p.Id == id).FirstOrDefaultAsync();
                if (_resultCDHA != null)
                {
                    _resultCDHA.Active = false;
                    _resultCDHA.UserUpdateId = userInsertOrUpdate;
                    await _db.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SaveService(long patientId, long serviceId, long? userInsertIdOrUpdateId, DateTime dateTime, long doctorId, string categoryCode)
        {
            try
            {
                var _resultCDHA = await _db.ResultCDHAs.Where(p => p.Active == true && p.PatientId == patientId && p.ServiceId == serviceId).FirstOrDefaultAsync();
                if (_resultCDHA != null) return false;
                _resultCDHA = new ResultCDHA();
                _resultCDHA.PatientId = patientId;
                _resultCDHA.ServiceId = serviceId;
                _resultCDHA.UserInsertId = userInsertIdOrUpdateId;
                _resultCDHA.InsertTime = dateTime;
                _resultCDHA.DoctorId = doctorId;
                _resultCDHA.KeyResultForHis = categoryCode + "-" + Guid.NewGuid().ToString();
                _resultCDHA.Active = true;

                // ============================================================
                // Validation state theo từng dịch vụ
                // Dịch vụ mới chắc chắn chưa từng Valid.
                // ============================================================
                _resultCDHA.IsValidated = false;
                _resultCDHA.LastValidatedAt = null;
                _resultCDHA.LastValidatedByUserId = null;

                await _db.ResultCDHAs.AddAsync(_resultCDHA);
                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Update(ResultCDHAModel resultCDHA, long? userInsertIdOrUpdateId, DateTime dateTime, Device device)
        {
            try
            {
                // Nếu kết quả đã từng Valid, chỉ đúng ResultCDHA.Id đã được Invalid mới được sửa.
                if (!await _resultEditUnlockBL.CanEditCDHAAsync(
                    resultCDHA.resultCDHAId,
                    ToolBL.Get_DateNow()))
                {
                    return false;
                }

                var _resultCDHA = await _db.ResultCDHAs.Where(p => p.Active == true && p.Id == resultCDHA.resultCDHAId).FirstOrDefaultAsync();
                if (_resultCDHA == null) return false;
                _resultCDHA.UserUpdateId = userInsertIdOrUpdateId;
                _resultCDHA.UpdateTime = dateTime;
                _resultCDHA.Description = resultCDHA.description;
                _resultCDHA.Result = resultCDHA.result;
                _resultCDHA.Suggest = resultCDHA.suggest;
                _resultCDHA.DeviceCodeBHYT = device?.CodeBHYT;
                _resultCDHA.SieuAmTim = resultCDHA.sieuamtim;
                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ValidateCDHAAsync(
            ResultCDHAModel resultCDHA,
            string moduleCode,
            long loginUserId,
            DateTime validatedAt)
        {
            if (resultCDHA == null)
                return false;

            return await ValidateCDHAMultipleAsync(
                new List<ResultCDHAModel>
                {
                    resultCDHA
                },
                moduleCode,
                loginUserId,
                validatedAt
            );
        }

        public async Task<bool> ValidateCDHAMultipleAsync(
            IReadOnlyCollection<ResultCDHAModel> resultCDHAs,
            string moduleCode,
            long loginUserId,
            DateTime validatedAt)
        {
            try
            {
                moduleCode = (moduleCode ?? string.Empty)
                    .Trim()
                    .ToUpperInvariant();

                if (!CdhaValidationModules.Contains(moduleCode) ||
                    resultCDHAs == null ||
                    resultCDHAs.Count == 0 ||
                    loginUserId <= 0)
                {
                    return false;
                }

                var items = resultCDHAs
                    .Where(x =>
                        x != null &&
                        x.resultCDHAId > 0 &&
                        x.patientId > 0)
                    .ToList();

                if (items.Count != resultCDHAs.Count)
                    return false;

                // Không cho duplicate ResultCDHAId.
                var ids = items
                    .Select(x => x.resultCDHAId)
                    .Distinct()
                    .ToList();

                if (ids.Count != items.Count)
                    return false;

                // Tất cả phải thuộc cùng 1 Patient.
                var patientIds = items
                    .Select(x => x.patientId)
                    .Distinct()
                    .ToList();

                if (patientIds.Count != 1)
                    return false;

                var patientId = patientIds[0];

                // =====================================================
                // PHASE 1:
                // Kiểm tra TOÀN BỘ trước.
                //
                // Không Valid service đầu rồi mới phát hiện service thứ 2
                // không hợp lệ.
                // =====================================================
                foreach (var item in items)
                {
                    if (!await _resultEditUnlockBL.CanEditCDHAAsync(
                        item.resultCDHAId,
                        validatedAt))
                    {
                        return false;
                    }
                }

                // Không tin ResultId/module từ client.
                var results = await _db.ResultCDHAs
                    .Where(x =>
                        x.Active &&
                        x.PatientId == patientId &&
                        ids.Contains(x.Id) &&
                        x.Service.Category.Code == moduleCode)
                    .ToListAsync();

                if (results.Count != ids.Count)
                    return false;

                var resultsById =
                    results.ToDictionary(x => x.Id);

                // =====================================================
                // PHASE 2:
                // Valid từng ResultCDHA.
                // =====================================================
                foreach (var item in items)
                {
                    if (!resultsById.TryGetValue(
                            item.resultCDHAId,
                            out var result))
                    {
                        return false;
                    }

                    result.IsValidated = true;
                    result.LastValidatedAt = validatedAt;
                    result.LastValidatedByUserId = loginUserId;
                }

                // =====================================================
                // PHASE 3:
                // Tính lại aggregate Patient.
                //
                // Chỉ lấy metadata trả kết quả từ item đầu tiên.
                // Đối với MarkPatientAsDone TDCN, các item cùng một lần
                // thao tác nên metadata Patient là chung.
                // =====================================================
                var first = items[0];

                var aggregateUpdated =
                    await RecalculatePatientModuleStateAsync(
                        patientId,
                        moduleCode,
                        loginUserId,
                        first.returnResultTime,
                        first.userReturnResult);

                if (!aggregateUpdated)
                    return false;

                // ResultCDHA + Patient cùng SaveChanges.
                await _db.SaveChangesAsync();

                // =====================================================
                // PHASE 4:
                // Revoke đúng từng ResultCDHA vừa Valid.
                // =====================================================
                foreach (var result in results)
                {
                    await _resultEditUnlockBL
                        .RevokeAfterValidCDHAAsync(
                            patientId,
                            moduleCode,
                            result.Id,
                            loginUserId,
                            validatedAt);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RecalculatePatientModuleStateAsync(
            long patientId,
            string moduleCode,
            long userId,
            DateTime? returnResultTime = null,
            long? userReturnResult = null)
        {
            moduleCode = (moduleCode ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

            if (patientId <= 0 ||
                userId <= 0 ||
                !CdhaValidationModules.Contains(moduleCode))
            {
                return false;
            }

            var patient = await _db.Patients
                .Where(x =>
                    x.Active &&
                    x.Id == patientId)
                .FirstOrDefaultAsync();

            if (patient == null)
                return false;

            // =========================================================
            // Lưu state legacy TRƯỚC khi tính lại Patient.
            // =========================================================
            var legacyModuleValid =
                GetPatientModuleValid(
                    patient,
                    moduleCode);

            var legacyReturnTime =
                ResultEditUnlockBL.GetReturnResultTime(
                    patient,
                    moduleCode);

            // Không dùng AsNoTracking().
            //
            // Nếu caller vừa đổi IsValidated của ResultCDHA nhưng chưa
            // SaveChanges thì EF tracking vẫn trả đúng state mới.
            var results = await _db.ResultCDHAs
                .Where(x =>
                    x.Active &&
                    x.PatientId == patientId &&
                    x.Service.Category.Code == moduleCode)
                .ToListAsync();

            var hasValidated = false;
            var hasPending = false;

            foreach (var result in results)
            {
                // =====================================================
                // CƠ CHẾ MỚI
                // =====================================================
                if (result.IsValidated.HasValue)
                {
                    if (result.IsValidated.Value)
                    {
                        hasValidated = true;
                    }
                    else
                    {
                        hasPending = true;
                    }

                    continue;
                }

                // =====================================================
                // LEGACY
                //
                // Record được tạo trước schema mới vẫn fallback theo
                // Patient.Valid* + ReturnResultTime*.
                //
                // Nếu InsertTime sau lần trả kết quả cũ thì đó là một
                // dịch vụ mới chưa Valid.
                // =====================================================
                var legacyValidated =
                    legacyModuleValid &&
                    legacyReturnTime.HasValue &&
                    (
                        !result.InsertTime.HasValue ||
                        result.InsertTime.Value <=
                            legacyReturnTime.Value
                    );

                if (legacyValidated)
                {
                    hasValidated = true;
                }
                else
                {
                    hasPending = true;
                }
            }

            ApplyPatientModuleState(
                patient,
                moduleCode,
                hasValidated,
                hasPending,
                returnResultTime,
                userReturnResult);

            patient.UserUpdateId = userId;

            // Persist aggregate ngay trong chính DbContext của ResultCDHABL.
            // Không phụ thuộc ResultInvalidBL và ResultCDHABL có dùng cùng LABContext hay không.
            await _db.SaveChangesAsync();

            return true;
        }

        private static bool GetPatientModuleValid(
            Patient patient,
            string moduleCode)
        {
            return moduleCode switch
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

        private static void ApplyPatientModuleState(
            Patient patient,
            string moduleCode,
            bool hasValidated,
            bool hasPending,
            DateTime? returnResultTime,
            long? userReturnResult)
        {
            switch (moduleCode)
            {
                case "SA":
                    patient.WaitSA = false;
                    patient.ProcessSA = hasPending;
                    patient.ValidSA = hasValidated;

                    if (returnResultTime.HasValue)
                        patient.ReturnResultTimeSA =
                            returnResultTime.Value;

                    if (userReturnResult.HasValue)
                        patient.UserReturnResultSA =
                            userReturnResult.Value;

                    break;

                case "SAT":
                    patient.WaitSAT = false;
                    patient.ProcessSAT = hasPending;
                    patient.ValidSAT = hasValidated;

                    if (returnResultTime.HasValue)
                        patient.ReturnResultTimeSAT =
                            returnResultTime.Value;

                    if (userReturnResult.HasValue)
                        patient.UserReturnResultSAT =
                            userReturnResult.Value;

                    break;

                case "DDT":
                    patient.WaitDDT = false;
                    patient.ProcessDDT = hasPending;
                    patient.ValidDDT = hasValidated;

                    if (returnResultTime.HasValue)
                        patient.ReturnResultTimeDDT =
                            returnResultTime.Value;

                    if (userReturnResult.HasValue)
                        patient.UserReturnResultDDT =
                            userReturnResult.Value;

                    break;

                case "NS":
                    patient.WaitNS = false;
                    patient.ProcessNS = hasPending;
                    patient.ValidNS = hasValidated;

                    if (returnResultTime.HasValue)
                        patient.ReturnResultTimeNS =
                            returnResultTime.Value;

                    if (userReturnResult.HasValue)
                        patient.UserReturnResultNS =
                            userReturnResult.Value;

                    break;

                case "NSCTC":
                    patient.WaitNSCTC = false;
                    patient.ProcessNSCTC = hasPending;
                    patient.ValidNSCTC = hasValidated;

                    if (returnResultTime.HasValue)
                        patient.ReturnResultTimeNSCTC =
                            returnResultTime.Value;

                    if (userReturnResult.HasValue)
                        patient.UserReturnResultNSCTC =
                            userReturnResult.Value;

                    break;

                case "XQ":
                    patient.WaitXQ = false;
                    patient.ProcessXQ = hasPending;
                    patient.ValidXQ = hasValidated;

                    if (returnResultTime.HasValue)
                        patient.ReturnResultTimeXQ =
                            returnResultTime.Value;

                    if (userReturnResult.HasValue)
                        patient.UserReturnResultXQ =
                            userReturnResult.Value;

                    break;

                case "TDCN":
                    patient.WaitTDCN = false;
                    patient.ProcessTDCN = hasPending;
                    patient.ValidTDCN = hasValidated;

                    if (returnResultTime.HasValue)
                        patient.ReturnResultTimeTDCN =
                            returnResultTime.Value;

                    if (userReturnResult.HasValue)
                        patient.UserReturnResultTDCN =
                            userReturnResult.Value;

                    break;
            }
        }

        public async Task<ResultCDHA> GetResultCDHAByPatientId_ForValidPrint(long resultCDHAId)
        {
            try
            {
                return await _db.ResultCDHAs.Where(p => p.Active == true && p.Id == resultCDHAId).FirstOrDefaultAsync();
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<ResultCDHA>> GetListResultCDHAByPatientId(long patientId, string categoryCode)
        {
            try
            {
                var a = await _db.ResultCDHAs.Where(p => p.Active == true && p.PatientId == patientId && p.Service.Category.Code == categoryCode).ToListAsync();
                return await _db.ResultCDHAs.Where(p => p.Active == true && p.PatientId == patientId && p.Service.Category.Code == categoryCode).ToListAsync();
            }
            catch
            {
                return null;
            }
        }

        //public async Task<List<ImageCDHA>> GetImageForService(long id)
        //{
        //    try
        //    {
        //        return await _db.ImageCDHAs.Where(p => p.ResultCDHAId == id).ToListAsync();
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

        public async Task<ResultCDHA> GetResultCDHA(long id)
        {
            try
            {
                return await _db.ResultCDHAs.Where(p => p.Active == true && p.Id == id).FirstOrDefaultAsync();
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> SaveImage(long id, string imagePath, string imagePath1)
        {
            try
            {
                var _imageCDHA = new ImageCDHA();
                _imageCDHA.ResultCDHAId = id;
                _imageCDHA.ImagePath = imagePath;
                _imageCDHA.ImagePath1 = imagePath1;
                await _db.ImageCDHAs.AddAsync(_imageCDHA);
                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> DeleteImage(long id)
        {
            var imagePath1_resultCDHAId = string.Empty;
            try
            {

                var item = await _db.ImageCDHAs.Where(p => p.Id == id).FirstOrDefaultAsync();
                if (item != null)
                {
                    imagePath1_resultCDHAId = item.ImagePath1 + ";" + item.ResultCDHAId;
                    _db.ImageCDHAs.Remove(item);
                    await _db.SaveChangesAsync();
                }
                return imagePath1_resultCDHAId;
            }
            catch
            {
                return imagePath1_resultCDHAId;
            }
        }

        public async Task<bool> ExitResultXN_Update_GetSampleTime(long patientId, DateTime getSampleTime, string categoryCode)
        {
            try
            {
                var resultCDHA = await _db.ResultCDHAs.Where(p => p.Active == true && p.PatientId == patientId && p.Service.Category.Code == categoryCode).FirstOrDefaultAsync();
                if (resultCDHA != null)
                {
                    if (categoryCode == "SA")
                    {
                        resultCDHA.Patient.GetSampleTimeSA = getSampleTime;
                    }
                    else if (categoryCode == "SAT")
                    {
                        resultCDHA.Patient.GetSampleTimeSAT = getSampleTime;
                    }
                    else if (categoryCode == "NS")
                    {
                        resultCDHA.Patient.GetSampleTimeNS = getSampleTime;
                    }
                    else if (categoryCode == "XQ")
                    {
                        resultCDHA.Patient.GetSampleTimeXQ = getSampleTime;
                    }
                    else if (categoryCode == "DDT")
                    {
                        resultCDHA.Patient.GetSampleTimeDDT = getSampleTime;
                    }
                    else if (categoryCode == "TDCN")
                    {
                        resultCDHA.Patient.GetSampleTimeTDCN = getSampleTime;
                    }
                    await _db.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Delete(string ticketItemId, string categoryCode)
        {
            try
            {
                ResultCDHA resultCDHA = null;
                if (categoryCode == "SA")
                {
                    resultCDHA = await _db.ResultCDHAs.Where(p => p.Active == true && p.TicketItemId == ticketItemId && p.Patient.WaitSA == true).FirstOrDefaultAsync();
                }
                else if (categoryCode == "SAT")
                {
                    resultCDHA = await _db.ResultCDHAs.Where(p => p.Active == true && p.TicketItemId == ticketItemId && p.Patient.WaitSAT == true).FirstOrDefaultAsync();
                }
                else if (categoryCode == "DDT")
                {
                    resultCDHA = await _db.ResultCDHAs.Where(p => p.Active == true && p.TicketItemId == ticketItemId && p.Patient.WaitDDT == true).FirstOrDefaultAsync();
                }
                else if (categoryCode == "XQ")
                {
                    resultCDHA = await _db.ResultCDHAs.Where(p => p.Active == true && p.TicketItemId == ticketItemId && p.Patient.WaitXQ == true).FirstOrDefaultAsync();
                }
                else if (categoryCode == "NS")
                {
                    resultCDHA = await _db.ResultCDHAs.Where(p => p.Active == true && p.TicketItemId == ticketItemId && p.Patient.WaitNS == true).FirstOrDefaultAsync();
                }
                else if (categoryCode == "TDCN")
                {
                    resultCDHA = await _db.ResultCDHAs.Where(p => p.Active == true && p.TicketItemId == ticketItemId && p.Patient.WaitTDCN == true).FirstOrDefaultAsync();
                }
                if (resultCDHA != null)
                {
                    resultCDHA.Active = false;
                    await _db.SaveChangesAsync();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateSignStoreId_CKS(long resultCDHAId, long signStoreId, long userInsertIdOrUpdateId, DateTime dateTime)
        {
            try
            {
                var _resultCDHA = await _db.ResultCDHAs.Where(p => p.Active == true && p.Id == resultCDHAId).FirstOrDefaultAsync();
                if (_resultCDHA == null) return false;
                _resultCDHA.UserUpdateId = userInsertIdOrUpdateId;
                _resultCDHA.UpdateTime = dateTime;
                _resultCDHA.SignStoreId = signStoreId;
                _resultCDHA.SignStatus = 1;
                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<ResultCDHA> GetResultCDHAByKeyResultForHis(string keyResultForHis)
        {
            try
            {
                return await _db.ResultCDHAs
                    .Include(r => r.Patient)
                    .Include(r => r.Service)
                    .Where(r => r.Active == true && r.KeyResultForHis == keyResultForHis)
                    .FirstOrDefaultAsync();
            }
            catch
            {
                return null;
            }
        }
    }
}
