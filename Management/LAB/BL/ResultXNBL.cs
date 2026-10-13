using Management.Models;
using Management.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using ZXing;
using static Management.Controllers.XN_ProcessController;
using Object = Management.Models.Object;

namespace Management.BL
{
    public class ResultXNBL
    {
        private readonly LABContext _db;
        private readonly ServiceTestBL _serviceTestBL;
        private readonly ResultEditUnlockBL _resultEditUnlockBL;
        public ResultXNBL(
            LABContext db,
            ServiceTestBL serviceTestBL,
            ResultEditUnlockBL resultEditUnlockBL)
        {
            _db = db;
            _serviceTestBL = serviceTestBL;
            _resultEditUnlockBL = resultEditUnlockBL;
        }

        public async Task<ResultXN> GetResultXN(long id)
        {
            try
            {
                return await _db.ResultXNs.Where(p => p.Active == true && p.Id == id).FirstOrDefaultAsync();
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> DeleteByPatientId(long patientId, long? userInsertOrUpdate)
        {
            try
            {
                var _lstResultXN = await _db.ResultXNs.Where(p => p.Active == true && p.PatientId == patientId).ToListAsync();
                if (_lstResultXN != null)
                {
                    foreach (var _item in _lstResultXN)
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
                var _resultXN = await _db.ResultXNs.Where(p => p.Active == true && p.Id == id).FirstOrDefaultAsync();
                if (_resultXN != null)
                {
                    if (_resultXN.TestCode.IsTestHead)
                    {
                        var _lstChild = await _db.ResultXNs.Where(p => p.Active == true && p.PatientId == _resultXN.PatientId && p.ServiceId == _resultXN.ServiceId).ToListAsync();
                        if (_lstChild != null)
                        {
                            foreach (var _item in _lstChild)
                            {
                                _item.Active = false;
                                _item.UserUpdateId = userInsertOrUpdate;
                            }
                            await _db.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        _resultXN.Active = false;
                        _resultXN.UserUpdateId = userInsertOrUpdate;
                        await _db.SaveChangesAsync();
                    }
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SaveService(long patientId, long serviceId, long? userInsertIdOrUpdateId, DateTime dateTime, long doctorId)
        {
            try
            {
                var _lstServiceTest = await _serviceTestBL.GetListServiceTestByServiceId(serviceId);
                if (_lstServiceTest == null || _lstServiceTest.Count == 0) return false;

                var _lstResultXN = await _db.ResultXNs.Where(p => p.Active == true && p.PatientId == patientId).OrderBy(p => p.InsertTime).ToListAsync();
                var _resultXN = _lstResultXN.Where(p => p.ServiceId == serviceId).FirstOrDefault();
                if (_resultXN != null) return false;

                var keyResultForHis = "xn-" + Guid.NewGuid().ToString();
                if (_lstResultXN != null && _lstResultXN.Count > 0) keyResultForHis = _lstResultXN[0].KeyResultForHis;

                foreach (var item in _lstServiceTest)
                {
                    _resultXN = new ResultXN();
                    _resultXN.PatientId = patientId;
                    _resultXN.TestCodeId = item.TestCodeId;
                    _resultXN.ServiceId = item.ServiceId;
                    _resultXN.UserInsertId = userInsertIdOrUpdateId;
                    _resultXN.ValidPrint = true;
                    _resultXN.Status = 0;
                    _resultXN.StatusResult = StatusForResult.NotResult;
                    _resultXN.Result = item.TestCode.IsTestHead ? "." : string.Empty;
                    _resultXN.InsertTime = dateTime;
                    _resultXN.DoctorId = doctorId;
                    _resultXN.KeyResultForHis = keyResultForHis;
                    _resultXN.Active = true;
                    await _db.ResultXNs.AddAsync(_resultXN);
                }
                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<ResultXN>> GetListResultXNByPatientId_ForValidPrint(long patientId)
        {
            try
            {
                return await _db.ResultXNs.Where(p => p.Active == true && p.PatientId == patientId && p.ValidPrint == true).OrderBy(p => p.Service.Category.PrintOrder).ThenBy(p => p.Service.PrintOrder).ThenBy(p => p.TestCode.PrintOrder).ToListAsync();
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<ResultXN>> GetListResultXNByPatientId(long id)
        {
            try
            {
                return await _db.ResultXNs.Where(p => p.Active == true && p.PatientId == id).OrderBy(p => p.Service.Category.PrintOrder).ThenBy(p => p.Service.PrintOrder).ThenBy(p => p.TestCode.PrintOrder).ToListAsync();
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<ResultXN>> GetListResultXNByPatientIdAndLastestResults(long id, string patientId)
        {
            try
            {
                // 0. Lấy thông tin bệnh nhân hiện tại
                var currentPatient = await _db.Patients
                    .Where(p => p.Active == true && p.Id == id)
                    .Select(p => new { p.Id, p.PatientId, p.Sex, p.InsertTime, p.Age })
                    .FirstOrDefaultAsync();

                if (currentPatient == null)
                    return new List<ResultXN>();

                // 1. Lấy danh sách kết quả XN hiện tại
                var currentResults = await _db.ResultXNs
                    .Where(p => p.Active == true && p.PatientId == id)
                    .OrderBy(p => p.Service.Category.PrintOrder)
                    .ThenBy(p => p.Service.PrintOrder)
                    .ThenBy(p => p.TestCode.PrintOrder)
                    .ToListAsync();

                if (currentResults == null || currentResults.Count == 0)
                    return new List<ResultXN>();

                // 2. Lấy lần khám trước (nếu có)
                var targetPatient = await _db.Patients
                    .Where(p => p.Active == true &&
                                p.PatientId == currentPatient.PatientId &&
                                p.Id != id &&
                                p.InsertTime < (currentPatient.InsertTime ?? DateTime.Now))
                    .OrderByDescending(p => p.InsertTime)
                    .FirstOrDefaultAsync();

                // 3. Mapping kết quả trước (nếu có)
                if (targetPatient != null)
                {
                    var previousResults = await _db.ResultXNs
                        .Where(p => p.Active == true && p.PatientId == targetPatient.Id)
                        .ToListAsync();

                    foreach (var currentItem in currentResults)
                    {
                        var previousItem = previousResults.FirstOrDefault(p => p.TestCodeId == currentItem.TestCodeId);
                        if (previousItem != null)
                        {
                            var previousResultText =
                                !string.IsNullOrEmpty(previousItem.Result) && !string.IsNullOrEmpty(previousItem.PosNeg)
                                    ? $"{previousItem.PosNeg} ({previousItem.Result})"
                                    : !string.IsNullOrEmpty(previousItem.Result)
                                        ? previousItem.Result
                                        : !string.IsNullOrEmpty(previousItem.PosNeg)
                                            ? previousItem.PosNeg
                                            : "";

                            var returnResultTimeText = targetPatient.ReturnResultTimeXN?.ToString("dd/MM/yyyy HH:mm") ?? "";

                            currentItem.SetPreviousResult(previousResultText);
                            currentItem.SetPreviousReturnResultTime(returnResultTimeText);
                            currentItem.SetPreviousStatus(previousItem.Status ?? 0);
                        }
                    }
                }

                // 4. Tính eGFR nếu có test Creatinin (676) và eGFR (677)
                var creatinineItem = currentResults.FirstOrDefault(x => x.TestCodeId == 676);
                var egfrItem = currentResults.FirstOrDefault(x => x.TestCodeId == 677);

                if (creatinineItem != null
                    && egfrItem != null
                    && await _resultEditUnlockBL.CanEditXNAsync(id, ToolBL.Get_DateNow()))
                {
                    // Parse kết quả creatinin (µmol/L)
                    double? ParseNumber(string s)
                    {
                        if (string.IsNullOrWhiteSpace(s)) return null;
                        var normalized = new string(s.Trim().Replace(',', '.')
                            .Where(ch => char.IsDigit(ch) || ch == '.' || ch == '-').ToArray());
                        return double.TryParse(normalized, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out var val) ? val : null;
                    }

                    var creatinineUmol = ParseNumber(creatinineItem.Result);
                    if (creatinineUmol.HasValue)
                    {
                        // Tính tuổi (từ cột Age)
                        int CalcAge(object ageField)
                        {
                            try
                            {
                                var dob = Convert.ToDateTime(ageField);
                                var today = DateTime.Today;
                                int ageYears = today.Year - dob.Year;
                                if (dob.Date > today.AddYears(-ageYears)) ageYears--;
                                return Math.Max(ageYears, 0);
                            }
                            catch { return 0; }
                        }

                        int age = CalcAge(currentPatient.Age);

                        bool isFemale = false;
                        var sex = (currentPatient.Sex ?? "").Trim().ToLower();
                        if (sex == "nữ" || sex == "nu" || sex == "female" || sex == "f")
                            isFemale = true;

                        // Gọi hàm tính eGFR
                        double egfr = EGFR_CKDEPI_2021_Umol(creatinineUmol.Value, age, isFemale);

                        // Gán kết quả eGFR
                        egfrItem.Result = egfr.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
                        egfrItem.Status = egfr < 60 ? 1 : 0;
                        egfrItem.UpdateTime = DateTime.Now;

                        // Update trực tiếp vào DB
                        _db.ResultXNs.Update(egfrItem);
                        await _db.SaveChangesAsync();
                        await _db.Entry(egfrItem).ReloadAsync();
                    }
                }

                return currentResults;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi xử lý eGFR: " + ex.Message);
                return null;
            }
        }

        public async Task<List<Search>> GetListSearchByPatientId(long patientId)
        {
            try
            {
                var lstSearch = new List<Search>();

                var lstSearchXN = (from result in _db.ResultXNs
                                   where result.PatientId == patientId && result.Active == true
                                   select new Search
                                   {
                                       PatientIdTable = result.PatientId,
                                       KeyResultForHis = result.KeyResultForHis,
                                       ServiceId = result.Service.Id,
                                       ServiceName = result.Service.Name,
                                       Status = result.Patient.WaitXN ? "Wait" : (result.Patient.ProcessXN ? "Process" : "Valid")
                                   }).Distinct().ToList();
                lstSearch.AddRange(lstSearchXN);

                var lstResultCDHA = await _db.ResultCDHAs.Where(p => p.Active == true && p.PatientId == patientId).ToListAsync();
                if (lstResultCDHA != null)
                {
                    foreach (var item in lstResultCDHA)
                    {
                        if (item.Service.Category.Code == "SA")
                        {
                            lstSearch.Add(new Search
                            {
                                KeyResultForHis = item.KeyResultForHis,
                                ServiceId = item.Service.Id,
                                ServiceName = item.Service.Name,
                                Status = item.Patient.WaitSA ? "Wait" : (item.Patient.ProcessSA ? "Process" : "Valid")
                            });
                        }
                        else if (item.Service.Category.Code == "SAT")
                        {
                            lstSearch.Add(new Search
                            {
                                KeyResultForHis = item.KeyResultForHis,
                                ServiceId = item.Service.Id,
                                ServiceName = item.Service.Name,
                                Status = item.Patient.WaitSAT ? "Wait" : (item.Patient.ProcessSAT ? "Process" : "Valid")
                            });
                        }
                        else if (item.Service.Category.Code == "DDT")
                        {
                            lstSearch.Add(new Search
                            {
                                KeyResultForHis = item.KeyResultForHis,
                                ServiceId = item.Service.Id,
                                ServiceName = item.Service.Name,
                                Status = item.Patient.WaitDDT ? "Wait" : (item.Patient.ProcessDDT ? "Process" : "Valid")
                            });
                        }
                        else if (item.Service.Category.Code == "XQ")
                        {
                            lstSearch.Add(new Search
                            {
                                KeyResultForHis = item.KeyResultForHis,
                                ServiceId = item.Service.Id,
                                ServiceName = item.Service.Name,
                                Status = item.Patient.WaitXQ ? "Wait" : (item.Patient.ProcessXQ ? "Process" : "Valid")
                            });
                        }
                        else if (item.Service.Category.Code == "NS")
                        {
                            lstSearch.Add(new Search
                            {
                                KeyResultForHis = item.KeyResultForHis,
                                ServiceId = item.Service.Id,
                                ServiceName = item.Service.Name,
                                Status = item.Patient.WaitNS ? "Wait" : (item.Patient.ProcessNS ? "Process" : "Valid")
                            });
                        }
                        else if (item.Service.Category.Code == "TDCN")
                        {
                            lstSearch.Add(new Search
                            {
                                KeyResultForHis = item.KeyResultForHis,
                                ServiceId = item.Service.Id,
                                ServiceName = item.Service.Name,
                                Status = item.Patient.WaitTDCN ? "Wait" : (item.Patient.ProcessTDCN ? "Process" : "Valid")
                            });
                        }
                    }
                }
                return lstSearch;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Hàm này dùng cho việc in từng dịch vụ xét nghiệm riêng lẻ ở màn hình Xem Kết Quả
        /// Get all test results (TestCode and Result) for a specific ServiceId and PatientId
        /// </summary>
        /// <param name="serviceId">The ServiceId to filter results</param>
        /// <param name="patientId">The PatientId to filter results</param>
        /// <returns>List of ResultXN containing TestCode and Result data</returns>
        public async Task<List<ResultXN>> GetResultsByServiceIdAndPatientId(long serviceId, long patientId)
        {
            try
            {
                return await _db.ResultXNs
                    .Where(p => p.Active == true &&
                               p.ServiceId == serviceId &&
                               p.PatientId == patientId &&
                               p.ValidPrint == true)
                    .Include(p => p.TestCode)
                    .Include(p => p.Service)
                    .ThenInclude(s => s.Category)
                    .Include(p => p.Patient)
                    .ThenInclude(pt => pt.Object)
                    .Include(p => p.Patient)
                    .ThenInclude(pt => pt.Doctor)
                    .Include(p => p.Patient)
                    .ThenInclude(pt => pt.Location)
                    .Include(p => p.Patient)
                    .ThenInclude(pt => pt.UserXN)
                    .OrderBy(p => p.Service.Category.PrintOrder)
                    .ThenBy(p => p.Service.PrintOrder)
                    .ThenBy(p => p.TestCode.PrintOrder)
                    .ToListAsync();
            }
            catch
            {
                return new List<ResultXN>();
            }
        }

        /// <summary>
        /// Hàm này dùng cho việc lấy kết quả xét nghiệm theo CategoryId và PatientId
        /// Get all test results grouped by Category for a specific PatientId
        /// </summary>
        /// <param name="categoryId">The CategoryId to filter results</param>
        /// <param name="patientId">The PatientId to filter results</param>
        /// <returns>List of ResultXN containing TestCode and Result data for the category</returns>
        public async Task<List<ResultXN>> GetResultsByCategoryIdAndPatientId(long categoryId, long patientId)
        {
            try
            {
                return await _db.ResultXNs
                    .Where(p => p.Active == true &&
                               p.Service.CategoryId == categoryId &&
                               p.PatientId == patientId &&
                               p.ValidPrint == true)
                    .Include(p => p.TestCode)
                    .Include(p => p.Service)
                    .ThenInclude(s => s.Category)
                    .Include(p => p.Patient)
                    .ThenInclude(pt => pt.Object)
                    .Include(p => p.Patient)
                    .ThenInclude(pt => pt.Doctor)
                    .Include(p => p.Patient)
                    .ThenInclude(pt => pt.Location)
                    .Include(p => p.Patient)
                    .ThenInclude(pt => pt.UserXN)
                    .OrderBy(p => p.Service.Category.PrintOrder)
                    .ThenBy(p => p.Service.PrintOrder)
                    .ThenBy(p => p.TestCode.PrintOrder)
                    .ToListAsync();
            }
            catch
            {
                return new List<ResultXN>();
            }
        }

        /// <summary>
        /// Hàm này dùng cho việc lấy danh sách kết quả xét nghiệm được nhóm theo Category
        /// Get search results grouped by Category for better organization
        /// </summary>
        /// <param name="patientId">The PatientId to filter results</param>
        /// <returns>List of grouped search results by category</returns>
        public async Task<List<object>> GetListSearchByPatientIdGroupedByCategory(long patientId)
        {
            try
            {
                var groupedResults = new List<object>();

                // Lấy tất cả kết quả XN và group theo Category
                var xnResults = await _db.ResultXNs
                    .Where(result => result.PatientId == patientId && result.Active == true)
                    .Include(r => r.Service)
                    .ThenInclude(s => s.Category)
                    .Include(r => r.TestCode)
                    .OrderBy(result => result.Service.Category.PrintOrder)
                    .ThenBy(result => result.Service.PrintOrder)
                    .ThenBy(result => result.TestCode.PrintOrder)
                    .Select(result => new
                    {
                        PatientIdTable = result.PatientId,
                        KeyResultForHis = result.KeyResultForHis,
                        ServiceId = result.Service.Id,
                        ServiceName = result.Service.Name,
                        CategoryId = result.Service.CategoryId,
                        CategoryName = result.Service.Category.Name,
                        CategoryCode = result.Service.Category.Code,
                        CategoryPrintOrder = result.Service.Category.PrintOrder,
                        ServicePrintOrder = result.Service.PrintOrder,
                        //GroupCLS = result.Service.Category.Group.Id,
                        Status = result.Patient.WaitXN ? "Wait" : (result.Patient.ProcessXN ? "Process" : "Valid")
                    })
                    .Distinct()
                    .ToListAsync();

                // Group theo Category và sort lại theo PrintOrder
                var xnGroupedByCategory = xnResults
                    .GroupBy(x => new { x.CategoryId, x.CategoryName, x.CategoryCode, x.CategoryPrintOrder })
                    .OrderBy(g => g.Key.CategoryPrintOrder)
                    .ToList();

                foreach (var categoryGroup in xnGroupedByCategory)
                {
                    // Sort services trong category theo ServicePrintOrder
                    var services = categoryGroup
                        .OrderBy(s => s.ServicePrintOrder)
                        .ToList();

                    groupedResults.Add(new
                    {
                        CategoryId = categoryGroup.Key.CategoryId,
                        CategoryName = categoryGroup.Key.CategoryName,
                        CategoryCode = categoryGroup.Key.CategoryCode,
                        IsCategory = true,
                        ServiceCount = services.Count,
                        Status = services.Any(s => s.Status == "Valid") ? "Valid" :
                                services.Any(s => s.Status == "Process") ? "Process" : "Wait",
                        ChildServices = services.Select(s => new
                        {
                            PatientIdTable = s.PatientIdTable,
                            ServiceId = s.ServiceId,
                            ServiceName = s.ServiceName,
                            Status = s.Status,
                            KeyResultForHis = s.KeyResultForHis
                        }).ToList()
                    });
                }

                // Thêm các dịch vụ CDHA với sorting đúng
                var lstResultCDHA = await _db.ResultCDHAs
                    .Where(p => p.Active == true && p.PatientId == patientId)
                    .Include(p => p.Service)
                    .ThenInclude(s => s.Category)
                    .OrderBy(p => p.Service.Category.PrintOrder)
                    .ThenBy(p => p.Service.PrintOrder)
                    .ToListAsync();

                if (lstResultCDHA != null)
                {
                    foreach (var item in lstResultCDHA)
                    {
                        string status = "";
                        if (item.Service.Category.Code == "SA")
                        {
                            status = item.Patient.WaitSA ? "Wait" : (item.Patient.ProcessSA ? "Process" : "Valid");
                        }
                        else if (item.Service.Category.Code == "SAT")
                        {
                            status = item.Patient.WaitSAT ? "Wait" : (item.Patient.ProcessSAT ? "Process" : "Valid");
                        }
                        else if (item.Service.Category.Code == "DDT")
                        {
                            status = item.Patient.WaitDDT ? "Wait" : (item.Patient.ProcessDDT ? "Process" : "Valid");
                        }
                        else if (item.Service.Category.Code == "XQ")
                        {
                            status = item.Patient.WaitXQ ? "Wait" : (item.Patient.ProcessXQ ? "Process" : "Valid");
                        }
                        else if (item.Service.Category.Code == "NS")
                        {
                            status = item.Patient.WaitNS ? "Wait" : (item.Patient.ProcessNS ? "Process" : "Valid");
                        }
                        else if (item.Service.Category.Code == "TDCN")
                        {
                            status = item.Patient.WaitTDCN ? "Wait" : (item.Patient.ProcessTDCN ? "Process" : "Valid");
                        }

                        groupedResults.Add(new
                        {
                            CategoryId = item.Service.CategoryId,
                            CategoryName = item.Service.Category.Name,
                            CategoryCode = item.Service.Category.Code,
                            IsCategory = false,
                            ServiceId = item.Service.Id,
                            ServiceName = item.Service.Name,
                            KeyResultForHis = item.KeyResultForHis,
                            Status = status,
                            PatientIdTable = patientId
                        });
                    }
                }

                return groupedResults;
            }
            catch
            {
                return new List<object>();
            }
        }

        public async Task<bool> ExitResultXN_Update_GetSampleTime(long patientId, DateTime getSampleTime)
        {
            try
            {
                var resultXN = await _db.ResultXNs.Where(p => p.Active == true && p.PatientId == patientId).FirstOrDefaultAsync();
                if (resultXN != null)
                {
                    resultXN.Patient.GetSampleTimeXN = getSampleTime;
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

        //public async Task<bool> Update(List<ResultXNModel> lstResult, bool isValidPrint, long? userInsertOrUpdate)
        //{
        //    try
        //    {
        //        foreach (var item in lstResult)
        //        {
        //            var _resultXN = await _db.ResultXNs.Where(p => p.Active == true && p.Id == item.id).FirstOrDefaultAsync();
        //            if (_resultXN == null) continue;
        //            _resultXN.Status = item.status;
        //            _resultXN.Result = item.result;
        //            _resultXN.PosNeg = null;
        //            _resultXN.Note = !String.IsNullOrEmpty(item.note) ? item.note : null;
        //            if (isValidPrint)
        //            {
        //                _resultXN.ValidPrint = (item.validPrint && !string.IsNullOrEmpty(item.result)) ? true : false;
        //                _resultXN.ValidTime = DateTime.Now;
        //            }
        //            else
        //            {
        //                _resultXN.ValidPrint = item.validPrint;
        //                _resultXN.ValidTime = DateTime.Now;

        //            }
        //            _resultXN.UserUpdateId = userInsertOrUpdate;
        //        }
        //        await _db.SaveChangesAsync();
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        public async Task<bool> Update(List<ResultXNModel> lstResult, bool isValidPrint, long? userInsertOrUpdate)
        {
            try
            {
                if (lstResult == null || lstResult.Count == 0) return true;

                // Lấy trước tất cả các Id cần xử lý để tránh N+1
                var ids = lstResult.Select(x => x.id).Distinct().ToList();

                var dict = await _db.ResultXNs
                    .Where(p => p.Active == true && ids.Contains(p.Id))
                    .ToDictionaryAsync(x => x.Id);

                var patientIds = dict.Values
                    .Where(x => x.PatientId.HasValue)
                    .Select(x => x.PatientId!.Value)
                    .Distinct()
                    .ToList();

                // XN mở theo cả lần làm (Patient.Id), không mở theo từng dòng ResultXN.
                var now = ToolBL.Get_DateNow();
                foreach (var patientId in patientIds)
                {
                    if (!await _resultEditUnlockBL.CanEditXNAsync(patientId, now))
                        return false;
                }

                var anyChanged = false;

                foreach (var item in lstResult)
                {
                    if (!dict.TryGetValue(item.id, out var r)) continue;

                    bool rowChanged = false;

                    // 1) ƯU TIÊN: cập nhật NOTE cho TẤT CẢ các dòng (không phụ thuộc thay đổi kết quả)
                    var newNote = string.IsNullOrWhiteSpace(item.note) ? null : item.note.Trim();
                    if (!string.Equals(r.Note, newNote, StringComparison.Ordinal))
                    {
                        r.Note = newNote;
                        rowChanged = true;
                    }

                    // 2) ƯU TIÊN: cập nhật VALIDPRINT / VALIDTIME theo logic hiện có
                    bool newValidPrint = r.ValidPrint; // mặc định giữ nguyên
                    if (isValidPrint)
                    {
                        newValidPrint = (item.validPrint && !string.IsNullOrWhiteSpace(item.result));
                    }
                    else
                    {
                        newValidPrint = item.validPrint;
                    }

                    if (r.ValidPrint != newValidPrint)
                    {
                        r.ValidPrint = newValidPrint;
                        r.ValidTime = now;
                        rowChanged = true;
                    }

                    // Chuẩn hoá để so sánh Result
                    var current = (r.Result ?? string.Empty).Trim();
                    var incoming = (item.result ?? string.Empty).Trim();

                    // Chuẩn hoá để so sánh Status
                    var currentStatus = r.Status;
                    var incomingStatus = item.status;

                    // Chỉ set ValidPrint/ValidTime khi có thay đổi (theo yêu cầu gắn với việc đổi kết quả)
                    //if (isValidPrint)
                    //{
                    //    r.ValidPrint = (item.validPrint && !string.IsNullOrWhiteSpace(item.result));
                    //    r.ValidTime = now;
                    //}
                    //else
                    //{
                    //    // Nếu không bật chế độ "validate print cưỡng bức",
                    //    // vẫn cập nhật theo input nhưng CHỈ khi có thay đổi Result.
                    //    r.ValidPrint = item.validPrint;
                    //    r.ValidTime = now;
                    //}

                    // Nếu kết quả không đổi -> BỎ QUA hoàn toàn dòng này
                    if (!string.Equals(current, incoming, StringComparison.Ordinal) || currentStatus != incomingStatus)
                    {
                        r.Result = item.result;
                        r.Status = item.status;
                        r.PosNeg = null;

                        r.UserUpdateId = userInsertOrUpdate; // user cập nhật cho dòng này
                        rowChanged = true;
                    }
                    if (rowChanged) anyChanged = true;
                }

                if (anyChanged)
                    await _db.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> FullResultXN(long patientId)
        {
            var _item = await _db.ResultXNs.Where(p => p.Active == true && p.PatientId == patientId && string.IsNullOrEmpty(p.Result.Trim()) && string.IsNullOrEmpty(p.PosNeg.Trim())).FirstOrDefaultAsync();
            if (_item != null)
                return false;
            else
                return true;
        }

        public async Task<bool> NotFullResultXN(long patientId)
        {
            var _item = await _db.ResultXNs.Where(p => p.Active == true && p.PatientId == patientId && string.IsNullOrEmpty(p.Result.Trim()) && string.IsNullOrEmpty(p.PosNeg.Trim())).FirstOrDefaultAsync();
            if (_item != null)
                return true;
            else
                return false;
        }

        public async Task<bool> Delete(string ticketItemId)
        {
            try
            {
                var _lstResultXN = await _db.ResultXNs.Where(p => p.Active == true && p.TicketItemId == ticketItemId && p.Patient.WaitXN == true).ToListAsync();
                if (_lstResultXN != null)
                {
                    foreach (var _item in _lstResultXN)
                    {
                        _item.Active = false;
                    }
                    await _db.SaveChangesAsync();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateSignStoreId_CKS(
            long resultXNId,
            long signStoreId,
            long userInsertIdOrUpdateId,
            DateTime dateTime)
        {
            try
            {
                if (resultXNId <= 0 || signStoreId <= 0)
                    return false;

                // Tìm record
                var resultXNItem = await _db.ResultXNs
                    .Where(p => p.Active == true && p.Id == resultXNId)
                    .FirstOrDefaultAsync();

                if (resultXNItem == null)
                    return false;

                // Gán giá trị mới
                resultXNItem.SignStoreId = signStoreId;
                resultXNItem.UserUpdateId = userInsertIdOrUpdateId;
                resultXNItem.UpdateTime = dateTime;
                resultXNItem.SignStatus = 1;

                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Cập nhật kết quả đọc từ PDF vào các ResultXN đã được chỉ định trước.
        /// Chỉ cho phép TestCode.Code kết thúc bằng '_taman' (không phân biệt bệnh viện).
        /// Không tạo ResultXN mới. Mã không thấy, giá trị trống/trùng/không rõ sẽ bỏ qua.
        /// </summary>
        public async Task<int> UpdateFromExternal(
            long patientId,
            IEnumerable<Models.ExternalLabItem> items,
            bool overwrite,
            bool markValid,
            long? userId)
        {
            if (patientId <= 0 || items == null) return 0;
            if (!await _resultEditUnlockBL.CanEditXNAsync(patientId, ToolBL.Get_DateNow()))
                return 0;

            var patientResults = await _db.ResultXNs
                .Where(r => r.Active == true && r.PatientId == patientId)
                .ToListAsync();
            if (patientResults.Count == 0) return 0;

            // Giữ gate bằng _taman theo chủ đích: chỉ mã do IT kích hoạt mới được cập nhật.
            const string suffix = "_taman";
            var allTestCodes = await _db.TestCodes.ToListAsync();
            var eligibleCodes = allTestCodes
                .Where(t => t != null && !t.IsTestHead &&
                            !string.IsNullOrWhiteSpace(t.Code) &&
                            t.Code.Trim().EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                .GroupBy(t => NormalizeExternalCodeForImport(t.Code), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

            // Không chọn dòng đầu tiên khi có nhiều mã cùng code hoặc nhiều kết quả
            // của một mã trong PDF. Để người dùng kiểm tra thủ công trường hợp nhập nhằng.
            var groups = items
                .Where(x => x != null &&
                            !string.IsNullOrWhiteSpace(x.Code) &&
                            !string.IsNullOrWhiteSpace(x.Value))
                .GroupBy(x => NormalizeExternalCodeForImport(x.Code), StringComparer.OrdinalIgnoreCase)
                .ToList();

            int changed = 0;
            var now = ToolBL.Get_DateNow();
            foreach (var group in groups)
            {
                if (string.IsNullOrEmpty(group.Key)) continue;
                if (!eligibleCodes.TryGetValue(group.Key, out var matchingCodes) || matchingCodes.Count != 1)
                    continue;

                var values = group.Select(x => x.Value.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                if (values.Count != 1) continue;

                var testCode = matchingCodes[0];
                var rows = patientResults.Where(r => r.TestCodeId == testCode.Id).ToList();
                if (rows.Count != 1) continue;

                // Parse chặt: không biến '4-6' thành '4', không lấy nhầm số từ chữ.
                SplitValueAndQualifier(values[0], out var result, out var numericValue, out var posNeg);
                if (string.IsNullOrWhiteSpace(result) && string.IsNullOrWhiteSpace(posNeg)) continue;

                var row = rows[0];
                // Không ghi đè kết quả đã ký số. Muốn sửa phải đi qua flow Invalid.
                if (row.SignStoreId > 0 && row.SignStatus == 1) continue;

                bool isEmpty = string.IsNullOrWhiteSpace(row.Result) &&
                               string.IsNullOrWhiteSpace(row.PosNeg);
                if (!overwrite && !isEmpty) continue;

                string nextResult = result ?? string.Empty;
                string nextPosNeg = posNeg ?? string.Empty;
                int? nextStatus = row.Status;

                if (!string.IsNullOrWhiteSpace(nextPosNeg) &&
                    !string.IsNullOrWhiteSpace(testCode.NormalResult))
                {
                    var normalizedNormal = NormalizeImportedQualifier(testCode.NormalResult);
                    nextStatus = string.Equals(nextPosNeg, normalizedNormal,
                        StringComparison.OrdinalIgnoreCase) ? 0 : 2;
                }
                else if (!double.IsNaN(numericValue) && !double.IsInfinity(numericValue))
                {
                    if (testCode.LowerLimit.HasValue && numericValue < testCode.LowerLimit.Value)
                        nextStatus = 1;
                    else if (testCode.HigherLimit.HasValue && numericValue > testCode.HigherLimit.Value)
                        nextStatus = 2;
                    else if (testCode.LowerLimit.HasValue || testCode.HigherLimit.HasValue)
                        nextStatus = 0;
                }

                bool nextValid = markValid
                    ? (!string.IsNullOrWhiteSpace(nextResult) || !string.IsNullOrWhiteSpace(nextPosNeg))
                    : row.ValidPrint;

                if (string.Equals(row.Result ?? string.Empty, nextResult, StringComparison.Ordinal) &&
                    string.Equals(row.PosNeg ?? string.Empty, nextPosNeg, StringComparison.Ordinal) &&
                    row.Status == nextStatus && row.ValidPrint == nextValid)
                    continue;

                row.Result = nextResult;
                row.PosNeg = nextPosNeg;  // Xóa giá trị định tính cũ nếu PDF mới chỉ có số.
                row.Status = nextStatus;
                row.ValidPrint = nextValid;
                row.UserUpdateId = userId;
                row.UpdateTime = now;
                changed++;
            }

            if (changed > 0) await _db.SaveChangesAsync();
            return changed;
        }

        private static string NormalizeExternalCodeForImport(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return string.Empty;
            code = code.Trim();
            if (code.EndsWith("_taman", StringComparison.OrdinalIgnoreCase))
                code = code.Substring(0, code.Length - "_taman".Length);
            code = code.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder();
            foreach (char c in code)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
                if (c == 'đ' || c == 'Đ') { sb.Append('d'); continue; }
                if (char.IsLetterOrDigit(c)) sb.Append(char.ToLowerInvariant(c));
            }
            return sb.ToString();
        }

        private static string NormalizeImportedQualifier(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            var v = value.Trim().ToLowerInvariant();
            return v switch
            {
                "âm tính" => "Negative",
                "âm" => "Negative",
                "am tinh" => "Negative",
                "negative" => "Negative",
                "dương tính" => "Positive",
                "dương" => "Positive",
                "duong tinh" => "Positive",
                "positive" => "Positive",
                _ => value.Trim()
            };
        }

        public async Task<ResultXN> GetResultXNByKeyResultForHis(string keyResultForHis)
        {
            try
            {
                return await _db.ResultXNs
                    .Include(r => r.Patient)
                    .Where(r => r.Active == true && r.KeyResultForHis == keyResultForHis)
                    .FirstOrDefaultAsync();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Tách phần định lượng (số) và định tính (Positive|Negative|Âm|Dương) từ chuỗi giá trị.
        /// Ví dụ: "0.41 Negative" -> result="0.41", posneg="Negative"
        /// </summary>
        private static void SplitValueAndQualifier(
            string input, out string? result, out double resultValue, out string posneg)
        {
            result = null;
            resultValue = double.NaN;
            posneg = string.Empty;
            if (string.IsNullOrWhiteSpace(input)) return;
            var raw = Regex.Replace(input.Trim(), @"\s+", " ");

            // Một kết quả chỉ có định tính: không lưu 'Âm tính' vào Result.
            var qualifierOnly = NormalizeImportedQualifier(raw);
            if (qualifierOnly == "Positive" || qualifierOnly == "Negative")
            {
                posneg = qualifierOnly;
                return;
            }

            // Định lượng + định tính ở cuối, ví dụ '0.41 Negative'.
            var mixed = Regex.Match(raw,
                @"^(?<num>[<>≤≥=]?\s*[+-]?\d+(?:[.,]\d+)?(?:[eE][+-]?\d+)?)\s+(?<qual>Positive|Negative|âm\s*tính|dương\s*tính|âm|dương)$",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (mixed.Success)
            {
                posneg = NormalizeImportedQualifier(mixed.Groups["qual"].Value);
                raw = mixed.Groups["num"].Value.Trim();
            }

            // Chỉ bỏ cờ (H)/(L) cuối giá trị nếu cả ô là số.
            // Giữ nguyên kết quả dạng khoảng '4-6', kết quả chữ, đơn vị phức tạp.
            var numeric = Regex.Match(raw,
                @"^(?<op>>=|<=|[><≤≥=])?\s*(?<num>[+-]?\d+(?:[.,]\d+)?(?:[eE][+-]?\d+)?)\s*(?:\((?:H|L|CV)\))?$",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (numeric.Success)
            {
                var op = numeric.Groups["op"].Value.Replace("≥", ">=").Replace("≤", "<=");
                var n = numeric.Groups["num"].Value.Replace(',', '.');
                if (double.TryParse(n, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
                    resultValue = parsed;
                result = string.IsNullOrEmpty(op) ? n : op + " " + n;
                return;
            }

            // Fail-safe: lưu nguyên văn vào Result cho user kiểm tra; KHÔNG tự ý
            // suy ra số để tính trạng thái (ví dụ '4-6' không biến thành '4').
            result = raw;
        }

        private static string NormalizePosNeg(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return "";
            token = token.Trim().ToLowerInvariant();

            // Chuẩn hoá về tiếng Anh để thống nhất lưu trữ
            return token switch
            {
                "positive" => "Positive",
                "dương" => "Positive",
                "duong" => "Positive",
                "negative" => "Negative",
                "âm" => "Negative",
                "am" => "Negative",
                _ => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(token)
            };
        }

        public string Format_Decimal(double? result)
        {
            try
            {
                var stringResult = result?.ToString("0.0##", GetCultureInfo());
                return stringResult;
            }
            catch (Exception ex)
            {
                return result.ToString();
            }
        }
        public CultureInfo GetCultureInfo()
        {
            CultureInfo cultureInfo = new CultureInfo("en-US");
            cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
            return cultureInfo;
        }

        // ============================================
        // 🔹 Hàm tính eGFR CKD-EPI 2021
        // ============================================
        public static double EGFR_CKDEPI_2021_Umol(double creatinineUmolL, int age, bool isFemale)
        {
            double scr = creatinineUmolL / 88.4; // µmol/L → mg/dL
            double kappa = isFemale ? 0.7 : 0.9;
            double alpha = isFemale ? -0.241 : -0.302;
            double femaleFactor = isFemale ? 1.012 : 1.0;

            double ratio = scr / kappa;
            double minPart = Math.Min(ratio, 1.0);
            double maxPart = Math.Max(ratio, 1.0);

            double egfr = 142.0
                          * Math.Pow(minPart, alpha)
                          * Math.Pow(maxPart, -1.200)
                          * Math.Pow(0.9938, age)
                          * femaleFactor;

            return Math.Round(egfr, 2);
        }

        public async Task UpdateUrineHeadValidPrint(
            long patientId,
            long urineServiceId)
        {
            var urineHeadResults = await _db.ResultXNs
                .Where(x =>
                    x.PatientId == patientId &&
                    x.ServiceId == urineServiceId &&
                    x.TestCode != null &&
                    x.TestCode.IsTestHead == true)
                .ToListAsync();

            if (!urineHeadResults.Any())
            {
                return;
            }

            foreach (var resultXN in urineHeadResults)
            {
                // false tương ứng với giá trị 0 trong SQL Server.
                resultXN.ValidPrint = false;
            }

            await _db.SaveChangesAsync();
        }
    }
}
