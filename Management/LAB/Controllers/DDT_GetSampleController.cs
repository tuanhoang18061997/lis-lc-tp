using Management.BL;
using Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;
using Object = Management.Models.Object;

namespace Management.Controllers
{
    public class DDT_GetSampleController : Controller
    {
        public readonly ILogger<DDT_GetSampleController> _logger;
        public readonly PatientCDHABL _patientCDHABL;
        public readonly ObjectBL _objectBL;
        public readonly LocationBL _locationBL;
        public readonly DoctorBL _doctorBL;
        public readonly UserBL _userBL;
        public readonly CategoryBL _categoryBL;
        public readonly ServiceBL _serviceBL;
        public readonly ResultCDHABL _resultCDHABL;
        public readonly SettingBL _settingBL;
        public readonly GroupBL _groupBL;
        public readonly string _DDT = "DDT";
        private readonly ExternalFileBL _externalFileBL;
        public readonly IWebHostEnvironment _environment;

        public DDT_GetSampleController(ILogger<DDT_GetSampleController> logger, PatientCDHABL patientCDHABL, ObjectBL objectBL, LocationBL locationBL,
                            DoctorBL doctorBL, UserBL userBL, CategoryBL categoryBL, ServiceBL serviceBL, ResultCDHABL resultCDHABL, SettingBL settingBL, GroupBL groupBL, DeviceBL deviceBL, ExternalFileBL externalFileBL, IWebHostEnvironment environment)
        {
            _logger = logger;
            _patientCDHABL = patientCDHABL;
            _objectBL = objectBL;
            _locationBL = locationBL;
            _doctorBL = doctorBL;
            _userBL = userBL;
            _categoryBL = categoryBL;
            _serviceBL = serviceBL;
            _resultCDHABL = resultCDHABL;
            _settingBL = settingBL;
            _groupBL = groupBL;
            _environment = environment;
            _externalFileBL = externalFileBL;

        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetSample()
        {
            var dateTimeNow = ToolBL.Get_DateNow();
            var defaultFrom = new DateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, 00, 00, 00);
            var defaultTo = new DateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, 23, 59, 59);

            // Try to get saved dates from session/cookies first
            var savedFromStr = HttpContext.Request.Cookies[SessionKeyModel._sessionTimeSearchFrom];
            var savedToStr = HttpContext.Request.Cookies[SessionKeyModel._sessionTimeSearchTo];

            DateTime from = defaultFrom;
            DateTime to = defaultTo;

            if (!string.IsNullOrEmpty(savedFromStr) && DateTime.TryParse(savedFromStr, out var savedFrom))
            {
                // Ensure time is set to 00:00:00 for the from date
                from = new DateTime(savedFrom.Year, savedFrom.Month, savedFrom.Day, 00, 00, 00);
            }
            if (!string.IsNullOrEmpty(savedToStr) && DateTime.TryParse(savedToStr, out var savedTo))
            {
                // Ensure time is set to 23:59:59 for the to date
                to = new DateTime(savedTo.Year, savedTo.Month, savedTo.Day, 23, 59, 59);
            }

            ViewData["lstPatient"] = await _patientCDHABL.Get_ListPatient(from, to, true, false, false, _DDT);
            ViewData["lstObject"] = await _objectBL.GetListObject();
            ViewData["lstLocation"] = await _locationBL.GetListLocation();
            ViewData["lstDoctor"] = await _doctorBL.GetList();
            ViewData["lstSexModel"] = await SexModel.GetListSexModel();
            ViewData["lstBenhAnModel"] = await BenhAnModel.GetListBenhAnModel();
            ViewData["lstService"] = await _serviceBL.GetListServiceByCategory(_DDT);

            // Pass the selected dates to the view
            ViewData["timeSearchFrom"] = from.ToString("yyyy-MM-dd");
            ViewData["timeSearchTo"] = to.ToString("yyyy-MM-dd");

            await this.Get_Count();

            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get_Count()
        {
            var dateTimeNow = ToolBL.Get_DateNow();
            var defaultFrom = new DateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, 00, 00, 00);
            var defaultTo = new DateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, 23, 59, 59);

