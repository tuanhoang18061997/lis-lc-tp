using Management.BL;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GetResultPdfFileV2Controller : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ResultXNBL _resultXNBL;

        public GetResultPdfFileV2Controller(IWebHostEnvironment environment, ResultXNBL resultXNBL)
        {
            _environment = environment;
            _resultXNBL = resultXNBL;
        }

        [HttpGet]
        public async Task<IActionResult> GetResultPdfFileV2(string sid)
        {
            if (string.IsNullOrWhiteSpace(sid)) return BadRequest("sid is required");

            var categoryCode = sid.Split('-')[0];

            if (categoryCode.Equals("xn", StringComparison.OrdinalIgnoreCase))
            {
                return await GetBothPdfAndExternalLabFilesV2(sid, categoryCode);
            }

            var folder = Path.Combine(_environment.WebRootPath, "pdf", categoryCode);
            var fileName = sid + ".pdf";
            var filePath = Path.Combine(folder, fileName);

            if (System.IO.File.Exists(filePath))
            {
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "application/pdf", fileName);
            }
            return NotFound($"Dịch vụ chưa hoàn thành theo mã : {sid}.pdf");
        }

        private async Task<IActionResult> GetBothPdfAndExternalLabFilesV2(string sid, string categoryCode, bool merge = true)
        {
            try
            {
                var fileBySid = new List<string>();
                var fileExternalLab = new List<string>();

                var folder = Path.Combine(_environment.WebRootPath, "pdf", categoryCode);
                var basePdfPath = Path.Combine(folder, sid + ".pdf");
                if (System.IO.File.Exists(basePdfPath))
                {
                    fileBySid.Add(basePdfPath);
                }

                string? patientId = null;
                string? maBenhAn = null;
                string patientName = "Unknown";
                await TryResolvePatientAsync(sid).ContinueWith(t =>
                {
                    if (t.Status == TaskStatus.RanToCompletion)
                    {
                        (patientId, maBenhAn, patientName) = t.Result;
                    }
                });

                var externalLabFolder = Path.Combine(_environment.WebRootPath, "uploads", "external_lab");
                if (Directory.Exists(externalLabFolder))
                {
                    if (!string.IsNullOrWhiteSpace(maBenhAn))
                    {
                        var pattern = $"patient_{maBenhAn}_*.pdf";
                        var files = Directory.EnumerateFiles(externalLabFolder, pattern, SearchOption.TopDirectoryOnly)
                                             .OrderByDescending(System.IO.File.GetCreationTimeUtc)
                                             .Take(100)
                                             .ToList();
                        fileExternalLab.AddRange(files);
                    }

                    if (!string.IsNullOrWhiteSpace(patientId))
                    {
                        var pattern = $"patient_{patientId}_*.pdf";
                        var files = Directory.EnumerateFiles(externalLabFolder, pattern, SearchOption.TopDirectoryOnly)
                                             .OrderByDescending(System.IO.File.GetCreationTimeUtc)
                                             .Take(100)
                                             .ToList();
                        fileExternalLab.AddRange(files);
                    }

                    fileExternalLab = fileExternalLab.Distinct().ToList();
                }

                var allFiles = fileBySid.Concat(fileExternalLab).ToList();

                if (!allFiles.Any())
                {
                    return NotFound($"Không tìm thấy kết quả xét nghiệm theo mã kết quả SID: {sid}");
                }

                // ✅ Sửa đoạn này:
                if (merge && allFiles.Count > 1)
                {
                    try
                    {
                        var merged = MergePdfs(allFiles);
                        var outName = $"{sid}_merged.pdf";
                        return File(merged, "application/pdf", outName);
                    }
                    catch (Exception ex)
                    {
                        // Có thể log nếu cần
                    }
                }

                if (allFiles.Count == 1)
                {
                    var f = allFiles[0];
                    var fileBytes = System.IO.File.ReadAllBytes(f);
                    var onlyName = Path.GetFileName(f);
                    return File(fileBytes, "application/pdf", onlyName);
                }

                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var fileBySidUrls = fileBySid.Where(System.IO.File.Exists).Select(absPath => ToPublicUrl(absPath, baseUrl)).Where(url => url != null).Cast<string>().ToList();
                var fileExternalLabUrls = fileExternalLab.Where(System.IO.File.Exists).Select(absPath => ToPublicUrl(absPath, baseUrl)).Where(url => url != null).Cast<string>().ToList();

                return new JsonResult(new
                {
                    success = true,
                    sid,
                    patientName,
                    fileBySid = fileBySidUrls,
                    fileExternalLab = fileExternalLabUrls,
                    fileCount = allFiles.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private string? ToPublicUrl(string absolutePath, string baseUrl)
        {
            try
            {
                var rel = absolutePath.Replace(_environment.WebRootPath, string.Empty).Replace("\\", "/");
                if (!rel.StartsWith('/')) rel = "/" + rel;
                return baseUrl + rel;
            }
            catch
            {
                return null;
            }
        }

        private async Task<(string? patientId, string? maBenhAn, string? patientName)> TryResolvePatientAsync(string sid)
        {
            try
            {
                var record = await _resultXNBL.GetResultXNByKeyResultForHis(sid);
                if (record == null) return (null, null, "Unknown");

                object? patientObj = GetProp(record, "Patient");
                if (patientObj == null) return (null, null, "Unknown");

                string? patientId = GetProp(patientObj, "PatientId") as string;
                string? maBenhAn = GetProp(patientObj, "MaBenhAn") as string;
                string? patientName = GetProp(patientObj, "PatientName") as string ?? "Unknown";

                return (patientId, maBenhAn, patientName);
            }
            catch
            {
                return (null, null, "Unknown");
            }
        }

        private object? GetProp(object obj, string name)
        {
            if (obj == null) return null;
            var t = obj.GetType();
            var prop = t.GetProperty(name);
            return prop?.GetValue(obj);
        }

        private static byte[] MergePdfs(IEnumerable<string> absolutePaths)
        {
            var output = new PdfDocument();

            foreach (var path in absolutePaths.Distinct())
            {
                if (!System.IO.File.Exists(path)) continue;

                using (var input = PdfReader.Open(path, PdfDocumentOpenMode.Import))
                {
                    for (int i = 0; i < input.PageCount; i++)
                    {
                        output.AddPage(input.Pages[i]);
                    }
                }
            }

            using var ms = new MemoryStream();
            output.Save(ms, false);
            return ms.ToArray();
        }

    }
}
