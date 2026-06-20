using Management.Interfaces;
using Management.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.RegularExpressions;
using System.Net.Http.Headers;
using Management.Models.SignServer;

namespace Management.BL.Services
{
    public class SignServerService : ISignServerService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _cfg;
        private readonly ILogger<SignServerService> _log;

        public SignServerService(IHttpClientFactory f, IConfiguration cfg, ILogger<SignServerService> log)
        {
            _httpClientFactory = f; _cfg = cfg; _log = log;
        }

        // Lấy URL từ server ký 
        public string GetSignedPdfViewUrl(string signStoreId)
        {
            if (string.IsNullOrWhiteSpace(signStoreId))
                throw new ArgumentException("signStoreId is required.", nameof(signStoreId));

            var baseUrl = _cfg["SignServerMySign:BaseUrl"] ?? throw new InvalidOperationException("SignServerMySign:BaseUrl missing");
            return $"{baseUrl.TrimEnd('/')}/signserver/api/ext/pdfs/{signStoreId}";
        }

        // Call Server ký để ký số
        public async Task<string> SignPdfAsync(
            string signerCccd,
            IFormFile pdfFile,
            SignContext ctx,
            string? targetTextOverride = null,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(signerCccd)) throw new ArgumentException(nameof(signerCccd));
            if (pdfFile == null || pdfFile.Length == 0) throw new ArgumentException(nameof(pdfFile));
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));

            var baseUrl = _cfg["SignServerMySign:BaseUrl"] ?? throw new InvalidOperationException("SignServerMySign:BaseUrl missing");
            var endpoint = _cfg["SignServerMySign:SignEndpoint"] ?? throw new InvalidOperationException("SignServerMySign:SignEndpoint missing");
            var taxcode = _cfg["SignServerMySign:TaxCode"] ?? "";
            var defTarget = _cfg["SignServerMySign:DefaultTargetText"] ?? "";
            var dateFmt = _cfg["SignServerMySign:DateFormatForFile"] ?? "yyyyMMdd";

            // 1) tạo tên file theo template
            string fileName = BuildFileName(ctx, dateFmt);

            // 2) targetText: ưu tiên override -> DoctorName -> default
            string targetText = !string.IsNullOrWhiteSpace(targetTextOverride)
                ? targetTextOverride.Trim()
                : (!string.IsNullOrWhiteSpace(ctx.DoctorName) ? ctx.DoctorName!.Trim() : defTarget.Trim());

            var client = _httpClientFactory.CreateClient("mysign");
            client.BaseAddress = new Uri(baseUrl);

            // Set 2-minute timeout for signing process
            client.Timeout = TimeSpan.FromSeconds(120);

            using var form = new MultipartFormDataContent();

            //var streamContent = new StreamContent(pdfFile.OpenReadStream());
            //streamContent.Headers.ContentType = new MediaTypeHeaderValue(pdfFile.ContentType ?? "application/pdf");
            //streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            //{
            //    Name = "\"file\"",
            //    FileName = $"\"{fileName}\"",
            //    FileNameStar = fileName
            //};
            await using var pdfStream = pdfFile.OpenReadStream();   // pdfFile: IFormFile
            var fileContent = new StreamContent(pdfStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(pdfFile.ContentType ?? "application/pdf");
            form.Add(fileContent, "file", fileName);

            form.Add(new StringContent(signerCccd.Trim()), "signUserId");
            form.Add(new StringContent(taxcode.Trim()), "taxcode");
            form.Add(new StringContent(targetText.Trim()), "targetText");
            form.Add(new StringContent("true"), "loop");

            try
            {
                using var resp = await client.PostAsync(endpoint, form, ct);
                var raw = await resp.Content.ReadAsStringAsync(ct);
                if (!resp.IsSuccessStatusCode)
                    _log.LogWarning("MySign non-success {Code}: {Body}", (int)resp.StatusCode, raw);
                return raw;
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _log.LogError("MySign signing timeout after 2 minutes");
                throw new TimeoutException("Hết thời gian chờ ký số (2 phút)", ex);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Call MySign failed");
                throw;
            }
        }

        // Helper BuildFileName: dùng để build tên file gửi qua server ký
        private string BuildFileName(SignContext ctx, string dateFmt)
        {
            // lấy template theo form type
            string key = ctx.FormType.ToString(); // trùng với cấu hình
            string? template = _cfg[$"SignServerMySign:FileNameTemplates:{key}"]
                                ?? _cfg["SignServerMySign:FileNameTemplates:Khac"]
                                ?? "{PatientCode}_{Date}.pdf";

            string date = DateTime.Now.ToString(dateFmt);

            string Replace(string? s) => string.IsNullOrWhiteSpace(s) ? "" : s.Trim();
            string result = template
                .Replace("{ResultId}", Replace(ctx.ResultId))
                .Replace("{PatientCode}", Replace(ctx.PatientCode))
                .Replace("{PatientMaBenhAn}", Replace(ctx.PatientMaBenhAn))
                .Replace("{Date}", date);
            //.Replace("{PatientName}", Replace(ctx.PatientName))
            //.Replace("{ServiceCode}", Replace(ctx.ServiceCode))
            //.Replace("{Doctor}", Replace(ctx.DoctorName))
            //.Replace("{Extra}", Replace(ctx.Extra))

            // chuẩn hoá tên file: bỏ ký tự cấm & rút gọn khoảng trắng
            result = Regex.Replace(result, @"[^\w\-. _]", "_"); // chỉ giữ chữ/số/_/.-/space
            result = Regex.Replace(result, @"\s+", " ").Trim();

            // đảm bảo .pdf
            if (!result.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                result += ".pdf";

            // tránh tên rỗng
            if (string.IsNullOrWhiteSpace(Path.GetFileNameWithoutExtension(result)))
                result = $"file_{date}.pdf";

            return result;
        }
    }
}
