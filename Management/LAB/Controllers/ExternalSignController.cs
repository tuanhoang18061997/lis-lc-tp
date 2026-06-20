using Management.Interfaces;
using Management.Models;
using Management.Models.SignServer;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Management.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExternalSignController : ControllerBase
    {
        private readonly ISignServerService _signService;
        private readonly IConfiguration _cfg;
        public ExternalSignController(ISignServerService signService) => _signService = signService;

        /// Client từ mọi form post vào đây
        [HttpPost("sign-pdf")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> SignPdf(
            [FromForm] string signerCCCD,
            [FromForm] IFormFile file,
            [FromForm] SignFormType formType,
            [FromForm] string patientCode,
            [FromForm] string patientMaBenhAn,
            [FromForm] string? patientName,
            [FromForm] string? serviceCode,
            [FromForm] string? doctorName,
            [FromForm] DateTime? performedAt,
            [FromForm] string? extra,
            [FromForm] string? targetText // optional override
        )
        {
            if (string.IsNullOrWhiteSpace(signerCCCD)) return BadRequest("signerCCCD is required.");
            if (file == null) return BadRequest("file is required.");
            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only PDF files are accepted.");

            var ctx = new SignContext
            {
                FormType = formType,
                PatientCode = patientCode,
                PatientMaBenhAn = patientMaBenhAn,
                PatientName = patientName,
                ServiceCode = serviceCode,
                DoctorName = doctorName,
                PerformedAt = performedAt,
                Extra = extra
            };

            var raw = await _signService.SignPdfAsync(signerCCCD, file, ctx, targetText, HttpContext.RequestAborted);

            // chỉ trả response.data nếu có
            try
            {
                using var doc = JsonDocument.Parse(raw);
                if (doc.RootElement.TryGetProperty("data", out var data))
                    return Content(data.GetRawText(), "application/json");
                return Content(doc.RootElement.GetRawText(), "application/json");
            }
            catch
            {
                return Ok(raw); // không phải JSON
            }
        }

        /// <summary>
        /// Ký NHIỀU file PDF trong 1 request
        /// FE: append("file", file1); append("resultIds", id1); append("file", file2); append("resultIds", id2);
        /// </summary>
        [HttpPost("sign-pdf-multi")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> SignPdfMulti(
            [FromForm] string signerCCCD,
            [FromForm] List<IFormFile> files,
            [FromForm] List<string> resultIds,     // <— để biết file nào ứng với resultCDHAId nào
            [FromForm] SignFormType formType,
            [FromForm] string patientCode,
            [FromForm] string patientMaBenhAn,
            [FromForm] string? patientName,
            [FromForm] string? serviceCode,
            [FromForm] string? doctorName,
            [FromForm] DateTime? performedAt,
            [FromForm] string? extra,
            [FromForm] string? targetText // optional override
        )
        {
            if (string.IsNullOrWhiteSpace(signerCCCD))
                return BadRequest("signerCCCD is required.");
            if (files == null || files.Count == 0)
                return BadRequest("No file uploaded.");

            var ctx = new SignContext
            {
                FormType = formType,
                PatientCode = patientCode,
                PatientMaBenhAn = patientMaBenhAn,
                PatientName = patientName,
                ServiceCode = serviceCode,
                DoctorName = doctorName,
                PerformedAt = performedAt,
                Extra = extra
            };

            var items = new List<object>();

            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];

                if (file == null)
                {
                    items.Add(new
                    {
                        index = i,
                        resultCDHAId = (resultIds != null && i < resultIds.Count) ? resultIds[i] : null,
                        success = false,
                        error = "File is null"
                    });
                    continue;
                }

                if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    items.Add(new
                    {
                        index = i,
                        resultCDHAId = (resultIds != null && i < resultIds.Count) ? resultIds[i] : null,
                        success = false,
                        error = "Only PDF files are accepted."
                    });
                    continue;
                }

                string? relatedResultId = null;
                if (resultIds != null && i < resultIds.Count)
                    relatedResultId = resultIds[i];
                ctx.ResultId = resultIds[i].ToString();

                try
                {
                    var raw = await _signService.SignPdfAsync(
                        signerCCCD,
                        file,
                        ctx,
                        targetText,
                        HttpContext.RequestAborted
                    );
                    //var raw = await _signService.SignPdfAsync(signerCCCD, file, ctx, targetText, HttpContext.RequestAborted);
                    // chuẩn hoá giống hàm single: cố gắng lấy phần "data"
                    object? payload;
                    try
                    {
                        using var doc = JsonDocument.Parse(raw);
                        if (doc.RootElement.TryGetProperty("type", out var typeEl))
                        {
                            // Kiểm tra lỗi từ API bên ngoài
                            string errorType = typeEl.GetString();
                            if (errorType == "https://www.jhipster.tech/problem/problem-with-message")
                            {
                                // Lấy thông tin lỗi từ response
                                var errorMessage = doc.RootElement.GetProperty("detail").GetString();
                                items.Add(new
                                {
                                    index = i,
                                    resultCDHAId = relatedResultId,
                                    success = false,
                                    error = errorMessage ?? "Unknown error"
                                });
                                return StatusCode(500, new
                                {
                                    success = false,
                                    message = errorMessage ?? "Unknown error"
                                });
                            }
                        }
                        if (doc.RootElement.TryGetProperty("data", out var dataEl))
                        {
                            payload = JsonSerializer.Deserialize<object>(dataEl.GetRawText());
                        }
                        else
                        {
                            payload = JsonSerializer.Deserialize<object>(doc.RootElement.GetRawText());
                        }
                    }
                    catch
                    {
                        payload = raw;
                    }

                    items.Add(new
                    {
                        index = i,
                        resultCDHAId = relatedResultId,
                        success = true,
                        // cái dưới để FE lấy signStoreId
                        data = payload,
                        // để FE lưu nguyên response về bảng Digital_Sign
                        raw = raw
                    });
                }
                catch (OperationCanceledException)
                {
                    // Khi thao tác bị hủy do timeout
                    items.Add(new
                    {
                        index = i,
                        resultCDHAId = relatedResultId,
                        success = false,
                        error = "Operation timed out after 2 minutes."
                    });
                    // Bạn có thể trả về BadRequest hoặc lỗi khác tùy theo yêu cầu của bạn
                    return StatusCode(408, new
                    {
                        success = false,
                        message = "Hết thời gian chờ ký số (2 phút)."
                    });
                }
                catch (Exception ex)
                {
                    items.Add(new
                    {
                        index = i,
                        resultCDHAId = relatedResultId,
                        success = false,
                        error = ex.Message
                    });

                    // Bạn có thể trả về BadRequest hoặc lỗi khác tùy theo yêu cầu của bạn
                    return StatusCode(408, new
                    {
                        success = false,
                        message = "Hết thời gian chờ ký số (2 phút)."
                    });
                }
            }

            return Ok(new
            {
                success = true,
                items
            });
        }

        [HttpGet("view-signed/{id}")]
        public async Task<IActionResult> ViewPdfSignedServer(string id)
        {
            var directUrl = _signService.GetSignedPdfViewUrl(id);
            return Redirect(directUrl);

            //// Nếu CẦN token → proxy bytes về cho client
            //var bytes = await _signService.FetchSignedPdfAsync(id, HttpContext.RequestAborted);
            //return File(bytes, "application/pdf");
        }

    }
}
