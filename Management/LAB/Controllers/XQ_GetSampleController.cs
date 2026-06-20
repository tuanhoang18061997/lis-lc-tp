using Management.BL;
using Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;
using Object = Management.Models.Object;

namespace Management.Controllers
{
    public class XQ_GetSampleController : Controller
    {
        public readonly ILogger<XQ_GetSampleController> _logger;
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
        public readonly string _XQ = "XQ";

        public XQ_GetSampleController(ILogger<XQ_GetSampleController> logger, PatientCDHABL patientCDHABL, ObjectBL objectBL, LocationBL locationBL,
                            DoctorBL doctorBL, UserBL userBL, CategoryBL categoryBL, ServiceBL serviceBL, ResultCDHABL resultCDHABL, SettingBL settingBL, GroupBL groupBL, DeviceBL deviceBL)
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
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetSample()
        {
            var dateTimeNow = ToolBL.Get_DateNow();
            var defaultFrom = new DateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, 00, 00, 00).AddDays(-7);
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

            ViewData["lstPatient"] = await _patientCDHABL.Get_ListPatient(from, to, true, false, false, _XQ);
            ViewData["lstObject"] = await _objectBL.GetListObject();
            ViewData["lstLocation"] = await _locationBL.GetListLocation();
            ViewData["lstDoctor"] = await _doctorBL.GetList();
            ViewData["lstSexModel"] = await SexModel.GetListSexModel();
            ViewData["lstBenhAnModel"] = await BenhAnModel.GetListBenhAnModel();
            ViewData["lstService"] = await _serviceBL.GetListServiceByCategory(_XQ);

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

            var countGetSample = await _patientCDHABL.Get_CountPatient(from, to, true, false, false, _XQ);
            var countProcess = await _patientCDHABL.Get_CountPatient(from, to, false, true, false, _XQ);
            var countReturnResult = await _patientCDHABL.Get_CountPatient(from, to, false, false, true, _XQ);
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

            ViewData["lstPatient"] = await _patientCDHABL.Get_ListPatientByPidOrSid(from, to, true, false, false, null, _XQ);

            return PartialView("_XQ_GetSample_ListPatient");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Search(string pidorseq, DateTime timeSearchFrom, DateTime timeSearchTo)
        {
            // Save selected dates to session/cookies
            SaveSearchDatesToSession(timeSearchFrom, timeSearchTo);

            var from = new DateTime(timeSearchFrom.Year, timeSearchFrom.Month, timeSearchFrom.Day, 00, 00, 00);
            var to = new DateTime(timeSearchTo.Year, timeSearchTo.Month, timeSearchTo.Day, 23, 59, 59);
            ViewData["lstPatient"] = await _patientCDHABL.Get_ListPatientByPidOrSid(from, to, true, false, false, pidorseq, _XQ);

            return PartialView("_XQ_GetSample_ListPatient");
        }
        //[HttpGet]
        //[Authorize]
        //public async Task<IActionResult> Search(string pidorseq, DateTime timeSearchFrom, DateTime timeSearchTo)
        //{
        //    var dateTimeNow = ToolBL.Get_DateNow();
        //    var defaultFrom = new DateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, 00, 00, 00);
        //    var defaultTo = new DateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, 23, 59, 59);

        //    // Try to get saved dates from session/cookies first
        //    var savedFromStr = HttpContext.Request.Cookies[SessionKeyModel._sessionTimeSearchFrom];
        //    var savedToStr = HttpContext.Request.Cookies[SessionKeyModel._sessionTimeSearchTo];

        //    DateTime from = defaultFrom;
        //    DateTime to = defaultTo;

        //    if (!string.IsNullOrEmpty(savedFromStr) && DateTime.TryParse(savedFromStr, out var savedFrom))
        //    {
        //        // Ensure time is set to 00:00:00 for the from date
        //        from = new DateTime(savedFrom.Year, savedFrom.Month, savedFrom.Day, 00, 00, 00);
        //    }
        //    if (!string.IsNullOrEmpty(savedToStr) && DateTime.TryParse(savedToStr, out var savedTo))
        //    {
        //        // Ensure time is set to 23:59:59 for the to date
        //        to = new DateTime(savedTo.Year, savedTo.Month, savedTo.Day, 23, 59, 59);
        //    }

        //    // Save selected dates to session/cookies
        //    SaveSearchDatesToSession(timeSearchFrom, timeSearchTo);
        //    ViewData["lstPatient"] = await _patientCDHABL.Get_ListPatientByPidOrSid(from, to, true, false, false, pidorseq, _XQ);

        //    return PartialView("_XQ_GetSample_ListPatient");
        //}


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

            return PartialView("_XQ_GetSample_PatientInfo");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetServiceForPatient(long patientId)
        {
            ViewData["lstResultCDHAs"] = await _resultCDHABL.GetListResultCDHAByPatientId(patientId, _XQ);
            ViewData["lstService"] = await _serviceBL.GetListServiceByCategory(_XQ);
            return PartialView("_XQ_GetSample_ListService");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeletePatientAndService(long id)
        {
            var _status = string.Empty;
            var _userLogin = this.GetUserLogin();
            if (await _resultCDHABL.DeleteByPatientId(id, _userLogin.Value))
            {
                _status = await _patientCDHABL.DeleteById(id, _XQ) ? "/XQ_GetSample/Refresh/" : string.Empty;
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
            var _patient = await _patientCDHABL.SaveOrUpdate(_id, patientId, seq, sid, patientName, age, sex, obj, type, location, doctor, getSampleTime, address, diagnostic, _userLogin.Value, _dateTime, true, false, false, _XQ);

            return Content(_patient == null ? string.Empty : _patient.Id.ToString());
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddServiceForPatient(long patientId, long serviceId, long doctorId)
        {
            var _userLogin = this.GetUserLogin();
            var _dateTime = ToolBL.Get_DateNow();
            var _save = await _resultCDHABL.SaveService(patientId, serviceId, _userLogin.Value, _dateTime, doctorId, _XQ.ToLower());

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
            var _exitResultCDHA = await _resultCDHABL.ExitResultXN_Update_GetSampleTime(id, getSampleTime, _XQ);
            var _getSample = false;
            if (_exitResultCDHA)
            {
                var _userLogin = this.GetUserLogin();
                _getSample = await _patientCDHABL.GetSample_ProcessResult_ReturnResult(id, false, true, false, _userLogin.Value, _XQ);
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
    }
}
