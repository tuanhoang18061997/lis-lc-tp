using Microsoft.AspNetCore.Mvc;

[Route("")]
public class CkController : Controller
{
    // ===== Upload ảnh (hiện tab Upload + hỗ trợ drag&drop) =====
    // CKEditor gọi: config.uploadUrl / filebrowserImageUploadUrl
    [HttpPost("ckupload/image")]
    [RequestSizeLimit(20_000_000)] // 20 MB - tùy bạn
    public async Task<IActionResult> UploadImage(IFormFile upload)
    {
        if (upload == null || upload.Length == 0)
            return Json(new { uploaded = 0, error = new { message = "No file" } });

        var ext = Path.GetExtension(upload.FileName).ToLowerInvariant();
        var allow = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        if (!allow.Contains(ext))
            return Json(new { uploaded = 0, error = new { message = "Invalid file type" } });

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var saveDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "ckeditor");
        Directory.CreateDirectory(saveDir);
        var savePath = Path.Combine(saveDir, fileName);
        using (var fs = System.IO.File.Create(savePath))
            await upload.CopyToAsync(fs);

        var url = Url.Content($"~/uploads/ckeditor/{fileName}");
        // format JSON tiêu chuẩn CKEditor (uploadimage)
        return Json(new { uploaded = 1, fileName, url });
    }

    // ===== Browse Server: duyệt ảnh đã upload và trả về cho CKEditor =====
    // CKEditor gọi: config.filebrowserImageBrowseUrl / filebrowserBrowseUrl
    [HttpGet("ckbrowse/images")]
    public IActionResult BrowseImages()
    {
        var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "ckeditor");
        Directory.CreateDirectory(dir);

        var urls = Directory
            .EnumerateFiles(dir)
            .Where(p => new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" }
                        .Contains(Path.GetExtension(p).ToLowerInvariant()))
            .Select(p => Url.Content("~/uploads/ckeditor/" + Path.GetFileName(p)))
            .ToList();

        return View("CkBrowseImages", urls);
    }

    // (nếu muốn duyệt cả file, có thể map sang BrowseImages)
    [HttpGet("ckbrowse/files")]
    public IActionResult BrowseFiles() => BrowseImages();
}
