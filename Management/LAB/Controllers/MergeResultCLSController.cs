using Management.BL;
using Management.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MergeResultCLSController : ControllerBase
    {
        public readonly ILogger<MergeResultCLSController> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly APIBL _apiBL;
        private readonly ServiceBL _serviceBL;
        private readonly ResultXNBL _resultXNBL;
        private readonly ResultCDHABL _resultCDHABL;
        private readonly PatientXNBL _patientXNBL;
        private readonly PatientCDHABL _patientCDHABL;
        private readonly ExternalFileBL _externalFileBL;

        public MergeResultCLSController(ILogger<MergeResultCLSController> logger, IWebHostEnvironment environment, APIBL apiBL, ServiceBL serviceBL, ResultXNBL resultXNBL, ResultCDHABL resultCDHABL, PatientXNBL patientXNBL, PatientCDHABL patientCDHABL, ExternalFileBL externalFileBL)
        {
            _logger = logger;
            _environment = environment;
            _apiBL = apiBL;
            _serviceBL = serviceBL;
            _resultXNBL = resultXNBL;
            _resultCDHABL = resultCDHABL;
            _patientXNBL = patientXNBL;
            _patientCDHABL = patientCDHABL;
            _externalFileBL = externalFileBL;
        }

        [HttpGet]
        public async Task<IActionResult> GetMergedPdfByMaBenhAnAndPatientId(string maBenhAn, string patientId)
        {
            if (string.IsNullOrWhiteSpace(maBenhAn) || string.IsNullOrWhiteSpace(patientId))
            {
                return BadRequest("Thiếu mã bệnh án hoặc patientId.");
            }

            try
            {
                using (var db = new LABContext())
                {
                    // 1) Tìm patient theo đúng điều kiện
                    var patient = await db.Patients
                        .Where(x => x.PatientId == patientId
                                 && x.MaBenhAn == maBenhAn
                                 && x.BenhAn == "pakage")
                        .OrderByDescending(x => x.Id)
                        .FirstOrDefaultAsync();

                    if (patient == null)
                    {
                        return NotFound($"Không tìm thấy bệnh nhân với maBenhAn={maBenhAn}, patientId={patientId}");
                    }

                    var tablePatientId = patient.Id;

                    // 2) Lấy SID XN
                    var xnSids = await db.ResultXNs
                        .Where(x => x.PatientId == tablePatientId
                                 && x.Active == true
                                 && x.KeyResultForHis != null
                                 && x.KeyResultForHis != "")
                        .Select(x => x.KeyResultForHis)
                        .Distinct()
                        .ToListAsync();

                    // 3) Lấy SID CDHA
                    var allowedCategories = new[] { "sa", "xq", "ns", "nsctc", "sat", "ddt", "tdcn" };

                    var cdhaSids = await db.ResultCDHAs
                        .Where(x => x.PatientId == tablePatientId
                                 && x.Active == true
                                 && x.KeyResultForHis != null
                                 && x.KeyResultForHis != "")
                        .Select(x => x.KeyResultForHis)
                        .Distinct()
                        .ToListAsync();

                    cdhaSids = cdhaSids
                        .Where(sid =>
                        {
                            var category = GetCategoryCodeFromSid(sid);
                            return allowedCategories.Contains(category, StringComparer.OrdinalIgnoreCase);
                        })
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    // 4) Gom tất cả file
                    var allFiles = new List<string>();

                    // 4.1) File XN
                    foreach (var sid in xnSids)
                    {
                        var xnFiles = await CollectLabFilesForMergeBySidAsync(
                            sid: sid,
                            patientId: patientId,
                            maBenhAn: maBenhAn
                        );

                        allFiles.AddRange(xnFiles);
                    }

                    // 4.2) File CDHA
                    foreach (var sid in cdhaSids)
                    {
                        var categoryCode = GetCategoryCodeFromSid(sid);
                        if (string.IsNullOrWhiteSpace(categoryCode)) continue;

                        var cdhaFiles = await CollectCdhaFilesForMergeBySidAsync(
                            sid: sid,
                            categoryCode: categoryCode,
                            patientId: patientId,
                            maBenhAn: maBenhAn
                        );

                        allFiles.AddRange(cdhaFiles);
                    }

                    // 5) Loại trùng + check tồn tại
                    allFiles = allFiles
                        .Where(System.IO.File.Exists)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    if (!allFiles.Any())
                    {
                        return NotFound($"Không tìm thấy file PDF nào với maBenhAn={maBenhAn}, patientId={patientId}");
                    }

                    // 6) Nếu chỉ có 1 file
                    if (allFiles.Count == 1)
                    {
                        var f = allFiles[0];
                        var onlyName = Path.GetFileName(f);
                        Response.Headers["Content-Disposition"] = $"inline; filename=\"{onlyName}\"";
                        return PhysicalFile(f, "application/pdf", enableRangeProcessing: true);
                    }

                    // 7) Merge tất cả thành 1 PDF
                    var merged = MergePdfs(allFiles);
                    var outName = $"{maBenhAn}_{patientId}_merged.pdf";
                    Response.Headers["Content-Disposition"] = $"inline; filename=\"{outName}\"";
                    return File(merged, "application/pdf");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetMergedPdfByMaBenhAnAndPatientId failed. maBenhAn={MaBenhAn}, patientId={PatientId}", maBenhAn, patientId);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        //-------------- Helper----------------
        private string GetCategoryCodeFromSid(string sid)
        {
            if (string.IsNullOrWhiteSpace(sid)) return string.Empty;

            var parts = sid.Split('-');
            if (parts.Length == 0) return string.Empty;

            return parts[0]?.Trim().ToLower() ?? string.Empty;
        }

        private async Task<List<string>> CollectLabFilesForMergeBySidAsync(string sid, string patientId, string maBenhAn)
        {
            var fileBySid = new List<string>();
            var fileExternalLabPhysical = new List<string>();
            var fileExternalLab = new List<string>();

            // 1) File chính theo SID
            var folder = Path.Combine(_environment.WebRootPath, "pdf", "xn");
            var basePdfPath = Path.Combine(folder, sid + ".pdf");
            if (System.IO.File.Exists(basePdfPath))
            {
                fileBySid.Add(basePdfPath);
            }

            var sanitizedPId = SanitizeFilePart(patientId) ?? "unknown";
            var sanitizedMaBenhAn = SanitizeFilePart(maBenhAn) ?? "unknown";

            // 2) Scan folder mới
            var newFolder = Path.Combine(_environment.WebRootPath, "uploads", "external_lab", sanitizedPId, sanitizedMaBenhAn);
            if (Directory.Exists(newFolder))
            {
                var filesInNewFolder = Directory.EnumerateFiles(newFolder, "*.pdf", SearchOption.TopDirectoryOnly)
                    .OrderByDescending(System.IO.File.GetCreationTimeUtc)
                    .Take(100)
                    .ToList();

                fileExternalLabPhysical.AddRange(filesInNewFolder);
            }

            // 3) Fallback folder cũ
            var legacyFolder = Path.Combine(_environment.WebRootPath, "uploads", "external_lab");
            if (Directory.Exists(legacyFolder))
            {
                var patternByMaBenhAn = $"patient_{maBenhAn}_*.pdf";
                var byMaBenhAn = Directory.EnumerateFiles(legacyFolder, patternByMaBenhAn, SearchOption.TopDirectoryOnly)
                    .OrderByDescending(System.IO.File.GetCreationTimeUtc)
                    .Take(100)
                    .ToList();

                fileExternalLabPhysical.AddRange(byMaBenhAn);

                var patternByPatientId = $"patient_{patientId}_*.pdf";
                var byPatientId = Directory.EnumerateFiles(legacyFolder, patternByPatientId, SearchOption.TopDirectoryOnly)
                    .OrderByDescending(System.IO.File.GetCreationTimeUtc)
                    .Take(100)
                    .ToList();

                fileExternalLabPhysical.AddRange(byPatientId);
            }

            fileExternalLabPhysical = fileExternalLabPhysical
                .Where(System.IO.File.Exists)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // 4) Sync metadata nếu thiếu
            if (fileExternalLabPhysical.Any())
            {
                try
                {
                    await _externalFileBL.SyncPhysicalFilesIfMissingAsync(
                        physicalFilePaths: fileExternalLabPhysical,
                        webRootPath: _environment.WebRootPath,
                        pId: patientId,
                        maBenhAn: maBenhAn,
                        tablePatientId: 0,
                        moduleType: ExternalFileModule.Lab,
                        patientName: null,
                        createdBy: null,
                        isVisibleToUser: true
                    );
                }
                catch (Exception exSync)
                {
                    _logger.LogWarning(exSync, "CollectLabFilesForMergeBySidAsync sync failed. sid={Sid}", sid);
                }
            }

            // 5) Query lại từ DB chỉ lấy file visible
            var dbFiles = await _externalFileBL.GetVisibleFilesAsync(
                tablePatientId: null,
                pId: patientId,
                maBenhAn: maBenhAn,
                moduleType: ExternalFileModule.Lab
            );

            foreach (var dbItem in dbFiles)
            {
                var normalizedRelativePath = (dbItem.RelativePath ?? "").Replace("\\", "/").TrimStart('/');
                if (string.IsNullOrWhiteSpace(normalizedRelativePath)) continue;

                var absPath = Path.Combine(
                    _environment.WebRootPath,
                    normalizedRelativePath.Replace("/", Path.DirectorySeparatorChar.ToString())
                );

                if (System.IO.File.Exists(absPath))
                {
                    fileExternalLab.Add(absPath);
                }
            }

            return fileBySid
                .Concat(fileExternalLab)
                .Where(System.IO.File.Exists)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private async Task<List<string>> CollectCdhaFilesForMergeBySidAsync(string sid, string categoryCode, string patientId, string maBenhAn)
        {
            var fileBySid = new List<string>();
            var fileExternalResultPhysical = new List<string>();
            var fileExternalResult = new List<string>();

            // 1) File chính theo SID
            var folder = Path.Combine(_environment.WebRootPath, "pdf", categoryCode);
            var basePdfPath = Path.Combine(folder, sid + ".pdf");
            if (System.IO.File.Exists(basePdfPath))
            {
                fileBySid.Add(basePdfPath);
            }

            // 2) Resolve serviceCode nếu cần rule ignore
            string resolvedPatientId = patientId;
            string resolvedMaBenhAn = maBenhAn;
            string patientName = "Unknown";
            string serviceCode = "Unknown";

            var result = await TryResolvePatientCDHAAsync(sid);
            if (!string.IsNullOrWhiteSpace(result.patientId)) resolvedPatientId = result.patientId;
            if (!string.IsNullOrWhiteSpace(result.maBenhAn)) resolvedMaBenhAn = result.maBenhAn;
            patientName = result.patientName;
            serviceCode = result.serviceCode;

            var ignoreExternalServiceCodes = new List<string>
            {
                "XQ52"
            };

            bool ignoreExternal = ignoreExternalServiceCodes
                .Any(x => x.Equals(serviceCode, StringComparison.OrdinalIgnoreCase));

            if (!ignoreExternal)
            {
                var sanitizedPId = SanitizeFilePart(resolvedPatientId) ?? "unknown";
                var sanitizedMaBenhAn = SanitizeFilePart(resolvedMaBenhAn) ?? "unknown";

                // 3) Scan folder mới
                var newFolder = Path.Combine(_environment.WebRootPath, "uploads", "external_result_cdha", sanitizedPId, sanitizedMaBenhAn);
                if (Directory.Exists(newFolder))
                {
                    var filesInNewFolder = Directory.EnumerateFiles(newFolder, "*.pdf", SearchOption.TopDirectoryOnly)
                        .OrderByDescending(System.IO.File.GetCreationTimeUtc)
                        .Take(100)
                        .ToList();

                    fileExternalResultPhysical.AddRange(filesInNewFolder);
                }

                // 4) Fallback folder cũ
                var legacyFolder = Path.Combine(_environment.WebRootPath, "uploads", "external_result_cdha");
                if (Directory.Exists(legacyFolder))
                {
                    if (!string.IsNullOrWhiteSpace(resolvedMaBenhAn))
                    {
                        var pattern = $"patient_{resolvedMaBenhAn}_*.pdf";
                        var files = Directory.EnumerateFiles(legacyFolder, pattern, SearchOption.TopDirectoryOnly)
                            .OrderByDescending(System.IO.File.GetCreationTimeUtc)
                            .Take(100)
                            .ToList();

                        fileExternalResultPhysical.AddRange(files);
                    }

                    if (!string.IsNullOrWhiteSpace(resolvedPatientId))
                    {
                        var pattern = $"patient_{resolvedPatientId}_*.pdf";
                        var files = Directory.EnumerateFiles(legacyFolder, pattern, SearchOption.TopDirectoryOnly)
                            .OrderByDescending(System.IO.File.GetCreationTimeUtc)
                            .Take(100)
                            .ToList();

                        fileExternalResultPhysical.AddRange(files);
                    }
                }

                fileExternalResultPhysical = fileExternalResultPhysical
                    .Where(System.IO.File.Exists)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                // 5) Sync metadata
                if (fileExternalResultPhysical.Any())
                {
                    try
                    {
                        await _externalFileBL.SyncPhysicalFilesIfMissingAsync(
                            physicalFilePaths: fileExternalResultPhysical,
                            webRootPath: _environment.WebRootPath,
                            pId: resolvedPatientId,
                            maBenhAn: resolvedMaBenhAn,
                            tablePatientId: 0,
                            moduleType: ExternalFileModule.CDHA,
                            patientName: patientName,
                            createdBy: null,
                            isVisibleToUser: true
                        );
                    }
                    catch (Exception exSync)
                    {
                        _logger.LogWarning(exSync, "CollectCdhaFilesForMergeBySidAsync sync failed. sid={Sid}", sid);
                    }
                }

                // 6) Query lại từ DB chỉ lấy file visible
                var dbFiles = await _externalFileBL.GetVisibleFilesAsync(
                    tablePatientId: null,
                    pId: resolvedPatientId,
                    maBenhAn: resolvedMaBenhAn,
                    moduleType: ExternalFileModule.CDHA
                );

                foreach (var dbItem in dbFiles)
                {
                    var normalizedRelativePath = (dbItem.RelativePath ?? "").Replace("\\", "/").TrimStart('/');
                    if (string.IsNullOrWhiteSpace(normalizedRelativePath)) continue;

                    var absPath = Path.Combine(
                        _environment.WebRootPath,
                        normalizedRelativePath.Replace("/", Path.DirectorySeparatorChar.ToString())
                    );

                    if (System.IO.File.Exists(absPath))
                    {
                        fileExternalResult.Add(absPath);
                    }
                }
            }

            return fileBySid
                .Concat(fileExternalResult)
                .Where(System.IO.File.Exists)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
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
        private async Task<(string? patientId, string? maBenhAn, string? patientName, string? serviceCode)> TryResolvePatientCDHAAsync(string sid)
        {
            try
            {
                var record = await _resultCDHABL.GetResultCDHAByKeyResultForHis(sid);
                if (record == null) return (null, null, "Unknown", "Unknown");

                var patient = record.Patient;
                if (patient == null) return (null, null, "Unknown", "Unknown");

                var service = record.Service;


                return (patient.PatientId, patient.MaBenhAn, patient.PatientName, service?.Code ?? "Unknown");
            }
            catch
            {
                return (null, null, "Unknown", "Unknown");
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

        private static string SanitizeFilePart(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            // bỏ ký tự lạ để tránh lỗi tên file/url
            var cleaned = new string(s.Where(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_').ToArray());
            return string.IsNullOrEmpty(cleaned) ? null : cleaned;
        }
    }
}
