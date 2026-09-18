using DocumentFormat.OpenXml.Office2016.Excel;
using Management.BL;
using Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Net;

namespace Management.Controllers
{
    public class TDCN_ReturnResultController : Controller
    {
        public readonly ILogger<TDCN_ReturnResultController> _logger;
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
        private readonly ToolBL _toolBL;
        private readonly HospitalBL _hospitalBL;
        private readonly IWebHostEnvironment _environment;
        private readonly ResultInvalidBL _resultInvalidBL;
        public readonly string _TDCN = "TDCN";

        public TDCN_ReturnResultController(ILogger<TDCN_ReturnResultController> logger, PatientCDHABL patientBL, ObjectBL objectBL, LocationBL locationBL,
                            DoctorBL doctorBL, UserBL userBL, CategoryBL categoryBL, ServiceBL serviceBL, ResultCDHABL resultCDHABL, SettingBL settingBL,
                            GroupBL groupBL, IWebHostEnvironment environment, ToolBL toolBL, HospitalBL hospitalBL, ResultInvalidBL resultInvalidBL)
        {
            _logger = logger;
            _patientCDHABL = patientBL;
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
            _toolBL = toolBL;
            _hospitalBL = hospitalBL;
            _resultInvalidBL = resultInvalidBL;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ReturnResult()
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

            var _userLoginId = this.GetUserLogin();
            if (_userLoginId != null)
            {
                ViewData["lstUserFunction"] = await _userBL.GetUserFunction(_userLoginId.Value);
            }

            ViewData["lstPatient"] = await _patientCDHABL.Get_ListPatient_Flexible(from, to, _TDCN, "valid");
            ViewData["lstBenhAnModel"] = await BenhAnModel.GetListBenhAnModel();

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


            var countGetSample = await _patientCDHABL.Get_CountPatient_New(from, to, true, false, false, _TDCN);
            var countProcess = await _patientCDHABL.Get_CountPatient_New(from, to, false, true, false, _TDCN);
            var countReturnResult = await _patientCDHABL.Get_CountPatient_New(from, to, false, false, true, _TDCN);
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
            ViewData["lstPatient"] = await _patientCDHABL.Get_ListPatientByPidOrSid_New(from, to, false, false, true, null, _TDCN);
            SaveSearchDatesToSession(from, to);

            return PartialView("_TDCN_ReturnResult_ListPatient");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Search(string pidorseq, DateTime timeSearchFrom, DateTime timeSearchTo)
        {
            // Save selected dates to session/cookies
            SaveSearchDatesToSession(timeSearchFrom, timeSearchTo);

            var from = new DateTime(timeSearchFrom.Year, timeSearchFrom.Month, timeSearchFrom.Day, 00, 00, 00);
            var to = new DateTime(timeSearchTo.Year, timeSearchTo.Month, timeSearchTo.Day, 23, 59, 59);
            ViewData["lstPatient"] = await _patientCDHABL.Get_ListPatientByPidOrSid_New(from, to, false, false, true, pidorseq, _TDCN);

            return PartialView("_TDCN_ReturnResult_ListPatient");
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
            ViewData["lstBenhAnModel"] = await BenhAnModel.GetListBenhAnModel();
            ViewData["patientInfo"] = await _patientCDHABL.Get_PatientBySid(id);

            return PartialView("_TDCN_ReturnResult_PatientInfo");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetServiceForPatient(long patientId)
        {
            ViewData["lstResultCDHAs"] = await _resultCDHABL.GetListResultCDHAByPatientId(patientId, _TDCN);
            return PartialView("_TDCN_ReturnResult_ListService_ReloadPDF");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get_Description_Result_Suggest_ForService(long id)
        {
            var _resultCDHA = await _resultCDHABL.GetResultCDHA(id);
            if (_resultCDHA != null)
            {
                var _lstItem = new List<object>() { new { Description = _resultCDHA.Description, Result = _resultCDHA.Result, Suggest = _resultCDHA.Suggest } };
                if (_resultCDHA.ImageCDHAs != null && _resultCDHA.ImageCDHAs.Count > 0)
                {
                    foreach (var item in _resultCDHA.ImageCDHAs)
                    {
                        _lstItem.Add(new { Id = item.Id, Name = item.ImagePath, Description = item.ResultCDHA.Description, Result = item.ResultCDHA.Result, Suggest = item.ResultCDHA.Suggest });
                    }
                }
                else
                {
                    _lstItem.Add(new { Description = _resultCDHA.Description, Result = _resultCDHA.Result, Suggest = _resultCDHA.Suggest });

                }
                return Json(_lstItem);
            }
            else
            {
                return Json(null);
            }
        }

        //[HttpPost]
        //[Authorize]
        //public async Task<IActionResult> Invalid(long id)
        //{
        //    var _userLogin = this.GetUserLogin();
        //    var dateTimeNow = ToolBL.Get_DateNow();
        //    var from = new DateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, 23, 59, 59).AddDays(-1);
        //    var to = new DateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, 23, 59, 59);
        //    var _process = await _patientCDHABL.GetSample_ProcessResult_ReturnResult(id, false, true, false, _userLogin.Value, _TDCN);
        //    return Content(_process.ToString());
        //}

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Invalid([FromBody] InvalidRequestModel request)
        {
            try
            {
                if (request == null ||
                    request.PatientId <= 0 ||
                    request.ResultIds == null ||
                    request.ResultIds.Count == 0)
                {
                    return BadRequest("Vui lòng chọn ít nhất một dịch vụ.");
                }

                var _userLogin = this.GetUserLogin();
                if (!_userLogin.HasValue)
                {
                    return Unauthorized("Không xác định được người dùng.");
                }

                var result = await _resultInvalidBL.InvalidCDHAAsync(
                    request.PatientId,
                    request.ResultIds,
                    _TDCN,
                    _userLogin.Value
                );

                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }

                return Content("True");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Invalid CDHA failed for PatientId: {PatientId}, Module: {ModuleCode}",
                    request?.PatientId,
                    _TDCN
                );

                return Content("False");
            }
        }

        // Model để nhận request từ client
        public class InvalidRequestModel
        {
            public long PatientId { get; set; }
            public List<long> ResultIds { get; set; } = new();
            public string CategoryCode { get; set; }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetValidatedPdf(long resultCDHAId)
        {
            if (resultCDHAId <= 0)
            {
                return Json(new { success = false, message = "Thiếu mã kết quả dịch vụ." });
            }

            var result = await _resultCDHABL.GetResultCDHA(resultCDHAId);
            if (result == null)
            {
                return Json(new { success = false, message = "Không tìm thấy kết quả dịch vụ." });
            }

            if (string.IsNullOrWhiteSpace(result.KeyResultForHis))
            {
                return Json(new { success = false, message = "Kết quả chưa có KeyResultForHis nên chưa thể lấy PDF." });
            }

            var folder = Path.Combine(_environment.WebRootPath, "pdf", "tdcn");
            var fileName = $"{result.KeyResultForHis}.pdf";
            var filePath = Path.Combine(folder, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return Json(new
                {
                    success = false,
                    keyResultForHis = result.KeyResultForHis,
                    fileName,
                    message = "Chưa tìm thấy file PDF kết quả đã Valid."
                });
            }

            var fileUrl = Url.Content($"~/pdf/tdcn/{Uri.EscapeDataString(fileName)}");
            return Json(new
            {
                success = true,
                resultCDHAId = result.Id,
                keyResultForHis = result.KeyResultForHis,
                fileName,
                url = fileUrl,
                signStoreId = result.SignStoreId,
                signStatus = result.SignStatus,
                isDigitallySigned = result.SignStoreId != 0 && result.SignStatus == 1
            });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Print(long resultCDHAId)
        {
            var _result = await _resultCDHABL.GetResultCDHAByPatientId_ForValidPrint(resultCDHAId);
            var fileBase64 = string.Empty;
            if (_result != null)
            {
                // === LẤY THÔNG TIN BÁC SĨ THỰC HIỆN (ReturnUser) ===
                if (_result.UserUpdateId > 0)   // vì userReturnResult là kiểu int/long
                {
                    var returnUser = await _userBL.GetUser(_result.UserUpdateId ?? 0);
                    if (returnUser != null)
                    {
                        ViewData["ReturnUser"] = returnUser; // để Content/Header/Footer dùng khi in
                    }
                }
                var _serviceCDHA = _serviceBL.GetById(_result?.Service?.Id);
                if (_serviceCDHA != null)
                {
                    ViewData["ServiceCDHA"] = _serviceCDHA.Result;
                }
                else
                {
                    ViewData["ServiceCDHA"] = null;
                }
                var _folder = Path.Combine(_environment.WebRootPath, "pdf", "tdcn");
                var _file = Path.Combine(_folder, _result?.KeyResultForHis + ".pdf");
                try
                {
                    if (System.IO.File.Exists(_file))
                    {
                        fileBase64 = Convert.ToBase64String(System.IO.File.ReadAllBytes(_file));
                    }
                    else
                    {
                        ViewData["ResultCDHA"] = _result;
                        var _hospital = await _hospitalBL.GetHospital();
                        var content = await this.RenderViewAsync("Content", _hospital);
                        var header = await this.RenderViewAsync("Header", _hospital);
                        var footer = await this.RenderViewAsync("Footer", _hospital);
                        fileBase64 = await _toolBL.ExportPdf_Result(_folder, _file, header, content, footer);
                    }
                }
                catch { }
            }
            return Content(fileBase64);
        }

        //[HttpGet]
        //public async Task<IActionResult> Print()
        //{
        //    var _result = await _resultCDHABL.GetResultCDHAByPatientId_ForValidPrint(45);
        //    ViewData["ResultCDHA"] = _result;
        //    var _hospital = await _hospitalBL.GetHospital();
        //    return View(_hospital);
        //}


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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetSignStoreId(long resultId)
        {
            if (resultId <= 0)
                return Json(new { signStoreId = (string?)null });

            // Tận dụng BL hiện có để lấy record kết quả rồi đọc SignStoreId
            var result = await _resultCDHABL.GetResultCDHA(resultId);
            var id = result?.SignStoreId;          // tên property đúng với cột DB của bạn

            // Trả JSON để JS dùng trực tiếp
            return Json(new { signStoreId = id });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserInfo(long userId)
        {
            var u = await _userBL.GetUser(userId);
            if (u == null) return Json(null);

            // Trả về các field bạn cần dùng ở UI/PDF
            return Json(new
            {
                u.Name,
                u.Active,
                u.MaBHYT,
                u.Cccd,
                u.Cks
            });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SaveSignStoreIdForResultCDHA(long resultCDHAId, long signStoreId)
        {
            var _dateTime = ToolBL.Get_DateNow();
            var _userLogin = this.GetUserLogin();

            if (resultCDHAId <= 0 || signStoreId < 0)
                return BadRequest("resultCDHAId hoặc signStoreId không hợp lệ.");

            try
            {
                // Gọi BL để lưu (giả định chữ ký: UpdateSignStoreId_CKS(long id, string signStoreId))
                var ok = await _resultCDHABL.UpdateSignStoreId_CKS(resultCDHAId, signStoreId, _userLogin.Value, _dateTime);
                if (ok == true)
                {
                    var _resultCDHA = await _resultCDHABL.GetResultCDHA(resultCDHAId);
                    var _keyResult = _resultCDHA.KeyResultForHis;
                    return Json(new { success = true, keyResult = _keyResult });
                }
                else
                {
                    return Json(new { success = false, message = "Lỗi lưu signStoreId." });
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SaveSignStoreIdForResultCDHA failed");
                return StatusCode(500, "Lỗi lưu signStoreId.");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChuyenDangThucHien(long patientId)
        {
            if (patientId <= 0)
            {
                return Json(new { success = false, message = "Thiếu dữ liệu patientId." });
            }

            var _userLogin = this.GetUserLogin();
            if (_userLogin == null)
            {
                return Json(new { success = false, message = "Không xác định được user đăng nhập." });
            }

            try
            {
                var patient = await _patientCDHABL.Get_PatientBySid(patientId);
                if (patient == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bệnh nhân." });
                }

                var results = await _resultCDHABL.GetListResultCDHAByPatientId(patientId, _TDCN);
                if (results == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy danh sách dịch vụ TDCN." });
                }

                var resultIds = results
                    .Where(x =>
                        x.IsValidated == true ||
                        (!x.IsValidated.HasValue &&
                         patient.ValidTDCN &&
                         patient.ReturnResultTimeTDCN.HasValue &&
                         (!x.InsertTime.HasValue || x.InsertTime.Value <= patient.ReturnResultTimeTDCN.Value)))
                    .Select(x => x.Id)
                    .Distinct()
                    .ToList();

                // Không còn service Valid thì coi như bệnh nhân đã ở trạng thái cần thiết.
                if (resultIds.Count == 0)
                {
                    return Content("True");
                }

                var result = await _resultInvalidBL.InvalidCDHAAsync(patientId, resultIds, _TDCN, _userLogin.Value);
                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }

                return Content("True");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ChuyenDangThucHien failed for PatientId: {PatientId}", patientId);
                return Content("False");
            }
        }
    }
}
