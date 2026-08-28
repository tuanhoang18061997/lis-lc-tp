using Management.BL;
using Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Management.Controllers
{
    public class XN_ReturnResultController : Controller
    {
        public readonly ILogger<XN_ReturnResultController> _logger;
        public readonly PatientXNBL _patientBL;
        public readonly ObjectBL _objectBL;
        public readonly LocationBL _locationBL;
        public readonly DoctorBL _doctorBL;
        public readonly UserBL _userBL;
        public readonly CategoryBL _categoryBL;
        public readonly ServiceBL _serviceBL;
        public readonly ResultXNBL _resultXNBL;
        public readonly SettingBL _settingBL;
        public readonly GroupBL _groupBL;
        private readonly ToolBL _toolBL;
        private readonly HospitalBL _hospitalBL;
        private readonly IWebHostEnvironment _environment;
        private readonly ResultInvalidBL _resultInvalidBL;

        public XN_ReturnResultController(ILogger<XN_ReturnResultController> logger, PatientXNBL patientBL, ObjectBL objectBL, LocationBL locationBL,
                            DoctorBL doctorBL, UserBL userBL, CategoryBL categoryBL, ServiceBL serviceBL, ResultXNBL resultXNBL, SettingBL settingBL, 
                            GroupBL groupBL, IWebHostEnvironment environment, ToolBL toolBL, HospitalBL hospitalBL,
                            ResultInvalidBL resultInvalidBL)
        {
            _logger = logger;
            _patientBL = patientBL;
            _objectBL = objectBL;
            _locationBL = locationBL;
            _doctorBL = doctorBL;
            _userBL = userBL;
            _categoryBL = categoryBL;
            _serviceBL = serviceBL;
            _resultXNBL = resultXNBL;
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

            ViewData["lstPatient"] = await _patientBL.Get_ListPatient(from, to, false, false, true);
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

            var countGetSample = await _patientBL.Get_CountPatient(from, to, true, false, false);
            var countProcessNotFullResult = await _patientBL.Get_CountPatientNotFullResult(from, to, false, true, false);
            var countProcessFullResult = await _patientBL.Get_CountPatientFullResult(from, to, false, true, false);          
            var countReturnResult = await _patientBL.Get_CountPatient(from, to, false, false, true);
            ViewData["countGetSample"] = countGetSample;
            ViewData["countProcessNotFullResult"] = countProcessNotFullResult;
            ViewData["countProcessFullResult"] = countProcessFullResult;
            ViewData["countReturnResult"] = countReturnResult;

            return Content(countGetSample + ";" + countProcessNotFullResult + ";" + countProcessFullResult + ";" + countReturnResult);
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

            ViewData["lstPatient"] = await _patientBL.Get_ListPatientByPidOrSid(from, to, false, false, true, null);

            return PartialView("_XN_ReturnResult_ListPatient");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Search(string pidorseq, DateTime timeSearchFrom, DateTime timeSearchTo)
        {
            // Save selected dates to session/cookies
            SaveSearchDatesToSession(timeSearchFrom, timeSearchTo);

            var from = new DateTime(timeSearchFrom.Year, timeSearchFrom.Month, timeSearchFrom.Day, 00, 00, 00);
            var to = new DateTime(timeSearchTo.Year, timeSearchTo.Month, timeSearchTo.Day, 23, 59, 59);
            ViewData["lstPatient"] = await _patientBL.Get_ListPatientByPidOrSid(from, to, false, false, true, pidorseq);

            return PartialView("_XN_ReturnResult_ListPatient");
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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Invalid(long id)
        {
            var _userLogin = this.GetUserLogin();
            if (!_userLogin.HasValue)
                return Unauthorized();

            var result = await _resultInvalidBL.InvalidXNAsync(id, _userLogin.Value);
            if (!result.Success)
                return BadRequest(result.Message);

            return Content("True");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetPatientInfo(long id)
        {
            ViewData["lstBenhAnModel"] = await BenhAnModel.GetListBenhAnModel();
            ViewData["patientInfo"] = await _patientBL.Get_PatientBySid(id);

            return PartialView("_XN_ReturnResult_PatientInfo");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetServiceForPatient(long patientId)
        {
            var listXNs = await _resultXNBL.GetListResultXNByPatientId(patientId);

            ViewData["lstResultXNs"] = listXNs;
            ViewData["note"] = listXNs[0].Note;

            return PartialView("_XN_ReturnResult_ListService");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Print(long patientId, string sid)
        {
            var _lstResult = await _resultXNBL.GetListResultXNByPatientId_ForValidPrint(patientId);
            var fileBase64 = string.Empty;
            if (_lstResult != null)
            {
                var _folder = Path.Combine(_environment.WebRootPath, "pdf", "xn");
                var _file = Path.Combine(_folder, _lstResult[0]?.KeyResultForHis + ".pdf");
                try
                {
                    // === LẤY THÔNG TIN BÁC SĨ THỰC HIỆN (ReturnUser) ===
                    if (_lstResult[0].UserUpdateId > 0)   // vì userReturnResult là kiểu int/long
                    {
                        var returnUser = await _userBL.GetUserById(_lstResult[0].UserUpdateId);
                        if (returnUser != null)
                        {
                            ViewData["ReturnUser"] = returnUser; // để Content/Header/Footer dùng khi in
                        }
                    }
                    if (System.IO.File.Exists(_file))
                    {
                        fileBase64 = Convert.ToBase64String(System.IO.File.ReadAllBytes(_file));
                    }
                    else
                    {
                        ViewData["ListResultXN"] = _lstResult;
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
        public async Task<IActionResult> GetSignStoreId(long patientId)
        {
            if (patientId <= 0)
                return Json(new { signStoreId = (string?)null });
            long? id = null;
            // Tận dụng BL hiện có để lấy record kết quả rồi đọc SignStoreId
            var result = await _resultXNBL.GetListResultXNByPatientId(patientId);
            if(result.Count > 0)
            {
                id = result[0].SignStoreId;          // tên property đúng với cột DB của bạn
            }

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
        public async Task<IActionResult> SaveSignStoreIdForResultXN(long signStoreId, [FromBody] List<long> ids)
        {
            if (signStoreId <= 0 || ids == null || ids.Count == 0)
                return BadRequest("Dữ liệu không hợp lệ");

            var _dateTime = ToolBL.Get_DateNow();
            var _userLogin = this.GetUserLogin();
            if (!_userLogin.HasValue) return Unauthorized();
            var res = false;
            var objFirst = await _resultXNBL.GetResultXN(ids[0]);
            var _keyResult = objFirst != null ? objFirst.KeyResultForHis : null;
            foreach (var id in ids)
            {
                if (id > 0)
                {
                    res = await _resultXNBL.UpdateSignStoreId_CKS(id, signStoreId, _userLogin.Value, _dateTime);
                }
            }
            if (res == true && objFirst != null)
            {
                return Json(new { success = true, keyResult = _keyResult });
            }
            else
            {
                return Json(new { success = false, keyResult = _keyResult });
            }
        }
    }
}
