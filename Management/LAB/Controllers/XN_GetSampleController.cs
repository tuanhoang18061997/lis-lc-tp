using iTextSharp.text;
using iTextSharp.text.pdf;
using Management.BL;
using Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Globalization;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;
using static QuestPDF.Helpers.Colors;
using Object = Management.Models.Object;

namespace Management.Controllers
{
    public class XN_GetSampleController : Controller
    {
        public readonly ILogger<XN_GetSampleController> _logger;
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
        public readonly HospitalBL _hospitalBL;

        public XN_GetSampleController(ILogger<XN_GetSampleController> logger, PatientXNBL patientBL, ObjectBL objectBL, LocationBL locationBL, HospitalBL hospitalBL,
                            DoctorBL doctorBL, UserBL userBL, CategoryBL categoryBL, ServiceBL serviceBL, ResultXNBL resultXNBL, SettingBL settingBL, GroupBL groupBL)
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
            _hospitalBL = hospitalBL;
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

            ViewData["lstPatient"] = await _patientBL.Get_ListPatient(from, to, true, false, false);
            ViewData["lstObject"] = await _objectBL.GetListObject();
            ViewData["lstLocation"] = await _locationBL.GetListLocation();
            ViewData["lstDoctor"] = await _doctorBL.GetList();
            ViewData["lstSexModel"] = await SexModel.GetListSexModel();
            ViewData["lstBenhAnModel"] = await BenhAnModel.GetListBenhAnModel();
            ViewData["lstCategory"] = await _categoryBL.GetListCategoryByGroup(GroupBL.XN);
            ViewData["lstService"] = await _serviceBL.GetListServiceByGroup(GroupBL.XN);
            ViewData["lstHospital"] = await _hospitalBL.GetListHospital();

            // Pass the selected dates to the view
            ViewData["timeSearchFrom"] = from.ToString("yyyy-MM-dd");
            ViewData["timeSearchTo"] = to.ToString("yyyy-MM-dd");

            await this.Get_Count();
            GetUserLogin();

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

            ViewData["lstPatient"] = await _patientBL.Get_ListPatientByPidOrSid(from, to, true, false, false, null);

