using Management.BL;
using Management.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GetResultPdfFileController : ControllerBase
    {
        public readonly ILogger<GetResultPdfFileController> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly APIBL _apiBL;
        private readonly ServiceBL _serviceBL;
        private readonly ResultXNBL _resultXNBL;
        private readonly ResultCDHABL _resultCDHABL;
        private readonly PatientXNBL _patientXNBL;
        private readonly PatientCDHABL _patientCDHABL;
        private readonly ExternalFileBL _externalFileBL;

        public GetResultPdfFileController(ILogger<GetResultPdfFileController> logger, IWebHostEnvironment environment, APIBL apiBL, ServiceBL serviceBL, ResultXNBL resultXNBL, ResultCDHABL resultCDHABL, PatientXNBL patientXNBL, PatientCDHABL patientCDHABL, ExternalFileBL externalFileBL)
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

        //[HttpGet]
        //public async Task<IActionResult> GetResultPdfFile(string sid)
        //{
        //    var categoryCode = sid.Split('-')[0];
        //    var _folder = Path.Combine(_environment.WebRootPath, "pdf", categoryCode);
        //    var _fileName = sid + ".pdf";
        //    var _filePath = Path.Combine(_folder, _fileName);

        //    if (System.IO.File.Exists(_filePath))
        //    {
        //        var fileByte = System.IO.File.ReadAllBytes(_filePath);
        //        return File(fileByte, "application/pdf");
        //    }
        //    return NotFound($"Dịch vụ chưa hoàn thành theo mã : {sid}.pdf");
        //}

        //[HttpPost]
        //public async Task<IActionResult> AddService([FromBody] IEnumerable<PatientInfo_Add> lstPatientInfo)
        //{
        //    try
        //    {
        //        if (lstPatientInfo != null && lstPatientInfo.Count() > 0)
        //        {
        //            var listUpdateStatusTicketItemId = new List<StatusTicketItemId>();
        //            var lstSIDForTicketItem = new List<SIDForTicketItem>();
        //            var dateTimeServer = DateTime.Now;
        //            var groupPatientInfo = lstPatientInfo.GroupBy(p => p.PatientId);
        //            if (groupPatientInfo != null)
        //            {
        //                foreach (var group in groupPatientInfo)
        //                {
        //                    try
        //                    {
        //                        var lstGroup = group.ToList();
        //                        if (lstGroup != null && lstGroup.Count > 0)
        //                        {
        //                            Patient patient = null;
        //                            DateTime? age = null;
        //                            var sex = string.Empty;
        //                            var objectName = "Thu phí";
        //                            try
        //                            {
        //                                age = DateTime.Parse(lstGroup[0].Age);
        //                            }
        //                            catch { }

        //                            if (lstGroup[0].Sex.Trim() == "male")
        //                            {
        //                                sex = "Nam";
        //                            }
        //                            else
        //                            {
        //                                sex = "Nữ";
        //                            }

        //                            if (!string.IsNullOrEmpty(lstGroup[0].ObjectName.Trim()))
        //                            {
        //                                objectName = lstGroup[0].ObjectName.Trim();
        //                            }

        //                            try
        //                            {
        //                                patient = await _apiBL.AddPatient(null, null, lstGroup[0].PatientId, lstGroup[0].TicketId, lstGroup[0].PatientName, sex, lstGroup[0].Address, age, lstGroup[0].Diagnostic, objectName, lstGroup[0].LocationName, lstGroup[0].DoctorName, dateTimeServer, lstGroup[0].AssignDate, lstGroup[0].Type);
        //                            }
        //                            catch (Exception ex)
        //                            {
        //                                foreach (var item in lstGroup)
        //                                {
        //                                    if (item != null)
        //                                    {
        //                                        listUpdateStatusTicketItemId.Add(new StatusTicketItemId { ticket_item_id = item.TicketItemId, type = item.Type, status = StatusForHIS.open });
        //                                    }
        //                                }
        //                                continue;
        //                            }
        //                            if (patient == null) continue;
        //                            var keyResultForHis_XN = "xn-" + Guid.NewGuid().ToString();
        //                            var keyResultForHis_CDHA = string.Empty;
        //                            foreach (var item in lstGroup)
        //                            {
        //                                if (item != null)
        //                                {
        //                                    try
        //                                    {
        //                                        var service = await _apiBL.GetService(item.ServiceId);
        //                                        if (service != null)
        //                                        {
        //                                            if (service.Category.Group.Code == "XN")
        //                                            {
        //                                                await _apiBL.AddResultXN(patient.Id, item.TicketItemId, dateTimeServer, service, item.DoctorName, keyResultForHis_XN);
        //                                                lstSIDForTicketItem.Add(new SIDForTicketItem { ticketItems = item.TicketItemId, sid = keyResultForHis_XN });
        //                                            }
        //                                            else if (service.Category.Group.Code == "CDHA")
        //                                            {
        //                                                keyResultForHis_CDHA = service.Category.Code.ToLower() + "-" + Guid.NewGuid().ToString();
        //                                                await _apiBL.AddResultCDHA(patient.Id, item.TicketItemId, dateTimeServer, service, item.DoctorName, keyResultForHis_CDHA);
        //                                                lstSIDForTicketItem.Add(new SIDForTicketItem { ticketItems = item.TicketItemId, sid = keyResultForHis_CDHA });
        //                                            }
        //                                            listUpdateStatusTicketItemId.Add(new StatusTicketItemId { ticket_item_id = item.TicketItemId, type = item.Type, status = StatusForHIS.waitting });
        //                                        }
        //                                    }
        //                                    catch
        //                                    {
        //                                        listUpdateStatusTicketItemId.Add(new StatusTicketItemId { ticket_item_id = item.TicketItemId, type = item.Type, status = StatusForHIS.open });
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                    catch { }
        //                }
        //            }

        //            if (listUpdateStatusTicketItemId != null && listUpdateStatusTicketItemId.Count > 0)
        //            {
        //                await _apiBL.UpdateStatusTicketItemId(listUpdateStatusTicketItemId);
        //            }
        //            if (lstSIDForTicketItem != null && lstSIDForTicketItem.Count > 0)
        //            {
        //                await _apiBL.UpdateSIDForTicketItemId(lstSIDForTicketItem);
        //            }
        //        }
        //        return Content("True");
        //    }
        //    catch
        //    {
        //        return Content("False");
        //    }
        //}

        //[HttpPost]
        //public async Task<IActionResult> DeleteService([FromBody] IEnumerable<PatientInfo_Del> lstPatientInfo)
        //{
        //    try
        //    {
        //        if (lstPatientInfo != null && lstPatientInfo.Count() > 0)
        //        {
        //            foreach (var item in lstPatientInfo)
        //            {
        //                var service = await _serviceBL.Get(item.ServiceId);
        //                if (service == null) continue;
        //                var sid = string.Empty;
        //                if (service.Category.Group.Code == "XN")
        //                {
        //                    await _resultXNBL.Delete(item.TicketItemId);
        //                }
        //                else if (service.Category.Group.Code == "CDHA")
        //                {
        //                    await _resultCDHABL.Delete(item.TicketItemId, service.Category.Code);
        //                }
        //            }
        //        }
        //        return Content("True");
        //    }
        //    catch
        //    {
        //        return Content("False");
        //    }
        //}
        [HttpGet]
        public async Task<IActionResult> GetResultPdfFile(string sid)
        {
            if (string.IsNullOrWhiteSpace(sid)) return BadRequest("sid is required");

            var categoryCode = sid.Split('-')[0];

            if (categoryCode.Equals("xn", StringComparison.OrdinalIgnoreCase))
            {
                return await GetBothPdfAndExternalLabFiles(sid, categoryCode);
            }

            if (categoryCode.Equals("xq", StringComparison.OrdinalIgnoreCase))
            {
                return await GetBothPdfAndExternalResultFiles(sid, categoryCode);
            }

            var folder = Path.Combine(_environment.WebRootPath, "pdf", categoryCode);
            var fileName = sid + ".pdf";
            var filePath = Path.Combine(folder, fileName);

            if (System.IO.File.Exists(filePath))
            {
                // Cho trình duyệt mở trực tiếp
                Response.Headers["Content-Disposition"] = $"inline; filename=\"{fileName}\"";
                // Bật range processing nếu cần tua/trượt trang trên viewer
                return PhysicalFile(filePath, "application/pdf", enableRangeProcessing: true);
            }
            return NotFound($"Dịch vụ chưa hoàn thành theo mã : {sid}.pdf");
        }

        //private async Task<IActionResult> GetBothPdfAndExternalLabFiles(string sid, string categoryCode, bool merge = true)
        //{
        //    try
        //    {
        //        var fileBySid = new List<string>();
        //        var fileExternalLab = new List<string>();

        //        var folder = Path.Combine(_environment.WebRootPath, "pdf", categoryCode);
        //        var basePdfPath = Path.Combine(folder, sid + ".pdf");
        //        if (System.IO.File.Exists(basePdfPath))
        //        {
        //            fileBySid.Add(basePdfPath);
        //        }

        //        string? patientId = null;
        //        string? maBenhAn = null;
        //        string patientName = "Unknown";
        //        await TryResolvePatientAsync(sid).ContinueWith(t =>
        //        {
        //            if (t.Status == TaskStatus.RanToCompletion)
        //            {
        //                (patientId, maBenhAn, patientName) = t.Result;
        //            }
        //        });
        //        // ========== 1) Tìm trong folder MỚI: uploads/external_lab/{patientId}/{maBenhAn}/ ==========
        //        if (!string.IsNullOrWhiteSpace(patientId) && !string.IsNullOrWhiteSpace(maBenhAn))
        //        {
        //            var newFolder = Path.Combine(_environment.WebRootPath, "uploads", "external_lab", patientId, maBenhAn);
        //            if (Directory.Exists(newFolder))
        //            {
        //                var filesInNewFolder = Directory.EnumerateFiles(newFolder, "*.pdf", SearchOption.TopDirectoryOnly)
        //                                                .OrderByDescending(System.IO.File.GetCreationTimeUtc)
        //                                                .Take(100)
        //                                                .ToList();
        //                fileExternalLab.AddRange(filesInNewFolder);
        //            }
        //        }

        //        // ========== 2) Fallback: tìm trong folder CŨ (flat): uploads/external_lab/ ==========
        //        var legacyFolder = Path.Combine(_environment.WebRootPath, "uploads", "external_lab");
        //        if (Directory.Exists(legacyFolder))
        //        {
        //            if (!string.IsNullOrWhiteSpace(maBenhAn))
        //            {
        //                var pattern = $"patient_{maBenhAn}_*.pdf";
        //                var files = Directory.EnumerateFiles(legacyFolder, pattern, SearchOption.TopDirectoryOnly)
        //                                     .OrderByDescending(System.IO.File.GetCreationTimeUtc)
        //                                     .Take(100)
        //                                     .ToList();
        //                fileExternalLab.AddRange(files);
        //            }

        //            if (!string.IsNullOrWhiteSpace(patientId))
        //            {
        //                var pattern = $"patient_{patientId}_*.pdf";
        //                var files = Directory.EnumerateFiles(legacyFolder, pattern, SearchOption.TopDirectoryOnly)
        //                                     .OrderByDescending(System.IO.File.GetCreationTimeUtc)
        //                                     .Take(100)
        //                                     .ToList();
        //                fileExternalLab.AddRange(files);
        //            }
        //        }
        //        // Loại trùng (theo full path)
        //        fileExternalLab = fileExternalLab.Distinct().ToList();

        //        var allFiles = fileBySid.Concat(fileExternalLab).ToList();

        //        if (!allFiles.Any())
        //        {
        //            return NotFound($"Không tìm thấy kết quả xét nghiệm theo mã kết quả SID: {sid}");
        //        }

        //        // ✅ Sửa đoạn này:
        //        if (merge && allFiles.Count > 1)
        //        {
        //            try
        //            {
        //                var merged = MergePdfs(allFiles);
        //                var outName = $"{sid}_merged.pdf";
        //                Response.Headers["Content-Disposition"] = $"inline; filename=\"{outName}\"";
        //                return File(merged, "application/pdf"); // KHÔNG truyền fileDownloadName
        //            }
        //            catch (Exception ex)
        //            {
        //                // Có thể log nếu cần
        //            }
        //        }

        //        if (allFiles.Count == 1)
        //        {
        //            var f = allFiles[0];
        //            var onlyName = Path.GetFileName(f);
        //            Response.Headers["Content-Disposition"] = $"inline; filename=\"{onlyName}\"";
        //            return PhysicalFile(f, "application/pdf", enableRangeProcessing: true);
        //        }

        //        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        //        var fileBySidUrls = fileBySid.Where(System.IO.File.Exists).Select(absPath => ToPublicUrl(absPath, baseUrl)).Where(url => url != null).Cast<string>().ToList();
        //        var fileExternalLabUrls = fileExternalLab.Where(System.IO.File.Exists).Select(absPath => ToPublicUrl(absPath, baseUrl)).Where(url => url != null).Cast<string>().ToList();

        //        return new JsonResult(new
        //        {
        //            success = true,
        //            sid,
        //            patientName,
        //            fileBySid = fileBySidUrls,
        //            fileExternalLab = fileExternalLabUrls,
        //            fileCount = allFiles.Count
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }
        //}

        private async Task<IActionResult> GetBothPdfAndExternalLabFiles(string sid, string categoryCode, bool merge = true)
        {
            try
            {
                var fileBySid = new List<string>();
                var fileExternalLabPhysical = new List<string>();
                var fileExternalLab = new List<string>();

                // 1) File PDF chính theo SID
                var folder = Path.Combine(_environment.WebRootPath, "pdf", categoryCode);
                var basePdfPath = Path.Combine(folder, sid + ".pdf");
                if (System.IO.File.Exists(basePdfPath))
                {
                    fileBySid.Add(basePdfPath);
                }

                // 2) Resolve patient info
                string? patientId = null;
                string? maBenhAn = null;
                string patientName = "Unknown";

                var patientResult = await TryResolvePatientAsync(sid);
                (patientId, maBenhAn, patientName) = patientResult;

                var sanitizedPId = SanitizeFilePart(patientId) ?? "unknown";
                var sanitizedMaBenhAn = SanitizeFilePart(maBenhAn) ?? "unknown";

                // 3) Scan file vật lý trong folder mới
                if (!string.IsNullOrWhiteSpace(patientId) && !string.IsNullOrWhiteSpace(maBenhAn))
                {
                    var newFolder = Path.Combine(_environment.WebRootPath, "uploads", "external_lab", sanitizedPId, sanitizedMaBenhAn);
                    if (Directory.Exists(newFolder))
                    {
                        var filesInNewFolder = Directory.EnumerateFiles(newFolder, "*.pdf", SearchOption.TopDirectoryOnly)
                            .OrderByDescending(System.IO.File.GetCreationTimeUtc)
                            .Take(100)
                            .ToList();

                        fileExternalLabPhysical.AddRange(filesInNewFolder);
                    }
                }

                // 4) Fallback folder cũ (legacy flat)
                var legacyFolder = Path.Combine(_environment.WebRootPath, "uploads", "external_lab");
                if (Directory.Exists(legacyFolder))
                {
                    if (!string.IsNullOrWhiteSpace(maBenhAn))
                    {
                        var pattern = $"patient_{maBenhAn}_*.pdf";
                        var files = Directory.EnumerateFiles(legacyFolder, pattern, SearchOption.TopDirectoryOnly)
                            .OrderByDescending(System.IO.File.GetCreationTimeUtc)
                            .Take(100)
                            .ToList();

                        fileExternalLabPhysical.AddRange(files);
                    }

                    if (!string.IsNullOrWhiteSpace(patientId))
                    {
                        var pattern = $"patient_{patientId}_*.pdf";
                        var files = Directory.EnumerateFiles(legacyFolder, pattern, SearchOption.TopDirectoryOnly)
                            .OrderByDescending(System.IO.File.GetCreationTimeUtc)
                            .Take(100)
                            .ToList();

                        fileExternalLabPhysical.AddRange(files);
                    }
                }

                fileExternalLabPhysical = fileExternalLabPhysical
                    .Where(System.IO.File.Exists)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                // 5) Sync metadata nếu scan được file vật lý
                try
                {
                    if (fileExternalLabPhysical.Any())
                    {
                        await _externalFileBL.SyncPhysicalFilesIfMissingAsync(
                            physicalFilePaths: fileExternalLabPhysical,
                            webRootPath: _environment.WebRootPath,
                            pId: patientId,
                            maBenhAn: maBenhAn,
                            tablePatientId: 0, // API này đang ưu tiên pId + maBenhAn
                            moduleType: ExternalFileModule.Lab,
                            patientName: patientName,
                            createdBy: null,
                            isVisibleToUser: true
                        );
                    }
                }
                catch (Exception exSync)
                {
                    _logger.LogWarning(exSync, "SyncPhysicalFilesIfMissingAsync failed in GetBothPdfAndExternalLabFiles. sid={Sid}", sid);
                }

                // 6) Query lại từ DB để chỉ lấy file visible
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

                fileExternalLab = fileExternalLab
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var allFiles = fileBySid
                    .Concat(fileExternalLab)
                    .Where(System.IO.File.Exists)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (!allFiles.Any())
                {
                    return NotFound($"Không tìm thấy kết quả xét nghiệm theo mã kết quả SID: {sid}");
                }

                // 7) Merge nếu cần
                if (merge && allFiles.Count > 1)
                {
                    try
                    {
                        var merged = MergePdfs(allFiles);
                        var outName = $"{sid}_merged.pdf";
                        Response.Headers["Content-Disposition"] = $"inline; filename=\"{outName}\"";
                        return File(merged, "application/pdf");
                    }
                    catch (Exception exMerge)
                    {
                        _logger.LogWarning(exMerge, "MergePdfs failed in GetBothPdfAndExternalLabFiles. sid={Sid}", sid);
                    }
                }

                // 8) Chỉ có 1 file
                if (allFiles.Count == 1)
                {
                    var f = allFiles[0];
                    var onlyName = Path.GetFileName(f);
                    Response.Headers["Content-Disposition"] = $"inline; filename=\"{onlyName}\"";
                    return PhysicalFile(f, "application/pdf", enableRangeProcessing: true);
                }

                // 9) Nếu không merge thì trả JSON
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var fileBySidUrls = fileBySid
                    .Where(System.IO.File.Exists)
                    .Select(absPath => ToPublicUrl(absPath, baseUrl))
                    .Where(url => url != null)
                    .Cast<string>()
                    .ToList();

                var fileExternalLabUrls = fileExternalLab
                    .Where(System.IO.File.Exists)
                    .Select(absPath => ToPublicUrl(absPath, baseUrl))
                    .Where(url => url != null)
                    .Cast<string>()
                    .ToList();

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
                _logger.LogError(ex, "GetBothPdfAndExternalLabFiles failed. sid={Sid}", sid);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy PDF kết quả CĐHA (XQ, ...) kèm file external đã import trong uploads/external_result_cdha/{patientId}/{maBenhAn}/
        /// Không merge — trả file kết quả chính (pdf/{categoryCode}/{sid}.pdf), nếu không có thì trả danh sách JSON.
        /// </summary>
        //private async Task<IActionResult> GetBothPdfAndExternalResultFiles(string sid, string categoryCode)
        //{
        //    try
        //    {
        //        var fileBySid = new List<string>();
        //        var fileExternalResult = new List<string>();

        //        // File kết quả chính theo SID
        //        var folder = Path.Combine(_environment.WebRootPath, "pdf", categoryCode);
        //        var basePdfPath = Path.Combine(folder, sid + ".pdf");
        //        if (System.IO.File.Exists(basePdfPath))
        //        {
        //            fileBySid.Add(basePdfPath);
        //        }

        //        // Resolve patientId / maBenhAn từ ResultCDHA
        //        string? patientId = null;
        //        string? maBenhAn = null;
        //        string patientName = "Unknown";
        //        string serviceCode = "Unknown";
        //        //await TryResolvePatientCDHAAsync(sid).ContinueWith(t =>
        //        //{
        //        //    if (t.Status == TaskStatus.RanToCompletion)
        //        //    {
        //        //        (patientId, maBenhAn, patientName, serviceCode) = t.Result;
        //        //    }
        //        //});

        //        var result = await TryResolvePatientCDHAAsync(sid);
        //        (patientId, maBenhAn, patientName, serviceCode) = result;

        //        // ===== CONFIG: danh sách bỏ qua external =====
        //        var ignoreExternalServiceCodes = new List<string>
        //        {
        //            "XQ52"
        //        };

        //        bool ignoreExternal = ignoreExternalServiceCodes
        //            .Any(x => x.Equals(serviceCode, StringComparison.OrdinalIgnoreCase));
        //        // ===== CHỈ chạy nếu KHÔNG nằm trong danh sách =====
        //        if (!ignoreExternal)
        //        {
        //            // ========== 1) Tìm trong folder MỚI: uploads/external_result_cdha/{patientId}/{maBenhAn}/ ==========
        //            if (!string.IsNullOrWhiteSpace(patientId) && !string.IsNullOrWhiteSpace(maBenhAn))
        //            {
        //                var newFolder = Path.Combine(_environment.WebRootPath, "uploads", "external_result_cdha", patientId, maBenhAn);
        //                if (Directory.Exists(newFolder))
        //                {
        //                    var filesInNewFolder = Directory.EnumerateFiles(newFolder, "*.pdf", SearchOption.TopDirectoryOnly)
        //                                                    .OrderByDescending(System.IO.File.GetCreationTimeUtc)
        //                                                    .Take(100)
        //                                                    .ToList();
        //                    fileExternalResult.AddRange(filesInNewFolder);
        //                }
        //            }

        //            // ========== 2) Fallback: tìm trong folder CŨ (flat): uploads/external_result_cdha/ ==========
        //            var legacyFolder = Path.Combine(_environment.WebRootPath, "uploads", "external_result_cdha");
        //            if (Directory.Exists(legacyFolder))
        //            {
        //                if (!string.IsNullOrWhiteSpace(maBenhAn))
        //                {
        //                    var pattern = $"patient_{maBenhAn}_*.pdf";
        //                    var files = Directory.EnumerateFiles(legacyFolder, pattern, SearchOption.TopDirectoryOnly)
        //                                         .OrderByDescending(System.IO.File.GetCreationTimeUtc)
        //                                         .Take(100)
        //                                         .ToList();
        //                    fileExternalResult.AddRange(files);
        //                }

        //                if (!string.IsNullOrWhiteSpace(patientId))
        //                {
        //                    var pattern = $"patient_{patientId}_*.pdf";
        //                    var files = Directory.EnumerateFiles(legacyFolder, pattern, SearchOption.TopDirectoryOnly)
        //                                         .OrderByDescending(System.IO.File.GetCreationTimeUtc)
        //                                         .Take(100)
        //                                         .ToList();
        //                    fileExternalResult.AddRange(files);
        //                }
        //            }
        //        }


        //        // Loại trùng (theo full path)
        //        fileExternalResult = fileExternalResult.Distinct().ToList();

        //        var allFiles = fileBySid.Concat(fileExternalResult).ToList();

        //        if (!allFiles.Any())
        //        {
        //            return NotFound($"Không tìm thấy kết quả theo mã SID: {sid}");
        //        }

        //        // Nhiều file → merge thành 1 PDF
        //        if (allFiles.Count > 1)
        //        {
        //            try
        //            {
        //                var merged = MergePdfs(allFiles);
        //                var outName = $"{sid}_merged.pdf";
        //                Response.Headers["Content-Disposition"] = $"inline; filename=\"{outName}\"";
        //                return File(merged, "application/pdf");
        //            }
        //            catch (Exception ex)
        //            {
        //                // Merge thất bại → fallback trả file chính
        //            }
        //        }

        //        // Chỉ có 1 file hoặc merge thất bại → trả file đầu tiên
        //        var f = allFiles[0];
        //        var onlyName = Path.GetFileName(f);
        //        Response.Headers["Content-Disposition"] = $"inline; filename=\"{onlyName}\"";
        //        return PhysicalFile(f, "application/pdf", enableRangeProcessing: true);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }
        //}
        private async Task<IActionResult> GetBothPdfAndExternalResultFiles(string sid, string categoryCode)
        {
            try
            {
                var fileBySid = new List<string>();
                var fileExternalResultPhysical = new List<string>();
                var fileExternalResult = new List<string>();

                // 1) File kết quả chính theo SID
                var folder = Path.Combine(_environment.WebRootPath, "pdf", categoryCode);
                var basePdfPath = Path.Combine(folder, sid + ".pdf");
                if (System.IO.File.Exists(basePdfPath))
                {
                    fileBySid.Add(basePdfPath);
                }

                // 2) Resolve thông tin bệnh nhân / dịch vụ từ ResultCDHA
                string? patientId = null;
                string? maBenhAn = null;
                string patientName = "Unknown";
                string serviceCode = "Unknown";

                var result = await TryResolvePatientCDHAAsync(sid);
                (patientId, maBenhAn, patientName, serviceCode) = result;

                var sanitizedPId = SanitizeFilePart(patientId) ?? "unknown";
                var sanitizedMaBenhAn = SanitizeFilePart(maBenhAn) ?? "unknown";

                // 3) Danh sách service không lấy external
                var ignoreExternalServiceCodes = new List<string>
                {
                    "XQ52"
                };

                bool ignoreExternal = ignoreExternalServiceCodes
                    .Any(x => x.Equals(serviceCode, StringComparison.OrdinalIgnoreCase));

                if (!ignoreExternal)
                {
                    // 4) Scan folder mới: uploads/external_result_cdha/{pId}/{maBenhAn}/
                    if (!string.IsNullOrWhiteSpace(patientId) && !string.IsNullOrWhiteSpace(maBenhAn))
                    {
                        var newFolder = Path.Combine(_environment.WebRootPath, "uploads", "external_result_cdha", sanitizedPId, sanitizedMaBenhAn);
                        if (Directory.Exists(newFolder))
                        {
                            var filesInNewFolder = Directory.EnumerateFiles(newFolder, "*.pdf", SearchOption.TopDirectoryOnly)
                                .OrderByDescending(System.IO.File.GetCreationTimeUtc)
                                .Take(100)
                                .ToList();

                            fileExternalResultPhysical.AddRange(filesInNewFolder);
                        }
                    }

                    // 5) Fallback folder cũ: uploads/external_result_cdha/
                    var legacyFolder = Path.Combine(_environment.WebRootPath, "uploads", "external_result_cdha");
                    if (Directory.Exists(legacyFolder))
                    {
                        if (!string.IsNullOrWhiteSpace(maBenhAn))
                        {
                            var pattern = $"patient_{maBenhAn}_*.pdf";
                            var files = Directory.EnumerateFiles(legacyFolder, pattern, SearchOption.TopDirectoryOnly)
                                .OrderByDescending(System.IO.File.GetCreationTimeUtc)
                                .Take(100)
                                .ToList();

                            fileExternalResultPhysical.AddRange(files);
                        }

                        if (!string.IsNullOrWhiteSpace(patientId))
                        {
                            var pattern = $"patient_{patientId}_*.pdf";
                            var files = Directory.EnumerateFiles(legacyFolder, pattern, SearchOption.TopDirectoryOnly)
                                .OrderByDescending(System.IO.File.GetCreationTimeUtc)
                                .Take(100)
                                .ToList();

                            fileExternalResultPhysical.AddRange(files);
                        }
                    }
                }

                // 6) Loại trùng file vật lý
                fileExternalResultPhysical = fileExternalResultPhysical
                    .Where(System.IO.File.Exists)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                // 7) Sync metadata nếu có file vật lý và không nằm trong ignore list
                if (!ignoreExternal)
                {
                    try
                    {
                        if (fileExternalResultPhysical.Any())
                        {
                            await _externalFileBL.SyncPhysicalFilesIfMissingAsync(
                                physicalFilePaths: fileExternalResultPhysical,
                                webRootPath: _environment.WebRootPath,
                                pId: patientId,
                                maBenhAn: maBenhAn,
                                tablePatientId: 0,
                                moduleType: ExternalFileModule.CDHA,
                                patientName: patientName,
                                createdBy: null,
                                isVisibleToUser: true
                            );
                        }
                    }
                    catch (Exception exSync)
                    {
                        _logger.LogWarning(exSync,
                            "SyncPhysicalFilesIfMissingAsync failed in GetBothPdfAndExternalResultFiles. sid={Sid}",
                            sid);
                    }

                    // 8) Query lại từ DB để chỉ lấy file visible
                    var dbFiles = await _externalFileBL.GetVisibleFilesAsync(
                        tablePatientId: null,
                        pId: patientId,
                        maBenhAn: maBenhAn,
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

                fileExternalResult = fileExternalResult
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var allFiles = fileBySid
                    .Concat(fileExternalResult)
                    .Where(System.IO.File.Exists)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (!allFiles.Any())
                {
                    return NotFound($"Không tìm thấy kết quả theo mã SID: {sid}");
                }

                // 9) Nhiều file thì merge
                if (allFiles.Count > 1)
                {
                    try
                    {
                        var merged = MergePdfs(allFiles);
                        var outName = $"{sid}_merged.pdf";
                        Response.Headers["Content-Disposition"] = $"inline; filename=\"{outName}\"";
                        return File(merged, "application/pdf");
                    }
                    catch (Exception exMerge)
                    {
                        _logger.LogWarning(exMerge,
                            "MergePdfs failed in GetBothPdfAndExternalResultFiles. sid={Sid}",
                            sid);
                    }
                }

                // 10) Chỉ có 1 file hoặc merge lỗi
                var f = allFiles[0];
                var onlyName = Path.GetFileName(f);
                Response.Headers["Content-Disposition"] = $"inline; filename=\"{onlyName}\"";
                return PhysicalFile(f, "application/pdf", enableRangeProcessing: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetBothPdfAndExternalResultFiles failed. sid={Sid}", sid);
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

        /// <summary>
        /// Resolve patientId / maBenhAn từ ResultCDHA theo KeyResultForHis
        /// </summary>
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