            // Try to get saved dates from session/cookies first
            var savedFromStr = HttpContext.Request.Cookies[SessionKeyModel._sessionTimeSearchFrom];
            var savedToStr = HttpContext.Request.Cookies[SessionKeyModel._sessionTimeSearchTo];

            DateTime from = defaultFrom;
            DateTime to = defaultTo;

            if (!string.IsNullOrEmpty(savedFromStr) && DateTime.TryParse(savedFromStr, out var savedFrom))
            {
                // Ensure time is set to 00:00:00 for the from date
                from = new DateTime(savedFrom.Year, savedFrom.Month, savedFrom.Day, 00, 00, 00);
            }
            if (!string.IsNullOrEmpty(savedToStr) && DateTime.TryParse(savedToStr, out var savedTo))
            {
                // Ensure time is set to 23:59:59 for the to date
                to = new DateTime(savedTo.Year, savedTo.Month, savedTo.Day, 23, 59, 59);
            }
            SaveSearchDatesToSession(from, to);


            var countGetSample = await _patientCDHABL.Get_CountPatient(from, to, true, false, false, _DDT);
            var countProcess = await _patientCDHABL.Get_CountPatient(from, to, false, true, false, _DDT);
            var countReturnResult = await _patientCDHABL.Get_CountPatient(from, to, false, false, true, _DDT);
            ViewData["countGetSample"] = countGetSample;
            ViewData["countProcess"] = countProcess;
            ViewData["countReturnResult"] = countReturnResult;

