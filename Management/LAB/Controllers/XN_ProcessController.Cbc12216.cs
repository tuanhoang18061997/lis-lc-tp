using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Management.Services.LabParsing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UglyToad.PdfPig;

namespace Management.Controllers
{
    public partial class XN_ProcessController
    {
        /// <summary>
        /// Nhánh tạm CBC riêng; không đi qua ImportExternalLabPdf/UpdateFromExternal (_taman).
        /// Chỉ ghi đúng bệnh nhân + ServiceId 12216 sau khi xác thực tên, service, TestCode.
        /// </summary>
        [HttpPost]
        [Authorize]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> ImportCBC12216Pdf(
            string pMaBenhAn, string pId, long pidTablePatient, string pName,
            IFormFile file, bool overwrite = false, bool markValid = false,
            bool isVisibleToUser = true)
        {
            if (pidTablePatient <= 0 || string.IsNullOrWhiteSpace(pId) ||
                string.IsNullOrWhiteSpace(pMaBenhAn))
                return Json(new { success = false, message = "Chưa chọn bệnh nhân/mã bệnh án hợp lệ." });
            if (file == null || file.Length == 0 || file.Length > 10 * 1024 * 1024 ||
                !string.Equals(Path.GetExtension(file.FileName), ".pdf", StringComparison.OrdinalIgnoreCase))
                return Json(new { success = false, message = "Chỉ hỗ trợ tệp PDF tối đa 10MB." });

            string path = null;
            bool metadataSaved = false;
            bool resultUpdated = false;
            int updatedCount = 0;
            try
            {
                var safePId = SanitizeFilePart(pId) ?? "unknown";
                var safeMa = SanitizeFilePart(pMaBenhAn) ?? "unknown";
                var dir = Path.Combine(_environment.WebRootPath, "uploads", "external_lab", safePId, safeMa);
                Directory.CreateDirectory(dir);
                var fileName = $"patient_{safeMa}_{RemoveVietnameseAccentsAndSpaces(pName)}_CBC12216_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}.pdf";
                path = Path.Combine(dir, fileName);
                using (var stream = new FileStream(path, FileMode.CreateNew))
                    await file.CopyToAsync(stream);

                // Parser bảng dùng chung lấy ĐÚNG cột Kết quả, sau đó nhánh CBC
                // map bằng danh sách trắng 22 TestCodeId, không dựa trên '_taman'.
                var rawRows = ExtractLabResultRowsFromPdf(
                    path, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
                var cbcItems = Cbc12216ResultMapper.ParseStrict(rawRows);
                var pdfName = ExtractCBC12216PdfPatientName(path);
                if (string.IsNullOrWhiteSpace(pdfName))
                    throw new InvalidDataException("Không đọc được họ tên từ PDF CBC.");

                // Chạy preflight READ-ONLY trước khi đưa file vào danh sách bệnh nhân:
                // kiểm tra họ tên PDF == DB, dịch vụ 12216, ServiceTest, ResultXN, ký số.
                var userLogin = GetUserLogin();
                await _resultXNBL.UpdateCBC12216FromExternal(
                    pidTablePatient, cbcItems, pdfName, pId, pMaBenhAn,
                    overwrite: false, markValid: false, userId: userLogin, dryRun: true);

                // File gốc được lưu metadata để truy vết trước khi ghi kết quả;
                // nếu DB update thất bại sẽ giữ file đã lưu và báo lỗi rõ ràng.
                var relativePath = _externalFileBL.BuildRelativePath(
                    rootFolderName: "external_lab", pId: safePId,
                    maBenhAn: safeMa, storedFileName: fileName);
                var externalFileId = await _externalFileBL.EnsureFileSyncedAsync(
                    storedFileName: fileName,
                    relativePath: relativePath,
                    pId: pId,
                    maBenhAn: pMaBenhAn,
                    tablePatientId: pidTablePatient,
                    moduleType: Management.Models.ExternalFileModule.Lab,
                    patientName: pName,
                    createdBy: userLogin,
                    isVisibleToUser: isVisibleToUser);
                if (externalFileId <= 0)
                    throw new InvalidOperationException("Không ghi được metadata PDF vào ExternalFiles.");
                metadataSaved = true;

                updatedCount = await _resultXNBL.UpdateCBC12216FromExternal(
                    pidTablePatient, cbcItems, pdfName, pId, pMaBenhAn,
                    overwrite, markValid, userLogin);
                resultUpdated = true;

                _logger.LogInformation(
                    "CBC12216 import successful: patientId={PatientId}, matched={Matched}, updated={Updated}, file={File}",
                    pidTablePatient, cbcItems.Count, updatedCount, fileName);
                return Json(new
                {
                    success = true, mode = "CBC12216", fileName,
                    parsedCount = rawRows.Count, matchedCount = cbcItems.Count,
                    count = updatedCount,
                    message = $"Đã map {cbcItems.Count}/22 chỉ số CBC, cập nhật {updatedCount} kết quả."
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "CBC12216 import failed: patientId={PatientId}, metadataSaved={MetadataSaved}, updated={Updated}",
                    pidTablePatient, metadataSaved, resultUpdated);
                // Chưa ghi metadata và chưa sửa ResultXN: xóa bản copy thử.
                if (!metadataSaved && !resultUpdated && !string.IsNullOrEmpty(path))
                {
                    try { if (System.IO.File.Exists(path)) System.IO.File.Delete(path); }
                    catch (Exception cleanupError) { _logger.LogWarning(cleanupError, "CBC12216 temp cleanup failed"); }
                }
                var safeMessage = ex is InvalidDataException || ex is InvalidOperationException
                    ? ex.Message : "Lỗi xử lý PDF CBC. Kiểm tra log máy chủ.";
                return Json(new
                {
                    success = false, mode = "CBC12216", count = updatedCount,
                    fileSaved = metadataSaved,
                    message = metadataSaved
                        ? "File PDF đã được lưu, nhưng chưa hoàn tất cập nhật CBC: " + safeMessage
                        : safeMessage
                });
            }
        }

        /// <summary>
        /// Preview chỉ đọc, tuyệt đối không insert/update database và không lưu PDF công khai.
        /// Trả mapping đã kiểm tra đủ 22 mã, danh tính và dịch vụ 12216.
        /// </summary>
        [HttpPost]
        [Authorize]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> PreviewCBC12216Pdf(
            string pMaBenhAn, string pId, long pidTablePatient, IFormFile file)
        {
            if (pidTablePatient <= 0 || string.IsNullOrWhiteSpace(pMaBenhAn) ||
                string.IsNullOrWhiteSpace(pId))
                return Json(new { success = false, message = "Chưa chọn ca bệnh hợp lệ." });
            if (file == null || file.Length == 0 || file.Length > 10 * 1024 * 1024 ||
                !string.Equals(Path.GetExtension(file.FileName), ".pdf", StringComparison.OrdinalIgnoreCase))
                return Json(new { success = false, message = "Chỉ chấp nhận PDF tối đa 10MB." });

            string temporaryPath = Path.GetTempFileName();
            try
            {
                using (var stream = new FileStream(temporaryPath, FileMode.Create))
                    await file.CopyToAsync(stream);
                var rows = ExtractLabResultRowsFromPdf(
                    temporaryPath, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
                var mapped = Cbc12216ResultMapper.ParseStrict(rows);
                var pdfName = ExtractCBC12216PdfPatientName(temporaryPath);
                await _resultXNBL.UpdateCBC12216FromExternal(
                    pidTablePatient, mapped, pdfName, pId, pMaBenhAn,
                    overwrite: false, markValid: false, userId: GetUserLogin(), dryRun: true);
                // Preview trạng thái dùng chính giới hạn LIS; cờ H/L PDF không quyết định Status.
                var lisPreview = await _resultXNBL.PreviewCBC12216LisStatusesAsync(pidTablePatient, mapped);
                var statusById = lisPreview.ToDictionary(x => x.TestCodeId);
                return Json(new
                {
                    success = true,
                    matchedCount = mapped.Count,
                    statusSource = "TestCode.LowerLimit/HigherLimit (LIS; Nữ ưu tiên ngưỡng F)",
                    rows = mapped.Select(item => new
                    {
                        testCodeId = item.TestCodeId,
                        code = Cbc12216ResultMapper.ExpectedCodes[item.TestCodeId],
                        pdfName = item.PdfName,
                        value = item.Value,
                        lowerLimit = statusById[item.TestCodeId].LowerLimit,
                        higherLimit = statusById[item.TestCodeId].HigherLimit,
                        lisStatus = statusById[item.TestCodeId].LisStatus,
                        hasLimits = statusById[item.TestCodeId].HasLimits
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "CBC12216 preview failed patientId={PatientId}", pidTablePatient);
                return Json(new
                {
                    success = false,
                    message = ex is InvalidOperationException || ex is InvalidDataException
                        ? ex.Message : "Không thể kiểm tra PDF CBC. Xem log máy chủ."
                });
            }
            finally
            {
                try { System.IO.File.Delete(temporaryPath); }
                catch (Exception cleanupError) { _logger.LogWarning(cleanupError, "CBC preview temp cleanup failed"); }
            }
        }

        private static string ExtractCBC12216PdfPatientName(string path)
        {
            using (var pdf = PdfDocument.Open(path))
            {
                foreach (var page in pdf.GetPages())
                {
                    var lines = GroupLabPdfWords(page.GetWords()
                        .Where(w => !string.IsNullOrWhiteSpace(w.Text))
                        .Select(w => new ImportPdfWord
                        {
                            Text = w.Text,
                            Left = w.BoundingBox.Left,
                            Right = w.BoundingBox.Right,
                            MiddleY = (w.BoundingBox.Bottom + w.BoundingBox.Top) / 2.0
                        }));
                    foreach (var line in lines)
                    {
                        var text = string.Join(" ", line.Words.Select(w => w.Text));
                        var m = Regex.Match(text,
                            @"Họ\s+và\s+tên\s*:\s*(?<name>.+?)(?=\s+Năm\s+sinh\s*:|\s+Giới\s*:|$)",
                            RegexOptions.IgnoreCase);
                        if (m.Success) return m.Groups["name"].Value.Trim();
                    }
                }
            }
            return null;
        }
    }
}
