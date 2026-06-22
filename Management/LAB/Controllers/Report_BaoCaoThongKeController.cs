using Management.BL;
using Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Management.Controllers
{
    public class Report_BaoCaoThongKeController : Controller
    {
        
        public readonly ILogger<Report_BaoCaoThongKeController> _logger;
        public readonly PatientCDHABL _patientCDHABL;
        public readonly PatientXNBL _patientXNBL;
        public readonly LocationBL _locationBL;
        public readonly UserBL _userBL;
        public readonly ServiceBL _serviceBL;
        public readonly ResultCDHABL _resultCDHABL;
        public readonly ResultXNBL _resultXNBL;
        public readonly HospitalBL _hospitalBL;
        public readonly ReportBL _reportBL;
        public readonly IWebHostEnvironment _environment;
        public readonly ToolBL _toolBL;

        public Report_BaoCaoThongKeController(ILogger<Report_BaoCaoThongKeController> logger, PatientCDHABL patientCDHABL, PatientXNBL patientXNBL, LocationBL locationBL, ToolBL toolBL,
                                              UserBL userBL, ServiceBL serviceBL, ResultCDHABL resultCDHABL, ResultXNBL resultXNBL, ReportBL reportBL, HospitalBL hospitalBL, IWebHostEnvironment environment)
        {
            _logger = logger;
            _patientCDHABL = patientCDHABL;
            _patientXNBL = patientXNBL;
            _resultCDHABL = resultCDHABL;
            _resultXNBL = resultXNBL;
            _locationBL = locationBL;
            _userBL = userBL;
            _serviceBL = serviceBL;
            _reportBL = reportBL;
            _hospitalBL = hospitalBL;
            _environment = environment;
            _toolBL = toolBL;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            ViewData["lstUser"] = await _userBL.GetListUser();          
            ViewData["lstHospital"] = await _hospitalBL.GetListHospital();
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Search(DateTime from, DateTime to, string type, string location, long userid, string hospitalId)
        {
            var fromtime = new DateTime(from.Year, from.Month, from.Day, 23, 59, 59).AddDays(-1);
            var totime = new DateTime(to.Year, to.Month, to.Day, 23, 59, 59);
            var user = await _userBL.GetUser(userid);
            var hospital = await _hospitalBL.GetHospital();
            long? hospitalGuiMau = (string.IsNullOrEmpty(hospitalId) || hospitalId == "null") ? null : long.Parse(hospitalId);

                if (type == "1")
            {
                ViewData["lstReportResult"] = await _reportBL.GetReportByService(fromtime, totime, location, user.Name, hospitalGuiMau);
                return PartialView("_ReportByService");
            }
            else if(type == "2")
            {
                ViewData["lstReportResult"] = await _reportBL.GetReportByPatient(fromtime, totime, location, user.Name, hospitalGuiMau);
                return PartialView("_ReportByPatient");
            }
            else if (type == "3")
            {
                ViewData["lstReportResult"] = await _reportBL.GetReportByProcess(fromtime, totime, location, user.Name, hospitalGuiMau);
                return PartialView("_ReportByProcess");
            }
            else if (type == "4")
            {
                ViewData["lstReportResult"] = await _reportBL.GetReportByTestCovid(fromtime, totime, user.Name, hospitalGuiMau);
                return PartialView("_ReportByTestCovid");
            }
            return null;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> LC_Search(DateTime from, DateTime to, string type, string location, long userid, string categoryCode = null)
        {
            var fromtime = new DateTime(from.Year, from.Month, from.Day, 23, 59, 59).AddDays(-1);
            var totime = new DateTime(to.Year, to.Month, to.Day, 23, 59, 59);
            var user = await _userBL.GetUser(userid);

            if (type == "1")
            {
                ViewData["lstReportResult"] = await _reportBL.LC_GetReportByService(fromtime, totime, location, user.Name, categoryCode);
                return PartialView("_ReportByService");
            }
            else if (type == "2")
            {
                ViewData["lstReportResult"] = await _reportBL.LC_GetReportByPatient(fromtime, totime, location, user.Name);
                return PartialView("_ReportByPatient");
            }
            else if (type == "3")
            {
                var mergedData = await _reportBL.LC_GetReportByProcessWithPatients(from, to, location, user.Name);
                var summaryData = _reportBL.LC_FlattenReportProcessForView(mergedData);

                ViewData["MergedProcessJson"] = System.Text.Json.JsonSerializer.Serialize(
                    mergedData.Select(d => new
                    {
                        d.DoctorID,
                        d.DoctorName,
                        Services = d.Services.Select(s => new
                        {
                            s.ServiceID,
                            s.ServiceName,
                            Patients = s.Patients.Select(p => new
                            {
                                p.PatientID,
                                p.PatientName,
                                p.Sid,
                                p.Seq,
                                p.MaBenhAn,
                                InsertTimeText = p.InsertTime.HasValue
                                    ? p.InsertTime.Value.ToString("dd/MM/yyyy HH:mm")
                                    : ""
                            })
                        })
                    })
                );

                return PartialView("_ReportByProcess", summaryData);
            }
            else if (type == "4")
            {
                //var model = await _reportBL.LC_GetPatientResultByDateRangeXN(fromtime, totime);
                //return PartialView("_ReportByDateRangeXNDetail", model);

                var model = await _reportBL.LC_GetPatientResultGroupedByServiceXN(fromtime, totime);
                return PartialView("_ReportGroupedByServiceXNDetail", model);
            }
            else if (type == "5")
            {
                var model = await _reportBL.LC_GetPatientServicesByDateXN(fromtime, totime);
                return PartialView("_ReportPatientServicesByDateXN", model);
            }
            return null;
        }

        public async Task<IActionResult> ServiceUsageByMaDotKham(string maDotKham)
        {
            var report = await _reportBL.LC_GetServiceUsageByMaDotKham(maDotKham);

            // report.Services: list cột dịch vụ
            // report.Rows: list bệnh nhân + dictionary ServiceMarks

            return PartialView("_Report_BenhNhanThucHienDichVu");
        }


        [HttpGet]
        public async Task<ActionResult> GetPatientResultByServiceXN(long serviceId, DateTime fromtime, DateTime totime)
        {
            var debugFrom = fromtime;
            var debugTo = totime;
            var from = new DateTime(fromtime.Year, fromtime.Month, fromtime.Day, 23, 59, 59).AddDays(-1);
            var to = new DateTime(totime.Year, totime.Month, totime.Day, 23, 59, 59);
            var debugFromb = from;
            var debugToc = to;
            var model = await _reportBL.LC_GetPatientResultByServiceXN(serviceId, fromtime, totime);
            return PartialView("_ReportByServiceXNDetail", model);
        }

        [HttpGet]
        public async Task<ActionResult> GetPatientResultByServiceCDHA(long serviceId, DateTime fromtime, DateTime totime, string location)
        {
            var from = new DateTime(fromtime.Year, fromtime.Month, fromtime.Day, 23, 59, 59).AddDays(-1);
            var to = new DateTime(totime.Year, totime.Month, totime.Day, 23, 59, 59);

            var model = await _reportBL.LC_GetPatientResultByServiceCDHA(serviceId, fromtime, totime, location);
            return PartialView("_ReportByServiceCDHADetail", model);
        }

        [HttpGet]
        public async Task<ActionResult> GetPatientResultByDateRangeXN(DateTime fromtime, DateTime totime)
        {
            var model = await _reportBL.LC_GetPatientResultByDateRangeXN(fromtime, totime);
            return PartialView("_ReportByDateRangeXNDetail", model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Export_Pdf(DateTime from, DateTime to, string type, string location, long userid, string hospitalId)
        {
            var fromtime = new DateTime(from.Year, from.Month, from.Day, 23, 59, 59).AddDays(-1);
            var totime = new DateTime(to.Year, to.Month, to.Day, 23, 59, 59);
            var user = await _userBL.GetUser(userid);
            var hospital = await _hospitalBL.GetHospital();
            long? hospitalGuiMau = (string.IsNullOrEmpty(hospitalId) || hospitalId == "null") ? null : long.Parse(hospitalId);

            if (type == "1")
            {
                var lstReportResult = await _reportBL.GetReportByService(fromtime, totime, location, user.Name, hospitalGuiMau);
                if (lstReportResult != null && lstReportResult.Count > 0)
                {
                    try
                    {
                        ViewData["lstReportResult"] = lstReportResult;
                        var content = await this.RenderViewAsync("_Service_Content", hospital);
                        var header = await this.RenderViewAsync("_Service_Header", hospital);
                        var footer = await this.RenderViewAsync("_Service_Footer", hospital);
                        var _folder = Path.Combine(_environment.WebRootPath, "pdf", "report");
                        var _file = Path.Combine(_folder, Guid.NewGuid() + ".pdf");
                        var fileBase64 = await _toolBL.ExportPdf_Report(_folder, _file, header, content, footer);
                        return Content(fileBase64);
                    }
                    catch { }
                }
                return Content(string.Empty);
            }
            else if (type == "2")
            {
                var lstReportResult = await _reportBL.GetReportByPatient(fromtime, totime, location, user.Name, hospitalGuiMau);
                if (lstReportResult != null && lstReportResult.Count > 0)
                {
                    try
                    {
                        ViewData["lstReportResult"] = lstReportResult;
                        var _hospital = await _hospitalBL.GetHospital();
                        var content = await this.RenderViewAsync("_Patient_Content", hospital);
                        var header = await this.RenderViewAsync("_Patient_Header", hospital);
                        var footer = await this.RenderViewAsync("_Patient_Footer", hospital);
                        var _folder = Path.Combine(_environment.WebRootPath, "pdf", "report");
                        var _file = Path.Combine(_folder, Guid.NewGuid() + ".pdf");
                        var fileBase64 = await _toolBL.ExportPdf_Report(_folder, _file, header, content, footer);
                        return Content(fileBase64);
                    }
                    catch { }
                }
                return Content(string.Empty);
            }
            else if (type == "3")
            {
                var lstReportResult = await _reportBL.GetReportByProcess(fromtime, totime, location, user.Name, hospitalGuiMau);
                if (lstReportResult != null && lstReportResult.Count > 0)
                {
                    try
                    {
                        ViewData["lstReportResult"] = lstReportResult;
                        var _hospital = await _hospitalBL.GetHospital();
                        var content = await this.RenderViewAsync("_Process_Content", hospital);
                        var header = await this.RenderViewAsync("_Process_Header", hospital);
                        var footer = await this.RenderViewAsync("_Process_Footer", hospital);
                        var _folder = Path.Combine(_environment.WebRootPath, "pdf", "report");
                        var _file = Path.Combine(_folder, Guid.NewGuid() + ".pdf");
                        var fileBase64 = await _toolBL.ExportPdf_Report(_folder, _file, header, content, footer);
                        return Content(fileBase64);
                    }
                    catch { }
                }
                return Content(string.Empty);
            }
            else if (type == "4")
            {
                var lstReportResult = await _reportBL.GetReportByTestCovid(fromtime, totime, user.Name, hospitalGuiMau);
                if (lstReportResult != null && lstReportResult.Count > 0)
                {
                    try
                    {
                        ViewData["lstReportResult"] = lstReportResult;
                        var _hospital = await _hospitalBL.GetHospital();
                        var content = await this.RenderViewAsync("_Covid_Content", hospital);
                        var header = await this.RenderViewAsync("_Covid_Header", hospital);
                        var footer = await this.RenderViewAsync("_Covid_Footer", hospital);
                        var _folder = Path.Combine(_environment.WebRootPath, "pdf", "report");
                        var _file = Path.Combine(_folder, Guid.NewGuid() + ".pdf");
                        var fileBase64 = await _toolBL.ExportPdf_Report_Landscape(_folder, _file, header, content, footer);
                        return Content(fileBase64);
                    }
                    catch { }
                }
                return Content(string.Empty);
            }
            return Content(string.Empty);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetPatientDetail(DateTime fromtime, DateTime totime, string location)
        {
            try
            {
                var from = new DateTime(fromtime.Year, fromtime.Month, fromtime.Day, 23, 59, 59).AddDays(-1);
                var to = new DateTime(totime.Year, totime.Month, totime.Day, 23, 59, 59);
                // Lấy dữ liệu báo cáo
                var user = GetUserLogin();
                var reportResult = await _reportBL.LC_GetReportByPatient(from, to, location, user.Name);
                
                // Lấy danh sách chi tiết bệnh nhân
                List<ReportPatientDetail> patientDetails = new List<ReportPatientDetail>();
                if (reportResult != null && reportResult.Count > 0 && reportResult[0].PatientDetails != null)
                {
                    patientDetails = reportResult[0].PatientDetails;
                }
                
                return PartialView("_ReportByPatientDetail", patientDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting patient detail");
                return PartialView("_ReportByPatientDetail", new List<ReportPatientDetail>());
            }
        }

        private UserLogin GetUserLogin()
        {
            var userJson = HttpContext.Session.GetString(SessionKeyModel._sessionUserLogin);
            if (!string.IsNullOrEmpty(userJson))
            {
                return JsonConvert.DeserializeObject<UserLogin>(userJson);
            }
            return new UserLogin { Name = "System" };
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ExportPatientServicesByDateXN(DateTime fromtime, DateTime totime)
        {
            var model = await _reportBL.LC_GetPatientServicesByDateXN(fromtime, totime);

            var fileBytes = _reportBL.ExportPatientServicesByDateToExcel(model);

            var fileName = $"BaoCaoXN_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        [HttpGet]
        public async Task<IActionResult> GetXNReportCategories()
        {
            try
            {
                var categories = await _reportBL.GetXNReportCategories();

                return Json(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}
