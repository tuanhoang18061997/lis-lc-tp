using Management.BL;
using Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SelectPdf;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using UglyToad.PdfPig;                 // <— NuGet: UglyToad.PdfPig
using UglyToad.PdfPig.Content;
using Management.Services.LabParsing;

namespace Management.Controllers
{
    public class XN_ProcessController : Controller
    {
        private readonly ExternalLabParserFactory _parserFactory;

        private readonly ILogger<XN_ProcessController> _logger;
        private readonly PatientXNBL _patientBL;
        private readonly ObjectBL _objectBL;
        private readonly LocationBL _locationBL;
        private readonly DoctorBL _doctorBL;
        private readonly UserBL _userBL;
        private readonly CategoryBL _categoryBL;
        private readonly ServiceBL _serviceBL;
        private readonly ResultXNBL _resultXNBL;
        private readonly SettingBL _settingBL;
        private readonly GroupBL _groupBL;
        private readonly ToolBL _toolBL;
        private readonly HospitalBL _hospitalBL;
        private readonly ExternalFileBL _externalFileBL;
        private readonly IWebHostEnvironment _environment;

        public XN_ProcessController(ILogger<XN_ProcessController> logger, PatientXNBL patientBL, ObjectBL objectBL, LocationBL locationBL,
                            DoctorBL doctorBL, UserBL userBL, CategoryBL categoryBL, ServiceBL serviceBL, ResultXNBL resultXNBL, 
                            SettingBL settingBL, GroupBL groupBL, IWebHostEnvironment environment, ToolBL toolBL, HospitalBL hospitalBL, ExternalLabParserFactory parserFactory, ExternalFileBL externalFileBL)
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
            _parserFactory = parserFactory;
            _externalFileBL = externalFileBL;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Process()
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

            ViewData["lstPatient"] = await _patientBL.Get_ListPatient(from, to, false, true, false);
            ViewData["lstUser"] = await _userBL.GetListUser(UserTypeModel.XN);
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
            //var dateTimeNow = ToolBL.Get_DateNow();
            //var from = new DateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, 00, 00, 00);
            //var to = new DateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, 23, 59, 59);
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

            ViewData["lstPatient"] = await _patientBL.Get_ListPatientByPidOrSid(from, to, false, true, false, null);

            return PartialView("_XN_Process_ListPatient");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Search(string pidorseq, DateTime timeSearchFrom, DateTime timeSearchTo, string maDotKham)
        {
            // Save selected dates to session/cookies
            SaveSearchDatesToSession(timeSearchFrom, timeSearchTo);

            var from = new DateTime(timeSearchFrom.Year, timeSearchFrom.Month, timeSearchFrom.Day, 00, 00, 00);
            var to = new DateTime(timeSearchTo.Year, timeSearchTo.Month, timeSearchTo.Day, 23, 59, 59);
            ViewData["lstPatient"] = await _patientBL.Get_ListPatientByPidOrSid(from, to, false, true, false, pidorseq, maDotKham);

            return PartialView("_XN_Process_ListPatient");
        }