            return Content(countGetSample + ";" + countProcess + ";" + countReturnResult);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Refresh()
        {
            var dateTimeNow = ToolBL.Get_DateNow();
            var defaultFrom = new DateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, 00, 00, 00);
            var defaultTo = new DateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, 23, 59, 59);

            // Try to get saved dates from session/cookies first
            var savedFromStr = HttpContext.Request.Cookies[SessionKeyModel._sessionTimeSearchFrom];
            var savedToStr = HttpContext.Request.Cookies[SessionKeyModel._sessionTimeSearchTo];

            DateTime from = defaultFrom;
            DateTime to = defaultTo;

            if (!string.IsNullOrEmpty(savedFromStr) && DateTime.TryParse(savedFromStr, out var savedFrom))
            {
                // Ensure time is set to 00:00:00 for the from date
                from = new DateTime(savedFrom.Year, savedFrom.Month, savedFrom.Day, 00, 00, 00);
            }
            if (!string.IsNullOrEmpty(savedToStr) && DateTime.TryParse(savedToStr, out var savedTo))
            {
                // Ensure time is set to 23:59:59 for the to date
                to = new DateTime(savedTo.Year, savedTo.Month, savedTo.Day, 23, 59, 59);
            }
            SaveSearchDatesToSession(from, to);

            ViewData["lstPatient"] = await _patientCDHABL.Get_ListPatientByPidOrSid(from, to, true, false, false, null, _DDT);

            return PartialView("_DDT_GetSample_ListPatient");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Search(string pidorseq, DateTime timeSearchFrom, DateTime timeSearchTo)
        {
            // Save selected dates to session/cookies
            SaveSearchDatesToSession(timeSearchFrom, timeSearchTo);

            var from = new DateTime(timeSearchFrom.Year, timeSearchFrom.Month, timeSearchFrom.Day, 00, 00, 00);
            var to = new DateTime(timeSearchTo.Year, timeSearchTo.Month, timeSearchTo.Day, 23, 59, 59);
            ViewData["lstPatient"] = await _patientCDHABL.Get_ListPatientByPidOrSid(from, to, true, false, false, pidorseq, _DDT);

            return PartialView("_DDT_GetSample_ListPatient");
        }

        [HttpPost]
        [Authorize]
        public IActionResult SaveSearchDates(DateTime timeSearchFrom, DateTime timeSearchTo)
        {
            SaveSearchDatesToSession(timeSearchFrom, timeSearchTo);
            return Json(new { success = true });
        }

        // Helper method to save dates to session
        private void SaveSearchDatesToSession(DateTime timeSearchFrom, DateTime timeSearchTo)
        {
            try
            {
                CookieOptions cookieOptions = new CookieOptions()
                {
                    Expires = new DateTimeOffset(DateTime.Now.AddDays(1)) // Keep for 30 days
                };

                HttpContext.Response.Cookies.Append(SessionKeyModel._sessionTimeSearchFrom,
                    timeSearchFrom.ToString("yyyy-MM-dd"), cookieOptions);
                HttpContext.Response.Cookies.Append(SessionKeyModel._sessionTimeSearchTo,
                    timeSearchTo.ToString("yyyy-MM-dd"), cookieOptions);
            }
            catch { }
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetPatientInfo(long id)
        {
            ViewData["lstObject"] = await _objectBL.GetListObject();
            ViewData["lstLocation"] = await _locationBL.GetListLocation();
            ViewData["lstDoctor"] = await _doctorBL.GetList();
            ViewData["lstSexModel"] = await SexModel.GetListSexModel();
            ViewData["lstBenhAnModel"] = await BenhAnModel.GetListBenhAnModel();
            ViewData["patientInfo"] = await _patientCDHABL.Get_PatientBySid(id);

            return PartialView("_DDT_GetSample_PatientInfo");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetServiceForPatient(long patientId)
        {
            ViewData["lstResultCDHAs"] = await _resultCDHABL.GetListResultCDHAByPatientId(patientId, _DDT);
            ViewData["lstService"] = await _serviceBL.GetListServiceByCategory(_DDT);
            return PartialView("_DDT_GetSample_ListService");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeletePatientAndService(long id)
        {
            var _status = string.Empty;
            var _userLogin = this.GetUserLogin();
            if (await _resultCDHABL.DeleteByPatientId(id, _userLogin.Value))
            {
                _status = await _patientCDHABL.DeleteById(id, _DDT) ? "/DDT_GetSample/Refresh/" : string.Empty;
            }

            return Content(_status);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetSID(string seq)
        {
            var _sid = string.Empty;
            if (!string.IsNullOrEmpty(seq))
            {
                var _dateTime = ToolBL.Get_DateNow();
                _sid = _dateTime.Day.ToString().PadLeft(2, '0') + _dateTime.Month.ToString().PadLeft(2, '0') + _dateTime.Year.ToString().Substring(2, 2) + "-" + seq;
            }
            return Content(_sid);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SavePatient(string id, string patientId, string seq, string sid, string patientName, DateTime age,
                                                               string sex, string obj, string type, string location, string doctor, DateTime getSampleTime,
                                                               string address, string diagnostic, string service)
        {
            var _dateTime = ToolBL.Get_DateNow();
            if (string.IsNullOrEmpty(seq) && string.IsNullOrEmpty(sid))
            {
                seq = await _settingBL.GetSeq();
                sid = _dateTime.Day.ToString().PadLeft(2, '0') + _dateTime.Month.ToString().PadLeft(2, '0') + _dateTime.Year.ToString().Substring(2, 2) + "-" + seq;
            }
            var _userLogin = this.GetUserLogin();
            long? _id = string.IsNullOrEmpty(id) ? null : long.Parse(id);
            var _patient = await _patientCDHABL.SaveOrUpdate(_id, patientId, seq, sid, patientName, age, sex, obj, type, location, doctor, getSampleTime, address, diagnostic, _userLogin.Value, _dateTime, true, false, false, _DDT);

            return Content(_patient == null ? string.Empty : _patient.Id.ToString());
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddServiceForPatient(long patientId, long serviceId, long doctorId)
        {
            var _userLogin = this.GetUserLogin();
            var _dateTime = ToolBL.Get_DateNow();
            var _save = await _resultCDHABL.SaveService(patientId, serviceId, _userLogin.Value, _dateTime, doctorId, _DDT.ToLower());

            return Content(_save.ToString());
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteServiceForPatient(long id)
        {
            var _userLogin = this.GetUserLogin();
            var _delete = await _resultCDHABL.DeleteById(id, _userLogin.Value);
            return Content(_delete.ToString());
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ProcessResult(long id, DateTime getSampleTime)
        {
            var _exitResultCDHA = await _resultCDHABL.ExitResultXN_Update_GetSampleTime(id, getSampleTime, _DDT);
            var _getSample = false;
            if (_exitResultCDHA)
            {
                var _userLogin = this.GetUserLogin();
                _getSample = await _patientCDHABL.GetSample_ProcessResult_ReturnResult(id, false, true, false, _userLogin.Value, _DDT);
            }
            return Content(_getSample.ToString());
        }

        public long? GetUserLogin()
        {
            long? userId = null;
            try
            {
                userId = long.Parse(HttpContext?.User?.Claims?.FirstOrDefault(p => p.Type == "UserId")?.Value);
            }
            catch { }
            return userId;
        }

        // ========================= IMPORT EXTERNAL RESULT PDF =========================
        // Yêu cầu NuGet: UglyToad.PdfPig (để trích text PDF)
        // using UglyToad.PdfPig;
        // using UglyToad.PdfPig.Content;

        [HttpPost]
        [Authorize]
        [DisableRequestSizeLimit] // tùy cân nhắc
        public async Task<IActionResult> ImportExternalResultPdf(string pMaBenhAn, string pId, long pidTablePatient, string pName, IFormFile file,
            bool overwrite = false, bool markValid = false, bool isVisibleToUser = true)
        {
            if (pMaBenhAn == null) return Json(new { success = false, message = "Thiếu pMaBenhAn." });
            if (file == null || file.Length == 0) return Json(new { success = false, message = "Không có tệp PDF được gửi lên." });

            try
            {
                // 1) Lưu file theo cấu trúc: uploads/external_result_cdha/{pId}/{pMaBenhAn}/file
                var sanitizedPId = SanitizeFilePart(pId) ?? "unknown";
                var sanitizedMaBenhAn = SanitizeFilePart(pMaBenhAn) ?? "unknown";
                var tmpFolder = Path.Combine(_environment.WebRootPath, "uploads", "external_result_cdha", sanitizedPId, sanitizedMaBenhAn);
                if (!Directory.Exists(tmpFolder)) Directory.CreateDirectory(tmpFolder);

                // Format pName: bỏ dấu và khoảng trắng
                var sanitizedName = RemoveVietnameseAccentsAndSpaces(pName);
                var fileName = $"patient_{pMaBenhAn}_{sanitizedName}_{DateTime.Now:yyyyMMddHHmmss}.pdf";

                //var fileName = $"patient_{pMaBenhAn}_{pName}_{DateTime.Now:yyyyMMddHHmmss}.pdf";

                var tempPath = Path.Combine(tmpFolder, fileName);
                using (var fs = new FileStream(tempPath, FileMode.Create))
                {
                    await file.CopyToAsync(fs);
                }

                var userLogin = GetUserLogin();

                // 2) Lưu metadata file vào ExternalFiles
                var relativePath = _externalFileBL.BuildRelativePath(
                    rootFolderName: "external_result_cdha",
                    pId: sanitizedPId,
                    maBenhAn: sanitizedMaBenhAn,
                    storedFileName: fileName
                );

                await _externalFileBL.EnsureFileSyncedAsync(
                    storedFileName: fileName,
                    relativePath: relativePath,
                    pId: pId,
                    maBenhAn: pMaBenhAn,
                    tablePatientId: pidTablePatient,
                    moduleType: ExternalFileModule.CDHA,
                    patientName: pName,
                    createdBy: userLogin,
                    isVisibleToUser: isVisibleToUser
                );
                int updatedCount = 0;
                bool parsedSuccess = false;

                return Json(new
                {
                    success = true,
                    count = updatedCount,
                    parsedSuccess = parsedSuccess,
                    message = parsedSuccess
                        ? "Import file thành công."
                        : "Import file thành công."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ImportExternalResultPdf failed");
                return Json(new { success = false, message = "Lỗi xử lý import." });
            }
        }

        #region ============== Helpers For ImportExternalLabPdf =============
        private static string RemoveVietnameseAccentsAndSpaces(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            // Bảng chuyển đổi ký tự có dấu sang không dấu
            var vietnameseChars = new[]
            {
                "áàảãạăắằẳẵặâấầẩẫậ",
                "éèẻẽẹêếềểễệ",
                "íìỉĩị",
                "óòỏõọôốồổỗộơớờởỡợ",
                "úùủũụưứừửữự",
                "ýỳỷỹỵ",
                "đ"
            };

            var replacementChars = new[] { 'a', 'e', 'i', 'o', 'u', 'y', 'd' };

            text = text.ToLower();

            for (int i = 0; i < vietnameseChars.Length; i++)
            {
                foreach (char c in vietnameseChars[i])
                {
                    text = text.Replace(c, replacementChars[i]);
                }
            }

            // Bỏ khoảng trắng và các ký tự đặc biệt, chỉ giữ chữ cái và số
            var cleaned = new string(text.Where(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_').ToArray());

            return string.IsNullOrEmpty(cleaned) ? "unknown" : cleaned;
        }
        private static string SanitizeFilePart(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            // bỏ ký tự lạ để tránh lỗi tên file/url
            var cleaned = new string(s.Where(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_').ToArray());
            return string.IsNullOrEmpty(cleaned) ? null : cleaned;
        }

        private static string ExtractTextFromPdf(string path)
        {
            using (var doc = UglyToad.PdfPig.PdfDocument.Open(path))
            {
                var sb = new System.Text.StringBuilder();
                foreach (var page in doc.GetPages())
                {
                    foreach (var w in page.GetWords())
                    {
                        sb.Append(w.Text);
                        sb.Append(' ');
                    }
                    sb.AppendLine();
                }
                return sb.ToString();
            }
        }
        #endregion
        #region Lấy danh sách file pdf đã import
        //[HttpGet]
        //public IActionResult GetExternalResultFiles(string pMaBenhAn, string pId, long patientIdTable) // patientId để kiểu long là sai nhưng để vày để load những ca đã import sai trước đó lên. Chủ yếu dùng pMaBenhAn
        //{
        //    if (string.IsNullOrWhiteSpace(pMaBenhAn))
        //        return Json(new { success = false, message = "Thiếu pMaBenhAn." });

        //    var sanitizedPId = SanitizeFilePart(pId) ?? "unknown";
        //    var sanitizedMaBenhAn = SanitizeFilePart(pMaBenhAn) ?? "unknown";

        //    var newFolder = Path.Combine(_environment.WebRootPath, "uploads", "external_result_cdha", sanitizedPId, sanitizedMaBenhAn);

        //    var physicalFiles = new List<ExternalLabPhysicalFileVm>();

        //    // Tạo folder nếu chưa tồn tại
        //    if (!Directory.Exists(folder))
        //    {
        //        return Json(new { success = true, merged = new List<object>() });
        //    }

        //    List<dynamic> files = new List<dynamic>();
        //    // Mặc định: đặt tên file khi lưu là "patient_{id}_{yyyyMMddHHmmss}[_vendor].pdf"
        //    var pattern = $"patient_{sanitizedMaBenhAn}_*.pdf";
        //    var filesByMaBenhAn = Directory.EnumerateFiles(folder, pattern, SearchOption.TopDirectoryOnly)
        //                         .OrderByDescending(System.IO.File.GetCreationTimeUtc)
        //                         .Take(100)
        //                         .Select(full =>
        //                         {
        //                             var name = Path.GetFileName(full);
        //                             var url = Url.Content($"~/uploads/external_result_cdha/{sanitizedPId}/{sanitizedMaBenhAn}/{name}");
        //                             var created = System.IO.File.GetCreationTime(full);
        //                             // tách vendor nếu có
        //                             string vendor = null;
        //                             var parts = Path.GetFileNameWithoutExtension(name).Split('_');
        //                             // patient_{id}_{timestamp}[_vendor]
        //                             if (parts.Length >= 4) vendor = parts[3];
        //                             return new { name, url, created, vendor };
        //                         }).ToList();
        //    files.AddRange(filesByMaBenhAn);

        //    // --- 3️⃣ Gộp và sắp xếp (loại trùng theo tên file nếu có) ---
        //    var merged = files
        //        .GroupBy(f => (string)f.name) // loại trùng nếu 2 file trùng tên
        //        .Select(g => g.First())
        //        .OrderByDescending(f => f.created)
        //        .Take(100)
        //        .ToList();
        //    return Json(new { success = true, merged });
        //}

        [HttpGet]
        public async Task<IActionResult> GetExternalResultFiles(string pMaBenhAn, string pId, long patientIdTable)
        {
            if (string.IsNullOrWhiteSpace(pMaBenhAn))
                return Json(new { success = false, message = "Thiếu pMaBenhAn." });

            var sanitizedPId = SanitizeFilePart(pId) ?? "unknown";
            var sanitizedMaBenhAn = SanitizeFilePart(pMaBenhAn) ?? "unknown";

            var newFolder = Path.Combine(_environment.WebRootPath, "uploads", "external_result_cdha", sanitizedPId, sanitizedMaBenhAn);

            var physicalFiles = new List<ExternalLabPhysicalFileVm>();

            try
            {
                // ========= 1) Scan folder mới =========
                if (Directory.Exists(newFolder))
                {
                    var filesInNewFolder = Directory.EnumerateFiles(newFolder, "*.pdf", SearchOption.TopDirectoryOnly)
                        .OrderByDescending(System.IO.File.GetCreationTimeUtc)
                        .Take(100)
                        .ToList();

                    physicalFiles.AddRange(filesInNewFolder.Select(full => MapPhysicalResultFile(
                        fullPath: full,
                        sanitizedPId: sanitizedPId,
                        sanitizedMaBenhAn: sanitizedMaBenhAn
                    )));
                }

                // ========= 2) Loại trùng file vật lý theo relative path =========
                physicalFiles = physicalFiles
                    .GroupBy(x => x.RelativePath, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.OrderByDescending(x => x.Created).First())
                    .OrderByDescending(x => x.Created)
                    .Take(100)
                    .ToList();

                // ========= 3) Sync metadata nếu thiếu =========
                try
                {
                    if (physicalFiles.Any())
                    {
                        var userLogin = GetUserLogin();

                        await _externalFileBL.SyncPhysicalFilesIfMissingAsync(
                            physicalFilePaths: physicalFiles.Select(x => x.FullPath).ToList(),
                            webRootPath: _environment.WebRootPath,
                            pId: pId,
                            maBenhAn: pMaBenhAn,
                            tablePatientId: patientIdTable,
                            moduleType: ExternalFileModule.CDHA,
                            patientName: null,
                            createdBy: userLogin,
                            isVisibleToUser: true
                        );
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Sync metadata failed in GetExternalResultFiles. pMaBenhAn={MaBenhAn}, pId={PId}, patientIdTable={PatientIdTable}",
                        pMaBenhAn, pId, patientIdTable);
                }

                // ========= 4) Query lại DB sau khi sync =========
                var dbFiles = await _externalFileBL.GetAllFilesAsync(
                    tablePatientId: patientIdTable,
                    pId: pId,
                    maBenhAn: pMaBenhAn,
                    moduleType: ExternalFileModule.CDHA
                );

                // ========= 5) Build response final =========
                var dbBackedFiles = dbFiles
                    .Select(x =>
                    {
                        var normalizedRelativePath = (x.RelativePath ?? "").Replace("\\", "/").TrimStart('/');
                        var fullPath = Path.Combine(_environment.WebRootPath, normalizedRelativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));

                        if (!System.IO.File.Exists(fullPath))
                            return null;

                        return new ExternalLabPhysicalFileVm
                        {
                            Name = x.StoredFileName,
                            FullPath = fullPath,
                            RelativePath = normalizedRelativePath,
                            Url = Url.Content("~/" + normalizedRelativePath),
                            Created = x.CreatedOn,
                        };
                    })
                    .Where(x => x != null)
                    .ToList();

                var allFiles = physicalFiles
                    .Concat(dbBackedFiles)
                    .GroupBy(x => x.RelativePath, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.OrderByDescending(x => x.Created).First())
                    .OrderByDescending(x => x.Created)
                    .Take(100)
                    .ToList();

                var merged = allFiles
                    .Select(x =>
                    {
                        var compareKey = BuildExternalFileCompareKey(x.Name, x.RelativePath);

                        var dbItem = dbFiles.FirstOrDefault(d =>
                            BuildExternalFileCompareKey(d.StoredFileName, d.RelativePath) == compareKey);

                        return new
                        {
                            id = dbItem != null ? dbItem.Id : 0,
                            name = x.Name,
                            url = x.Url,
                            created = x.Created,
                            isSynced = dbItem != null,
                            isVisibleToUser = dbItem != null ? dbItem.IsVisibleToUser : false
                        };
                    })
                    .OrderByDescending(x => x.created)
                    .Take(100)
                    .ToList();

                return Json(new
                {
                    success = true,
                    merged
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "GetExternalResultFiles failed. pMaBenhAn={MaBenhAn}, pId={PId}, patientIdTable={PatientIdTable}",
                    pMaBenhAn, pId, patientIdTable);

                return Json(new
                {
                    success = false,
                    message = "Lỗi xử lý lấy danh sách file import."
                });
            }
        }
        #endregion 
        #region Delete file pdf đã import - chỉ xóa file vật lý và mark IsDeleted=true trong DB, KHÔNG xóa record DB cứng để tránh mất lịch sử nếu có lỗi import sau này. Yêu cầu file phải thuộc về bệnh nhân này (theo mã bệnh án) và tồn tại trên hệ thống mới được phép xóa. Nếu file không tồn tại cũng trả về lỗi để tránh nhầm lẫn.
        [HttpPost]
        [Authorize]
        //public IActionResult DeleteExternalResultFile(string fileName, string pMaBenhAn, string pId)
        //{
        //    if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(pMaBenhAn) || string.IsNullOrWhiteSpace(pId))
        //        return Json(new { success = false, message = "Thiếu thông tin file hoặc mã bệnh án." });

        //    try
        //    {
        //        var sanitizedPId = SanitizeFilePart(pId) ?? "unknown";
        //        var sanitizedMaBenhAn = SanitizeFilePart(pMaBenhAn) ?? "unknown";

        //        // Tìm file trong folder mới trước
        //        var newFolder = Path.Combine(_environment.WebRootPath, "uploads", "external_result_cdha", sanitizedPId, sanitizedMaBenhAn);
        //        var filePath = Path.Combine(newFolder, fileName);


        //        // Kiểm tra tính hợp lệ: file phải thuộc về bệnh nhân này và tồn tại
        //        if (!fileName.StartsWith($"patient_{sanitizedMaBenhAn}_"))
        //        {
        //            return Json(new { success = false, message = "File không thuộc về bệnh nhân này." });
        //        }

        //        if (!System.IO.File.Exists(filePath))
        //        {
        //            return Json(new { success = false, message = "File không tồn tại." });
        //        }

        //        // Xóa file
        //        System.IO.File.Delete(filePath);

        //        _logger.LogInformation($"Deleted external result file: {fileName} for patient: {pMaBenhAn}");

        //        return Json(new { success = true, message = "Xóa file thành công." });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Error deleting external result file: {fileName}");
        //        return Json(new { success = false, message = "Lỗi khi xóa file." });
        //    }
        //}
        public async Task<IActionResult> DeleteExternalResultFile(string fileName, string pMaBenhAn, string pId)
        {
            if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(pMaBenhAn))
                return Json(new { success = false, message = "Thiếu thông tin file hoặc mã bệnh án." });

            try
            {
                var sanitizedPId = SanitizeFilePart(pId) ?? "unknown";
                var sanitizedMaBenhAn = SanitizeFilePart(pMaBenhAn) ?? "unknown";

                string filePath = null;
                string relativePath = null;

                // 1) Chỉ tìm file trong folder mới
                var newFolder = Path.Combine(_environment.WebRootPath, "uploads", "external_result_cdha", sanitizedPId, sanitizedMaBenhAn);
                var newFilePath = Path.Combine(newFolder, fileName);

                if (System.IO.File.Exists(newFilePath))
                {
                    filePath = newFilePath;
                    relativePath = _externalFileBL.BuildRelativePath(
                        rootFolderName: "external_result_cdha",
                        pId: sanitizedPId,
                        maBenhAn: sanitizedMaBenhAn,
                        storedFileName: fileName
                    );
                }

                // 2) Nếu không có file vật lý, vẫn thử mark IsDeleted trong DB theo path mới
                if (string.IsNullOrWhiteSpace(relativePath))
                {
                    var possibleNewRelativePath = _externalFileBL.BuildRelativePath(
                        rootFolderName: "external_result_cdha",
                        pId: sanitizedPId,
                        maBenhAn: sanitizedMaBenhAn,
                        storedFileName: fileName
                    );

                    var deletedInDb = await _externalFileBL.SoftDeleteByFileAsync(
                        storedFileName: fileName,
                        relativePath: possibleNewRelativePath,
                        moduleType: ExternalFileModule.CDHA
                    );

                    if (deletedInDb)
                    {
                        return Json(new { success = true, message = "Đã đánh dấu xóa file trong hệ thống." });
                    }

                    return Json(new { success = false, message = "File không tồn tại." });
                }

                // 3) Xóa file vật lý
                System.IO.File.Delete(filePath);

                // 4) Mark IsDeleted trong DB
                await _externalFileBL.SoftDeleteByFileAsync(
                    storedFileName: fileName,
                    relativePath: relativePath,
                    moduleType: ExternalFileModule.CDHA
                );

                _logger.LogInformation("Deleted external result file: {FileName} for patient: {MaBenhAn}", fileName, pMaBenhAn);

                return Json(new { success = true, message = "Xóa file thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting external result file: {FileName}", fileName);
                return Json(new { success = false, message = "Lỗi khi xóa file." });
            }
        }
        #endregion
        [HttpPost]
        public async Task<IActionResult> SaveExternalResultFileVisibility(long id, bool isVisibleToUser)
        {
            if (id <= 0)
                return Json(new { success = false, message = "Thiếu id file." });

            try
            {
                var ok = await _externalFileBL.UpdateVisibility(id, isVisibleToUser);

                if (!ok)
                {
                    return Json(new { success = false, message = "Không tìm thấy file để cập nhật." });
                }

                return Json(new
                {
                    success = true,
                    message = "Lưu thay đổi thành công."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SaveExternalResultFileVisibility failed. id={Id}", id);
                return Json(new { success = false, message = "Lỗi khi lưu thay đổi." });
            }
        }
        private string BuildRelativePath(string root, string pId, string maBenhAn, string fileName, bool isLegacy = false)
        {
            if (isLegacy)
                return $"uploads/{root}/{fileName}".ToLower();

            return $"uploads/{root}/{pId}/{maBenhAn}/{fileName}".ToLower();
        }
        private string BuildExternalFileCompareKey(string fileName, string relativePath)
        {
            fileName = fileName?.Trim() ?? string.Empty;
            relativePath = (relativePath ?? string.Empty).Replace("\\", "/").Trim();

            return $"{fileName}|{relativePath}".ToLowerInvariant();
        }
        private class ExternalLabPhysicalFileVm
        {
            public string Name { get; set; }
            public string FullPath { get; set; }
            public string RelativePath { get; set; }
            public string Url { get; set; }
            public DateTime Created { get; set; }
        }
        private ExternalLabPhysicalFileVm MapPhysicalResultFile(
            string fullPath,
            string sanitizedPId,
            string sanitizedMaBenhAn)
        {
            var fileName = Path.GetFileName(fullPath);
            var relativePath = Path.Combine("uploads", "external_result_cdha", sanitizedPId, sanitizedMaBenhAn, fileName)
                .Replace("\\", "/");

            return new ExternalLabPhysicalFileVm
            {
                Name = fileName,
                FullPath = fullPath,
                RelativePath = relativePath,
                Url = Url.Content("~/" + relativePath),
                Created = System.IO.File.GetCreationTime(fullPath)
            };
        }
    }
}
