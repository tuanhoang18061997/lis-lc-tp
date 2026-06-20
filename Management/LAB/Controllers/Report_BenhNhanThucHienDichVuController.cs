using Management.BL;
using Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Management.Controllers
{
    public class Report_BenhNhanThucHienDichVuController : Controller
    {
        
        public readonly ILogger<Report_BenhNhanThucHienDichVuController> _logger;
        public readonly PatientXNBL _patientXNBL;
        public readonly LocationBL _locationBL;
        public readonly UserBL _userBL;
        public readonly ServiceBL _serviceBL;
        public readonly ResultXNBL _resultXNBL;
        public readonly HospitalBL _hospitalBL;
        public readonly ReportBL _reportBL;
        public readonly IWebHostEnvironment _environment;
        public readonly ToolBL _toolBL;

        public Report_BenhNhanThucHienDichVuController(ILogger<Report_BenhNhanThucHienDichVuController> logger, PatientXNBL patientXNBL, LocationBL locationBL, ToolBL toolBL,
                                              UserBL userBL, ServiceBL serviceBL, ResultXNBL resultXNBL, ReportBL reportBL, HospitalBL hospitalBL, IWebHostEnvironment environment)
        {
            _logger = logger;
            _patientXNBL = patientXNBL;
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
        public async Task<IActionResult> GetBenhNhanThucHienDichVu(string maDotKham)
        {
            var report = await _reportBL.LC_GetServiceUsageByMaDotKham(maDotKham);
            ViewData["Report"] = report;
            return PartialView("_ReportBenhNhanThucHienDichVu");
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

        //public async Task<IActionResult> Full()
        //{
        //    var fromtime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59).AddDays(-1);
        //    var totime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
        //    var user = await _userBL.GetUser(5);

        //    ViewData["lstReportResult"] = await _reportBL.GetReportByTestCovid(fromtime, totime,"Trần Ngọc Oanh");
        //    var _hospital = await _hospitalBL.GetHospital();

        //    return View(_hospital);
        //}
    }
}
