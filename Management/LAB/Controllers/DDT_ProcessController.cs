using Management.BL;
using Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SelectPdf;
using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Management.Controllers
{
    public class DDT_ProcessController : Controller
    {
        private readonly ILogger<DDT_ProcessController> _logger;
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
        public readonly string _DDT = "DDT";
        public readonly DeviceBL _deviceBL;

        public DDT_ProcessController(ILogger<DDT_ProcessController> logger, PatientCDHABL patientCDHABL, ObjectBL objectBL, LocationBL locationBL,
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

            ViewData["lstPatient"] = await _patientCDHABL.Get_ListPatient(from, to, false, true, false, _DDT);
            ViewData["lstUser"] = await _userBL.GetListUser(UserTypeModel.DDT);
            ViewData["lstBenhAnModel"] = await BenhAnModel.GetListBenhAnModel();
            ViewData["lstSampleResult"] = await _sampleBL.GetListSampleByCategory(_DDT);
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

            ViewData["lstPatient"] = await _patientCDHABL.Get_ListPatientByPidOrSid(from, to, false, true, false, null, _DDT);

            return PartialView("_DDT_Process_ListPatient");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Search(string pidorseq, DateTime timeSearchFrom, DateTime timeSearchTo)
        {
            // Save selected dates to session/cookies
            SaveSearchDatesToSession(timeSearchFrom, timeSearchTo);

            var from = new DateTime(timeSearchFrom.Year, timeSearchFrom.Month, timeSearchFrom.Day, 23, 59, 59).AddDays(-1);
            var to = new DateTime(timeSearchTo.Year, timeSearchTo.Month, timeSearchTo.Day, 23, 59, 59);
            ViewData["lstPatient"] = await _patientCDHABL.Get_ListPatientByPidOrSid(from, to, false, true, false, pidorseq, _DDT);

            return PartialView("_DDT_Process_ListPatient");
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
            var _getSample = await _patientCDHABL.GetSample_ProcessResult_ReturnResult(id, true, false, false, _userLogin.Value, _DDT);
            return Content(_getSample.ToString());
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetPatientInfo(long id)
        {
            var _userLoginId = this.GetUserLogin();
            ViewData["userLoginId"] = _userLoginId;
            ViewData["lstUser"] = await _userBL.GetListUser(UserTypeModel.DDT);
            ViewData["lstBenhAnModel"] = await BenhAnModel.GetListBenhAnModel();
            ViewData["patientInfo"] = await _patientCDHABL.Get_PatientBySid(id);

            return PartialView("_DDT_Process_PatientInfo");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetServiceForPatient(long patientId)
        {
            ViewData["lstResultCDHAs"] = await _resultCDHABL.GetListResultCDHAByPatientId(patientId, _DDT);
            ViewData["patientInfos"] = await _patientCDHABL.Get_PatientBySid(patientId);
            return PartialView("_DDT_Process_ListService");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetSampleForService(long id)
        {
            var _sample = await _sampleBL.GetSample(id);
            var _item = new { Description = _sample.Description, Result = _sample.Result, Suggest = _sample.Suggest };
            return Json(_item);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get_Description_Result_Suggest_ForService(long id)
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
                await _patientCDHABL.Update(resultCDHA.patientId, resultCDHA.returnResultTime, resultCDHA.userReturnResult, _userLogin.Value, _dateTime, _DDT);
                _saveResult = await _resultCDHABL.Update(resultCDHA, _userLogin.Value, _dateTime, _device);
            }
            return Content(_saveResult.ToString());
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ValidPrint([FromBody] ResultCDHAModel resultCDHA)
        {
            if (resultCDHA != null)
            {
                var _dateTime = ToolBL.Get_DateNow();
                var _userLogin = this.GetUserLogin();
                var _device = this.GetSession_Device();
                if (await _resultCDHABL.Update(resultCDHA, _userLogin.Value, _dateTime, _device))
                {
                    if (await _patientCDHABL.Update(resultCDHA.patientId, resultCDHA.returnResultTime, resultCDHA.userReturnResult, false, false, true, _userLogin.Value, _DDT))
                    {
                        var _result = await _resultCDHABL.GetResultCDHAByPatientId_ForValidPrint(resultCDHA.resultCDHAId);
                        if (_result != null)
                        {
                            try
                            {
                                // === LẤY THÔNG TIN BÁC SĨ THỰC HIỆN (ReturnUser) ===
                                if (resultCDHA.userReturnResult > 0)   // vì userReturnResult là kiểu int/long
                                {
                                    var returnUser = await _userBL.GetUser(resultCDHA.userReturnResult);
                                    if (returnUser != null)
                                    {
                                        ViewData["ReturnUser"] = returnUser; // để Content/Header/Footer dùng khi in
                                    }
                                }
                                ViewData["ResultCDHA"] = _result;
                                var _serviceCDHA = _serviceBL.GetById(_result?.Service?.Id);
                                if (_serviceCDHA != null)
                                {
                                    ViewData["ServiceCDHA"] = _serviceCDHA.Result;
                                }
                                else
                                {
                                    ViewData["ServiceCDHA"] = null;
                                }
                                var _hospital = await _hospitalBL.GetHospital();
                                var content = await this.RenderViewAsync("Content", _hospital);
                                var header = await this.RenderViewAsync("Header", _hospital);
                                var footer = await this.RenderViewAsync("Footer", _hospital);
                                var _folder = Path.Combine(_environment.WebRootPath, "pdf", "ddt");
                                var _file = Path.Combine(_folder, _result?.KeyResultForHis + ".pdf");
                                var fileBase64 = await _toolBL.ExportPdf_Result(_folder, _file, header, content, footer);

                                return Content(fileBase64);
                            }
                            catch { }
                        }
                    }
                }
            }
            return Content(string.Empty);
        }
        public async Task<IActionResult> ValidPrintMultiple([FromBody] ResultCDHAModel resultCDHA)
        {
            if (resultCDHA != null)
            {
                var _dateTime = ToolBL.Get_DateNow();
                var _userLogin = this.GetUserLogin();
                var _device = this.GetSession_Device();
                if (await _patientCDHABL.Update(resultCDHA.patientId, resultCDHA.returnResultTime, resultCDHA.userReturnResult, false, false, true, _userLogin.Value, _DDT))
                {
                    var _result = await _resultCDHABL.GetResultCDHAByPatientId_ForValidPrint(resultCDHA.resultCDHAId);
                    if (_result != null)
                    {
                        try
                        {
                            // === LẤY THÔNG TIN BÁC SĨ THỰC HIỆN (ReturnUser) ===
                            if (resultCDHA.userReturnResult > 0)   // vì userReturnResult là kiểu int/long
                            {
                                var returnUser = await _userBL.GetUser(resultCDHA.userReturnResult);
                                if (returnUser != null)
                                {
                                    ViewData["ReturnUser"] = returnUser; // để Content/Header/Footer dùng khi in
                                }
                            }
                            // ========== FILTER ẢNH THEO DANH SÁCH NGƯỜI DÙNG CHỌN ==========
                            if (resultCDHA.selectedImageIds != null && resultCDHA.selectedImageIds.Count > 0 && _result.ImageCDHAs != null)
                            {
                                // Bảo toàn thứ tự người dùng chọn
                                var orderMap = resultCDHA.selectedImageIds
                                                    .Select((id, idx) => new { id, idx })
                                                    .ToDictionary(x => x.id, x => x.idx);

                                // Lọc & sắp xếp theo thứ tự người dùng
                                _result.ImageCDHAs = _result.ImageCDHAs
                                    .Where(img => orderMap.ContainsKey(img.Id))         // nếu Id là string: img.ImageCDHAIdString
                                    .OrderBy(img => orderMap[img.Id])
                                    .ToList();
                            }
                            // ==================================================================
                            ViewData["ResultCDHA"] = _result;
                            var _hospital = await _hospitalBL.GetHospital();
                            var _serviceCDHA = _serviceBL.GetById(_result?.Service?.Id);
                            if (_serviceCDHA != null)
                            {
                                ViewData["ServiceCDHA"] = _serviceCDHA.Result;
                            }
                            else
                            {
                                ViewData["ServiceCDHA"] = null;
                            }
                            // In ngang
                            //var header = await this.RenderViewAsync("Header_Landscape", _hospital);
                            //var content_result = await this.RenderViewAsync("Content_Result_Landscape", _hospital);
                            //var content_image = await this.RenderViewAsync("Content_Image_Landscape", _hospital);
                            //var footer = await this.RenderViewAsync("Footer_Landscape", _hospital);
                            // In dọc
                            var header = await this.RenderViewAsync("Header", _hospital);
                            var content = await this.RenderViewAsync("Content", _hospital);
                            var footer = await this.RenderViewAsync("Footer", _hospital);
                            var _folder = Path.Combine(_environment.WebRootPath, "pdf", "ddt");
                            var _file = Path.Combine(_folder, _result?.KeyResultForHis + ".pdf");
                            var fileBase64 = await _toolBL.ExportPdf_Result(_folder, _file, header, content, footer);
                            //var fileBase64 = await _toolBL.ExportPdf_Result_Landscape_OnePage_FromFiles(_folder, _file, header, content_result, content_image, footer, Path.Combine(_environment.WebRootPath, "pdf_templates", "Landscape2ColRaw_SA.html"));
                            return Content(fileBase64);
                        }
                        catch { }
                    }
                }
            }
            return Content(string.Empty);
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
        //        if (await _resultCDHABL.Update(resultCDHA, _userLogin.Value, _dateTime, _device))
        //        {
        //            if (await _patientCDHABL.Update(resultCDHA.patientId, resultCDHA.returnResultTime, resultCDHA.userReturnResult, false, false, true, _userLogin.Value, _SA))
        //            {
        //                var _result = await _resultCDHABL.GetResultCDHAByPatientId_ForValidPrint(resultCDHA.resultCDHAId);
        //                if (_result != null)
        //                {
        //                    try
        //                    {
        //                        var bytePdf = await CreatePdf(_result);
        //                        var fileBase64 = Convert.ToBase64String(bytePdf);
        //                        return Content(fileBase64);
        //                    }
        //                    catch { }
        //                }
        //            }
        //        }
        //    }
        //    return Content(string.Empty);
        //}

        //public async Task<byte[]> CreatePdf(ResultCDHA model)
        //{
        //    try
        //    {
        //        return Document.Create(container =>
        //        {
        //            container.Page(page =>
        //            {
        //                page.Size(PageSizes.A4.Portrait());
        //                page.MarginTop(10);
        //                page.MarginLeft(30);
        //                page.MarginRight(20);
        //                page.MarginBottom(10);
        //                page.DefaultTextStyle(TextStyle.Default.FontSize(13).FontColor(Colors.Black).FontFamily(Fonts.TimesNewRoman));

        //                page.Content().Column(columns =>
        //                {
        //                    columns.Item().Row(row =>
        //                    {
        //                        row.RelativeItem().ExtendHorizontal().PaddingRight(10).Column(col1 =>
        //                        {
        //                            col1.Spacing(2);
        //                            col1.Item().Row(row1 =>
        //                            {
        //                                row1.RelativeItem().Column(col3 =>
        //                                {
        //                                    var env = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
        //                                    var logoPath = Path.Combine(env.WebRootPath, "images", "logo", "Logo.png");
        //                                    var logoByte = System.IO.File.ReadAllBytes(logoPath);
        //                                    col3.Item().PaddingTop(5).Width(120).Image(logoByte);
        //                                });

        //                                row1.RelativeItem().Column(col3 =>
        //                                {

        //                                });
        //                            });

        //                            col1.Item().PaddingTop(10).AlignCenter().Text($"PHIẾU KẾT QUẢ ĐO VẬN TỐC SÓNG MẠCH").Style(TextStyle.Default.FontSize(18)).Bold();

        //                            col1.Item().PaddingTop(10).Row(row1 =>
        //                            {
        //                                row1.RelativeItem().Column(col3 =>
        //                                {
        //                                    col3.Item().ExtendHorizontal().AlignLeft().Text(text =>
        //                                    {
        //                                        text.Span("Họ tên: ").Bold();
        //                                    });
        //                                });

        //                                row1.ConstantItem(70).Column(col3 =>
        //                                {
        //                                    col3.Item().ExtendHorizontal().AlignLeft().Text(text =>
        //                                    {
        //                                        text.Span("Tuổi: ").Bold();
        //                                    });
        //                                });

        //                                row1.ConstantItem(110).Column(col3 =>
        //                                {
        //                                    col3.Item().ExtendHorizontal().AlignLeft().Text(text =>
        //                                    {
        //                                        text.Span("Giới tính: ").Bold();
        //                                    });
        //                                });
        //                            });

        //                            col1.Item().PaddingTop(3).Row(row1 =>
        //                            {
        //                                row1.RelativeItem().Column(col3 =>
        //                                {
        //                                    col3.Item().ExtendHorizontal().AlignLeft().Text(text =>
        //                                    {
        //                                        text.Span("Địa chỉ: ").Bold();
        //                                    });
        //                                });
        //                            });

        //                            col1.Item().PaddingTop(3).Row(row1 =>
        //                            {
        //                                row1.RelativeItem().Column(col3 =>
        //                                {
        //                                    col3.Item().ExtendHorizontal().AlignLeft().Text(text =>
        //                                    {
        //                                        text.Span("Khoa phòng: ").Bold();
        //                                    });
        //                                });
        //                                row1.RelativeItem().Column(col3 =>
        //                                {
        //                                    col3.Item().ExtendHorizontal().AlignLeft().Text(text =>
        //                                    {
        //                                        text.Span("Bác sĩ chỉ định: ").Bold();
        //                                    });
        //                                });
        //                            });

        //                            col1.Item().PaddingTop(3).Row(row1 =>
        //                            {
        //                                row1.RelativeItem().Column(col3 =>
        //                                {
        //                                    col3.Item().ExtendHorizontal().AlignLeft().Text(text =>
        //                                    {
        //                                        text.Span("Chẩn đoán: ").Bold();
        //                                    });
        //                                });
        //                            });

        //                            //if (model?.Images != null && model?.Images?.Length >= 2)
        //                            //{
        //                            //    var path = Path.Combine("wwwroot", model?.Images[1]?.HinhAnhUrl.TrimStart('/'));
        //                            //    byte[] hinh = System.IO.File.ReadAllBytes(path);
        //                            //    col1.Item().AlignCenter().PaddingTop(2).Width(490).Height(530).Image(hinh).FitUnproportionally();
        //                            //}

        //                            col1.Item().PaddingTop(10).EnsureSpace().Row(row1 =>
        //                            {
        //                                row1.RelativeItem().Text("");
        //                                row1.ConstantItem(150).Text("");
        //                                row1.RelativeItem().Text(text =>
        //                                {
        //                                    var time = DateTime.Now;
        //                                    text.Line("Ngày " + time.ToString("dd") + " tháng " + time.ToString("MM") + " năm " + time.ToString("yyyy")).Italic();
        //                                    text.Line($"Bác sĩ đọc kết quả").Bold();
        //                                    text.Line("(Ký, ghi rõ họ tên)").Italic();
        //                                    text.Span("VITRICHUKYBSTHUCHIEN").FontColor("#FFFFFF").FontSize(10f);
        //                                    text.AlignCenter();
        //                                });
        //                            });
        //                        });
        //                    });
        //                });

        //                page.Footer().AlignCenter().Text(text =>
        //                {
        //                    text.Span("Lưu ý: mang theo kết quả này cho lần khám sau").FontSize(11).Italic();
        //                });
        //            });
        //        }).GeneratePdf();
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

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
                    HttpContext.Response.Cookies.Append(SessionKeyModel._sessionDeviceDDT, value, cookieOptions);
                    _status = true;
                }
            }
            catch { }
            return Content(_status.ToString());
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
            string _value = HttpContext.Request.Cookies[SessionKeyModel._sessionDeviceDDT];
            Device _device = null;
            if (!string.IsNullOrEmpty(_value))
            {
                _device = JsonConvert.DeserializeObject<Device>(_value);
            }
            return _device;
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UploadHinh(IFormFile file, string resultCDHAId)
        {
            var guid = Guid.NewGuid();
            var _folder = Path.Combine(_environment.WebRootPath, "images", "result", "ddt");
            var _imagePath1 = Path.Combine(_folder, resultCDHAId + "-" + guid + ".png");

            using var _stream = file.OpenReadStream();
            using var _image = Image.FromStream(_stream);
            var _saveImage = _toolBL.SaveImage(_folder, _imagePath1, _image);

            if (_saveImage)
            {
                var _imagePath = "/images/result/ddt/" + resultCDHAId + "-" + guid + ".png";
                _saveImage = await _resultCDHABL.SaveImage(long.Parse(resultCDHAId), _imagePath, _imagePath1);
            }
            return Content(_saveImage.ToString());
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
        public async Task<IActionResult> ScanFolderAndSyncImages(long resultCDHAId, string maBenhAn)
        {
            // 1) Thư mục CVS-03 export (đổi theo thực tế)
            var ecgFolder = Path.Combine(_environment.WebRootPath, "ecg_export");
            if (!Directory.Exists(ecgFolder)) return Content("False");

            // 2) Thư mục lưu dấu file đã import để tránh trùng
            var importedRoot = Path.Combine(ecgFolder, "_imported");
            Directory.CreateDirectory(importedRoot);

            // 3) Mẫu đuôi tệp thường gặp
            var patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.pdf" };
            var candidates = new List<string>();
            foreach (var p in patterns)
                candidates.AddRange(Directory.EnumerateFiles(ecgFolder, p, SearchOption.TopDirectoryOnly));

            // 4) Regex bắt “-<code>-00-RES-”, ví dụ: -BAxxxxxxx-00-RES-
            var today = DateTime.Now;
            var todayDate = today.ToString("yyyyMMdd");
            var reCode = new System.Text.RegularExpressions.Regex($@"(?<code>{maBenhAn})", System.Text.RegularExpressions.RegexOptions.IgnoreCase);


            // 5) Chỉ giữ file có mã khớp với maBenhAn
            var matched = candidates.Where(f =>
            {
                var name = Path.GetFileNameWithoutExtension(f);
                var m = reCode.Match(name);
                return m.Success && string.Equals(m.Groups["code"].Value, maBenhAn, StringComparison.OrdinalIgnoreCase);
            })
            // Ưu tiên theo thời gian mới nhất
            .OrderByDescending(f => new FileInfo(f).LastWriteTimeUtc)
            .ToList();

            if (matched.Count == 0) return Content("False");

            var destFolder = Path.Combine(_environment.WebRootPath, "images", "result", "ddt");
            Directory.CreateDirectory(destFolder);

            var importedCount = 0;

            foreach (var src in matched)
            {
                // chờ file ghi xong
                if (!IsFileReady(src)) { Thread.Sleep(300); if (!IsFileReady(src)) continue; }

                var guid = Guid.NewGuid().ToString("N");
                var ext = Path.GetExtension(src);
                var destName = $"{resultCDHAId}-{guid}{ext}";
                var destPath = Path.Combine(destFolder, destName);

                System.IO.File.Copy(src, destPath, overwrite: true);

                // Lưu DB: (web path hiển thị + physical path)
                var webPath = "/images/result/ddt/" + destName;
                var ok = await _resultCDHABL.SaveImage(resultCDHAId, webPath, destPath);
                if (ok)
                {
                    importedCount++;

                    // move nguồn sang _imported/yyyyMMdd để đánh dấu đã dùng
                    var dayFolder = Path.Combine(importedRoot, DateTime.Now.ToString("yyyyMMdd"));
                    Directory.CreateDirectory(dayFolder);
                    var archived = Path.Combine(dayFolder, Path.GetFileName(src));
                    if (System.IO.File.Exists(archived))
                    {
                        var baseName = Path.GetFileNameWithoutExtension(archived);
                        archived = Path.Combine(dayFolder, $"{baseName}_{guid}{ext}");
                    }
                    System.IO.File.Move(src, archived);
                }
            }

            return Content(importedCount > 0 ? "True" : "False");
        }

        // helper: kiểm tra file đã sẵn sàng đọc (không còn bị lock)
        private bool IsFileReady(string path)
        {
            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
                return fs.Length > 0;
            }
            catch { return false; }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UploadECGAndSync(long resultCDHAId, string patientCode, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0) return Content("False");

                // 1) Lưu tạm file vào ecg_export
                var ecgExport = Path.Combine(_environment.WebRootPath, "ecg_export");
                Directory.CreateDirectory(ecgExport);

                var ext = Path.GetExtension(file.FileName);
                var tmpName = $"{file.FileName}";
                var savePath = Path.Combine(ecgExport, tmpName);

                using (var fs = new FileStream(savePath, FileMode.Create))
                {
                    await file.CopyToAsync(fs);
                }

                // 2) Gọi lại logic sync đã có (lọc theo yyMMdd-code-00-RES-)
                //    Bạn đã triển khai ScanFolderAndSyncImages(...) ở bước trước
                //    Ta tái sử dụng bằng việc "giả" một POST nội bộ:
                //    Ở đây gọi trực tiếp hàm private thì tiện hơn, nhưng để tối giản, ta gọi lại action qua code:
                //    => đơn giản nhất: return Content("True") để FE gọi tiếp ScanFolderAndSyncImages.
                return Content("True");
            }
            catch
            {
                return Content("False");
            }
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
    }
}