        // Thêm endpoint m?i d? l?y danh sách MaDotKham
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMaDotKhamList(DateTime timeSearchFrom, DateTime timeSearchTo)
        {
            var from = new DateTime(timeSearchFrom.Year, timeSearchFrom.Month, timeSearchFrom.Day, 00, 00, 00);
            var to = new DateTime(timeSearchTo.Year, timeSearchTo.Month, timeSearchTo.Day, 23, 59, 59);

            var lstPatient = await _patientBL.Get_ListPatient(from, to, false, true, false);
            var maDotKhamList = lstPatient
                .Where(p => !string.IsNullOrEmpty(p.MaDotKham))
                .Select(p => p.MaDotKham)
                .Distinct()
                .OrderBy(m => m)
                .ToList();

            return Json(maDotKhamList);
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
        public async Task<IActionResult> GetSample(long id)
        {
            var _userLogin = this.GetUserLogin();
            var _getSample = await _patientBL.GetSample_ProcessResult_ReturnResult(id, true, false, false, _userLogin.Value);
            return Content(_getSample.ToString());
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetPatientInfo(long id)
        {
            var _userLoginId = this.GetUserLogin();
            ViewData["userLoginId"] = _userLoginId;
            ViewData["lstUser"] = await _userBL.GetListUser(UserTypeModel.XN);
            ViewData["lstBenhAnModel"] = await BenhAnModel.GetListBenhAnModel();
            ViewData["patientInfo"] = await _patientBL.Get_PatientBySid(id);

            return PartialView("_XN_Process_PatientInfo");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetServiceForPatient(long patientId)
        {
            ViewData["lstResultXNs"] = await _resultXNBL.GetListResultXNByPatientId(patientId);
            return PartialView("_XN_Process_ListService");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetServiceAndLastestResultsForPatient(long id, string patientId)
        {
            var listXNs = await _resultXNBL.GetListResultXNByPatientIdAndLastestResults(id, patientId);
            ViewData["note"] = listXNs[0].Note;
            ViewData["lstResultXNs"] = listXNs;
            ViewData["patientInfo"] = await _patientBL.Get_PatientBySid(id);
            return PartialView("_XN_Process_ListService");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetServiceByCategory(long? categoryId)
        {
            var _lstService = await _serviceBL.GetListServiceByCategory(categoryId);
            var _lstItem = _lstService.Select(p => new { Id = p.Id, Name = p.Name }).ToList();

            return Json(_lstItem);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SaveResult([FromBody] List<ResultXNModel> lstResult)       
        {
            var _saveResult = false;
            var _fullResultXN = false;
            if (lstResult != null && lstResult.Count > 0)
            {
                var _userLogin = this.GetUserLogin();
                _saveResult = await _resultXNBL.Update(lstResult, false, _userLogin.Value);
                _fullResultXN = await _resultXNBL.FullResultXN(lstResult[0].patientId);
                await _patientBL.Update(lstResult[0].patientId, lstResult[0].returnResultTime, lstResult[0].userReturnResult, _userLogin.Value, _fullResultXN);

            }
            return Content(_saveResult.ToString());
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ValidPrint([FromBody] List<ResultXNModel> lstResult)
        {
            if (lstResult != null && lstResult.Count > 0)
            {
                var _userLogin = this.GetUserLogin();
                if (await _resultXNBL.Update(lstResult, true, _userLogin.Value))
                {
                    var _notFullResultXN = false;
                    var _fullResultXN = await _resultXNBL.FullResultXN(lstResult[0].patientId);
                    if (_fullResultXN == false)
                    {
                        _notFullResultXN = true;
                    }
                    if (await _patientBL.Update(lstResult[0].patientId, lstResult[0].returnResultTime, lstResult[0].userReturnResult, false, false, true, _userLogin.Value, _fullResultXN, _notFullResultXN))
                    {
                        var _lstResult = await _resultXNBL.GetListResultXNByPatientId_ForValidPrint(lstResult[0].patientId);

                        var urineServiceId = 12425;

                        var lstNormal = _lstResult.Where(x => x.ServiceId != urineServiceId).ToList(); // các xn # nước tiểu

                        var lstUrine = _lstResult.Where(x => x.ServiceId == urineServiceId).ToList(); // xn nước tiểu

                        if (_lstResult != null)
                        {
                            //try
                            //{
                            //    // === LẤY THÔNG TIN BÁC SĨ THỰC HIỆN (ReturnUser) ===
                            //    if (lstResult[0].userReturnResult > 0)   // vì userReturnResult là kiểu int/long
                            //    {
                            //        var returnUser = await _userBL.GetUser(lstResult[0].userReturnResult);
                            //        if (returnUser != null)
                            //        {
                            //            ViewData["ReturnUser"] = returnUser; // để Content/Header/Footer dùng khi in
                            //        }
                            //    }

                            //    ViewData["ListResultXN"] = _lstResult;
                            //    ViewData["Note"] = _lstResult[0].Note;
                            //    var _hospital = await _hospitalBL.GetHospital();
                            //    var content = await this.RenderViewAsync("Content", _hospital);
                            //    var header = await this.RenderViewAsync("Header", _hospital);
                            //    var footer = await this.RenderViewAsync("Footer", _hospital);
                            //    var _folder = Path.Combine(_environment.WebRootPath, "pdf", "xn");
                            //    var _file = Path.Combine(_folder, _lstResult[0]?.KeyResultForHis + ".pdf");
                            //    var fileBase64 = await _toolBL.ExportPdf_Result(_folder, _file, header, content, footer);
                            //    return Content(fileBase64);
                            //}
                            //catch { }
                            try
                            {
                                // === LẤY THÔNG TIN BÁC SĨ THỰC HIỆN (ReturnUser) ===
                                if (lstResult[0].userReturnResult > 0)
                                {
                                    var returnUser = await _userBL.GetUser(lstResult[0].userReturnResult);
                                    if (returnUser != null)
                                    {
                                        ViewData["ReturnUser"] = returnUser;
                                    }
                                }

                                ViewData["Note"] = _lstResult[0].Note;

                                var _hospital = await _hospitalBL.GetHospital();

                                var lstNormalResult = _lstResult
                                    .Where(x => x.ServiceId != urineServiceId)
                                    .ToList();

                                var lstUrineResult = _lstResult
                                    .Where(x => x.ServiceId == urineServiceId)
                                    .ToList();

                                var contentParts = new List<string>();

                                var pdfHardFixStyle = @"
<style>
    .xn-print-section {
        width: 100% !important;
        clear: both !important;
        display: block !important;
        float: none !important;
        overflow: visible !important;
        page-break-inside: auto !important;
        break-inside: auto !important;
    }
    .xn-print-section .result-xn {
        float: none !important;
        clear: both !important;
        display: block !important;
        height: auto !important;
        min-height: 0 !important;
        max-height: none !important;
        overflow: visible !important;
    }
    .xn-print-section .table-result {
        width: 100% !important;
        border-collapse: collapse !important;
        page-break-inside: auto !important;
        break-inside: auto !important;
    }
    .xn-print-section .table-result tr,
    .xn-print-section .table-result .tr-category,
    .xn-print-section .table-result .tr-content,
    .xn-print-section .table-result .tr-title-body {
        page-break-inside: avoid !important;
        break-inside: avoid !important;
    }
    .xn-force-new-page {
        page-break-before: always !important;
        break-before: page !important;
        clear: both !important;
        display: block !important;
        float: none !important;
    }
    .xn-clear-float {
        clear: both !important;
        display: block !important;
        height: 0 !important;
        line-height: 0 !important;
        font-size: 0 !important;
        overflow: hidden !important;
    }
</style>";

                                // Có nước tiểu hay không
                                var hasUrineResult = lstUrineResult.Any();

                                if (lstNormalResult.Any())
                                {
                                    ViewData["ListResultXN"] = lstNormalResult;
                                    ViewData["ShowNote"] = !hasUrineResult;
                                    ViewData["DisableThead"] = false;

                                    var normalContent = await this.RenderViewAsync("Content", _hospital);
                                    contentParts.Add($@"
<div class='xn-print-section' style='width:100%; clear:both; display:block; float:none; overflow:visible;'>
    {normalContent}
    <div class='xn-clear-float' style='clear:both; display:block; height:0; line-height:0; font-size:0; overflow:hidden;'></div>
</div>");
                                }

                                // 2. Render nhóm nước tiểu ở trang riêng
                                if (lstUrineResult.Any())
                                {
                                    ViewData["ListResultXN"] = lstUrineResult;
                                    ViewData["ShowNote"] = true;

                                    // Quan trọng: nước tiểu không dùng <thead>
                                    ViewData["DisableThead"] = true;

                                    var urineContent = await this.RenderViewAsync("Content", _hospital);

                                    if (contentParts.Any())
                                    {
                                        contentParts.Add($@"
<div class='xn-print-section xn-urine-page xn-force-new-page' style='page-break-before:always; break-before:page; width:100%; clear:both; display:block; float:none; overflow:visible;'>
    {urineContent}
    <div class='xn-clear-float' style='clear:both; display:block; height:0; line-height:0; font-size:0; overflow:hidden;'></div>
</div>");
                                    }
                                    else
                                    {
                                        contentParts.Add($@"
<div class='xn-print-section xn-urine-page' style='width:100%; clear:both; display:block; float:none; overflow:visible;'>
    {urineContent}
    <div class='xn-clear-float' style='clear:both; display:block; height:0; line-height:0; font-size:0; overflow:hidden;'></div>
</div>");
                                    }
                                }

                                var content = pdfHardFixStyle + string.Join("", contentParts);

                                var header = await this.RenderViewAsync("Header", _hospital);
                                var footer = await this.RenderViewAsync("Footer", _hospital);

                                var _folder = Path.Combine(_environment.WebRootPath, "pdf", "xn");
                                var _file = Path.Combine(_folder, _lstResult[0]?.KeyResultForHis + ".pdf");

                                var fileBase64 = await _toolBL.ExportPdf_Result_XN(_folder, _file, header, content, footer);
                                return Content(fileBase64);
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
            return Content(string.Empty);
        }

        public async Task<IActionResult> ValidNotFullResultXN([FromBody] List<ResultXNModel> lstResult)
        {
            if (lstResult != null && lstResult.Count > 0)
            {
                var _userLogin = this.GetUserLogin();
                if (await _resultXNBL.Update(lstResult, true, _userLogin.Value))
                {
                    var _fullResultXN = false;
                    var _notFullResultXN = await _resultXNBL.NotFullResultXN(lstResult[0].patientId);
                    if(_notFullResultXN == false)
                    {
                        _fullResultXN = true;
                    }
                    if (await _patientBL.Update(lstResult[0].patientId, lstResult[0].returnResultTime, lstResult[0].userReturnResult, false, true, false, _userLogin.Value, _fullResultXN, _notFullResultXN))
                    {
                        var _lstResult = await _resultXNBL.GetListResultXNByPatientId_ForValidPrint(lstResult[0].patientId);

                        var urineServiceId = 12425;

                        var lstNormal = _lstResult.Where(x => x.ServiceId != urineServiceId).ToList(); // các xn # nước tiểu

                        var lstUrine = _lstResult.Where(x => x.ServiceId == urineServiceId).ToList(); // xn nước tiểu
                        if (_lstResult != null)
                        {
                            //try
                            //{
                            //    // === LẤY THÔNG TIN BÁC SĨ THỰC HIỆN (ReturnUser) ===
                            //    if (lstResult[0].userReturnResult > 0)   // vì userReturnResult là kiểu int/long
                            //    {
                            //        var returnUser = await _userBL.GetUser(lstResult[0].userReturnResult);
                            //        if (returnUser != null)
                            //        {
                            //            ViewData["ReturnUser"] = returnUser; // để Content/Header/Footer dùng khi in
                            //        }
                            //    }
                            //    ViewData["ListResultXN"] = _lstResult;
                            //    ViewData["Note"] = _lstResult[0].Note;
                            //    var _hospital = await _hospitalBL.GetHospital();
                            //    var content = await this.RenderViewAsync("Content", _hospital);
                            //    var header = await this.RenderViewAsync("Header", _hospital);
                            //    var footer = await this.RenderViewAsync("Footer", _hospital);
                            //    var _folder = Path.Combine(_environment.WebRootPath, "pdf", "xn");
                            //    var _file = Path.Combine(_folder, _lstResult[0]?.KeyResultForHis + ".pdf");
                            //    var fileBase64 = await _toolBL.ExportPdf_Result(_folder, _file, header, content, footer);
                            //    return Content(fileBase64);
                            //}
                            //catch { }
                            try
                            {
                                // === LẤY THÔNG TIN BÁC SĨ THỰC HIỆN (ReturnUser) ===
                                if (lstResult[0].userReturnResult > 0)
                                {
                                    var returnUser = await _userBL.GetUser(lstResult[0].userReturnResult);
                                    if (returnUser != null)
                                    {
                                        ViewData["ReturnUser"] = returnUser;
                                    }
                                }

                                ViewData["Note"] = _lstResult[0].Note;

                                var _hospital = await _hospitalBL.GetHospital();

                                var lstNormalResult = _lstResult
                                    .Where(x => x.ServiceId != urineServiceId)
                                    .ToList();

                                var lstUrineResult = _lstResult
                                    .Where(x => x.ServiceId == urineServiceId)
                                    .ToList();

                                var contentParts = new List<string>();

                                // Có nước tiểu hay không
                                var hasUrineResult = lstUrineResult.Any();


                                if (lstNormalResult.Any())
                                {
                                    ViewData["ListResultXN"] = lstNormalResult;
                                    ViewData["ShowNote"] = !hasUrineResult;
                                    ViewData["DisableThead"] = false;

                                    var normalContent = await this.RenderViewAsync("Content", _hospital);
                                    contentParts.Add(normalContent);
                                }

                                // 2. Render nhóm nước tiểu ở trang riêng
                                if (lstUrineResult.Any())
                                {
                                    ViewData["ListResultXN"] = lstUrineResult;
                                    ViewData["ShowNote"] = true;

                                    // Quan trọng: nước tiểu không dùng <thead>
                                    ViewData["DisableThead"] = true;

                                    var urineContent = await this.RenderViewAsync("Content", _hospital);

                                    if (contentParts.Any())
                                    {
                                        contentParts.Add($@"
                                            <div style='page-break-before: always; break-before: page; clear: both; display: block; height: 1px;'></div>
                                            <div class='xn-urine-page'>
                                                {urineContent}
                                            </div>
                                        ");
                                    }
                                    else
                                    {
                                        contentParts.Add($@"
                                            <div class='xn-urine-page'>
                                                {urineContent}
                                            </div>
                                        ");
                                    }
                                }

                                var content = string.Join("", contentParts); 

                                var header = await this.RenderViewAsync("Header", _hospital);
                                var footer = await this.RenderViewAsync("Footer", _hospital);

                                var _folder = Path.Combine(_environment.WebRootPath, "pdf", "xn");
                                var _file = Path.Combine(_folder, _lstResult[0]?.KeyResultForHis + ".pdf");

                                var fileBase64 = await _toolBL.ExportPdf_Result(_folder, _file, header, content, footer);
                                return Content(fileBase64);
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
            return Content(string.Empty);
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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> PreviewValidPrint(
            long patientId,
            string sid,
            string returnResultTime,
            long? userReturnResult,
            string note)
        {
            if (patientId <= 0)
            {
                return Content("Thiếu thông tin bệnh nhân.", "text/plain; charset=utf-8");
            }

            // Lấy danh sách kết quả dùng chung với luồng ValidPrint
            var lstResult = await _resultXNBL.GetListResultXNByPatientId_ForValidPrint(patientId);
            if (lstResult == null || lstResult.Count == 0)
            {
                return Content("Không tìm thấy dữ liệu kết quả để xem trước.", "text/plain; charset=utf-8");
            }

            // Ghi đè ghi chú nếu từ màn hình có truyền lên
            if (!string.IsNullOrWhiteSpace(note))
            {
                foreach (var item in lstResult)
                {
                    item.Note = note;
                }
            }

            // Lấy thông tin người trả kết quả
            if (userReturnResult.HasValue && userReturnResult.Value > 0)
            {
                var returnUser = await _userBL.GetUser(userReturnResult.Value);
                if (returnUser != null)
                {
                    ViewData["ReturnUser"] = returnUser;
                }
            }

            // Set ViewData dùng trong 3 template
            ViewData["ListResultXN"] = lstResult;
            ViewData["Note"] = !string.IsNullOrWhiteSpace(note) ? note : lstResult[0].Note;
            ViewData["Sid"] = sid;
            ViewData["ReturnResultTime"] = returnResultTime;

            var hospital = await _hospitalBL.GetHospital();

            // View wrapper include 3 template Header/Content/Footer
            // Không tạo PDF, chỉ render HTML
            return View("_XN_ValidPrint_Preview", hospital);
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

        // ========================= IMPORT EXTERNAL LAB PDF - ONLY SAVE FILE=========================
        // Yêu cầu NuGet: UglyToad.PdfPig (để trích text PDF)
        // using UglyToad.PdfPig;
        // using UglyToad.PdfPig.Content;

        [HttpPost]
        [Authorize]
        [DisableRequestSizeLimit] // tùy cân nhắc
        public async Task<IActionResult> ImportExternalLabPdfOnlySave(string pMaBenhAn, string pId, IFormFile file, bool overwrite = false, bool markValid = false, string vendor = null)
        {
            if (pMaBenhAn == null) return Json(new { success = false, message = "Thiếu Mã Bệnh Án." });
            if (file == null || file.Length == 0) return Json(new { success = false, message = "Không có tệp PDF được gửi lên." });

            try
            {
                // 1) Tạo thư mục lưu trữ
                var tmpFolder = Path.Combine(_environment.WebRootPath, "uploads", "external_lab");
                if (!Directory.Exists(tmpFolder)) Directory.CreateDirectory(tmpFolder);

                // 2) Tạo tên file với vendor nếu có
                var safeVendor = SanitizeFilePart(vendor);
                var fileName = string.IsNullOrEmpty(safeVendor)
                    ? $"patient_{pMaBenhAn}_{DateTime.Now:yyyyMMddHHmmss}.pdf"
                    : $"patient_{pMaBenhAn}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                var filePath = Path.Combine(tmpFolder, fileName);

                // 3) Lưu file PDF
                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fs);
                }

                // 4) Trả về kết quả thành công
                return Json(new
                {
                    success = true,
                    message = "Lưu file PDF thành công.",
                    fileName = fileName,
                    filePath = $"~/uploads/external_lab/{fileName}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ImportExternalLabPdf failed");
                return Json(new { success = false, message = "Lỗi khi lưu file PDF." });
            }
        }

        // ========================= IMPORT EXTERNAL LAB PDF =========================
        // Yêu cầu NuGet: UglyToad.PdfPig (để trích text PDF)
        // using UglyToad.PdfPig;
        // using UglyToad.PdfPig.Content;

        [HttpPost]
        [Authorize]
        [DisableRequestSizeLimit] // tùy cân nhắc
        public async Task<IActionResult> ImportExternalLabPdf(string pMaBenhAn, string pId, long pidTablePatient, string pName, IFormFile file, bool overwrite = false,
            bool markValid = false, bool isVisibleToUser = true)
        {
            if (pMaBenhAn == null) return Json(new { success = false, message = "Thiếu pMaBenhAn." });
            if (file == null || file.Length == 0) return Json(new { success = false, message = "Không có tệp PDF được gửi lên." });

            try
            {
                // 1) Lưu file theo cấu trúc: uploads/external_lab/{pId}/{pMaBenhAn}/file
                var sanitizedPId = SanitizeFilePart(pId) ?? "unknown";
                var sanitizedMaBenhAn = SanitizeFilePart(pMaBenhAn) ?? "unknown";
                var tmpFolder = Path.Combine(_environment.WebRootPath, "uploads", "external_lab", sanitizedPId, sanitizedMaBenhAn);
                if (!Directory.Exists(tmpFolder)) Directory.CreateDirectory(tmpFolder);

                if (!Directory.Exists(tmpFolder)) Directory.CreateDirectory(tmpFolder);
                // Format pName: bỏ dấu và khoảng trắng
                var sanitizedName = RemoveVietnameseAccentsAndSpaces(pName);
                var fileName = $"patient_{pMaBenhAn}_{sanitizedName}_{DateTime.Now:yyyyMMddHHmmss}.pdf";

                var tempPath = Path.Combine(tmpFolder, fileName);
                using (var fs = new FileStream(tempPath, FileMode.Create))
                {
                    await file.CopyToAsync(fs);
                }

                var userLogin = GetUserLogin();

                // 2) Lưu metadata file vào ExternalFiles
                var relativePath = _externalFileBL.BuildRelativePath(
                    rootFolderName: "external_lab",
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
                    moduleType: ExternalFileModule.Lab,
                    patientName: pName,
                    createdBy: userLogin,
                    isVisibleToUser: isVisibleToUser
                );
                int updatedCount = 0;
                bool parsedSuccess = false;

                // 3) Đọc text PDF + parse + update kết quả
                //    Đây là bước phụ, lỗi không làm fail import file
                try
                {
                    string allText = ExtractTextFromPdf(tempPath);

                    var parsed = _parserFactory.ParseByName("TamAnLab", allText);

                    if (parsed != null && parsed.Count > 0)
                    {
                        updatedCount = await _resultXNBL.UpdateFromExternal(pidTablePatient, parsed, overwrite, markValid, userLogin);
                        parsedSuccess = true;

                        var full = await _resultXNBL.FullResultXN(pidTablePatient);
                        // await _patientBL.Update(pidTablePatient, null, null, userLogin, full);
                    }
                }
                catch (Exception exParse)
                {
                    _logger.LogWarning(exParse,
                        "ImportExternalLabPdf parse/update skipped. pId={PId}, maBenhAn={MaBenhAn}, fileName={FileName}",
                        pId, pMaBenhAn, fileName);
                }

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
                _logger.LogError(ex, "ImportExternalLabPdf failed");
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

        //#region Lấy danh sách file pdf đã import
        //[HttpGet]
        //public IActionResult GetExternalLabFiles(string pMaBenhAn, string pId, long patientId) // patientId để kiểu long là sai nhưng để vày để load những ca đã import sai trước đó lên. Chủ yếu dùng pMaBenhAn
        //{
        //    if (pMaBenhAn == null)
        //        return Json(new { success = false, message = "Thiếu pMaBenhAn." });

        //    var sanitizedPId = SanitizeFilePart(pId) ?? "unknown";
        //    var sanitizedMaBenhAn = SanitizeFilePart(pMaBenhAn) ?? "unknown";

        //    // Folder mới theo cấu trúc: uploads/external_lab/{pId}/{pMaBenhAn}/
        //    var newFolder = Path.Combine(_environment.WebRootPath, "uploads", "external_lab", sanitizedPId, sanitizedMaBenhAn);

        //    // Folder cũ (flat) để hỗ trợ file đã import trước đó
        //    var legacyFolder = Path.Combine(_environment.WebRootPath, "uploads", "external_lab");

        //    List<dynamic> files = new List<dynamic>();

        //    // --- Tìm file trong folder mới ---
        //    if (Directory.Exists(newFolder))
        //    {
        //        var filesInNewFolder = Directory.EnumerateFiles(newFolder, "*.pdf", SearchOption.TopDirectoryOnly)
        //                             .OrderByDescending(System.IO.File.GetCreationTimeUtc)
        //                             .Take(100)
        //                             .Select(full =>
        //                             {
        //                                 var name = Path.GetFileName(full);
        //                                 var url = Url.Content($"~/uploads/external_lab/{sanitizedPId}/{sanitizedMaBenhAn}/{name}");
        //                                 var created = System.IO.File.GetCreationTime(full);
        //                                 string vendor = null;
        //                                 var parts = Path.GetFileNameWithoutExtension(name).Split('_');
        //                                 if (parts.Length >= 4) vendor = parts[3];
        //                                 return new { name, url, created, vendor };
        //                             }).ToList();
        //        files.AddRange(filesInNewFolder);
        //    }

        //    // --- Tìm file cũ trong folder legacy (flat) để tương thích ngược ---
        //    if (Directory.Exists(legacyFolder))
        //    {
        //        var pattern = $"patient_{pMaBenhAn}_*.pdf";
        //        var filesByMaBenhAn = Directory.EnumerateFiles(legacyFolder, pattern, SearchOption.TopDirectoryOnly)
        //                             .OrderByDescending(System.IO.File.GetCreationTimeUtc)
        //                             .Take(100)
        //                             .Select(full =>
        //                             {
        //                                 var name = Path.GetFileName(full);
        //                                 var url = Url.Content($"~/uploads/external_lab/{name}");
        //                                 var created = System.IO.File.GetCreationTime(full);
        //                                 string vendor = null;
        //                                 var parts = Path.GetFileNameWithoutExtension(name).Split('_');
        //                                 if (parts.Length >= 4) vendor = parts[3];
        //                                 return new { name, url, created, vendor };
        //                             }).ToList();
        //        files.AddRange(filesByMaBenhAn);

        //        pattern = $"patient_{patientId}_*.pdf";
        //        var filesByPatientId = Directory.EnumerateFiles(legacyFolder, pattern, SearchOption.TopDirectoryOnly)
        //                         .OrderByDescending(System.IO.File.GetCreationTimeUtc)
        //                         .Take(100)
        //                         .Select(full =>
        //                         {
        //                             var name = Path.GetFileName(full);
        //                             var url = Url.Content($"~/uploads/external_lab/{name}");
        //                             var created = System.IO.File.GetCreationTime(full);
        //                             string vendor = null;
        //                             var parts = Path.GetFileNameWithoutExtension(name).Split('_');
        //                             if (parts.Length >= 4) vendor = parts[3];
        //                             return new { name, url, created, vendor };
        //                         }).ToList();
        //        files.AddRange(filesByPatientId);
        //    }

        //    // --- 3️⃣ Gộp và sắp xếp (loại trùng theo tên file nếu có) ---
        //    var merged = files
        //        .GroupBy(f => (string)f.name) // loại trùng nếu 2 file trùng tên
        //        .Select(g => g.First())
        //        .OrderByDescending(f => f.created)
        //        .Take(100)
        //        .ToList();
        //    return Json(new { success = true, merged });
        //}

        #region Lấy danh sách file pdf đã import
        [HttpGet]
        public async Task<IActionResult> GetExternalLabFiles(string pMaBenhAn, string pId, long pidTablePatient)
        {
            if (string.IsNullOrWhiteSpace(pMaBenhAn))
                return Json(new { success = false, message = "Thiếu pMaBenhAn." });

            var sanitizedPId = SanitizeFilePart(pId) ?? "unknown";
            var sanitizedMaBenhAn = SanitizeFilePart(pMaBenhAn) ?? "unknown";

            var newFolder = Path.Combine(_environment.WebRootPath, "uploads", "external_lab", sanitizedPId, sanitizedMaBenhAn);
            var legacyFolder = Path.Combine(_environment.WebRootPath, "uploads", "external_lab");

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

                    physicalFiles.AddRange(filesInNewFolder.Select(full => MapPhysicalLabFile(
                        fullPath: full,
                        isLegacy: false,
                        sanitizedPId: sanitizedPId,
                        sanitizedMaBenhAn: sanitizedMaBenhAn
                    )));
                }

                // ========= 2) Scan folder cũ (legacy flat) =========
                if (Directory.Exists(legacyFolder))
                {
                    var patternByMaBenhAn = $"patient_{pMaBenhAn}_*.pdf";
                    var filesByMaBenhAn = Directory.EnumerateFiles(legacyFolder, patternByMaBenhAn, SearchOption.TopDirectoryOnly)
                        .OrderByDescending(System.IO.File.GetCreationTimeUtc)
                        .Take(100)
                        .ToList();

                    physicalFiles.AddRange(filesByMaBenhAn.Select(full => MapPhysicalLabFile(
                        fullPath: full,
                        isLegacy: true,
                        sanitizedPId: sanitizedPId,
                        sanitizedMaBenhAn: sanitizedMaBenhAn
                    )));

                    var patternByPatientId = $"patient_{pidTablePatient}_*.pdf";
                    var filesByPatientId = Directory.EnumerateFiles(legacyFolder, patternByPatientId, SearchOption.TopDirectoryOnly)
                        .OrderByDescending(System.IO.File.GetCreationTimeUtc)
                        .Take(100)
                        .ToList();

                    physicalFiles.AddRange(filesByPatientId.Select(full => MapPhysicalLabFile(
                        fullPath: full,
                        isLegacy: true,
                        sanitizedPId: sanitizedPId,
                        sanitizedMaBenhAn: sanitizedMaBenhAn
                    )));
                }

                // ========= 3) Loại trùng file vật lý theo relative path =========
                physicalFiles = physicalFiles
                    .GroupBy(x => x.RelativePath, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.OrderByDescending(x => x.Created).First())
                    .OrderByDescending(x => x.Created)
                    .Take(100)
                    .ToList();

                // ========= 4) Sync metadata nếu thiếu =========
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
                            tablePatientId: pidTablePatient,
                            moduleType: ExternalFileModule.Lab,
                            patientName: null,
                            createdBy: userLogin,
                            isVisibleToUser: true
                        );
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Sync metadata failed in GetExternalLabFiles. pMaBenhAn={MaBenhAn}, pId={PId}, pidTablePatient={PidTablePatient}",
                        pMaBenhAn, pId, pidTablePatient);
                }

                // ========= 5) Query lại DB sau khi sync =========
                var dbFiles = await _externalFileBL.GetAllFilesAsync(
                    tablePatientId: pidTablePatient,
                    pId: pId,
                    maBenhAn: pMaBenhAn,
                    moduleType: ExternalFileModule.Lab
                );

                var dbFileKeySet = dbFiles
                    .Select(x => BuildExternalFileCompareKey(x.StoredFileName, x.RelativePath))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                // ========= 6) Build response final =========
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
                _logger.LogError(ex, "GetExternalLabFiles failed. pMaBenhAn={MaBenhAn}, pId={PId}, pidTablePatient={PidTablePatient}",
                    pMaBenhAn, pId, pidTablePatient);

                return Json(new
                {
                    success = false,
                    message = "Lỗi xử lý lấy danh sách file import."
                });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteExternalLabFile(string fileName, string pMaBenhAn, string pId)
        {
            if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(pMaBenhAn))
                return Json(new { success = false, message = "Thiếu thông tin file hoặc mã bệnh án." });

            try
            {
                var sanitizedPId = SanitizeFilePart(pId) ?? "unknown";
                var sanitizedMaBenhAn = SanitizeFilePart(pMaBenhAn) ?? "unknown";

                string filePath = null;
                string relativePath = null;

                // 1) Tìm file trong folder mới trước
                var newFolder = Path.Combine(_environment.WebRootPath, "uploads", "external_lab", sanitizedPId, sanitizedMaBenhAn);
                var newFilePath = Path.Combine(newFolder, fileName);

                if (System.IO.File.Exists(newFilePath))
                {
                    filePath = newFilePath;
                    relativePath = _externalFileBL.BuildRelativePath(
                        rootFolderName: "external_lab",
                        pId: sanitizedPId,
                        maBenhAn: sanitizedMaBenhAn,
                        storedFileName: fileName
                    );
                }
                else
                {
                    // 2) Fallback folder cũ (legacy)
                    var legacyFolder = Path.Combine(_environment.WebRootPath, "uploads", "external_lab");
                    var legacyFilePath = Path.Combine(legacyFolder, fileName);

                    if (System.IO.File.Exists(legacyFilePath))
                    {
                        filePath = legacyFilePath;
                        relativePath = $"uploads/external_lab/{fileName}".Replace("\\", "/");
                    }
                }

                // 3) Nếu không có file vật lý, vẫn thử mark IsDeleted trong DB theo 2 kiểu path
                if (string.IsNullOrWhiteSpace(relativePath))
                {
                    var possibleNewRelativePath = _externalFileBL.BuildRelativePath(
                        rootFolderName: "external_lab",
                        pId: sanitizedPId,
                        maBenhAn: sanitizedMaBenhAn,
                        storedFileName: fileName
                    );

                    var possibleLegacyRelativePath = $"uploads/external_lab/{fileName}".Replace("\\", "/");

                    var deletedInDb = await _externalFileBL.SoftDeleteByFileAsync(
                                          storedFileName: fileName,
                                          relativePath: possibleNewRelativePath,
                                          moduleType: ExternalFileModule.Lab)
                                      || await _externalFileBL.SoftDeleteByFileAsync(
                                          storedFileName: fileName,
                                          relativePath: possibleLegacyRelativePath,
                                          moduleType: ExternalFileModule.Lab);

                    if (deletedInDb)
                    {
                        return Json(new { success = true, message = "Đã đánh dấu xóa file trong hệ thống." });
                    }

                    return Json(new { success = false, message = "File không tồn tại." });
                }

                // 4) Xóa file vật lý
                System.IO.File.Delete(filePath);

                // 5) Mark IsDeleted trong DB
                await _externalFileBL.SoftDeleteByFileAsync(
                    storedFileName: fileName,
                    relativePath: relativePath,
                    moduleType: ExternalFileModule.Lab
                );

                _logger.LogInformation("Deleted external lab file: {FileName} for patient: {MaBenhAn}", fileName, pMaBenhAn);

                return Json(new { success = true, message = "Xóa file thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting external lab file: {FileName}", fileName);
                return Json(new { success = false, message = "Lỗi khi xóa file." });
            }
        }
        #endregion
        [HttpPost]
        public async Task<IActionResult> SaveExternalLabFileVisibility(long id, bool isVisibleToUser)
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
                _logger.LogError(ex, "SaveExternalLabFileVisibility failed. id={Id}", id);
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
        private ExternalLabPhysicalFileVm MapPhysicalLabFile(
            string fullPath,
            bool isLegacy,
            string sanitizedPId,
            string sanitizedMaBenhAn)
        {
            var name = Path.GetFileName(fullPath);
            var created = System.IO.File.GetCreationTime(fullPath);

            var parts = Path.GetFileNameWithoutExtension(name).Split('_');

            string relativePath;
            string url;

            if (isLegacy)
            {
                relativePath = $"uploads/external_lab/{name}".Replace("\\", "/");
                url = Url.Content($"~/uploads/external_lab/{name}");
            }
            else
            {
                relativePath = $"uploads/external_lab/{sanitizedPId}/{sanitizedMaBenhAn}/{name}".Replace("\\", "/");
                url = Url.Content($"~/uploads/external_lab/{sanitizedPId}/{sanitizedMaBenhAn}/{name}");
            }

            return new ExternalLabPhysicalFileVm
            {
                Name = name,
                FullPath = fullPath,
                RelativePath = relativePath,
                Url = url,
                Created = created,
            };
        }
        // Demo parser rất đơn giản: tùy chỉnh theo layout thực tế của PDF
        //private static List<ExternalLabItem> TamAnLabParser(string text)
        //{
        //    var result = new List<ExternalLabItem>();
        //    if (string.IsNullOrWhiteSpace(text)) return result;

        //    // 1) Chuẩn hoá để phá "dính cột"
        //    string s = text;
        //    s = Regex.Replace(s, @"([()])", " $1 ");                  // thêm space quanh ( )
        //    s = Regex.Replace(s, @"(?<=[A-Za-zµ%])(?=\d)", " ");      // Chữ→Số
        //    s = Regex.Replace(s, @"(?<=\d)(?=[A-Za-zµ%])", " ");      // Số→Chữ
        //    //s = Regex.Replace(s, @"(?<=[a-z])(?=[A-Z])", " ");        // a→A
        //    s = Regex.Replace(s, @"\s+", " ").Trim();                 // gộp trắng

        //    // 2) Token hoá theo space
        //    var tokens = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        //    int n = tokens.Length;

        //    // 3) Quét cửa sổ trượt để bắt mã xét nghiệm
        //    for (int i = 0; i < n; i++)
        //    {
        //        string code = null; int codeLen = 0;

        //        // --- Các mã 1 token
        //        if (Eq(tokens[i], "HBsAg")) { code = "HBsAg"; codeLen = 1; }
        //        else if (Eq(tokens[i], "CEA")) { code = "CEA"; codeLen = 1; }

        //        // --- Mã 2 token: CA 125, CA 15-3, Anti HBS
        //        else if (Eq(tokens[i], "CA") && i + 1 < n && (Eq(tokens[i + 1], "15-") && Eq(tokens[i + 2], "3")))
        //        { code = $"CA {tokens[i + 1]}" + " " +$"{tokens[i + 2]}"; codeLen = 3; }
        //        else if (Eq(tokens[i], "CA") && i + 1 < n && (Eq(tokens[i + 1], "125")))
        //        { code = $"CA {tokens[i + 1]}"; codeLen = 2; }
        //        else if (Eq(tokens[i], "Anti") && i + 1 < n && Eq(tokens[i + 1], "HBS"))
        //        { code = "Anti HBS"; codeLen = 2; }

        //        if (code == null) continue;

        //        // Bỏ qua STT ngay sau code nếu có (toàn số)
        //        int k = i + codeLen;
        //        if (k < n && IsDigits(tokens[k])) k++;

        //        int t = k;
        //        string qualifier = null, value = null, unit = null, refRange = null;

        //        // Check riêng code = HBsAg sẽ có Qualifier nằm ở vị trí t + 2
        //        if (code == "HBsAg")
        //        {
        //            t = t + 1;

        //            // Qualifier: Negative | Positive | Âm | Dương
        //            if (t >= 0 && IsQualifier(tokens[t])) { qualifier = tokens[t]; t --; }

        //            // Value (số): 16,30 | 1.50 | 0.41
        //            if (t >= 0) { value = NormalizeNumber(tokens[t]); t--; }
        //        } else
        //        {

        //            // Qualifier: Negative | Positive | Âm | Dương
        //            if (t >= 0 && IsQualifier(tokens[t])) { qualifier = tokens[t]; t--; }

        //            // Value (số): 16,30 | 1.50 | 0.41
        //            if (t >= 0 ) { value = NormalizeNumber(tokens[t]); t--; }
        //        }



        //        //// Ref range: ... )  ...  (
        //        //if (t >= 0 && tokens[t] == ")")
        //        //{
        //        //    int end = t; t--;
        //        //    var parts = new List<string>();
        //        //    while (t >= 0 && tokens[t] != "(") { parts.Add(tokens[t]); t--; }
        //        //    if (t >= 0 && tokens[t] == "(")
        //        //    {
        //        //        parts.Reverse();
        //        //        refRange = string.Join(" ", parts);
        //        //        t--; // lùi qua '('
        //        //    }
        //        //}

        //        // Unit: token trước ref hoặc trước value (U/mL, ng/mL, COI, …)
        //        if (t >= 0) unit = tokens[t];

        //        // 5) Kết quả hợp lệ thì add
        //        if (!string.IsNullOrEmpty(code) && (!string.IsNullOrEmpty(value) || !string.IsNullOrEmpty(qualifier)))
        //        {
        //            var finalValue = string.IsNullOrEmpty(qualifier) ? value : $"{value ?? ""} {qualifier}".Trim();
        //            result.Add(new ExternalLabItem
        //            {
        //                Code = code,
        //                Name = code,
        //                Value = finalValue ?? "", // có thể là số | qualifier | số - qualifier
        //                //Unit = unit ?? "",
        //                //RefRange = refRange ?? ""
        //            });
        //        }
        //        // tiếp tục quét; không cần nhảy i vì có thể có nhiều chỉ số trong cùng trang
        //    }

        //    return result;
        //}
        //// ================= helpers =================
        //static bool Eq(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        //static bool IsDigits(string s) => s.All(char.IsDigit);
        //static bool IsNumberLike(string s) => Regex.IsMatch(s, @"^\d+([.,]\d+)?$");
        //static string NormalizeNumber(string s) => (s ?? "").Replace(",", ".");
        //static bool IsQualifier(string s)
        //{
        //    if (string.IsNullOrWhiteSpace(s)) return false;
        //    var x = s.ToLowerInvariant();
        //    return x == "negative" || x == "positive" || x == "âm" || x == "dương";
        //}
        //private static readonly Dictionary<string, string> PartnerNameToInternalCode = new(StringComparer.OrdinalIgnoreCase)
        //{
        //    // Map tên hiển thị trong PDF → Code xét nghiệm nội bộ
        //    // Điều chỉnh theo hệ thống của bạn
        //    ["HBsAg"] = "HBsAg",
        //    ["Anti HBS"] = "ANTI_HBS",  // hoặc "Anti-HBs"
        //    ["CA 15-3"] = "CA15-3",
        //    ["CA 125"] = "CA-125",
        //    ["CEA"] = "CEA"
        //};
        //// Một số chuẩn hoá tên để dễ map
        //private static string NormalizeTestName(string raw)
        //{
        //    if (string.IsNullOrWhiteSpace(raw)) return raw?.Trim() ?? "";
        //    var name = raw.Trim();

        //    // Fix các biến thể hay gặp ở đối tác
        //    name = name.Replace("CA 15- 3", "CA 15-3", StringComparison.OrdinalIgnoreCase);
        //    name = name.Replace("Anti HBS", "Anti HBS", StringComparison.OrdinalIgnoreCase); // giữ nguyên
        //    name = name.Replace("CA125", "CA 125", StringComparison.OrdinalIgnoreCase);

        //    return name;
        //}
    }
}
