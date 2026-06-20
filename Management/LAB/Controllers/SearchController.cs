using Management.BL;
using Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Compression;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;
using Object = Management.Models.Object;

namespace Management.Controllers
{
    public class SearchController : Controller
    {
        public readonly ILogger<SearchController> _logger;
        public readonly PatientXNBL _patientXNBL;
        public readonly PatientCDHABL _patientCDHABL;
        public readonly ObjectBL _objectBL;
        public readonly LocationBL _locationBL;
        public readonly DoctorBL _doctorBL;
        public readonly UserBL _userBL;
        public readonly ServiceBL _serviceBL;
        public readonly ResultCDHABL _resultCDHABL;
        public readonly ResultXNBL _resultXNBL;
        private readonly ToolBL _toolBL;
        private readonly HospitalBL _hospitalBL;
        private readonly IWebHostEnvironment _environment;
        // Dictionary lưu trữ progress (thêm vào đầu class)
        private static readonly ConcurrentDictionary<string, DownloadProgress> _downloadProgress = new();
        private static readonly ConcurrentDictionary<string, DownloadProgress> _downloadProgressCDHA = new(); // Thêm cho CDHA

        public SearchController(ILogger<SearchController> logger, PatientCDHABL patientCDHABL, ObjectBL objectBL, LocationBL locationBL, HospitalBL hospitalBL, IWebHostEnvironment webHostEnvironment,
                            DoctorBL doctorBL, UserBL userBL, ServiceBL serviceBL, ResultCDHABL resultCDHABL, PatientXNBL patientXNBL, ResultXNBL resultXNBL, ToolBL toolBL)
        {
            _logger = logger;
            _patientCDHABL = patientCDHABL;
            _objectBL = objectBL;
            _locationBL = locationBL;
            _doctorBL = doctorBL;
            _userBL = userBL;
            _serviceBL = serviceBL;
            _resultCDHABL = resultCDHABL;
            _patientXNBL = patientXNBL;
            _resultXNBL = resultXNBL;
            _toolBL = toolBL;
            _hospitalBL = hospitalBL;
            _environment = webHostEnvironment;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
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

            ViewData["lstPatient"] = await _patientXNBL.Get_ListPatient(from, to);
            ViewData["lstObject"] = await _objectBL.GetListObject();
            ViewData["lstLocation"] = await _locationBL.GetListLocation();
            ViewData["lstDoctor"] = await _doctorBL.GetList();
            ViewData["lstSexModel"] = await SexModel.GetListSexModel();
            ViewData["lstBenhAnModel"] = await BenhAnModel.GetListBenhAnModel();
            return View();
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
            ViewData["lstPatient"] = await _patientXNBL.Get_ListPatient(from, to);

            return PartialView("_Search_ListPatient");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Search(DateTime timeSearchFrom, DateTime timeSearchTo, string pidorseq, string maDotKham)
        {
            // Save selected dates to session/cookies
            SaveSearchDatesToSession(timeSearchFrom, timeSearchTo);

            var from = new DateTime(timeSearchFrom.Year, timeSearchFrom.Month, timeSearchFrom.Day, 23, 59, 59).AddDays(-1);
            var to = new DateTime(timeSearchTo.Year, timeSearchTo.Month, timeSearchTo.Day, 23, 59, 59);

            List<Patient> lstPatient;

            // Xử lý trường hợp "Khách lẻ"
            if (maDotKham == "khach-le")
            {
                // Lấy danh sách bệnh nhân không có MaDotKham
                var allPatients = await _patientXNBL.Get_ListPatient(from, to);
                lstPatient = allPatients.Where(p => string.IsNullOrEmpty(p.MaDotKham)).ToList();

                // Nếu có pidorseq thì filter thêm
                if (!string.IsNullOrEmpty(pidorseq))
                {
                    lstPatient = lstPatient.Where(p =>
                        (!string.IsNullOrEmpty(p.PatientId) && p.PatientId.Contains(pidorseq)) ||
                        (!string.IsNullOrEmpty(p.Seq) && p.Seq.Contains(pidorseq)) ||
                        (!string.IsNullOrEmpty(p.MaBenhAn) && p.MaBenhAn.Contains(pidorseq))
                    ).ToList();
                }
            }
            else
            {
                lstPatient = await _patientXNBL.Get_ListPatientByPidOrSid(from, to, pidorseq, maDotKham);
            }

            ViewData["lstPatient"] = lstPatient;

            return PartialView("_Search_ListPatient");
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

        // Thêm endpoint lấy danh sách MaDotKham
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMaDotKhamList(DateTime timeSearchFrom, DateTime timeSearchTo)
        {
            var from = new DateTime(timeSearchFrom.Year, timeSearchFrom.Month, timeSearchFrom.Day, 00, 00, 00);
            var to = new DateTime(timeSearchTo.Year, timeSearchTo.Month, timeSearchTo.Day, 23, 59, 59);

            var lstPatient = await _patientXNBL.Get_ListPatient(from, to);

            var result = new
            {
                hasKhachLe = lstPatient.Any(p => string.IsNullOrEmpty(p.MaDotKham)),
                maDotKhamList = lstPatient
                    .Where(p => !string.IsNullOrEmpty(p.MaDotKham))
                    .Select(p => p.MaDotKham)
                    .Distinct()
                    .OrderBy(m => m)
                    .ToList()
            };

            return Json(result);
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
            ViewData["patientInfo"] = await _patientXNBL.Get_PatientBySid(id);

            return PartialView("_Search_PatientInfo");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetServiceForPatient(long patientId)
        {
            var searchResults = await _resultXNBL.GetListSearchByPatientId(patientId);

            // Group services by type similar to the download functionality
            var groupedServices = new List<object>();

            if (searchResults != null && searchResults.Any())
            {
                // Get all XN (lab test) services
                var xnServices = searchResults
                    .Where(x => !string.IsNullOrEmpty(x.KeyResultForHis) && x.KeyResultForHis.StartsWith("xn-"))
                    .ToList();

                // Get all CLS (clinical) services  
                var clsServices = searchResults
                    .Where(x => !string.IsNullOrEmpty(x.KeyResultForHis) && !x.KeyResultForHis.StartsWith("xn-"))
                    .ToList();

                // Add grouped XN services as single entry if any exist
                if (xnServices.Any())
                {
                    // Find the first XN service to get common properties
                    var firstXnService = xnServices.First();
                    groupedServices.Add(new
                    {
                        ServiceId = firstXnService.ServiceId,
                        ServiceName = "Xét nghiệm",
                        Status = xnServices.Any(x => x.Status == "Valid") ? "Valid" :
                                xnServices.Any(x => x.Status == "Process") ? "Process" : "Wait",
                        KeyResultForHis = firstXnService.KeyResultForHis,
                        IsGrouped = true,
                        GroupType = "XN",
                        ChildServices = xnServices
                    });
                }

                // Add individual CLS services
                foreach (var clsService in clsServices)
                {
                    groupedServices.Add(new
                    {
                        ServiceId = clsService.ServiceId,
                        ServiceName = clsService.ServiceName,
                        Status = clsService.Status,
                        KeyResultForHis = clsService.KeyResultForHis,
                        IsGrouped = false,
                        GroupType = GetServiceGroupType(clsService.KeyResultForHis),
                        ChildServices = (List<Search>)null
                    });
                }
            }

            ViewData["lstSearch"] = groupedServices;
            return PartialView("_Search_ListService");
        }

        /// <summary>
        /// Get service group type from KeyResultForHis
        /// </summary>
        private string GetServiceGroupType(string keyResultForHis)
        {
            if (string.IsNullOrEmpty(keyResultForHis)) return "OTHER";

            var categoryCode = keyResultForHis.Split("-")[0].ToUpper();
            return categoryCode switch
            {
                "XN" => "XN",
                "XQ" => "XQ",
                "SA" => "SA",
                "SAT" => "SAT",
                "DDT" => "DDT",
                "NS" => "NS",
                _ => "OTHER"
            };
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Print(string keyresultforhis)
        {
            var fileBase64 = string.Empty;
            try
            {                
                var categoryCode = keyresultforhis.Split("-")[0];
                if (!string.IsNullOrEmpty(categoryCode))
                {
                    var _file = Path.Combine(_environment.WebRootPath, "pdf", categoryCode, keyresultforhis + ".pdf");
                    if (System.IO.File.Exists(_file))
                    {
                        fileBase64 = Convert.ToBase64String(System.IO.File.ReadAllBytes(_file));
                    }
                }
            }
            catch { }          
            return Content(fileBase64);
        }

        /// <summary>
        /// Download tất cả PDF kết quả của bệnh nhân
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> DownloadAllResults(long patientId, string maBenhAn = null)
        {
            try
            {
                // 1. Lấy thông tin bệnh nhân
                var patient = await _patientXNBL.Get_PatientBySid(patientId);
                if (patient == null)
                {
                    return NotFound("Không tìm thấy thông tin bệnh nhân");
                }

                // 2. Lấy danh sách tất cả kết quả có KeyResultForHis
                var searchResults = await _resultXNBL.GetListSearchByPatientId(patientId);
                if (searchResults == null || !searchResults.Any())
                {
                    return BadRequest("Bệnh nhân chưa có kết quả nào");
                }

                // 3. Lọc chỉ những kết quả đã có PDF (status = "Valid") và có KeyResultForHis
                var completedResults = searchResults
                    .Where(x => !string.IsNullOrEmpty(x.KeyResultForHis) && x.Status == "Valid")
                    .ToList();

                if (!completedResults.Any())
                {
                    return BadRequest("Bệnh nhân chưa có kết quả hoàn thành nào");
                }

                // 4. GROUP theo KeyResultForHis để tránh trùng lặp PDF
                var uniqueResults = completedResults
                    .GroupBy(x => x.KeyResultForHis)
                    .Select(g => new
                    {
                        KeyResultForHis = g.Key,
                        CategoryCode = g.Key.Split("-")[0], // Lấy category code từ KeyResultForHis
                        ServiceName = g.First().ServiceName,
                        AllServiceNames = string.Join(", ", g.Select(r => r.ServiceName).Distinct()),
                        ResultCount = g.Count()
                    })
                    .OrderBy(x => x.KeyResultForHis)
                    .ToList();

                // 5. Tạo file ZIP chứa tất cả PDF (không trùng lặp)
                var zipFileName = $"KetQua_{patient?.PatientName}_{patient?.MaBenhAn ?? patient?.PatientId}_{DateTime.Now:yyyyMMddHHmmss}.zip";
                var tempZipPath = Path.Combine(Path.GetTempPath(), zipFileName);

                using (var archive = ZipFile.Open(tempZipPath, ZipArchiveMode.Create))
                {
                    int fileCount = 0;
                    foreach (var result in uniqueResults)
                    {
                        try
                        {
                            var pdfPath = Path.Combine(_environment.WebRootPath, "pdf", result.CategoryCode, result.KeyResultForHis + ".pdf");

                            if (System.IO.File.Exists(pdfPath))
                            {
                                // Tạo tên file chuẩn hóa dựa trên loại kết quả
                                var entryName = GetStandardizedFileName(++fileCount, result.CategoryCode, result.ServiceName, result.KeyResultForHis);

                                // Thêm file vào ZIP
                                archive.CreateEntryFromFile(pdfPath, entryName);
                                
                                _logger.LogInformation($"Added PDF to ZIP: {entryName} (Category: {result.CategoryCode}, from {result.ResultCount} results)");
                            }
                            else
                            {
                                _logger.LogWarning($"PDF file not found: {pdfPath}");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Không thể thêm file PDF {result.KeyResultForHis}: {ex.Message}");
                            // Continue với file tiếp theo
                        }
                    }
                }

                // 6. Kiểm tra ZIP có file không
                var zipInfo = new FileInfo(tempZipPath);
                if (zipInfo.Length == 0)
                {
                    System.IO.File.Delete(tempZipPath);
                    return BadRequest("Không có file PDF nào được tìm thấy");
                }

                // 7. Trả về file ZIP
                var zipBytes = await System.IO.File.ReadAllBytesAsync(tempZipPath);

                // Xóa file tạm
                System.IO.File.Delete(tempZipPath);

                // Log kết quả
                _logger.LogInformation($"Successfully created ZIP with {uniqueResults.Count} unique PDFs for patient {patient.PatientId}");

                return File(zipBytes, "application/zip", zipFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading all results for patient {patientId}");
                return StatusCode(500, "Có lỗi xảy ra khi tải kết quả");
            }
        }

        /// <summary>
        /// In KQXN theo danh mục 
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> PrintCategoryResults(long categoryId, long patientId)
        {
            try
            {
                // Get test results for the specific category and patient
                var testResults = await _resultXNBL.GetResultsByCategoryIdAndPatientId(categoryId, patientId);

                if (testResults == null || !testResults.Any())
                {
                    return NotFound("Không tìm thấy kết quả xét nghiệm cho danh mục này");
                }

                // Get hospital information
                var hospital = await _hospitalBL.GetHospital();
                if (hospital == null)
                {
                    return NotFound("Không tìm thấy thông tin bệnh viện");
                }

                // Get return user (similar to XN_Process logic)
                var returnUser = await _userBL.GetUser(GetUserLogin() ?? 0);

                // Prepare ViewData exactly like XN_Process
                ViewData["ListResultXN"] = testResults;
                ViewData["ReturnUser"] = returnUser;
                ViewData["Note"] = testResults[0].Note ?? "Các chỉ số bất thường được in đậm và có ghi chú tương ứng.";

                return View("PrintCategoryResults", hospital);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error printing category results for CategoryId={categoryId}, PatientId={patientId}");
                return StatusCode(500, "Có lỗi xảy ra khi in kết quả");
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetServiceForPatientGroupedByCategory(long patientId)
        {
            var searchResults = await _resultXNBL.GetListSearchByPatientIdGroupedByCategory(patientId);
            ViewData["lstSearchGrouped"] = searchResults;
            return PartialView("_Search_ListServiceGroupedByCategory");
        }

        /// <summary>
        /// In KQXN theo dịch vụ
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> PrintChildServiceResults(long serviceId, long patientId)
        {
            try
            {
                // Get test results for the specific service and patient
                var testResults = await _resultXNBL.GetResultsByServiceIdAndPatientId(serviceId, patientId);

                if (testResults == null || !testResults.Any())
                {
                    return NotFound("Không tìm thấy kết quả xét nghiệm cho dịch vụ này");
                }

                // Get hospital information
                var hospital = await _hospitalBL.GetHospital();
                if (hospital == null)
                {
                    return NotFound("Không tìm thấy thông tin bệnh viện");
                }

                // Get return user (similar to XN_Process logic)
                var returnUser = await _userBL.GetUser(GetUserLogin() ?? 0);

                // Prepare ViewData exactly like XN_Process
                ViewData["ListResultXN"] = testResults;
                ViewData["ReturnUser"] = returnUser;
                ViewData["Note"] = testResults[0].Note ?? "Các chỉ số bất thường được in đậm và có ghi chú tương ứng.";

                return View("PrintChildServiceResults", hospital);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error printing child service results for ServiceId={serviceId}, PatientId={patientId}");
                return StatusCode(500, "Có lỗi xảy ra khi in kết quả");
            }
        }

        /// <summary>
        /// Tạo tên file chuẩn hóa dựa trên loại kết quả
        /// </summary>
        private string GetStandardizedFileName(int fileCount, string categoryCode, string serviceName, string keyResultForHis)
        {
            var fileName = categoryCode.ToUpper() switch
            {
                "XN" => $"{fileCount:00}_Kết quả xét nghiệm.pdf",
                "XQ" => $"{fileCount:00}_Kết quả X-Quang_{serviceName}.pdf",
                "SA" => $"{fileCount:00}_Kết quả Siêu âm_{serviceName}.pdf",
                "SAT" => $"{fileCount:00}_Kết quả Siêu âm tim_{serviceName}.pdf",
                "DDT" => $"{fileCount:00}_Kết quả Điện tim_{serviceName}.pdf",
                "NS" => $"{fileCount:00}_Kết quả Nội soi_{serviceName}.pdf",
                _ => $"{fileCount:00}_Kết quả {serviceName}.pdf"
            };

            // Làm sạch tên file (loại bỏ ký tự không hợp lệ)
            fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            
            return fileName;
        }

        /// <summary>
        /// Tạo tên file chuẩn hóa với thông tin chi tiết (backup method)
        /// </summary>
        private string GetDetailedFileName(int fileCount, string categoryCode, string serviceName, string keyResultForHis)
        {
            var baseFileName = categoryCode.ToUpper() switch
            {
                "XN" => "Kết quả xét nghiệm",
                "XQ" => "Kết quả X-Quang", 
                "SA" => "Kết quả Siêu âm",
                "SAT" => "Kết quả Siêu âm tim",
                "DDT" => "Kết quả Điện tim",
                "NS" => "Kết quả Nội soi",
                _ => $"Kết quả {serviceName}"
            };

            var fileName = $"{fileCount:00}_{baseFileName}_{keyResultForHis}.pdf";

            // Làm sạch tên file (loại bỏ ký tự không hợp lệ)
            fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            
            // Giới hạn độ dài tên file
            if (fileName.Length > 200)
            {
                var extension = ".pdf";
                var nameWithoutExt = fileName.Substring(0, fileName.Length - extension.Length);
                fileName = nameWithoutExt.Substring(0, 200 - extension.Length) + extension;
            }
            
            return fileName;
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

        /// <summary>
        /// Download tất cả PDF kết quả xét nghiệm của nhiều bệnh nhân
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DownloadAllPatientsResults([FromBody] DownloadAllPatientsRequest request)
        {
            try
            {
                if (request?.PatientIds == null || !request.PatientIds.Any())
                {
                    return BadRequest("Danh sách bệnh nhân trống");
                }
                if (string.IsNullOrEmpty(request.MaDotKham))
                {
                    return BadRequest("Vui lòng chọn đợt khám");
                }

                var sessionId = Guid.NewGuid().ToString();
                var totalPatients = request.PatientIds.Count;

                // Initialize progress tracking
                _downloadProgress[sessionId] = new DownloadProgress
                {
                    Total = totalPatients,
                    Current = 0,
                    Status = "Đang khởi tạo...",
                    StartTime = DateTime.Now
                };

                _logger.LogInformation($"Starting download XN for {totalPatients} patients in MaDotKham: {request.MaDotKham} with SessionId: {sessionId}");

                var patientResultsDict = new Dictionary<long, PatientResultInfo>();
                int processedCount = 0;

                // 1. Lấy thông tin và kết quả của từng bệnh nhân
                foreach (var patientId in request.PatientIds)
                {
                    try
                    {
                        processedCount++;
                        _downloadProgress[sessionId].Current = processedCount;
                        _downloadProgress[sessionId].Status = $"Đang xử lý bệnh nhân {processedCount}/{totalPatients}";

                        // Lấy thông tin bệnh nhân
                        var patient = await _patientXNBL.Get_PatientBySid(patientId);
                        if (patient == null) continue;

                        // Lấy danh sách kết quả xét nghiệm (chỉ lấy XN, status = "Valid")
                        var searchResults = await _resultXNBL.GetListSearchByPatientId(patientId);
                        if (searchResults == null || !searchResults.Any()) continue;

                        // Lọc chỉ kết quả XN đã hoàn thành
                        var xnResults = searchResults
                            .Where(x => !string.IsNullOrEmpty(x.KeyResultForHis) &&
                                       x.KeyResultForHis.StartsWith("xn-") &&
                                       x.Status == "Valid")
                            .GroupBy(x => x.KeyResultForHis)
                            .Select(g => g.Key)
                            .ToList();

                        if (xnResults.Any())
                        {
                            patientResultsDict[patientId] = new PatientResultInfo
                            {
                                PatientId = patient.PatientId,
                                PatientName = patient.PatientName,
                                MaBenhAn = patient.MaBenhAn,
                                KeyResultForHisList = xnResults
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Error processing patient {patientId}: {ex.Message}");
                        continue;
                    }
                }

                if (!patientResultsDict.Any())
                {
                    _downloadProgress.TryRemove(sessionId, out _);
                    return BadRequest("Không có bệnh nhân nào có kết quả xét nghiệm hoàn thành");
                }

                // Update progress
                _downloadProgress[sessionId].Status = "Đang tạo file ZIP...";

                // 2. Tạo file ZIP với tên theo format: {maDotKham}_XN_{ddMMyyyyHHmmss}.zip
                var timestamp = DateTime.Now.ToString("ddMMyyyyHHmmss");
                var sanitizedMaDotKham = SanitizeFileName(request.MaDotKham);
                var zipFileName = $"{sanitizedMaDotKham}_XN_{timestamp}.zip";
                var tempZipPath = Path.Combine(Path.GetTempPath(), zipFileName);

                using (var archive = ZipFile.Open(tempZipPath, ZipArchiveMode.Create))
                {
                    int totalFiles = 0;
                    int patientIndex = 0;

                    foreach (var kvp in patientResultsDict)
                    {
                        patientIndex++;
                        var patientInfo = kvp.Value;

                        // Update progress
                        _downloadProgress[sessionId].Current = totalPatients; // Đặt = total vì đã xử lý xong phần lấy data
                        _downloadProgress[sessionId].Status = $"Đang nén file {patientIndex}/{patientResultsDict.Count}";

                        // Tạo folder riêng cho mỗi bệnh nhân trong ZIP
                        var patientFolderName = $"{patientIndex:000}_{SanitizeFileName(patientInfo.PatientName)}_{patientInfo.MaBenhAn ?? patientInfo.PatientId}";

                        int fileCount = 0;
                        foreach (var keyResultForHis in patientInfo.KeyResultForHisList)
                        {
                            try
                            {
                                // Đường dẫn file PDF
                                var pdfPath = Path.Combine(_environment.WebRootPath, "pdf", "xn", keyResultForHis + ".pdf");

                                if (System.IO.File.Exists(pdfPath))
                                {
                                    fileCount++;
                                    // Tên file trong ZIP
                                    var entryName = $"{patientFolderName}/{fileCount:00}_KetQuaXN_{keyResultForHis}.pdf";

                                    // Thêm file vào ZIP
                                    archive.CreateEntryFromFile(pdfPath, entryName);
                                    totalFiles++;

                                    _logger.LogInformation($"Added: {entryName}");
                                }
                                else
                                {
                                    _logger.LogWarning($"PDF not found: {pdfPath}");
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning($"Error adding PDF {keyResultForHis}: {ex.Message}");
                                continue;
                            }
                        }

                        // Log số file của mỗi bệnh nhân
                        _logger.LogInformation($"Patient {patientInfo.PatientName}: {fileCount} files");
                    }

                    _logger.LogInformation($"Total files added to ZIP: {totalFiles}");
                }

                // 3. Kiểm tra ZIP có file không
                var zipInfo = new FileInfo(tempZipPath);
                if (zipInfo.Length == 0)
                {
                    System.IO.File.Delete(tempZipPath);
                    _downloadProgress.TryRemove(sessionId, out _);
                    return BadRequest("Không có file PDF nào được tìm thấy");
                }

                // 4. Đọc file ZIP
                var zipBytes = await System.IO.File.ReadAllBytesAsync(tempZipPath);

                // Xóa file tạm
                System.IO.File.Delete(tempZipPath);

                // *** QUAN TRỌNG: CẬP NHẬT STATUS = "Completed" THAY VÌ XÓA ***
                _downloadProgress[sessionId].Status = "Hoàn thành";
                _downloadProgress[sessionId].Current = totalPatients;

                // Log kết quả
                _logger.LogInformation($"Successfully created ZIP for {patientResultsDict.Count} patients in {request.MaDotKham}");

                // 5. Set header TRƯỚC KHI return File
                Response.Headers.Add("X-Session-Id", sessionId);
                Response.Headers.Add("Access-Control-Expose-Headers", "X-Session-Id"); // Quan trọng cho CORS!

                // *** Schedule cleanup sau 30 giây ***
                _ = Task.Run(async () =>
                {
                    await Task.Delay(30000); // Đợi 30 giây
                    _downloadProgress.TryRemove(sessionId, out _);
                    _logger.LogInformation($"Cleaned up session {sessionId}");
                });

                return File(zipBytes, "application/zip", zipFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading all patients results");
                return StatusCode(500, "Có lỗi xảy ra khi tải kết quả");
            }
        }

        /// <summary>
        /// Download tất cả PDF kết quả CDHA (SA, SAT, XQ, DDT, NS) của nhiều bệnh nhân
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DownloadAllPatientsCDHAResults([FromBody] DownloadAllPatientsRequest request)
        {
            try
            {
                if (request?.PatientIds == null || !request.PatientIds.Any())
                {
                    return BadRequest("Danh sách bệnh nhân trống");
                }

                if (string.IsNullOrEmpty(request.MaDotKham))
                {
                    return BadRequest("Vui lòng chọn đợt khám");
                }

                var sessionId = Guid.NewGuid().ToString();
                var totalPatients = request.PatientIds.Count;

                // Initialize progress tracking
                _downloadProgressCDHA[sessionId] = new DownloadProgress
                {
                    Total = totalPatients,
                    Current = 0,
                    Status = "Đang khởi tạo CDHA...",
                    StartTime = DateTime.Now
                };

                _logger.LogInformation($"Starting CDHA download for {totalPatients} patients in MaDotKham: {request.MaDotKham} with SessionId: {sessionId}");

                // Danh sách các category code CDHA
                var cdhaCategories = new[] { "SA", "SAT", "XQ", "DDT", "NS" };

                // Dictionary để lưu thông tin: PatientId -> List<KeyResultForHis>
                var patientResultsDict = new Dictionary<long, PatientCDHAResultInfo>();
                int processedCount = 0;

                // 1. Lấy thông tin và kết quả CDHA của từng bệnh nhân
                foreach (var patientId in request.PatientIds)
                {
                    try
                    {
                        processedCount++;
                        _downloadProgressCDHA[sessionId].Current = processedCount;
                        _downloadProgressCDHA[sessionId].Status = $"Đang xử lý bệnh nhân CDHA {processedCount}/{totalPatients}";

                        // Lấy thông tin bệnh nhân
                        var patient = await _patientXNBL.Get_PatientBySid(patientId);
                        if (patient == null) continue;

                        // Lấy danh sách kết quả
                        var searchResults = await _resultXNBL.GetListSearchByPatientId(patientId);
                        if (searchResults == null || !searchResults.Any()) continue;

                        // Lọc chỉ kết quả CDHA đã hoàn thành (SA, SAT, XQ, DDT, NS)
                        var cdhaResults = searchResults
                            .Where(x => !string.IsNullOrEmpty(x.KeyResultForHis) &&
                                       x.Status == "Valid" &&
                                       cdhaCategories.Any(cat => x.KeyResultForHis.StartsWith(cat.ToLower() + "-", StringComparison.OrdinalIgnoreCase)))
                            .GroupBy(x => x.KeyResultForHis)
                            .Select(g => new
                            {
                                KeyResultForHis = g.Key,
                                CategoryCode = g.Key.Split("-")[0].ToUpper(),
                                ServiceName = g.First().ServiceName
                            })
                            .ToList();

                        if (cdhaResults.Any())
                        {
                            patientResultsDict[patientId] = new PatientCDHAResultInfo
                            {
                                PatientId = patient.PatientId,
                                PatientName = patient.PatientName,
                                MaBenhAn = patient.MaBenhAn,
                                CDHAResults = cdhaResults.Select(r => new CDHAResultItem
                                {
                                    KeyResultForHis = r.KeyResultForHis,
                                    CategoryCode = r.CategoryCode,
                                    ServiceName = r.ServiceName
                                }).ToList()
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Error processing CDHA for patient {patientId}: {ex.Message}");
                        continue;
                    }
                }

                if (!patientResultsDict.Any())
                {
                    _downloadProgressCDHA.TryRemove(sessionId, out _);
                    return BadRequest("Không có bệnh nhân nào có kết quả CDHA hoàn thành");
                }

                // Update progress
                _downloadProgressCDHA[sessionId].Status = "Đang tạo file ZIP CDHA...";

                // 2. Tạo file ZIP với tên theo format: {maDotKham}_CDHA_{ddMMyyyyHHmmss}.zip
                var timestamp = DateTime.Now.ToString("ddMMyyyyHHmmss");
                var sanitizedMaDotKham = SanitizeFileName(request.MaDotKham);
                var zipFileName = $"{sanitizedMaDotKham}_CDHA_{timestamp}.zip";
                var tempZipPath = Path.Combine(Path.GetTempPath(), zipFileName);

                using (var archive = ZipFile.Open(tempZipPath, ZipArchiveMode.Create))
                {
                    int totalFiles = 0;
                    int patientIndex = 0;

                    foreach (var kvp in patientResultsDict)
                    {
                        patientIndex++;
                        var patientInfo = kvp.Value;

                        _downloadProgressCDHA[sessionId].Current = totalPatients; // Đặt = total
                        _downloadProgressCDHA[sessionId].Status = $"Đang nén file CDHA {patientIndex}/{patientResultsDict.Count}";

                        // Tạo folder riêng cho mỗi bệnh nhân trong ZIP
                        var patientFolderName = $"{patientIndex:000}_{SanitizeFileName(patientInfo.PatientName)}_{patientInfo.MaBenhAn ?? patientInfo.PatientId}";

                        // Group results by category for better organization
                        var groupedByCategory = patientInfo.CDHAResults
                            .GroupBy(r => r.CategoryCode)
                            .OrderBy(g => g.Key);

                        int fileCount = 0;
                        foreach (var categoryGroup in groupedByCategory)
                        {
                            var categoryCode = categoryGroup.Key;

                            foreach (var result in categoryGroup)
                            {
                                try
                                {
                                    // Đường dẫn file PDF trong thư mục tương ứng
                                    var pdfPath = Path.Combine(_environment.WebRootPath, "pdf", categoryCode.ToLower(), result.KeyResultForHis + ".pdf");

                                    if (System.IO.File.Exists(pdfPath))
                                    {
                                        fileCount++;
                                        // Tên file trong ZIP với prefix category
                                        var categoryName = GetCategoryDisplayName(categoryCode);
                                        var entryName = $"{patientFolderName}/{fileCount:00}_{categoryName}_{SanitizeFileName(result.ServiceName)}.pdf";

                                        // Thêm file vào ZIP
                                        archive.CreateEntryFromFile(pdfPath, entryName);
                                        totalFiles++;
                                    }
                                    else
                                    {
                                        _logger.LogWarning($"CDHA PDF not found: {pdfPath}");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning($"Error adding CDHA PDF {result.KeyResultForHis}: {ex.Message}");
                                    continue;
                                }
                            }
                        }

                        // Log số file của mỗi bệnh nhân
                        _logger.LogInformation($"Patient {patientInfo.PatientName}: {fileCount} CDHA files");
                    }

                    _logger.LogInformation($"Total CDHA files added to ZIP: {totalFiles}");
                }

                // 3. Kiểm tra ZIP có file không
                var zipInfo = new FileInfo(tempZipPath);
                if (zipInfo.Length == 0)
                {
                    System.IO.File.Delete(tempZipPath);
                    _downloadProgressCDHA.TryRemove(sessionId, out _);
                    return BadRequest("Không có file PDF CDHA nào được tìm thấy");
                }

                // 4. Đọc file ZIP
                var zipBytes = await System.IO.File.ReadAllBytesAsync(tempZipPath);

                // Xóa file tạm
                System.IO.File.Delete(tempZipPath);

                // *** QUAN TRỌNG: CẬP NHẬT STATUS = "Completed" THAY VÌ XÓA ***
                _downloadProgressCDHA[sessionId].Status = "Hoàn thành";
                _downloadProgressCDHA[sessionId].Current = totalPatients;

                // Log kết quả
                _logger.LogInformation($"Successfully created CDHA ZIP for {patientResultsDict.Count} patients in {request.MaDotKham}");

                // 5. Set header TRƯỚC KHI return File
                Response.Headers.Add("X-Session-Id", sessionId);
                Response.Headers.Add("Access-Control-Expose-Headers", "X-Session-Id"); // Quan trọng cho CORS!

                // *** Schedule cleanup sau 30 giây ***
                _ = Task.Run(async () =>
                {
                    await Task.Delay(30000); // Đợi 30 giây
                    _downloadProgressCDHA.TryRemove(sessionId, out _);
                    _logger.LogInformation($"Cleaned up CDHA session {sessionId}");
                });

                return File(zipBytes, "application/zip", zipFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading all patients CDHA results");
                return StatusCode(500, "Có lỗi xảy ra khi tải kết quả CDHA");
            }
        }

        [HttpGet]
        [Authorize]
        public IActionResult GetDownloadProgress(string sessionId)
        {
            if (_downloadProgress.TryGetValue(sessionId, out var progress))
            {
                return Json(new
                {
                    total = progress.Total,
                    current = progress.Current,
                    status = progress.Status,
                    percentage = progress.Total > 0 ? (progress.Current * 100 / progress.Total) : 0
                });
            }
            return Json(new { total = 0, current = 0, status = "Not found", percentage = 0 });
        }

        [HttpGet]
        [Authorize]
        public IActionResult GetDownloadProgressCDHA(string sessionId)
        {
            if (_downloadProgressCDHA.TryGetValue(sessionId, out var progress))
            {
                return Json(new
                {
                    total = progress.Total,
                    current = progress.Current,
                    status = progress.Status,
                    percentage = progress.Total > 0 ? (progress.Current * 100 / progress.Total) : 0
                });
            }
            return Json(new { total = 0, current = 0, status = "Not found", percentage = 0 });
        }

        /// <summary>
        /// Làm sạch tên file, loại bỏ ký tự không hợp lệ
        /// </summary>
        private string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return "Unknown";

            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", fileName.Split(invalidChars));

            // Giới hạn độ dài
            if (sanitized.Length > 50)
            {
                sanitized = sanitized.Substring(0, 50);
            }

            return sanitized;
        }

        // Helper classes
        public class DownloadAllPatientsRequest
        {
            public List<long> PatientIds { get; set; }
            public string MaDotKham { get; set; }
        }

        private class PatientResultInfo
        {
            public string PatientId { get; set; }
            public string PatientName { get; set; }
            public string MaBenhAn { get; set; }
            public List<string> KeyResultForHisList { get; set; }
        }

        /// <summary>
        /// Lấy tên hiển thị theo category code
        /// </summary>
        private string GetCategoryDisplayName(string categoryCode)
        {
            return categoryCode.ToUpper() switch
            {
                "SA" => "SieuAm",
                "SAT" => "SieuAmTim",
                "XQ" => "XQuang",
                "DDT" => "DienTim",
                "NS" => "NoiSoi",
                _ => categoryCode
            };
        }

        // Helper classes for CDHA
        private class PatientCDHAResultInfo
        {
            public string PatientId { get; set; }
            public string PatientName { get; set; }
            public string MaBenhAn { get; set; }
            public List<CDHAResultItem> CDHAResults { get; set; }
        }

        private class CDHAResultItem
        {
            public string KeyResultForHis { get; set; }
            public string CategoryCode { get; set; }
            public string ServiceName { get; set; }
        }

        public class DownloadProgress
        {
            public int Total { get; set; }
            public int Current { get; set; }
            public string Status { get; set; }
            public DateTime StartTime { get; set; }
        }
    }
}
