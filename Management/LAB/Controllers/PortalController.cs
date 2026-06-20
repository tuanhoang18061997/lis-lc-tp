using Management.BL;
using Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Management.Controllers
{
    public class PortalController : Controller
    {
        private readonly ILogger<PortalController> _logger;
        private readonly PatientCDHABL _patientCDHABL;
        private readonly ResultCDHABL _resultCDHABL;
        private readonly ResultXNBL _resultXNBL;
        private readonly HospitalBL _hospitalBL;
        private readonly ServiceBL _serviceBL;
        private readonly UserBL _userBL;
        private readonly IConfiguration _configuration;

        public PortalController(ILogger<PortalController> logger,
            PatientCDHABL patientCDHABL,
            ResultCDHABL resultCDHABL,
            ResultXNBL resultXNBL,
            HospitalBL hospitalBL,
            ServiceBL serviceBL,
            UserBL userBL,
            IConfiguration configuration)
        {
            _logger = logger;
            _patientCDHABL = patientCDHABL;
            _resultCDHABL = resultCDHABL;
            _resultXNBL = resultXNBL;
            _hospitalBL = hospitalBL;
            _serviceBL = serviceBL;
            _userBL = userBL;
            _configuration = configuration;
        }

        /// <summary>
        /// Trang chính để xem kết quả cận lâm sàng của bệnh nhân
        /// URL: /Portal/ViewResults?token=xxx&maBenhAn=BA123456&fromDate=2024-01-01&toDate=2024-12-31
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ViewResults(string token, string maBenhAn, string fromDate = null, string toDate = null)
        {
            try
            {
                // 1. Validate token
                if (!ValidateToken(token, maBenhAn))
                {
                    ViewBag.ErrorMessage = "Token không hợp lệ hoặc đã hết hạn";
                    return View("Error");
                }

                // 2. Parse dates
                DateTime from = string.IsNullOrEmpty(fromDate)
                    ? DateTime.Now.AddMonths(-6)
                    : DateTime.Parse(fromDate);
                DateTime to = string.IsNullOrEmpty(toDate)
                    ? DateTime.Now
                    : DateTime.Parse(toDate);

                // 3. Get patient info
                var patient = await GetPatientByMaBenhAn(maBenhAn);
                if (patient == null)
                {
                    ViewBag.ErrorMessage = "Không tìm thấy thông tin bệnh nhân";
                    return View("Error");
                }

                // 4. Get all results by categories
                var allResults = await GetPatientAllResults(patient.Id, from, to);

                // 5. Get XN results grouped by category
                var xnResults = await GetPatientXNResults(patient.Id, from, to);
                var xnResultsGroupedByCategory = await GetPatientXNResultsGroupedByCategory(patient.Id, from, to);
                var xnResultsGroupedByDateAndCategory = await GetPatientXNResultsGroupedByDateAndCategory(patient.Id, from, to);

                // 6. Group CDHA results by date
                var groupedResults = allResults
                    .GroupBy(x => x.CreatedDate.Date)
                    .OrderByDescending(x => x.Key)
                    .ToList();

                ViewData["Patient"] = patient;
                ViewData["GroupedResults"] = groupedResults;
                ViewData["XNResults"] = xnResults;
                ViewData["XNResultsGroupedByCategory"] = xnResultsGroupedByCategory;
                ViewData["XNResultsGroupedByDateAndCategory"] = xnResultsGroupedByDateAndCategory;
                ViewData["Hospital"] = await _hospitalBL.GetHospital();
                ViewData["FromDate"] = from.ToString("dd/MM/yyyy");
                ViewData["ToDate"] = to.ToString("dd/MM/yyyy");
                ViewData["TotalResults"] = allResults.Count;
                ViewData["TotalXNResults"] = xnResults.Count;
                ViewData["Token"] = token;
                ViewData["MaBenhAn"] = maBenhAn;
                ViewData["PdfResultUrls"] = BuildPdfResultUrls(allResults, token, maBenhAn);

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ViewResults portal");
                ViewBag.ErrorMessage = "Có lỗi xảy ra khi tải dữ liệu";
                return View("Error");
            }
        }

        /// <summary>
        /// Xem chi tiết kết quả của 1 dịch vụ
        /// </summary>
        [HttpGet]

        public async Task<IActionResult> ViewResultDetail(string token, long resultId)
        {
            try
            {
                if (!ValidateTokenForResult(token, resultId))
                {
                    return Json(new { success = false, message = "Token không hợp lệ" });
                }

                var result = await _resultCDHABL.GetResultCDHA(resultId);
                if (result == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy kết quả" });
                }

                // Get service info
                var service = await _serviceBL.GetById(result.ServiceId);

                var resultDetail = new
                {
                    success = true,
                    data = new
                    {
                        id = result.Id,
                        serviceName = service?.Name,
                        categoryName = service?.Category?.Name,
                        description = result.Description,
                        resultText = result.Result,
                        suggest = result.Suggest,
                        createdDate = result.InsertTime?.ToString("dd/MM/yyyy HH:mm"),
                        returnDate = GetReturnDateByCategory(result),
                        doctorName = GetDoctorNameByCategory(result),
                        keyResult = result.KeyResultForHis,
                        images = result.ImageCDHAs?.Select(img => new {
                            id = img.Id,
                            path = img.ImagePath,
                            fullPath = Request.Scheme + "://" + Request.Host + img.ImagePath
                        }).ToList()
                    }
                };

                return Json(resultDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ViewResultDetail");
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        /// <summary>
        /// Xem file PDF kết quả đã lưu theo KeyResultForHis trong thư mục /wwwroot/pdf/{categoryCode}/{keyResultForHis}
        /// Hỗ trợ các dạng đường dẫn:
        /// - /pdf/sa/{keyResultForHis}.pdf
        /// - /pdf/sa/{keyResultForHis}
        /// - /pdf/sa/{keyResultForHis}/file.pdf
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ViewResultPdf(string token, string maBenhAn, string categoryCode, string keyResultForHis)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(categoryCode) || string.IsNullOrWhiteSpace(keyResultForHis))
                {
                    return BadRequest("Thiếu thông tin loại kết quả hoặc mã kết quả.");
                }

                if (!ValidateToken(token, maBenhAn))
                {
                    return Unauthorized("Token không hợp lệ hoặc đã hết hạn.");
                }

                var patient = await GetPatientByMaBenhAn(maBenhAn);
                if (patient == null)
                {
                    return NotFound("Không tìm thấy thông tin bệnh nhân.");
                }

                var normalizedCategoryCode = categoryCode.Trim().ToUpperInvariant();
                var patientResults = await _resultCDHABL.GetListResultCDHAByPatientId(patient.Id, normalizedCategoryCode);
                var matchedResult = patientResults?.FirstOrDefault(x =>
                    string.Equals(x.KeyResultForHis, keyResultForHis, StringComparison.OrdinalIgnoreCase));

                if (matchedResult == null)
                {
                    return NotFound("Không tìm thấy kết quả hoặc kết quả không thuộc bệnh nhân này.");
                }

                var pdfPath = FindPdfResultFile(normalizedCategoryCode, matchedResult.KeyResultForHis);
                if (string.IsNullOrWhiteSpace(pdfPath))
                {
                    return NotFound("Không tìm thấy file PDF kết quả.");
                }

                var fileName = $"{normalizedCategoryCode}_{matchedResult.KeyResultForHis}.pdf";
                Response.Headers["Content-Disposition"] = $"inline; filename=\"{fileName}\"";

                return PhysicalFile(pdfPath, "application/pdf", enableRangeProcessing: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ViewResultPdf. Category: {CategoryCode}, KeyResultForHis: {KeyResultForHis}", categoryCode, keyResultForHis);
                return StatusCode(500, "Có lỗi xảy ra khi tải file PDF.");
            }
        }

        /// <summary>
        /// API dành riêng cho HIS để tạo link portal
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreatePortalLink([FromBody] HISPortalRequest request)
        {
            try
            {
                // 1. Validate request từ HIS
                if (!ValidateHISRequest(request))
                {
                    return Unauthorized("Invalid HIS request");
                }

                // 2. Kiểm tra bệnh nhân có tồn tại không
                var patient = await GetPatientByMaBenhAn(request.MaBenhAn);
                if (patient == null)
                {
                    return NotFound($"Không tìm thấy bệnh nhân: {request.MaBenhAn}");
                }

                // 3. Tạo JWT token
                var secretKey = _configuration["HISIntegration:HISApiKey"] ?? "LisSecretKeyForHISLeanCare2025!@#$%";
                var expiryDays = _configuration.GetValue<int>("HISIntegration:TokenExpiryDays", 1);
                var token = GenerateJwtToken(request.MaBenhAn, secretKey, DateTime.Now.AddDays(expiryDays));

                // 4. Tạo portal URL
                var portalUrl = $"{Request.Scheme}://{Request.Host}/Portal/ViewResults?token={token}&maBenhAn={request.MaBenhAn}";

                if (!string.IsNullOrEmpty(request.FromDate))
                {
                    portalUrl += $"&fromDate={request.FromDate}";
                }

                if (!string.IsNullOrEmpty(request.ToDate))
                {
                    portalUrl += $"&toDate={request.ToDate}";
                }

                return Ok(new HISPortalResponse
                {
                    Success = true,
                    Token = token,
                    PortalUrl = portalUrl,
                    ExpiryDate = DateTime.Now.AddDays(expiryDays),
                    Message = "Tạo link portal thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating portal link for HIS");
                return BadRequest(new HISPortalResponse
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Validate request từ HIS system
        /// </summary>
        private bool ValidateHISRequest(HISPortalRequest request)
        {
            // 1. Kiểm tra required fields
            if (string.IsNullOrEmpty(request.MaBenhAn) || string.IsNullOrEmpty(request.HISApiKey))
            {
                return false;
            }

            // 2. Validate API key từ HIS
            var expectedApiKey = _configuration["HISIntegration:HISApiKey"];
            if (request.HISApiKey != expectedApiKey)
            {
                return false;
            }

            //// 3. Kiểm tra IP whitelisting (optional)
            //var allowedIPs = _configuration.GetSection("HISIntegration:AllowedIPs").Get<string[]>();
            //if (allowedIPs?.Length > 0)
            //{
            //    var clientIP = HttpContext.Connection.RemoteIpAddress?.ToString();
            //    if (!allowedIPs.Contains(clientIP))
            //    {
            //        return false;
            //    }
            //}

            return true;
        }

        public class HISPortalRequest
        {
            public string MaBenhAn { get; set; } = "";
            public string HISApiKey { get; set; } = "";
            public string FromDate { get; set; } = "";
            public string ToDate { get; set; } = "";
            public string UserId { get; set; } = ""; // User từ HIS
        }

        public class HISPortalResponse
        {
            public bool Success { get; set; }
            public string Token { get; set; } = "";
            public string PortalUrl { get; set; } = "";
            public DateTime ExpiryDate { get; set; }
            public string Message { get; set; } = "";
        }

        #region Private Methods

        private async Task<Patient> GetPatientByMaBenhAn(string maBenhAn)
        {
            try
            {
                // Tìm bệnh nhân theo MaBenhAn trong khoảng thời gian rộng
                return await _patientCDHABL.GetPatientByMaBenhAn(maBenhAn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting patient by MaBenhAn: {maBenhAn}");
                return null;
            }
        }

        private string GetReturnDateByCategory(ResultCDHA result)
        {
            try
            {
                var categoryCode = result.Service?.Category?.Code;
                return categoryCode switch
                {
                    "XQ" => result.Patient?.ReturnResultTimeXQ?.ToString("dd/MM/yyyy HH:mm"),
                    "SA" => result.Patient?.ReturnResultTimeSA?.ToString("dd/MM/yyyy HH:mm"),
                    "SAT" => result.Patient?.ReturnResultTimeSAT?.ToString("dd/MM/yyyy HH:mm"),
                    "DDT" => result.Patient?.ReturnResultTimeDDT?.ToString("dd/MM/yyyy HH:mm"),
                    "NS" => result.Patient?.ReturnResultTimeNS?.ToString("dd/MM/yyyy HH:mm"),
                    "TDCN" => result.Patient?.ReturnResultTimeTDCN?.ToString("dd/MM/yyyy HH:mm"),
                    _ => ""
                };
            }
            catch
            {
                return "";
            }
        }

        private string GetDoctorNameByCategory(ResultCDHA result)
        {
            try
            {
                var categoryCode = result.Service?.Category?.Code;
                return categoryCode switch
                {
                    "XQ" => result.Patient?.UserReturnXQ?.Name,
                    "SA" => result.Patient?.UserSA?.Name,
                    "SAT" => result.Patient?.UserSAT?.Name,
                    "DDT" => result.Patient?.UserDDT?.Name,
                    "NS" => result.Patient?.UserNS?.Name,
                    "TDCN" => result.Patient?.UserTDCN?.Name,
                    _ => ""
                } ?? "";
            }
            catch
            {
                return "";
            }
        }

        private async Task<List<PatientResultPortalModel>> GetPatientAllResults(long patientId, DateTime from, DateTime to)
        {
            var results = new List<PatientResultPortalModel>();

            try
            {
                // Get XQ results (X-ray)
                var xqResults = await _resultCDHABL.GetListResultCDHAByPatientId(patientId, "XQ");
                if (xqResults != null)
                {
                    foreach (var result in xqResults.Where(r => r.InsertTime >= from && r.InsertTime <= to))
                    {
                        var portalResult = new PatientResultPortalModel
                        {
                            Id = result.Id,
                            Type = "X-Quang",
                            TypeCode = "XQ",
                            ServiceName = result.Service?.Name ?? "X-Quang",
                            Description = result.Description,
                            Result = result.Result,
                            Suggest = result.Suggest,
                            CreatedDate = result.InsertTime ?? DateTime.MinValue,
                            ReturnResultDate = result.Patient?.ReturnResultTimeXQ,
                            DoctorName = result.Patient?.UserReturnXQ?.Name ?? "",
                            KeyResult = result.KeyResultForHis,
                            HasImages = result.ImageCDHAs?.Any() == true,
                            ImageCount = result.ImageCDHAs?.Count ?? 0
                        };

                        // Thêm thông tin ảnh
                        if (result.ImageCDHAs != null)
                        {
                            foreach (var img in result.ImageCDHAs)
                            {
                                var fullImageUrl = $"{Request.Scheme}://{Request.Host}{img.ImagePath}";
                                _logger.LogInformation($"Creating image URL: {fullImageUrl} from ImagePath: {img.ImagePath}");

                                portalResult.Images.Add(new PatientResultImageModel
                                {
                                    Id = img.Id,
                                    ImagePath = img.ImagePath,
                                    FullImageUrl = fullImageUrl,
                                    ThumbnailUrl = fullImageUrl // Có thể tạo thumbnail riêng nếu cần
                                });
                            }
                        }

                        results.Add(portalResult);
                    }
                }

                // Get SA results (Ultrasound)
                var saResults = await _resultCDHABL.GetListResultCDHAByPatientId(patientId, "SA");
                if (saResults != null)
                {
                    foreach (var result in saResults.Where(r => r.InsertTime >= from && r.InsertTime <= to))
                    {
                        var portalResult = new PatientResultPortalModel
                        {
                            Id = result.Id,
                            Type = "Siêu âm",
                            TypeCode = "SA",
                            ServiceName = result.Service?.Name ?? "Siêu âm",
                            Description = result.Description,
                            Result = result.Result,
                            Suggest = result.Suggest,
                            CreatedDate = result.InsertTime ?? DateTime.MinValue,
                            ReturnResultDate = result.Patient?.ReturnResultTimeSA,
                            DoctorName = result.Patient?.UserSA?.Name ?? "",
                            KeyResult = result.KeyResultForHis,
                            HasImages = result.ImageCDHAs?.Any() == true,
                            ImageCount = result.ImageCDHAs?.Count ?? 0
                        };

                        // Thêm thông tin ảnh
                        if (result.ImageCDHAs != null)
                        {
                            foreach (var img in result.ImageCDHAs)
                            {
                                portalResult.Images.Add(new PatientResultImageModel
                                {
                                    Id = img.Id,
                                    ImagePath = img.ImagePath,
                                    FullImageUrl = $"{Request.Scheme}://{Request.Host}{img.ImagePath}",
                                    ThumbnailUrl = $"{Request.Scheme}://{Request.Host}{img.ImagePath}"
                                });
                            }
                        }

                        results.Add(portalResult);
                    }
                }

                // Get SAT results (Cardiac Ultrasound)
                var satResults = await _resultCDHABL.GetListResultCDHAByPatientId(patientId, "SAT");
                if (satResults != null)
                {
                    foreach (var result in satResults.Where(r => r.InsertTime >= from && r.InsertTime <= to))
                    {
                        var portalResult = new PatientResultPortalModel
                        {
                            Id = result.Id,
                            Type = "Siêu âm tim",
                            TypeCode = "SAT",
                            ServiceName = result.Service?.Name ?? "Siêu âm tim",
                            Description = result.Description,
                            Result = result.Result,
                            Suggest = result.Suggest,
                            CreatedDate = result.InsertTime ?? DateTime.MinValue,
                            ReturnResultDate = result.Patient?.ReturnResultTimeSAT,
                            DoctorName = result.Patient?.UserSAT?.Name ?? "",
                            KeyResult = result.KeyResultForHis,
                            HasImages = result.ImageCDHAs?.Any() == true,
                            ImageCount = result.ImageCDHAs?.Count ?? 0
                        };

                        // Thêm thông tin ảnh
                        if (result.ImageCDHAs != null)
                        {
                            foreach (var img in result.ImageCDHAs)
                            {
                                portalResult.Images.Add(new PatientResultImageModel
                                {
                                    Id = img.Id,
                                    ImagePath = img.ImagePath,
                                    FullImageUrl = $"{Request.Scheme}://{Request.Host}{img.ImagePath}",
                                    ThumbnailUrl = $"{Request.Scheme}://{Request.Host}{img.ImagePath}"
                                });
                            }
                        }

                        results.Add(portalResult);
                    }
                }

                // Get DDT results (ECG)
                var ddtResults = await _resultCDHABL.GetListResultCDHAByPatientId(patientId, "DDT");
                if (ddtResults != null)
                {
                    foreach (var result in ddtResults.Where(r => r.InsertTime >= from && r.InsertTime <= to))
                    {
                        var portalResult = new PatientResultPortalModel
                        {
                            Id = result.Id,
                            Type = "Đo điện tim",
                            TypeCode = "DDT",
                            ServiceName = result.Service?.Name ?? "Đo điện tim",
                            Description = result.Description,
                            Result = result.Result,
                            Suggest = result.Suggest,
                            CreatedDate = result.InsertTime ?? DateTime.MinValue,
                            ReturnResultDate = result.Patient?.ReturnResultTimeDDT,
                            DoctorName = result.Patient?.UserDDT?.Name ?? "",
                            KeyResult = result.KeyResultForHis,
                            HasImages = result.ImageCDHAs?.Any() == true,
                            ImageCount = result.ImageCDHAs?.Count ?? 0
                        };

                        // Thêm thông tin ảnh
                        if (result.ImageCDHAs != null)
                        {
                            foreach (var img in result.ImageCDHAs)
                            {
                                portalResult.Images.Add(new PatientResultImageModel
                                {
                                    Id = img.Id,
                                    ImagePath = img.ImagePath,
                                    FullImageUrl = $"{Request.Scheme}://{Request.Host}{img.ImagePath}",
                                    ThumbnailUrl = $"{Request.Scheme}://{Request.Host}{img.ImagePath}"
                                });
                            }
                        }

                        results.Add(portalResult);
                    }
                }

                // Get NS results (Endoscopy)
                var nsResults = await _resultCDHABL.GetListResultCDHAByPatientId(patientId, "NS");
                if (nsResults != null)
                {
                    foreach (var result in nsResults.Where(r => r.InsertTime >= from && r.InsertTime <= to))
                    {
                        var portalResult = new PatientResultPortalModel
                        {
                            Id = result.Id,
                            Type = "Nội soi",
                            TypeCode = "NS",
                            ServiceName = result.Service?.Name ?? "Nội soi",
                            Description = result.Description,
                            Result = result.Result,
                            Suggest = result.Suggest,
                            CreatedDate = result.InsertTime ?? DateTime.MinValue,
                            ReturnResultDate = result.Patient?.ReturnResultTimeNS,
                            DoctorName = result.Patient?.UserNS?.Name ?? "",
                            KeyResult = result.KeyResultForHis,
                            HasImages = result.ImageCDHAs?.Any() == true,
                            ImageCount = result.ImageCDHAs?.Count ?? 0
                        };

                        // Thêm thông tin ảnh
                        if (result.ImageCDHAs != null)
                        {
                            foreach (var img in result.ImageCDHAs)
                            {
                                portalResult.Images.Add(new PatientResultImageModel
                                {
                                    Id = img.Id,
                                    ImagePath = img.ImagePath,
                                    FullImageUrl = $"{Request.Scheme}://{Request.Host}{img.ImagePath}",
                                    ThumbnailUrl = $"{Request.Scheme}://{Request.Host}{img.ImagePath}"
                                });
                            }
                        }

                        results.Add(portalResult);
                    }
                }

                var tdcnResults = await _resultCDHABL.GetListResultCDHAByPatientId(patientId, "TDCN");
                if (tdcnResults != null)
                {
                    foreach (var result in tdcnResults.Where(r => r.InsertTime >= from && r.InsertTime <= to))
                    {
                        var portalResult = new PatientResultPortalModel
                        {
                            Id = result.Id,
                            Type = "Thăm dò CN",
                            TypeCode = "TDCN",
                            ServiceName = result.Service?.Name ?? "Thăm dò CN",
                            Description = result.Description,
                            Result = result.Result,
                            Suggest = result.Suggest,
                            CreatedDate = result.InsertTime ?? DateTime.MinValue,
                            ReturnResultDate = result.Patient?.ReturnResultTimeTDCN,
                            DoctorName = result.Patient?.UserTDCN?.Name ?? "",
                            KeyResult = result.KeyResultForHis,
                            HasImages = result.ImageCDHAs?.Any() == true,
                            ImageCount = result.ImageCDHAs?.Count ?? 0
                        };

                        // Thêm thông tin ảnh
                        if (result.ImageCDHAs != null)
                        {
                            foreach (var img in result.ImageCDHAs)
                            {
                                portalResult.Images.Add(new PatientResultImageModel
                                {
                                    Id = img.Id,
                                    ImagePath = img.ImagePath,
                                    FullImageUrl = $"{Request.Scheme}://{Request.Host}{img.ImagePath}",
                                    ThumbnailUrl = $"{Request.Scheme}://{Request.Host}{img.ImagePath}"
                                });
                            }
                        }

                        results.Add(portalResult);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting patient results");
            }

            return results.OrderByDescending(x => x.CreatedDate).ToList();
        }

        private Dictionary<long, string> BuildPdfResultUrls(List<PatientResultPortalModel> results, string token, string maBenhAn)
        {
            var pdfUrls = new Dictionary<long, string>();

            if (results == null || results.Count == 0)
            {
                return pdfUrls;
            }

            foreach (var result in results)
            {
                if (result == null || string.IsNullOrWhiteSpace(result.TypeCode) || string.IsNullOrWhiteSpace(result.KeyResult))
                {
                    continue;
                }

                var pdfPath = FindPdfResultFile(result.TypeCode, result.KeyResult);
                if (string.IsNullOrWhiteSpace(pdfPath))
                {
                    continue;
                }

                var url = Url.Action(
                    action: nameof(ViewResultPdf),
                    controller: "Portal",
                    values: new
                    {
                        token,
                        maBenhAn,
                        categoryCode = result.TypeCode.ToLowerInvariant(),
                        keyResultForHis = result.KeyResult
                    });

                if (!string.IsNullOrWhiteSpace(url))
                {
                    pdfUrls[result.Id] = url;
                }
            }

            return pdfUrls;
        }

        private string FindPdfResultFile(string categoryCode, string keyResultForHis)
        {
            if (string.IsNullOrWhiteSpace(categoryCode) || string.IsNullOrWhiteSpace(keyResultForHis))
            {
                return null;
            }

            var safeCategoryCode = categoryCode.Trim();
            var safeKeyResult = keyResultForHis.Trim();

            if (!IsSafePathSegment(safeCategoryCode) || !IsSafePathSegment(safeKeyResult))
            {
                _logger.LogWarning("Invalid PDF path segment. Category: {CategoryCode}, KeyResultForHis: {KeyResultForHis}", categoryCode, keyResultForHis);
                return null;
            }

            var pdfRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdf");
            var categoryCandidates = new[]
            {
                safeCategoryCode,
                safeCategoryCode.ToLowerInvariant(),
                safeCategoryCode.ToUpperInvariant()
            }.Distinct().ToList();

            foreach (var category in categoryCandidates)
            {
                var categoryFolder = Path.Combine(pdfRoot, category);

                var fileCandidates = new[]
                {
                    Path.Combine(categoryFolder, safeKeyResult + ".pdf"),
                    Path.Combine(categoryFolder, safeKeyResult),
                    Path.Combine(categoryFolder, safeKeyResult, safeKeyResult + ".pdf")
                };

                foreach (var filePath in fileCandidates)
                {
                    if (System.IO.File.Exists(filePath))
                    {
                        return filePath;
                    }
                }

                var keyFolder = Path.Combine(categoryFolder, safeKeyResult);
                if (Directory.Exists(keyFolder))
                {
                    var firstPdf = Directory
                        .EnumerateFiles(keyFolder, "*.pdf", SearchOption.TopDirectoryOnly)
                        .OrderByDescending(System.IO.File.GetLastWriteTime)
                        .FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(firstPdf))
                    {
                        return firstPdf;
                    }
                }
            }

            return null;
        }

        private bool IsSafePathSegment(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            if (value.Contains("..") || value.Contains('/') || value.Contains('\\'))
            {
                return false;
            }

            return value.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
        }

        /// <summary>
        /// Validate JWT token for main portal access
        /// </summary>
        private bool ValidateToken(string token, string maBenhAn)
        {
            try
            {
                var secretKey = _configuration["HISIntegration:SecretKey"] ?? "LisSecretKeyForHISLeanCare2025!@#$%";

                // First try JWT validation
                if (ValidateJwtToken(token, maBenhAn, secretKey))
                {
                    return true;
                }

                // Fallback to old validation for backward compatibility
                return ValidateOldToken(token, maBenhAn, secretKey);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validate JWT token for result detail access
        /// </summary>
        private bool ValidateTokenForResult(string token, long resultId)
        {
            try
            {
                var secretKey = _configuration["HISIntegration:SecretKey"] ?? "LisSecretKeyForHISLeanCare2025!@#$%";
                return ValidateJwtTokenForResult(token, resultId, secretKey);
            }
            catch
            {
                return false;
            }
        }

        private string GenerateSimpleToken(string maBenhAn)
        {
            var data = $"{maBenhAn}|{DateTime.Now.AddDays(7):yyyy-MM-dd HH:mm:ss}";
            var bytes = Encoding.UTF8.GetBytes(data);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Tạo JWT token với HS256 algorithm theo chuẩn JWT (header.payload.signature)
        /// </summary>
        private string GenerateJwtToken(string maBenhAn, string secretKey, DateTime expiry)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("maBenhAn", maBenhAn),
                    new Claim("jti", Guid.NewGuid().ToString()),
                    new Claim("iat", new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                }),
                Expires = expiry,
                Issuer = "LIS-Portal",
                Audience = "HIS-System",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Validate JWT token với HS256 algorithm
        /// </summary>
        private bool ValidateJwtToken(string token, string maBenhAn, string secretKey)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(secretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = "LIS-Portal",
                    ValidateAudience = true,
                    ValidAudience = "HIS-System",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero // Không cho phép clock skew
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                // Kiểm tra thêm maBenhAn trong claims
                var tokenMaBenhAn = principal.FindFirst("maBenhAn")?.Value;
                return tokenMaBenhAn == maBenhAn;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JWT token validation failed");
                return false;
            }
        }

        /// <summary>
        /// Generate JWT token for result detail access
        /// </summary>
        private string GenerateJwtTokenForResult(string resultId, string secretKey)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("resultId", resultId),
                    new Claim("purpose", "result-detail"),
                    new Claim("jti", Guid.NewGuid().ToString()),
                    new Claim("iat", new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                }),
                Expires = DateTime.Now.AddHours(1), // Token for result detail expires in 1 hour
                Issuer = "LIS-Portal",
                Audience = "HIS-System",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Validate JWT token for result detail access
        /// </summary>
        private bool ValidateJwtTokenForResult(string token, long resultId, string secretKey)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(secretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = "LIS-Portal",
                    ValidateAudience = true,
                    ValidAudience = "HIS-System",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                // Kiểm tra purpose và resultId
                var purpose = principal.FindFirst("purpose")?.Value;
                var tokenResultId = principal.FindFirst("resultId")?.Value;

                return purpose == "result-detail" && tokenResultId == resultId.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JWT token validation for result failed");
                return false;
            }
        }

        /// <summary>
        /// Validate old token format for backward compatibility
        /// </summary>
        private bool ValidateOldToken(string token, string maBenhAn, string secretKey)
        {
            try
            {
                var parts = token.Split('.');
                if (parts.Length != 2) return false;

                var payload = Encoding.UTF8.GetString(Convert.FromBase64String(parts[0]));
                var signature = parts[1];

                var payloadParts = payload.Split('|');
                if (payloadParts.Length != 2) return false;

                var tokenMaBenhAn = payloadParts[0];
                var expiry = DateTime.Parse(payloadParts[1]);

                if (tokenMaBenhAn != maBenhAn || DateTime.Now > expiry) return false;

                var expectedSignature = GenerateSignature(payload, secretKey);
                return signature == expectedSignature;
            }
            catch
            {
                return false;
            }
        }

        private string GenerateSignature(string data, string secretKey)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hash);
            }
        }

        private async Task<List<PatientXNResultPortalModel>> GetPatientXNResults(long patientId, DateTime from, DateTime to)
        {
            var results = new List<PatientXNResultPortalModel>();

            try
            {
                // Lấy danh sách kết quả xét nghiệm từ ResultXNBL
                var xnResults = await _resultXNBL.GetListResultXNByPatientId(patientId);

                if (xnResults != null)
                {
                    foreach (var result in xnResults.Where(r => r.InsertTime >= from && r.InsertTime <= to && r.Active))
                    {
                        var portalResult = new PatientXNResultPortalModel
                        {
                            Id = result.Id,
                            TestCodeId = result.TestCodeId ?? 0,
                            TestCodeName = result.TestCode?.Name ?? "",
                            TestCodeCode = result.TestCode?.Code ?? "",
                            ServiceName = result.Service?.Name ?? "Xét nghiệm",
                            CategoryName = result.Service?.Category?.Name ?? "Xét nghiệm",
                            CategoryCode = result.Service?.Category?.Code ?? "",
                            Result = result.Result ?? "",
                            PosNeg = result.PosNeg ?? "",
                            Unit = result.TestCode?.Unit ?? "",
                            NormalRange = GetNormalRange(result.TestCode),
                            Status = result.Status ?? 0,
                            StatusText = GetStatusText(result.Status ?? 0),
                            CreatedDate = result.InsertTime ?? DateTime.MinValue,
                            ValidTime = result.ValidTime,
                            DoctorName = result.Patient?.UserXN?.Name ?? "",
                            ReturnResultDate = result.Patient?.ReturnResultTimeXN,
                            IsValidPrint = result.ValidPrint,
                            KeyResult = result.KeyResultForHis ?? ""
                        };

                        results.Add(portalResult);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting patient XN results");
            }

            return results.OrderByDescending(x => x.CreatedDate).ToList();
        }

        /// <summary>
        /// Lấy kết quả xét nghiệm được group theo danh mục
        /// </summary>
        private async Task<Dictionary<string, List<PatientXNResultPortalModel>>> GetPatientXNResultsGroupedByCategory(long patientId, DateTime from, DateTime to)
        {
            var results = await GetPatientXNResults(patientId, from, to);

            // Group theo CategoryName
            var groupedResults = results
                .GroupBy(x => new {
                    CategoryCode = x.CategoryCode ?? "OTHER",
                    CategoryName = x.CategoryName ?? "Khác"
                })
                .ToDictionary(
                    g => g.Key.CategoryName,
                    g => g.OrderBy(x => x.TestCodeName).ToList()
                );

            return groupedResults;
        }

        /// <summary>
        /// Lấy kết quả xét nghiệm được group theo ngày và danh mục
        /// </summary>
        private async Task<Dictionary<DateTime, Dictionary<string, List<PatientXNResultPortalModel>>>> GetPatientXNResultsGroupedByDateAndCategory(long patientId, DateTime from, DateTime to)
        {
            var results = await GetPatientXNResults(patientId, from, to);

            // Group theo ngày, sau đó group theo danh mục
            var groupedResults = results
                .GroupBy(x => x.CreatedDate.Date)
                .OrderByDescending(x => x.Key)
                .ToDictionary(
                    dateGroup => dateGroup.Key,
                    dateGroup => dateGroup
                        .GroupBy(x => new {
                            CategoryCode = x.CategoryCode ?? "OTHER",
                            CategoryName = x.CategoryName ?? "Khác"
                        })
                        .ToDictionary(
                            catGroup => catGroup.Key.CategoryName,
                            catGroup => catGroup.OrderBy(x => x.TestCodeName).ToList()
                        )
                );

            return groupedResults;
        }

        /// <summary>
        /// Get normal range text for TestCode
        /// </summary>
        private string GetNormalRange(TestCode testCode)
        {
            if (testCode == null) return "";

            //if (!string.IsNullOrEmpty(testCode.NormalRangeM) && !string.IsNullOrEmpty(testCode.NormalRangeF))
            //{
            //    return $"Nam: {testCode.NormalRangeM}, Nữ: {testCode.NormalRangeF}";
            //}
            //else if (!string.IsNullOrEmpty(testCode.NormalRangeM))
            //{
            //    return testCode.NormalRangeM;
            //}
            //else if (!string.IsNullOrEmpty(testCode.NormalRangeF))
            //{
            //    return testCode.NormalRangeF;
            //}
            //else if (!string.IsNullOrEmpty(testCode.NormalResult))
            //{
            //    return testCode.NormalResult;
            //}

            return testCode.NormalResult;
        }

        /// <summary>
        /// Get status text from status number
        /// </summary>
        private string GetStatusText(int status)
        {
            return status switch
            {
                0 => "Bình thường",
                1 => "Thấp",
                2 => "Cao",
                _ => ""
            };
        }
        #endregion
    }
}