            return PartialView("_XN_GetSample_ListPatient");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Search(string pidorseq, DateTime timeSearchFrom, DateTime timeSearchTo)
        {
            // Save selected dates to session/cookies
            SaveSearchDatesToSession(timeSearchFrom, timeSearchTo);

            var from = new DateTime(timeSearchFrom.Year, timeSearchFrom.Month, timeSearchFrom.Day, 00, 00, 00);
            var to = new DateTime(timeSearchTo.Year, timeSearchTo.Month, timeSearchTo.Day, 23, 59, 59);
            ViewData["lstPatient"] = await _patientBL.Get_ListPatientByPidOrSid(from, to, true, false, false, pidorseq);

            return PartialView("_XN_GetSample_ListPatient");
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
            ViewData["patientInfo"] = await _patientBL.Get_PatientBySid(id);
            ViewData["lstHospital"] = await _hospitalBL.GetListHospital();

            return PartialView("_XN_GetSample_PatientInfo");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetServiceForPatient(long id)
        {
            ViewData["lstResultXNs"] = await _resultXNBL.GetListResultXNByPatientId(id);
            ViewData["lstCategory"] = await _categoryBL.GetListCategoryByGroup(GroupBL.XN);
            ViewData["lstService"] = await _serviceBL.GetListServiceByGroup(GroupBL.XN);
            return PartialView("_XN_GetSample_ListService");
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
        public async Task<IActionResult> DeletePatientAndService(long id)
        {
            var _status = string.Empty;
            var _userLogin = this.GetUserLogin();
            if (await _resultXNBL.DeleteByPatientId(id, _userLogin.Value))
            {
                _status = await _patientBL.DeleteById(id) ? "/XN_GetSample/Refresh/" : string.Empty;
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
                                                               string address, string diagnostic, string service, string hospital)
        {
            var _dateTime = ToolBL.Get_DateNow();
            if (string.IsNullOrEmpty(seq) && string.IsNullOrEmpty(sid))
            {
                seq = await _settingBL.GetSeq();
                sid = _dateTime.Day.ToString().PadLeft(2, '0') + _dateTime.Month.ToString().PadLeft(2, '0') + _dateTime.Year.ToString().Substring(2, 2) + "-" + seq;
            }
            var _userLogin = this.GetUserLogin();
            long? _id = string.IsNullOrEmpty(id) ? null : long.Parse(id);
            if (string.IsNullOrEmpty(hospital) || hospital == "null")
            {
                var hospitalModel = await _hospitalBL.GetHospital();
                hospital = hospitalModel.Id.ToString();
            }
            var _patient = await _patientBL.SaveOrUpdate(_id, patientId, seq, sid, patientName, age, sex, obj, type, location, doctor, getSampleTime, address, diagnostic, _userLogin.Value, _dateTime, true, false, false, hospital);

            return Content(_patient == null ? string.Empty : _patient.Id.ToString());
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddServiceForPatient(long patientId, long serviceId, long doctorId)
        {
            var _userLogin = this.GetUserLogin();
            var _dateTime = ToolBL.Get_DateNow();
            var _save = await _resultXNBL.SaveService(patientId, serviceId, _userLogin.Value, _dateTime, doctorId);

            return Content(_save.ToString());
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteServiceForPatient(long id)
        {
            var _userLogin = this.GetUserLogin();
            var _delete = await _resultXNBL.DeleteById(id, _userLogin.Value);
            return Content(_delete.ToString());
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ProcessResult(long id, DateTime getSampleTime)
        {
            var _exitResultXN = await _resultXNBL.ExitResultXN_Update_GetSampleTime(id, getSampleTime);
            var _getSample = false;
            if (_exitResultXN)
            {
                var _userLogin = this.GetUserLogin();
                _getSample = await _patientBL.GetSample_ProcessResult_ReturnResult(id, false, true, false, _userLogin.Value);
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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ProcessResultAll([FromBody] List<long> patientIds)
        {
            if (patientIds == null || patientIds.Count == 0)
            {
                return Json(new { success = false, message = "Danh sách bệnh nhân trống!" });
            }

            var _userLogin = this.GetUserLogin();
            if (!_userLogin.HasValue)
            {
                return Json(new { success = false, message = "Không xác định được người dùng!" });
            }

            int successCount = 0;
            var failedPatients = new List<long>();

            foreach (var patientId in patientIds)
            {
                try
                {
                    // Kiểm tra xem patient có tồn tại ResultXN không trước khi xử lý
                    var _exitResultXN = await _resultXNBL.ExitResultXN_Update_GetSampleTime(patientId, ToolBL.Get_DateNow());
                    if (_exitResultXN)
                    {
                        var _getSample = await _patientBL.GetSample_ProcessResult_ReturnResult(patientId, false, true, false, _userLogin.Value);
                        if (_getSample)
                        {
                            successCount++;
                        }
                        else
                        {
                            failedPatients.Add(patientId);
                        }
                    }
                    else
                    {
                        failedPatients.Add(patientId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing patient {patientId}");
                    failedPatients.Add(patientId);
                }
            }

            return Json(new
            {
                success = successCount > 0,
                successCount = successCount,
                totalCount = patientIds.Count,
                failedCount = failedPatients.Count,
                message = failedPatients.Count > 0 ?
                    $"Có {failedPatients.Count} bệnh nhân không thể xử lý" :
                    "Tất cả bệnh nhân đã được xử lý thành công"
            });
        }
        static float Mm(float mm) => mm * 72f / 25.4f;

        [HttpPost]
        [Authorize]
        //public IActionResult PrintSEQ( // Siam
        //    string seq,
        //    string patientId,
        //    string patientName,
        //    string dob,
        //    string gender,
        //    string datePrint,   // yyyy-MM-dd
        //    string timePrint    // HH:mm
        //)
        //{
        //    if (string.IsNullOrWhiteSpace(seq)) return Content(string.Empty);
        //    seq = new string(seq.Where(char.IsDigit).ToArray());
        //    if (seq.Length != 6) return Content(string.Empty);

        //    // Chuẩn hoá
        //    string NormalizeSex(string s)
        //    {
        //        if (string.IsNullOrWhiteSpace(s)) return "";
        //        s = s.Trim().ToUpperInvariant();
        //        if (s == "M" || s == "NAM" || s == "1") return "Nam";
        //        if (s == "F" || s == "NU" || s == "N" || s == "Nữ" || s == "0" || s == "2") return "Nữ";
        //        return s;
        //    }
        //    var sexText = NormalizeSex(gender);

        //    DateTime dtPrint;
        //    if (!DateTime.TryParseExact($"{datePrint} {timePrint}", "yyyy-MM-dd HH:mm",
        //        CultureInfo.InvariantCulture, DateTimeStyles.None, out dtPrint))
        //        dtPrint = DateTime.Now;

        //    string dobOut = "";
        //    if (!string.IsNullOrWhiteSpace(dob))
        //    {
        //        if (DateTime.TryParseExact(dob, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d1) ||
        //            DateTime.TryParseExact(dob, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out d1))
        //            dobOut = d1.ToString("dd/MM/yyyy");
        //        else dobOut = dob;
        //    }

        //    // Font Unicode
        //    BaseFont bf;
        //    try
        //    {
        //        var fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "DejaVuSans.ttf");
        //        bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
        //    }
        //    catch
        //    {
        //        bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
        //    }
        //    var f7b = new Font(bf, 7, Font.BOLD, BaseColor.Black);   // nhỏ hơn f8b
        //    var f8 = new Font(bf, 8, 0, BaseColor.Black);
        //    var f8b = new Font(bf, 8, Font.BOLD, BaseColor.Black);
        //    var f9 = new Font(bf, 9, 0, BaseColor.Black);
        //    var f9b = new Font(bf, 9, Font.BOLD, BaseColor.Black);
        //    var f9b2 = new Font(bf, 9, Font.BOLD, BaseColor.Black);  // nhỏ hơn f10b
        //    var f10b = new Font(bf, 10, Font.BOLD, BaseColor.Black);


        //    // --- Upper tên theo văn hoá Việt để giữ đúng dấu (Đ, Â, Ê, Ư, ...) ---
        //    var viTI = CultureInfo.GetCultureInfo("vi-VN").TextInfo;
        //    string nameUpper = viTI.ToUpper((patientName ?? "").Trim());

        //    using (var ms = new MemoryStream())
        //    {
        //        // Nhãn 30x40mm – lề nhỏ để vừa 1 trang
        //        var pageSize = new Rectangle(Mm(30), Mm(40));
        //        float margin = Mm(1.2f);
        //        using (var doc = new Document(pageSize, Mm(1.2f), Mm(1.2f), Mm(2.0f), Mm(1.2f)))
        //        {
        //            var writer = PdfWriter.GetInstance(doc, ms);
        //            doc.Open();

        //            // ----- Bảng gốc 1 cột để gom mọi thứ trên 1 trang -----
        //            var root = new PdfPTable(1) { WidthPercentage = 100 };
        //            root.DefaultCell.Border = Rectangle.NO_BORDER;
        //            root.DefaultCell.Padding = 0;
        //            root.AddCell(new PdfPCell()
        //            {
        //                Border = Rectangle.NO_BORDER,
        //                FixedHeight = Mm(1.0f), // 1mm đệm
        //                Padding = 0
        //            });
        //            // 1) Barcode (ẩn số nhỏ bên dưới)
        //            var barcode = new Barcode128
        //            {
        //                Code = seq,
        //                CodeType = Barcode.CODE128,
        //                StartStopText = false,
        //                BarHeight = Mm(6.0f),
        //                X = 0.7f,
        //                Font = null   // <<< ẨN HUMAN-READABLE TEXT
        //            };
        //            var bcImg = barcode.CreateImageWithBarcode(writer.DirectContent, BaseColor.Black, BaseColor.Black);
        //            // Đảm bảo không vượt chiều ngang
        //            bcImg.ScaleToFit(Mm(25.5f), Mm(7.0f));
        //            var bcCell = new PdfPCell(bcImg)
        //            {
        //                Border = Rectangle.NO_BORDER,
        //                HorizontalAlignment = Element.ALIGN_CENTER,
        //                Padding = 0,
        //                PaddingBottom = 1f
        //            };
        //            root.AddCell(bcCell);

        //            // 2) Số SEQ to, đậm, giữa
        //            root.AddCell(new PdfPCell(new Phrase(seq, f9b2))
        //            {
        //                Border = Rectangle.NO_BORDER,
        //                HorizontalAlignment = Element.ALIGN_CENTER,
        //                Padding = 0,
        //                PaddingBottom = 1f
        //            });

        //            // 3) Dòng ngày (trái) – giờ (phải)
        //            var dt = new PdfPTable(2) { WidthPercentage = 100 };
        //            dt.SetWidths(new float[] { 1, 1 });
        //            dt.DefaultCell.Border = Rectangle.NO_BORDER;
        //            var cDate = new PdfPCell(new Phrase(dtPrint.ToString("dd/MM/yyyy"), f8))
        //            {
        //                Border = Rectangle.NO_BORDER,
        //                HorizontalAlignment = Element.ALIGN_LEFT,
        //                Padding = 0,
        //                NoWrap = true
        //            };
        //            var cTime = new PdfPCell(new Phrase(dtPrint.ToString("HH:mm"), f8))
        //            {
        //                Border = Rectangle.NO_BORDER,
        //                HorizontalAlignment = Element.ALIGN_RIGHT,
        //                Padding = 0,
        //                NoWrap = true
        //            };
        //            dt.AddCell(cDate); dt.AddCell(cTime);
        //            root.AddCell(new PdfPCell(dt) { Border = Rectangle.NO_BORDER, Padding = 0, PaddingBottom = 1f });

        //            // 4) PatientID
        //            if (!string.IsNullOrWhiteSpace(patientId))
        //            {
        //                var pidCell = new PdfPCell(new Phrase(patientId.Trim(), f7b))
        //                {
        //                    Border = Rectangle.NO_BORDER,
        //                    PaddingTop = 2f,      // thêm padding trên
        //                    PaddingBottom = 2f,   // thêm padding dưới
        //                    HorizontalAlignment = Element.ALIGN_LEFT
        //                };
        //                root.AddCell(pidCell);
        //            }
        //            // 5) HỌ TÊN (vi-VN upper, đậm; auto giảm size nếu quá dài)
        //            var nameFont = nameUpper.Length > 25 ? new Font(bf, 8.5f, Font.BOLD) : f9b;
        //            root.AddCell(new PdfPCell(new Phrase(nameUpper, nameFont))
        //            { Border = Rectangle.NO_BORDER, Padding = 0, PaddingBottom = 0.5f });

        //            // 6) Dòng DOB (trái) – Giới (phải)
        //            var bottom = new PdfPTable(2) { WidthPercentage = 100 };
        //            bottom.SetWidths(new float[] { 2.2f, 1f });
        //            bottom.DefaultCell.Border = Rectangle.NO_BORDER;
        //            bottom.AddCell(new PdfPCell(new Phrase(dobOut, f8))
        //            { Border = Rectangle.NO_BORDER, Padding = 0, NoWrap = true, HorizontalAlignment = Element.ALIGN_LEFT });
        //            bottom.AddCell(new PdfPCell(new Phrase(sexText, f8))
        //            { Border = Rectangle.NO_BORDER, Padding = 0, NoWrap = true, HorizontalAlignment = Element.ALIGN_RIGHT });
        //            root.AddCell(new PdfPCell(bottom) { Border = Rectangle.NO_BORDER, Padding = 0 });

        //            // Add bảng gốc vào doc (1 trang duy nhất)
        //            doc.Add(root);
        //            doc.Close();
        //        }

        //        return Content(Convert.ToBase64String(ms.ToArray()));
        //    }
        //}

        /*
         * In 4 tem hàng ngang KHỔ GIẤY: 4.31in x 0.90in ≈ 109.5mm x 22.9mm
         */
        [HttpPost]
        [Authorize]
        public IActionResult PrintSEQ(
            string seq,
            string patientId,
            string patientName,
            string dob,
            string gender,
            string datePrint,   // yyyy-MM-dd
            string timePrint,    // HH:mm
            string getSampleTime

        )
        {
            // --- Validate SEQ ---
            if (string.IsNullOrWhiteSpace(seq)) return Content(string.Empty);
            seq = new string(seq.Where(char.IsDigit).ToArray());
            var seqAndNgayLayMau = seq + " - " + getSampleTime;
            // --- Chuẩn hoá giới tính ---
            string NormalizeSex(string s)
            {
                if (string.IsNullOrWhiteSpace(s)) return "";
                s = s.Trim().ToUpperInvariant();
                if (s == "M" || s == "NAM" || s == "1") return "Nam";
                if (s == "F" || s == "NU" || s == "N" || s == "NỮ" || s == "0" || s == "2") return "Nữ";
                return s;
            }
            var sexText = NormalizeSex(gender);

            // --- Ngày in ---
            DateTime dtPrint;
            if (!DateTime.TryParseExact($"{datePrint} {timePrint}", "yyyy-MM-dd HH:mm",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out dtPrint))
                dtPrint = DateTime.Now;

            // --- DOB & Năm sinh ---
            string dobOut = "";
            int? yearOfBirth = null;
            if (!string.IsNullOrWhiteSpace(dob))
            {
                if (DateTime.TryParseExact(dob, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d1) ||
                    DateTime.TryParseExact(dob, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out d1))
                {
                    dobOut = d1.ToString("dd/MM/yyyy");
                    yearOfBirth = d1.Year;
                }
                else
                {
                    dobOut = dob;
                    // nếu chuỗi là yyyy
                    if (int.TryParse(dob.Trim(), out var y) && y >= 1900 && y <= DateTime.Now.Year) yearOfBirth = y;
                }
            }

            string dobAndSex = dobOut + " - " + sexText;
            // --- Font Unicode ---
            BaseFont bf;
            try
            {
                var fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "DejaVuSans.ttf");
                bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            }
            catch
            {
                bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
            }
            var f4 = new Font(bf, 4, 0, BaseColor.Black);
            var f5 = new Font(bf, 5, 0, BaseColor.Black);
            var f6 = new Font(bf, 6f, 0, BaseColor.Black);
            var f6b = new Font(bf, 6f, Font.BOLD, BaseColor.Black);
            var f65b = new Font(bf, 6.5f, Font.BOLD, BaseColor.Black);
            var f7 = new Font(bf, 7, 0, BaseColor.Black);
            var f7b = new Font(bf, 7, Font.BOLD, BaseColor.Black);  // PatientId
            var f8 = new Font(bf, 8, 0, BaseColor.Black);
            var f8b = new Font(bf, 8, Font.BOLD, BaseColor.Black);
            var f85b = new Font(bf, 8.5f, Font.BOLD, BaseColor.Black);
            var f9b = new Font(bf, 9, Font.BOLD, BaseColor.Black);  // SEQ (giảm từ 10 -> 9)

            // --- Upper tên đúng tiếng Việt ---
            var viTI = CultureInfo.GetCultureInfo("vi-VN").TextInfo;
            string nameUpper = viTI.ToUpper((patientName ?? "").Trim());

            using (var ms = new MemoryStream())
            {
                // KHỔ GIẤY: 4.31in x 0.90in ≈ 109.5mm x 22.9mm
                var pageSize = new Rectangle(Mm(109f), Mm(23f));
                float margin = Mm(2.0f); // was 1.2f

                using (var doc = new Document(pageSize, margin, margin, margin, margin))
                {
                    var writer = PdfWriter.GetInstance(doc, ms);
                    doc.Open();

                    // ===== Bảng 4 cột, mỗi cột là 1 tem =====
                    var root = new PdfPTable(4) { WidthPercentage = 100 };
                    root.DefaultCell.Border = Rectangle.NO_BORDER;
                    root.DefaultCell.Padding = 0;
                    root.SetWidths(new float[] { 1, 1, 1, 1 });

                    PdfPCell CreateLabelCell()
                    {
                        var tbl = new PdfPTable(1) { WidthPercentage = 100 };
                        tbl.DefaultCell.Border = Rectangle.NO_BORDER;
                        tbl.DefaultCell.Padding = 0;

                        // 1) Barcode CODE128, ẩn số bên dưới
                        var barcode = new Barcode128
                        {
                            Code = seq,
                            CodeType = Barcode.CODE128,
                            StartStopText = false,
                            BarHeight = Mm(6.5f),
                            X = 0.7f,
                            Font = null // ẩn human-readable
                        };
                        var bcImg = barcode.CreateImageWithBarcode(writer.DirectContent, BaseColor.Black, BaseColor.Black);
                        // Mỗi nhãn ~ (109 - 2*margin)/4 ≈ 26.6mm
                        bcImg.ScaleToFit(Mm(25.0f), Mm(7.5f)); // was 26.5 x 8.0
                        tbl.AddCell(new PdfPCell(bcImg)
                        {
                            Border = Rectangle.NO_BORDER,
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            Padding = 0,
                            PaddingBottom = 0.5f
                        });

                        // --- {SEQ - NgayLayMau} ---
                        tbl.AddCell(new PdfPCell(new Phrase(seqAndNgayLayMau, f5))     // f8b bạn đã khai báo phía trên
                        //tbl.AddCell(new PdfPCell(new Phrase(seq, f65b))
                        {
                            Border = Rectangle.NO_BORDER,
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            Padding = 0,
                            PaddingBottom = 0.5f
                        });

                        // --- {Họ tên} ---
                        tbl.AddCell(new PdfPCell(new Phrase(nameUpper, f6b))
                        {
                            Border = Rectangle.NO_BORDER,
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            Padding = 0,
                            PaddingTop = 1.4f,
                            PaddingBottom = 1.4f
                        });

                        // --- {Năm sinh – Giới tính (cùng 1 dòng)} ---
                        tbl.AddCell(new PdfPCell(new Phrase(dobAndSex, f6))     // f8b bạn đã khai báo phía trên
                        {
                            Border = Rectangle.NO_BORDER,
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            Padding = 0,
                            PaddingBottom = 0.8f
                        });
                        //var info = new PdfPTable(3) { WidthPercentage = 100 };
                        //info.SetWidths(new float[] { 2.6f, 1.0f, 0.9f }); // cột cuối hẹp bớt nhưng vẫn đủ

                        //var fName = new Font(bf, 4.7f, Font.BOLD); // was 7.5–8.5
                        //                                           // dùng Paragraph để set leading đẹp, cho phép ngắt dòng
                        //var namePara = new Paragraph(nameUpper, fName) { 
                        //    Leading = 8.2f,
                        //    Alignment = Element.ALIGN_LEFT 
                        //};
                        //info.AddCell(new PdfPCell(namePara)
                        //{
                        //    Border = Rectangle.NO_BORDER,
                        //    HorizontalAlignment = Element.ALIGN_LEFT,
                        //    PaddingLeft = 2.5f,   // chừa “vùng an toàn” bên trái
                        //    PaddingRight = 1.5f,  // cách cột năm sinh
                        //    PaddingTop = 0,
                        //    PaddingBottom = 2.5f,
                        //    NoWrap = false
                        //});

                        //var yearText = yearOfBirth.HasValue ? yearOfBirth.Value.ToString() : "";
                        //info.AddCell(new PdfPCell(new Phrase(yearText, f5))
                        //{
                        //    Border = Rectangle.NO_BORDER,
                        //    HorizontalAlignment = Element.ALIGN_CENTER,
                        //    PaddingLeft = 0,   // cách bên trái
                        //    PaddingRight = 2f,  // cách bên phải
                        //    PaddingTop = 0,
                        //    PaddingBottom = 0,
                        //    NoWrap = true
                        //});

                        //info.AddCell(new PdfPCell(new Phrase(sexText, f5))
                        //{
                        //    Border = Rectangle.NO_BORDER,
                        //    HorizontalAlignment = Element.ALIGN_LEFT,
                        //    PaddingLeft = 0,   // cách chữ năm sinh
                        //    PaddingRight = 0,
                        //    PaddingTop = 0,
                        //    PaddingBottom = 0,
                        //    NoWrap = true
                        //});

                        //tbl.AddCell(new PdfPCell(info)
                        //{
                        //    Border = Rectangle.NO_BORDER,
                        //    PaddingTop = 0.6f,
                        //    PaddingBottom = 0.6f
                        //});

                        return new PdfPCell(tbl)
                        {
                            Border = Rectangle.NO_BORDER,
                            PaddingLeft = Mm(1.2f),   // ~1.2mm mỗi bên
                            PaddingRight = Mm(1.2f),
                            PaddingTop = 0,
                            PaddingBottom = 0
                        };
                    }

                    for (int i = 0; i < 4; i++)
                        root.AddCell(CreateLabelCell());

                    doc.Add(root);
                    doc.Close();
                }

                return Content(Convert.ToBase64String(ms.ToArray()));
            }
        }

        /*
         * In 2 tem 1 hàng
         */
        [HttpPost]
        [Authorize]
        public IActionResult PrintSEQTemp(
            string seq,
            string patientId,
            string patientName,
            string dob,
            string gender,
            string datePrint,   // yyyy-MM-dd
            string timePrint,    // HH:mm
            string getSampleTime,
            string maDotKham,
            int soBangIn
        )
        {
            // --- Validate SEQ ---
            if (string.IsNullOrWhiteSpace(seq)) return Content(string.Empty);
            seq = new string(seq.Where(char.IsDigit).ToArray());
            var seqAndNgayLayMau = seq + " - " + getSampleTime;
            // --- Chuẩn hoá giới tính ---
            string NormalizeSex(string s)
            {
                if (string.IsNullOrWhiteSpace(s)) return "";
                s = s.Trim().ToUpperInvariant();
                if (s == "M" || s == "NAM" || s == "1") return "Nam";
                if (s == "F" || s == "NU" || s == "N" || s == "NỮ" || s == "0" || s == "2") return "Nữ";
                return s;
            }
            var sexText = NormalizeSex(gender);

            // --- Ngày in ---
            DateTime dtPrint;
            if (!DateTime.TryParseExact($"{datePrint} {timePrint}", "yyyy-MM-dd HH:mm",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out dtPrint))
                dtPrint = DateTime.Now;

            // --- DOB & Năm sinh ---
            string dobOut = "";
            int? yearOfBirth = null;
            if (!string.IsNullOrWhiteSpace(dob))
            {
                if (DateTime.TryParseExact(dob, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d1) ||
                    DateTime.TryParseExact(dob, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out d1))
                {
                    dobOut = d1.ToString("dd/MM/yyyy");
                    yearOfBirth = d1.Year;
                }
                else
                {
                    dobOut = dob;
                    // nếu chuỗi là yyyy
                    if (int.TryParse(dob.Trim(), out var y) && y >= 1900 && y <= DateTime.Now.Year) yearOfBirth = y;
                }
            }
            string dobAndSex = dobOut + " - " + sexText;
            // --- Font Unicode ---
            BaseFont bf;
            try
            {
                var fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "DejaVuSans.ttf");
                bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            }
            catch
            {
                bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
            }
            var f3b = new Font(bf, 3, Font.BOLD, BaseColor.Black);
            var f5b = new Font(bf, 5f, Font.BOLD, BaseColor.Black);
            var f4 = new Font(bf, 4, 0, BaseColor.Black);
            var f5 = new Font(bf, 5, 0, BaseColor.Black);
            var f55b = new Font(bf, 5.5f, Font.BOLD, BaseColor.Black);
            var f6 = new Font(bf, 6f, 0, BaseColor.Black);
            var f6b = new Font(bf, 6f, Font.BOLD, BaseColor.Black);
            var f65b = new Font(bf, 6.5f, Font.BOLD, BaseColor.Black);
            var f7 = new Font(bf, 7, 0, BaseColor.Black);
            var f7b = new Font(bf, 7, Font.BOLD, BaseColor.Black);  // PatientId
            var f8 = new Font(bf, 8, 0, BaseColor.Black);
            var f8b = new Font(bf, 8, Font.BOLD, BaseColor.Black);
            var f85b = new Font(bf, 8.5f, Font.BOLD, BaseColor.Black);
            var f9b = new Font(bf, 9, Font.BOLD, BaseColor.Black);  // SEQ (giảm từ 10 -> 9)

            // --- Upper tên đúng tiếng Việt ---
            var viTI = CultureInfo.GetCultureInfo("vi-VN").TextInfo;
            string nameUpper = viTI.ToUpper((patientName ?? "").Trim());
            string maDotKhamFormatted = viTI.ToUpper((maDotKham ?? "").Trim());

            // ==== KHAI BÁO KHỔ GIẤY THEO TEM THỰC TẾ ====
            // Label Size: 69.9mm (W) x 22.9mm (H)
            // Exposed Liner Left/Right: 1.3mm (=> lấy làm lề trái/phải)
            float pageWidthMm = 69.9f;
            float pageHeightMm = 22.9f;
            float marginLeftRightMm = 1.3f;   // theo hình bạn gửi
            float marginTopBottomMm = 3.0f;   // an toàn (có thể tăng/giảm 0.5–1mm tuỳ máy)
            // Tính bề rộng khả dụng và bề rộng mỗi cột (mm)
            var usableWidthMm = pageWidthMm - 2 * marginLeftRightMm;
            var colWidthMm = usableWidthMm / soBangIn;
            using (var ms = new MemoryStream())
            {
                // KHỔ GIẤY: 4.31in x 0.90in ≈ 109.5mm x 22.9mm
                var pageSize = new Rectangle(Mm(pageWidthMm), Mm(pageHeightMm));
                float margin = Mm(2.0f); // was 1.2f

                using (var doc = new Document(pageSize, marginLeftRightMm, marginLeftRightMm, marginTopBottomMm, marginTopBottomMm)) // left, right, top, bottom
                {
                    var writer = PdfWriter.GetInstance(doc, ms);
                    doc.Open();

                    // ===== Bảng 4 cột, mỗi cột là 1 tem =====
                    var root = new PdfPTable(soBangIn) { WidthPercentage = 100 };
                    root.DefaultCell.Border = Rectangle.NO_BORDER;
                    root.DefaultCell.Padding = 0;
                    //root.SetWidths(new float[] { 1, 1, 1, 1 });
                    root.SetWidths(Enumerable.Repeat(1f, soBangIn).ToArray());

                    PdfPCell CreateLabelCell()
                    {
                        var tbl = new PdfPTable(1) { WidthPercentage = 100 };
                        tbl.DefaultCell.Border = Rectangle.NO_BORDER;
                        tbl.DefaultCell.Padding = 0;

                        // 1) Barcode CODE128, ẩn số bên dưới
                        var barcode = new Barcode128
                        {
                            Code = seq,
                            CodeType = Barcode.CODE128,
                            StartStopText = false,
                            BarHeight = Mm(6.5f),    // 6.5–7.5mm là đẹp với tem 22.9mm
                            X = 0.7f,
                            Font = null // ẩn human-readable
                        };
                        var bcImg = barcode.CreateImageWithBarcode(writer.DirectContent, BaseColor.Black, BaseColor.Black);
                        // chừa một chút padding hai bên cột (~1.5mm mỗi bên)
                        var barcodeMaxWidthMm = Math.Max(10f, colWidthMm - 3.0f);
                        bcImg.ScaleToFit(Mm(barcodeMaxWidthMm), Mm(7.5f));
                        // Mỗi nhãn ~ (109 - 2*margin)/4 ≈ 26.6mm
                        //bcImg.ScaleToFit(Mm(25.0f), Mm(7.5f)); // was 26.5 x 8.0  // dòng này để chạy code 4 cột
                        tbl.AddCell(new PdfPCell(bcImg)
                        {
                            Border = Rectangle.NO_BORDER,
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            PaddingTop = 2.0f,
                            PaddingBottom = 1.2f
                        });

                        tbl.AddCell(new PdfPCell(new Phrase(seqAndNgayLayMau, f5))
                        {
                            Border = Rectangle.NO_BORDER,
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            PaddingBottom = 0.6f
                        });

                        tbl.AddCell(new PdfPCell(new Phrase(nameUpper, f5b))
                        {
                            Border = Rectangle.NO_BORDER,
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            PaddingTop = 1.0f,
                            PaddingBottom = 0.6f
                        });

                        tbl.AddCell(new PdfPCell(new Phrase(dobAndSex, f5))
                        {
                            Border = Rectangle.NO_BORDER,
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            PaddingBottom = 0.6f
                        });

                        tbl.AddCell(new PdfPCell(new Phrase(maDotKhamFormatted, f3b))
                        {
                            Border = Rectangle.NO_BORDER,
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            //PaddingBottom = 0.6f
                        });

                        return new PdfPCell(tbl)
                        {
                            Border = Rectangle.NO_BORDER,
                            PaddingLeft = Mm(1.5f),   // ~1.2mm mỗi bên
                            PaddingRight = Mm(1.5f),
                            PaddingTop = 0,
                            PaddingBottom = 0
                        };
                    }

                    for (int i = 0; i < soBangIn; i++)
                        root.AddCell(CreateLabelCell());

                    doc.Add(root);
                    doc.Close();
                }

                return Content(Convert.ToBase64String(ms.ToArray()));
            }
        }

        /*
         * In 2 tem 1 hàng , page break thành 2 trang để in thành 4 tem
         */
        public IActionResult PrintSEQTemp2x2New(
            string seq,
            string patientId,
            string patientName,
            string dob,
            string gender,
            string datePrint,   // yyyy-MM-dd
            string timePrint,   // HH:mm
            string getSampleTime,
            string maDotKham,
            int soBangIn
        )
        {
            if (string.IsNullOrWhiteSpace(seq)) return Content(string.Empty);
            seq = new string(seq.Where(char.IsDigit).ToArray());
            var seqAndNgayLayMau = seq + " - " + getSampleTime;

            string NormalizeSex(string s)
            {
                if (string.IsNullOrWhiteSpace(s)) return "";
                s = s.Trim().ToUpperInvariant();
                if (s == "M" || s == "NAM" || s == "1") return "Nam";
                if (s == "F" || s == "NU" || s == "N" || s == "NỮ" || s == "0" || s == "2") return "Nữ";
                return s;
            }
            var sexText = NormalizeSex(gender);

            DateTime dtPrint;
            if (!DateTime.TryParseExact($"{datePrint} {timePrint}", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out dtPrint))
                dtPrint = DateTime.Now;

            string dobOut = "";
            int? yearOfBirth = null;
            if (!string.IsNullOrWhiteSpace(dob))
            {
                if (DateTime.TryParseExact(dob, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d1) ||
                    DateTime.TryParseExact(dob, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out d1))
                {
                    dobOut = d1.ToString("dd/MM/yyyy");
                    yearOfBirth = d1.Year;
                }
                else
                {
                    dobOut = dob;
                    if (int.TryParse(dob.Trim(), out var y) && y >= 1900 && y <= DateTime.Now.Year) yearOfBirth = y;
                }
            }
            string dobAndSex = dobOut + " - " + sexText;

            BaseFont bf;
            try
            {
                var fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "DejaVuSans.ttf");
                bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            }
            catch
            {
                bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
            }
            var f3b = new Font(bf, 3, Font.BOLD, BaseColor.Black);
            var f5 = new Font(bf, 5, 0, BaseColor.Black);
            var f5b = new Font(bf, 5f, Font.BOLD, BaseColor.Black);
            var f55b = new Font(bf, 5.5f, Font.BOLD, BaseColor.Black);
            var f6 = new Font(bf, 6f, 0, BaseColor.Black);
            var f6b = new Font(bf, 6f, Font.BOLD, BaseColor.Black);

            var viTI = CultureInfo.GetCultureInfo("vi-VN").TextInfo;
            string nameUpper = viTI.ToUpper((patientName ?? "").Trim());
            string maDotKhamFormatted = viTI.ToUpper((maDotKham ?? "").Trim());

            // Kích thước tem 69.9 x 22.9mm (1 dòng 2 SID / trang)
            float pageWidthMm = 69.9f;
            float pageHeightMm = 22.9f;
            float marginLeftRightMm = 1.3f;
            float marginTopBottomMm = 3.0f;

            var usableWidthMm = pageWidthMm - 2 * marginLeftRightMm;
            var colWidthMm = usableWidthMm / soBangIn;

            using (var ms = new MemoryStream())
            {
                var pageSize = new Rectangle(Mm(pageWidthMm), Mm(pageHeightMm));
                using (var doc = new Document(pageSize, marginLeftRightMm, marginLeftRightMm, marginTopBottomMm, marginTopBottomMm))
                {
                    var writer = PdfWriter.GetInstance(doc, ms);
                    doc.Open();

                    // ===== helper: dựng 1 hàng (1 dòng) gồm soBangIn tem =====
                    PdfPTable BuildRow()
                    {
                        var root = new PdfPTable(soBangIn) { WidthPercentage = 100 };
                        root.DefaultCell.Border = Rectangle.NO_BORDER;
                        root.DefaultCell.Padding = 0;
                        root.SetWidths(Enumerable.Repeat(1f, soBangIn).ToArray());

                        PdfPCell CreateLabelCell()
                        {
                            var tbl = new PdfPTable(1) { WidthPercentage = 100 };
                            tbl.DefaultCell.Border = Rectangle.NO_BORDER;
                            tbl.DefaultCell.Padding = 0;

                            var barcode = new Barcode128
                            {
                                Code = seq,
                                CodeType = Barcode.CODE128,
                                StartStopText = false,
                                BarHeight = Mm(6.5f),    // 6.5–7.5mm là đẹp với tem 22.9mm
                                X = 0.7f,
                                Font = null // ẩn human-readable
                            };
                            var bcImg = barcode.CreateImageWithBarcode(writer.DirectContent, BaseColor.Black, BaseColor.Black);
                            // chừa một chút padding hai bên cột (~1.5mm mỗi bên)
                            var barcodeMaxWidthMm = Math.Max(10f, colWidthMm - 3.0f);
                            bcImg.ScaleToFit(Mm(barcodeMaxWidthMm), Mm(7.5f));

                            tbl.AddCell(new PdfPCell(bcImg)
                            {
                                Border = Rectangle.NO_BORDER,
                                HorizontalAlignment = Element.ALIGN_CENTER,
                                PaddingTop = 2.0f,
                                PaddingBottom = 1.2f
                            });

                            tbl.AddCell(new PdfPCell(new Phrase(seqAndNgayLayMau, f5))
                            {
                                Border = Rectangle.NO_BORDER,
                                HorizontalAlignment = Element.ALIGN_CENTER,
                                PaddingBottom = 0.6f
                            });

                            tbl.AddCell(new PdfPCell(new Phrase(nameUpper, f5b))
                            {
                                Border = Rectangle.NO_BORDER,
                                HorizontalAlignment = Element.ALIGN_CENTER,
                                PaddingTop = 1.0f,
                                PaddingBottom = 0.6f
                            });

                            tbl.AddCell(new PdfPCell(new Phrase(dobAndSex, f5))
                            {
                                Border = Rectangle.NO_BORDER,
                                HorizontalAlignment = Element.ALIGN_CENTER,
                                PaddingBottom = 0.6f
                            });

                            tbl.AddCell(new PdfPCell(new Phrase(maDotKhamFormatted, f3b))
                            {
                                Border = Rectangle.NO_BORDER,
                                HorizontalAlignment = Element.ALIGN_CENTER,
                                //PaddingBottom = 0.6f
                            });

                            return new PdfPCell(tbl)
                            {
                                Border = Rectangle.NO_BORDER,
                                PaddingLeft = Mm(1.5f),
                                PaddingRight = Mm(1.5f)
                            };
                        }

                        for (int i = 0; i < soBangIn; i++)
                            root.AddCell(CreateLabelCell());

                        return root;
                    }

                    // --- Trang 1: 1 dòng (2 SID)
                    doc.Add(BuildRow());

                    // --- Page break sang trang 2
                    doc.NewPage();

                    // --- Trang 2: 1 dòng (2 SID) giống trang 1
                    doc.Add(BuildRow());

                    doc.Close();
                }

                return Content(Convert.ToBase64String(ms.ToArray()));
            }

            // helper mm -> point
            float Mm(float mm) => (float)(mm * 72.0 / 25.4);
        }


        string NormalizeSex(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "";
            raw = raw.Trim().ToUpperInvariant();
            if (raw == "M" || raw == "NAM" || raw == "1") return "Nam";
            if (raw == "F" || raw == "NU" || raw == "NỮ" || raw == "0" || raw == "2") return "Nữ";
            return "Khác";
        }
    }

}
