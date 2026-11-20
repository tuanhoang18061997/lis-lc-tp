using Management.BL;
using Management.Models;
using Management.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SelectPdf;
using System;
using System.Drawing;
using System.IO;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using ZXing.QrCode.Internal;
using static Management.Services.IBarcodeService;
using static System.Net.Mime.MediaTypeNames;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Management.Controllers
{
    public class SA_ProcessController : Controller
    {
        private readonly ILogger<SA_ProcessController> _logger;
        private readonly PatientCDHABL _patientCDHABL;
        private readonly ObjectBL _objectBL;
        private readonly LocationBL _locationBL;
        private readonly DoctorBL _doctorBL;
        private readonly UserBL _userBL;
        private readonly CategoryBL _categoryBL;
        private readonly ServiceBL _serviceBL;
        public readonly ResultCDHABL _resultCDHABL;
        private readonly SettingBL _settingBL;
        private readonly GroupBL _groupBL;
        private readonly ToolBL _toolBL;
        private readonly HospitalBL _hospitalBL;
        private readonly SampleBL _sampleBL;
        private readonly IWebHostEnvironment _environment;
        public readonly DeviceBL _deviceBL;
        public readonly string _SA = "SA";

        public SA_ProcessController(ILogger<SA_ProcessController> logger, PatientCDHABL patientCDHABL, ObjectBL objectBL, LocationBL locationBL,
                            DoctorBL doctorBL, UserBL userBL, CategoryBL categoryBL, ServiceBL serviceBL, ResultCDHABL resultCDHABL,
                            SettingBL settingBL, GroupBL groupBL, IWebHostEnvironment environment, ToolBL toolBL, HospitalBL hospitalBL, SampleBL sampleBL, DeviceBL deviceBL)
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
            _toolBL = toolBL;
            _hospitalBL = hospitalBL;
            _sampleBL = sampleBL;
            _deviceBL = deviceBL;
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

            //ViewData["lstPatient"] = await _patientCDHABL.Get_ListPatient(from, to, false, true, false, _SA);
            ViewData["lstPatient"] = await _patientCDHABL.Get_ListPatient_Flexible(from, to, _SA, "process");
            ViewData["lstUser"] = await _userBL.GetListUser(UserTypeModel.SA);
            ViewData["lstBenhAnModel"] = await BenhAnModel.GetListBenhAnModel();
            ViewData["lstSampleResult"] = await _sampleBL.GetListSampleByCategory(_SA);
            ViewData["lstDevice"] = await _deviceBL.GetList();

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

            var countGetSample = await _patientCDHABL.Get_CountPatient_New(from, to, true, false, false, _SA);
            var countProcess = await _patientCDHABL.Get_CountPatient_New(from, to, false, true, false, _SA);
            var countReturnResult = await _patientCDHABL.Get_CountPatient_New(from, to, false, false, true, _SA);
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
            ViewData["lstPatient"] = await _patientCDHABL.Get_ListPatientByPidOrSid_New(from, to, false, true, false, null, _SA);

            return PartialView("_SA_Process_ListPatient");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Search(string pidorseq, DateTime timeSearchFrom, DateTime timeSearchTo)
        {
            // Save selected dates to session/cookies
            SaveSearchDatesToSession(timeSearchFrom, timeSearchTo);

            var from = new DateTime(timeSearchFrom.Year, timeSearchFrom.Month, timeSearchFrom.Day, 00, 00, 00);
            var to = new DateTime(timeSearchTo.Year, timeSearchTo.Month, timeSearchTo.Day, 23, 59, 59);
            //ViewData["lstPatient"] = await _patientCDHABL.Get_ListPatientByPidOrSid(from, to, false, true, false, pidorseq, _SA);
            ViewData["lstPatient"] = await _patientCDHABL.Get_ListPatientByPidOrSid_New(from, to, false, true, false, pidorseq, _SA);

            return PartialView("_SA_Process_ListPatient");
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
            var _getSample = await _patientCDHABL.GetSample_ProcessResult_ReturnResult(id, true, false, false, _userLogin.Value, _SA);
            return Content(_getSample.ToString());
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetPatientInfo(long id)
        {
            var userLoginId = this.GetUserLogin();          // <- user đang login
            ViewData["userLoginId"] = userLoginId;
            ViewData["lstUser"] = await _userBL.GetListUser(UserTypeModel.SA);
            ViewData["lstBenhAnModel"] = await BenhAnModel.GetListBenhAnModel();
            ViewData["patientInfo"] = await _patientCDHABL.Get_PatientBySid(id);

            return PartialView("_SA_Process_PatientInfo");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetServiceForPatient(long patientId)
        {
            ViewData["lstResultCDHAs"] = await _resultCDHABL.GetListResultCDHAByPatientId(patientId, _SA);
            return PartialView("_SA_Process_ListService");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetImageForService(long id)
        {
            var _resultCDHA = await _resultCDHABL.GetResultCDHA(id);
            if (_resultCDHA != null)
            {
                var _lstItem = new List<object>();
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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetSampleForService(long id)
        {
            var _sample = await _sampleBL.GetSample(id);
            var _item = new { Description = _sample.Description, Result = _sample.Result, Suggest = _sample.Suggest };
            return Json(_item);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SaveImageCDHA([FromBody] ImageModel imageObj)
        {
            var guid = Guid.NewGuid();
            var _folder = Path.Combine(_environment.WebRootPath, "images", "result", "sa");
            var _imagePath1 = Path.Combine(_folder, imageObj.resultCDHAId + "-" + guid + ".png");
            var _image = _toolBL.Base64ToImage(imageObj.imageString);
            var _saveImage = _toolBL.SaveImage(_folder, _imagePath1, _image);

            if (_saveImage)
            {
                var _imagePath = "/images/result/sa/" + imageObj.resultCDHAId + "-" + guid + ".png";
                _saveImage = await _resultCDHABL.SaveImage(long.Parse(imageObj.resultCDHAId), _imagePath, _imagePath1);
            }
            return Content(_saveImage.ToString());
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteImageCDHA(long imageCDHAId)
        {
            var _imagePath1_resultCDHAId = await _resultCDHABL.DeleteImage(imageCDHAId);
            var _imagePath1 = string.Empty;
            var _resultCDHAId = string.Empty;
            if (!string.IsNullOrEmpty(_imagePath1_resultCDHAId))
            {
                _imagePath1 = _imagePath1_resultCDHAId.Split(';')[0];
                var _delete = _toolBL.DeleteImage(_imagePath1);
                if (_delete) _resultCDHAId = _imagePath1_resultCDHAId.Split(';')[1];
            }
            return Content(_resultCDHAId);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SaveResult([FromBody] ResultCDHAModel resultCDHA)
        {
            var _saveResult = false;
            if (resultCDHA != null)
            {
                var _dateTime = ToolBL.Get_DateNow();
                var _userLogin = this.GetUserLogin();
                var _device = this.GetSession_Device();
                await _patientCDHABL.Update(resultCDHA.patientId, resultCDHA.returnResultTime, resultCDHA.userReturnResult, _userLogin.Value, _dateTime, _SA);
                _saveResult = await _resultCDHABL.Update(resultCDHA, resultCDHA.userReturnResult, _dateTime, _device);
            }
            return Content(_saveResult.ToString());
        }

        //[HttpPost]
        //[Authorize]
        //public async Task<IActionResult> ValidPrint([FromBody] ResultCDHAModel resultCDHA)
        //{
        //    if (resultCDHA != null)
        //    {
        //        var _dateTime = ToolBL.Get_DateNow();
        //        var _userLogin = this.GetUserLogin();
        //        var _device = this.GetSession_Device();
        //        if (await _resultCDHABL.Update(resultCDHA, resultCDHA.userReturnResult, _dateTime, _device))
        //        {
        //            if (await _patientCDHABL.Update(resultCDHA.patientId, resultCDHA.returnResultTime, resultCDHA.userReturnResult, false, false, true, _userLogin.Value, _SA))
        //            {
        //                var _result = await _resultCDHABL.GetResultCDHAByPatientId_ForValidPrint(resultCDHA.resultCDHAId);
        //                if (_result != null)
        //                {
        //                    try
        //                    {
        //                        // === LẤY THÔNG TIN BÁC SĨ THỰC HIỆN (ReturnUser) ===
        //                        if (resultCDHA.userReturnResult > 0)   // vì userReturnResult là kiểu int/long
        //                        {
        //                            var returnUser = await _userBL.GetUser(resultCDHA.userReturnResult);
        //                            if (returnUser != null)
        //                            {
        //                                ViewData["ReturnUser"] = returnUser; // để Content/Header/Footer dùng khi in
        //                            }
        //                        }
        //                        // ========== FILTER ẢNH THEO DANH SÁCH NGƯỜI DÙNG CHỌN ==========
        //                        if (resultCDHA.selectedImageIds != null && resultCDHA.selectedImageIds.Count > 0 && _result.ImageCDHAs != null)
        //                        {
        //                            // Bảo toàn thứ tự người dùng chọn
        //                            var orderMap = resultCDHA.selectedImageIds
        //                                                .Select((id, idx) => new { id, idx })
        //                                                .ToDictionary(x => x.id, x => x.idx);

        //                            // Lọc & sắp xếp theo thứ tự người dùng
        //                            _result.ImageCDHAs = _result.ImageCDHAs
        //                                .Where(img => orderMap.ContainsKey(img.Id))         // nếu Id là string: img.ImageCDHAIdString
        //                                .OrderBy(img => orderMap[img.Id])
        //                                .ToList();
        //                        }
        //                        // ==================================================================
        //                        ViewData["ResultCDHA"] = _result;
        //                        var _hospital = await _hospitalBL.GetHospital();
        //                        var _serviceCDHA = _serviceBL.GetById(_result?.Service?.Id);
        //                        if(_serviceCDHA != null )
        //                        {
        //                            ViewData["ServiceCDHA"] = _serviceCDHA.Result;
        //                        }
        //                        else
        //                        {
        //                            ViewData["ServiceCDHA"] = null;
        //                        }
        //                        // In ngang
        //                        //var header = await this.RenderViewAsync("Header_Landscape", _hospital);
        //                        //var content_result = await this.RenderViewAsync("Content_Result_Landscape", _hospital);
        //                        //var content_image = await this.RenderViewAsync("Content_Image_Landscape", _hospital);
        //                        //var footer = await this.RenderViewAsync("Footer_Landscape", _hospital);
        //                        // In dọc
        //                        var header = await this.RenderViewAsync("Header", _hospital);
        //                        var content = await this.RenderViewAsync("Content", _hospital);
        //                        var footer = await this.RenderViewAsync("Footer", _hospital);
        //                        var _folder = Path.Combine(_environment.WebRootPath, "pdf", "sa");
        //                        var _file = Path.Combine(_folder, _result?.KeyResultForHis + ".pdf");
        //                        var fileBase64 = await _toolBL.ExportPdf_Result(_folder, _file, header, content, footer);
        //                        //var fileBase64 = await _toolBL.ExportPdf_Result_Landscape_OnePage_FromFiles(_folder, _file, header, content_result, content_image, footer, Path.Combine(_environment.WebRootPath, "pdf_templates", "Landscape2ColRaw_SA.html"));
        //                        return Content(fileBase64);
        //                    }
        //                    catch { }
        //                }
        //            }
        //        }
        //    }
        //    return Content(string.Empty);
        //}

        // Version mới của ValidPrint 18/09/2026
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ValidPrint([FromBody] ResultCDHAModel resultCDHA)
        {
            if (resultCDHA == null)
            {
                return Content(string.Empty);
            }

            var _dateTime = ToolBL.Get_DateNow();
            var _userLogin = this.GetUserLogin();
            var _device = this.GetSession_Device();

            // =========================================================
            // PHẢI có user login.
            // Không dùng .Value trước khi kiểm tra.
            // =========================================================
            if (!_userLogin.HasValue)
            {
                return Unauthorized();
            }

            // =========================================================
            // STEP 1:
            // Lưu nội dung kết quả hiện tại.
            //
            // ResultCDHABL.Update() vẫn chịu trách nhiệm kiểm tra:
            // CanEditCDHAAsync(resultCDHAId)
            //
            // Nếu service:
            // - chưa từng Valid -> cho sửa
            // - đã Invalid đúng quyền -> cho sửa
            // - đang Valid / không có quyền -> chặn
            // =========================================================
            var updated = await _resultCDHABL.Update(
                resultCDHA,
                resultCDHA.userReturnResult,
                _dateTime,
                _device);

            if (!updated)
            {
                return Content(string.Empty);
            }

            // =========================================================
            // STEP 2:
            // Valid đúng ResultCDHA hiện tại.
            //
            // ValidateCDHAAsync chịu trách nhiệm:
            // - IsValidated = true
            // - LastValidatedAt = server time
            // - LastValidatedByUserId = login user
            //
            // Đồng thời giữ aggregate state Patient:
            // - WaitSA = false
            // - ProcessSA = false
            // - ValidSA = true
            // - ReturnResultTimeSA
            // - UserReturnResultSA
            //
            // Sau cùng revoke CHỈ permission của ResultCDHA này.
            // Không revoke các service SA khác.
            // =========================================================
            var validated = await _resultCDHABL.ValidateCDHAAsync(
                resultCDHA,
                _SA,
                _userLogin.Value,
                _dateTime);

            if (!validated)
            {
                return Content(string.Empty);
            }

            // =========================================================
            // STEP 3:
            // Load lại chính ResultCDHA vừa Valid để tạo PDF.
            // =========================================================
            var _result =
                await _resultCDHABL.GetResultCDHAByPatientId_ForValidPrint(
                    resultCDHA.resultCDHAId);

            if (_result == null)
            {
                return Content(string.Empty);
            }

            try
            {
                // =====================================================
                // LẤY THÔNG TIN BÁC SĨ THỰC HIỆN / TRẢ KẾT QUẢ
                // =====================================================
                if (resultCDHA.userReturnResult > 0)
                {
                    var returnUser =
                        await _userBL.GetUser(
                            resultCDHA.userReturnResult);

                    if (returnUser != null)
                    {
                        ViewData["ReturnUser"] = returnUser;
                    }
                }

                // =====================================================
                // FILTER ẢNH THEO DANH SÁCH USER ĐÃ CHỌN
                //
                // Giữ đúng thứ tự selectedImageIds.
                // =====================================================
                if (resultCDHA.selectedImageIds != null &&
                    resultCDHA.selectedImageIds.Count > 0 &&
                    _result.ImageCDHAs != null)
                {
                    var orderMap =
                        resultCDHA.selectedImageIds
                            .Select((id, idx) => new
                            {
                                id,
                                idx
                            })
                            .ToDictionary(
                                x => x.id,
                                x => x.idx);

                    _result.ImageCDHAs =
                        _result.ImageCDHAs
                            .Where(img =>
                                orderMap.ContainsKey(img.Id))
                            .OrderBy(img =>
                                orderMap[img.Id])
                            .ToList();
                }

                // =====================================================
                // DỮ LIỆU CHO VIEW PDF
                // =====================================================
                ViewData["ResultCDHA"] = _result;

                var _hospital =
                    await _hospitalBL.GetHospital();

                var _serviceCDHA =
                    _serviceBL.GetById(
                        _result?.Service?.Id);

                if (_serviceCDHA != null)
                {
                    ViewData["ServiceCDHA"] =
                        _serviceCDHA.Result;
                }
                else
                {
                    ViewData["ServiceCDHA"] = null;
                }

                // =====================================================
                // IN DỌC
                // =====================================================
                var header =
                    await this.RenderViewAsync(
                        "Header",
                        _hospital);

                var content =
                    await this.RenderViewAsync(
                        "Content",
                        _hospital);

                var footer =
                    await this.RenderViewAsync(
                        "Footer",
                        _hospital);

                // =====================================================
                // EXPORT PDF
                // =====================================================
                var _folder = Path.Combine(
                    _environment.WebRootPath,
                    "pdf",
                    "sa");

                var _file = Path.Combine(
                    _folder,
                    _result?.KeyResultForHis + ".pdf");

                var fileBase64 =
                    await _toolBL.ExportPdf_Result(
                        _folder,
                        _file,
                        header,
                        content,
                        footer);

                return Content(fileBase64);
            }
            catch
            {
                return Content(string.Empty);
            }
        }

        //public async Task<IActionResult> ValidPrintMultiple([FromBody] ResultCDHAModel resultCDHA)
        //{
        //    if (resultCDHA != null)
        //    {
        //        var _dateTime = ToolBL.Get_DateNow();
        //        var _userLogin = this.GetUserLogin();
        //        var _device = this.GetSession_Device();
        //        if (await _patientCDHABL.Update(resultCDHA.patientId, resultCDHA.returnResultTime, resultCDHA.userReturnResult, false, false, true, _userLogin.Value, _SA))
        //        {
        //            var _result = await _resultCDHABL.GetResultCDHAByPatientId_ForValidPrint(resultCDHA.resultCDHAId);
        //            if (_result != null)
        //            {
        //                try
        //                {
        //                    // === LẤY THÔNG TIN BÁC SĨ THỰC HIỆN (ReturnUser) ===
        //                    if (resultCDHA.userReturnResult > 0)   // vì userReturnResult là kiểu int/long
        //                    {
        //                        var returnUser = await _userBL.GetUser(resultCDHA.userReturnResult);
        //                        if (returnUser != null)
        //                        {
        //                            ViewData["ReturnUser"] = returnUser; // để Content/Header/Footer dùng khi in
        //                        }
        //                    }
        //                    // ========== FILTER ẢNH THEO DANH SÁCH NGƯỜI DÙNG CHỌN ==========
        //                    if (resultCDHA.selectedImageIds != null && resultCDHA.selectedImageIds.Count > 0 && _result.ImageCDHAs != null)
        //                    {
        //                        // Bảo toàn thứ tự người dùng chọn
        //                        var orderMap = resultCDHA.selectedImageIds
        //                                            .Select((id, idx) => new { id, idx })
        //                                            .ToDictionary(x => x.id, x => x.idx);

        //                        // Lọc & sắp xếp theo thứ tự người dùng
        //                        _result.ImageCDHAs = _result.ImageCDHAs
        //                            .Where(img => orderMap.ContainsKey(img.Id))         // nếu Id là string: img.ImageCDHAIdString
        //                            .OrderBy(img => orderMap[img.Id])
        //                            .ToList();
        //                    }
        //                    // ==================================================================
        //                    ViewData["ResultCDHA"] = _result;
        //                    var _hospital = await _hospitalBL.GetHospital();
        //                    var _serviceCDHA = _serviceBL.GetById(_result?.Service?.Id);
        //                    if (_serviceCDHA != null)
        //                    {
        //                        ViewData["ServiceCDHA"] = _serviceCDHA.Result;
        //                    }
        //                    else
        //                    {
        //                        ViewData["ServiceCDHA"] = null;
        //                    }
        //                    // In ngang
        //                    //var header = await this.RenderViewAsync("Header_Landscape", _hospital);
        //                    //var content_result = await this.RenderViewAsync("Content_Result_Landscape", _hospital);
        //                    //var content_image = await this.RenderViewAsync("Content_Image_Landscape", _hospital);
        //                    //var footer = await this.RenderViewAsync("Footer_Landscape", _hospital);
        //                    // In dọc
        //                    var header = await this.RenderViewAsync("Header", _hospital);
        //                    var content = await this.RenderViewAsync("Content", _hospital);
        //                    var footer = await this.RenderViewAsync("Footer", _hospital);
        //                    var _folder = Path.Combine(_environment.WebRootPath, "pdf", "sa");
        //                    var _file = Path.Combine(_folder, _result?.KeyResultForHis + ".pdf");
        //                    var fileBase64 = await _toolBL.ExportPdf_Result(_folder, _file, header, content, footer);
        //                    //var fileBase64 = await _toolBL.ExportPdf_Result_Landscape_OnePage_FromFiles(_folder, _file, header, content_result, content_image, footer, Path.Combine(_environment.WebRootPath, "pdf_templates", "Landscape2ColRaw_SA.html"));
        //                    return Content(fileBase64);
        //                }
        //                catch { }
        //            }
        //        }
        //    }
        //    return Content(string.Empty);
        //}


        // Version mới của ValidPrintMultiple 18/09/2026
        public async Task<IActionResult> ValidPrintMultiple([FromBody] ResultCDHAModel resultCDHA)
        {
            if (resultCDHA == null)
            {
                return Content(string.Empty);
            }

            var _dateTime = ToolBL.Get_DateNow();
            var _userLogin = this.GetUserLogin();

            if (!_userLogin.HasValue)
            {
                return Unauthorized();
            }

            // =========================================================
            // STEP 1:
            // Valid đúng ResultCDHA hiện tại.
            //
            // ValidPrintMultiple theo code cũ KHÔNG lưu lại
            // Description / Result / Suggest.
            //
            // Nó dùng dữ liệu đã Save trước đó.
            // Vì vậy ở đây KHÔNG gọi ResultCDHABL.Update().
            // =========================================================
            var validated =
                await _resultCDHABL.ValidateCDHAAsync(
                    resultCDHA,
                    _SA,
                    _userLogin.Value,
                    _dateTime);

            if (!validated)
            {
                return Content(string.Empty);
            }

            // =========================================================
            // STEP 2:
            // Load lại ResultCDHA vừa Valid.
            // =========================================================
            var _result =
                await _resultCDHABL.GetResultCDHAByPatientId_ForValidPrint(
                    resultCDHA.resultCDHAId);

            if (_result == null)
            {
                return Content(string.Empty);
            }

            try
            {
                // =====================================================
                // LẤY THÔNG TIN BÁC SĨ THỰC HIỆN / TRẢ KẾT QUẢ
                // =====================================================
                if (resultCDHA.userReturnResult > 0)
                {
                    var returnUser =
                        await _userBL.GetUser(
                            resultCDHA.userReturnResult);

                    if (returnUser != null)
                    {
                        ViewData["ReturnUser"] = returnUser;
                    }
                }

                // =====================================================
                // FILTER ẢNH ĐƯỢC USER CHỌN
                // =====================================================
                if (resultCDHA.selectedImageIds != null &&
                    resultCDHA.selectedImageIds.Count > 0 &&
                    _result.ImageCDHAs != null)
                {
                    var orderMap =
                        resultCDHA.selectedImageIds
                            .Select((id, idx) => new
                            {
                                id,
                                idx
                            })
                            .ToDictionary(
                                x => x.id,
                                x => x.idx);

                    _result.ImageCDHAs =
                        _result.ImageCDHAs
                            .Where(img =>
                                orderMap.ContainsKey(img.Id))
                            .OrderBy(img =>
                                orderMap[img.Id])
                            .ToList();
                }

                // =====================================================
                // DỮ LIỆU VIEW
                // =====================================================
                ViewData["ResultCDHA"] = _result;

                var _hospital =
                    await _hospitalBL.GetHospital();

                var _serviceCDHA =
                    _serviceBL.GetById(
                        _result?.Service?.Id);

                if (_serviceCDHA != null)
                {
                    ViewData["ServiceCDHA"] =
                        _serviceCDHA.Result;
                }
                else
                {
                    ViewData["ServiceCDHA"] = null;
                }

                // =====================================================
                // RENDER PDF
                // =====================================================
                var header =
                    await this.RenderViewAsync(
                        "Header",
                        _hospital);

                var content =
                    await this.RenderViewAsync(
                        "Content",
                        _hospital);

                var footer =
                    await this.RenderViewAsync(
                        "Footer",
                        _hospital);

                var _folder = Path.Combine(
                    _environment.WebRootPath,
                    "pdf",
                    "sa");

                var _file = Path.Combine(
                    _folder,
                    _result?.KeyResultForHis + ".pdf");

                var fileBase64 =
                    await _toolBL.ExportPdf_Result(
                        _folder,
                        _file,
                        header,
                        content,
                        footer);

                return Content(fileBase64);
            }
            catch
            {
                return Content(string.Empty);
            }
        }
        // SA-VP-BATCH-20261114: Valid nhiều ResultCDHA của cùng bệnh nhân,
        // rồi trả về MỘT PDF gộp. Không thay đổi API ValidPrint/ValidPrintMultiple cũ.
        public sealed class SAValidPrintBatchRequest
        {
            public long PatientId { get; set; }
            public List<long> ResultCDHAIds { get; set; } = new List<long>();
            public DateTime ReturnResultTime { get; set; }
            public long UserReturnResult { get; set; }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ValidPrintSelectedBatch([FromBody] SAValidPrintBatchRequest request)
        {
            var userLogin = GetUserLogin();
            if (!userLogin.HasValue) return Unauthorized();

            if (request == null || request.PatientId <= 0 ||
                request.ResultCDHAIds == null || request.ResultCDHAIds.Count < 2 ||
                request.ResultCDHAIds.Count > 100 ||
                request.ResultCDHAIds.Any(x => x <= 0) ||
                request.ResultCDHAIds.Distinct().Count() != request.ResultCDHAIds.Count ||
                request.UserReturnResult <= 0 || request.ReturnResultTime == default(DateTime))
            {
                return BadRequest(new { message = "Danh sách dịch vụ hoặc thông tin trả kết quả không hợp lệ." });
            }

            try
            {
                // Preflight TOÀN BỘ ResultCDHAId, tránh lấy nhầm bệnh nhân/module.
                // Explicit Include vì không được phụ thuộc Lazy Loading.
                List<ResultCDHA> selected;
                using (var db = new LABContext())
                {
                    selected = await db.ResultCDHAs
                        .AsNoTracking()
                        .Include(r => r.Patient)
                        .Include(r => r.Service)
                        .ThenInclude(service => service.Category)
                        .Where(r => r.Active && r.PatientId == request.PatientId &&
                            request.ResultCDHAIds.Contains(r.Id) && r.Service.Category.Code == _SA)
                        .ToListAsync();
                }

                if (selected.Count != request.ResultCDHAIds.Count)
                {
                    // Read-only diagnostic: không bỏ bộ lọc PatientId/SA/Active.
                    var acceptedIds = new HashSet<long>(selected.Select(x => x.Id));
                    var rejectedIds = request.ResultCDHAIds.Where(x => !acceptedIds.Contains(x)).ToList();
                    using (var diagnosisDb = new LABContext())
                    {
                        var details = await diagnosisDb.ResultCDHAs.AsNoTracking()
                            .Where(r => rejectedIds.Contains(r.Id))
                            .Select(r => new
                            {
                                r.Id,
                                r.PatientId,
                                r.Active,
                                CategoryCode = r.Service != null && r.Service.Category != null
                                    ? r.Service.Category.Code : null
                            }).ToListAsync();
                        // Chỉ ghi log server, không trả ID bệnh nhân khác về client.
                        _logger.LogWarning(
                            "SA batch rejected. RequestPatientId={PatientId}; RequestedIds={RequestedIds}; AcceptedIds={AcceptedIds}; RejectedIds={RejectedIds}; RejectedRecordDetails={Details}",
                            request.PatientId,
                            string.Join(",", request.ResultCDHAIds),
                            string.Join(",", selected.Select(x => x.Id)),
                            string.Join(",", rejectedIds),
                            Newtonsoft.Json.JsonConvert.SerializeObject(details));
                    }
                    return BadRequest(new { message = "Danh sách dịch vụ không đồng nhất với bệnh nhân đang chọn. Hãy tải lại bệnh nhân và dịch vụ, sau đó kiểm tra lại trước khi Valid & In." });
                }

                // Tất cả KeyResultForHis phải hợp lệ trước khi thay đổi trạng thái Valid.
                // Cấm trùng Key vì có thể khiến hai dịch vụ ghi đè cùng một PDF.
                if (selected.Select(r => r.KeyResultForHis)
                        .Distinct(StringComparer.OrdinalIgnoreCase).Count() != selected.Count)
                    return BadRequest(new { message = "Nhiều dịch vụ dùng trùng mã file PDF. Không thể in an toàn." });

                if (selected.Any(r => string.IsNullOrWhiteSpace(r.KeyResultForHis) ||
                    Path.GetFileName(r.KeyResultForHis) != r.KeyResultForHis ||
                    r.KeyResultForHis.Contains('\\') || r.KeyResultForHis.Contains('/')))
                {
                    return BadRequest(new { message = "Có dịch vụ thiếu mã file PDF hợp lệ. Không thực hiện Valid." });
                }

                // Dịch vụ đã Valid: không gọi ValidateCDHAAsync lần hai.
                // Với dữ liệu legacy (IsValidated=null), dùng đúng chính sách fallback
                // Patient.ValidSA + ReturnResultTimeSA + InsertTime của BL.
                var pending = selected.Where(r =>
                    !(r.IsValidated == true ||
                    (r.IsValidated == null && r.Patient != null &&
                     r.Patient.ValidSA && r.Patient.ReturnResultTimeSA.HasValue &&
                     (!r.InsertTime.HasValue || r.InsertTime.Value <= r.Patient.ReturnResultTimeSA.Value))))
                    .ToList();

                if (pending.Count > 0)
                {
                    var inputs = pending.Select(r => new ResultCDHAModel
                    {
                        patientId = request.PatientId,
                        resultCDHAId = r.Id,
                        returnResultTime = request.ReturnResultTime,
                        userReturnResult = request.UserReturnResult
                    }).ToList();

                    // BL kiểm tra CanEditCDHAAsync cho TOÀN BỘ trước khi Valid.
                    var validOk = await _resultCDHABL.ValidateCDHAMultipleAsync(
                        inputs, _SA, userLogin.Value, ToolBL.Get_DateNow());
                    if (!validOk)
                        return Conflict(new { message = "Một hoặc nhiều dịch vụ không thể Valid (đã khóa, chưa Invalid đúng quyền hoặc dữ liệu thay đổi). Không in được toàn bộ." });
                }

                var folder = Path.Combine(_environment.WebRootPath, "pdf", "sa");
                Directory.CreateDirectory(folder);
                var pendingIds = new HashSet<long>(pending.Select(r => r.Id));
                var paths = new List<string>();
                var hospital = await _hospitalBL.GetHospital();

                // Render thứ tự đúng như người dùng chọn; chỉ tái tạo PDF cho dịch vụ
                // vừa Valid hoặc dịch vụ đã Valid nhưng chưa có file PDF.
                foreach (var resultId in request.ResultCDHAIds)
                {
                    var item = selected.Single(r => r.Id == resultId);
                    var pdfPath = Path.Combine(folder, item.KeyResultForHis + ".pdf");
                    if (pendingIds.Contains(resultId) || !System.IO.File.Exists(pdfPath) ||
                        new FileInfo(pdfPath).Length == 0)
                    {
                        var result = await _resultCDHABL.GetResultCDHAByPatientId_ForValidPrint(resultId);
                        if (result == null || result.PatientId != request.PatientId)
                            throw new InvalidOperationException("Không thể tải lại ResultCDHA " + resultId);

                        ViewData["ResultCDHA"] = result;
                        ViewData["ReturnUser"] = await _userBL.GetUser(request.UserReturnResult);
                        var service = _serviceBL.GetById(result.Service?.Id);
                        ViewData["ServiceCDHA"] = service?.Result;

                        var header = await this.RenderViewAsync("Header", hospital);
                        var content = await this.RenderViewAsync("Content", hospital);
                        var footer = await this.RenderViewAsync("Footer", hospital);
                        var base64 = await _toolBL.ExportPdf_Result(folder, pdfPath, header, content, footer);
                        if (string.IsNullOrEmpty(base64) || !System.IO.File.Exists(pdfPath))
                            throw new InvalidOperationException("Không tạo được PDF cho ResultCDHA " + resultId);
                    }
                    paths.Add(pdfPath);
                }

                // Gộp PDF giữ nguyên kích thước/orientation và số trang của từng dịch vụ.
                // Không ép A4 portrait vì bản SA có thể sử dụng thiết kế landscape.
                using var output = new PdfSharpCore.Pdf.PdfDocument();
                foreach (var path in paths)
                {
                    using var source = PdfSharpCore.Pdf.IO.PdfReader.Open(
                        path, PdfSharpCore.Pdf.IO.PdfDocumentOpenMode.Import);
                    if (source.PageCount == 0)
                        throw new InvalidOperationException("PDF không có trang: " + Path.GetFileName(path));
                    for (var page = 0; page < source.PageCount; page++)
                        output.AddPage(source.Pages[page]);
                }
                if (output.PageCount == 0)
                    throw new InvalidOperationException("Không có trang PDF hợp lệ để in.");

                using var memory = new MemoryStream();
                output.Save(memory, false);
                Response.Headers["X-SA-Selected-Count"] = request.ResultCDHAIds.Count.ToString();
                Response.Headers["X-SA-Page-Count"] = output.PageCount.ToString();
                return File(memory.ToArray(), "application/pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "ValidPrintSelectedBatch failed. PatientId={PatientId}; Count={Count}",
                    request?.PatientId, request?.ResultCDHAIds?.Count ?? 0);
                return StatusCode(500, new
                {
                    message = "Không tạo được PDF gộp. Vui lòng kiểm tra log server. Các dịch vụ Valid thành công trước đó được giữ nguyên trạng thái."
                });
            }
        }

        public class ValidPrintAllInput
        {
            public int PatientId { get; set; }
            public DateTime ReturnResultTime { get; set; }
            public int UserReturnResult { get; set; }
            public bool UseSaved { get; set; } = true;
            public List<ValidPrintItem> Items { get; set; } = new();
        }
        public class ValidPrintItem
        {
            public int ResultCDHAId { get; set; }
            public bool IncludeAllImages { get; set; } = true;
            public List<long> SelectedImageIds { get; set; } // optional
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Check_SelectDevice()
        {
            var _device = GetSession_Device();
            var _selectDevice = _device == null ? false : true;
            return Content(_selectDevice.ToString());
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> SelectDevice(long deviceId)
        {
            var _status = false;
            try
            {
                var _device = await _deviceBL.Get(deviceId);
                if (_device != null)
                {
                    var device = new Device() { Id = _device.Id, Code = _device.Code, Name = _device.Name, CodeBHYT = _device.CodeBHYT };
                    CookieOptions cookieOptions = new CookieOptions() { Expires = new DateTimeOffset(DateTime.Now.AddDays(1)) };
                    string value = JsonConvert.SerializeObject(device, Formatting.Indented,
                                   new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    HttpContext.Response.Cookies.Append(SessionKeyModel._sessionDeviceSA, value, cookieOptions);
                    _status = true;
                }
            }
            catch { }
            return Content(_status.ToString());
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UploadHinh(IFormFile file, string resultCDHAId)
        {
            var guid = Guid.NewGuid();
            var _folder = Path.Combine(_environment.WebRootPath, "images", "result", "sa");
            var _imagePath1 = Path.Combine(_folder, resultCDHAId + "-" + guid + ".png");

            using var _stream = file.OpenReadStream();
            using var _image = System.Drawing.Image.FromStream(_stream);
            var _saveImage = _toolBL.SaveImage(_folder, _imagePath1, _image);

            if (_saveImage)
            {
                var _imagePath = "/images/result/sa/" + resultCDHAId + "-" + guid + ".png";
                _saveImage = await _resultCDHABL.SaveImage(long.Parse(resultCDHAId), _imagePath, _imagePath1);
            }
            return Content(_saveImage.ToString());
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

        public Device GetSession_Device()
        {
            string _value = HttpContext.Request.Cookies[SessionKeyModel._sessionDeviceSA];
            Device _device = null;
            if (!string.IsNullOrEmpty(_value))
            {
                _device = JsonConvert.DeserializeObject<Device>(_value);
            }
            return _device;
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

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> TestAutoGetPatientAndService()
        {
            try
            {
                var apiBL = new APIBL();
                await apiBL.Auto_GetPatientAndService();
                return Json(new { success = true, message = "Auto_GetPatientAndService executed successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Test_Auto_Update_ResultStandard_To_ResultXN()
        {
            try
            {
                var apiBL = new APIBL();
                await apiBL.Auto_Update_ResultStandard_To_ResultXN();
                return Json(new { success = true, message = "Auto_GetPatientAndService executed successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> TestAutoPushUpdateStatusHis()
        {
            try
            {
                var apiBL = new APIBL();
                await apiBL.Auto_PushResultBHYT();
                return Json(new { success = true, message = "Auto_PushResultBHYT executed successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}