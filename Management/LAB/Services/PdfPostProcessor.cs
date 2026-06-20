using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public interface IPdfPostProcessor
{
    /// <summary>
    /// “Nấu” PDF bằng Ghostscript. Trả về đường dẫn file out (có thể giống input nếu disabled/không cài).
    /// </summary>
    string FixPdfWithGhostscript(string inputPdfPath, string? outputPdfPath = null);
}

public sealed class PdfPostProcessor : IPdfPostProcessor
{
    private readonly ILogger<PdfPostProcessor> _log;
    private readonly IConfiguration _cfg;

    public PdfPostProcessor(ILogger<PdfPostProcessor> log, IConfiguration cfg)
    {
        _log = log;
        _cfg = cfg;
    }

    public string FixPdfWithGhostscript(string inputPdfPath, string? outputPdfPath = null)
    {
        if (string.IsNullOrWhiteSpace(inputPdfPath) || !File.Exists(inputPdfPath))
            throw new FileNotFoundException("Input PDF not found", inputPdfPath);

        var enabled = _cfg.GetValue<bool>("Ghostscript:Enabled");
        if (!enabled)
        {
            _log.LogInformation("Ghostscript disabled by config; return original PDF.");
            return inputPdfPath;
        }

        var exePath = GetGsPathByPlatform();
        if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
        {
            _log.LogWarning("Ghostscript executable not found at {Path}. Return original.", exePath);
            return inputPdfPath;
        }

        var argsBase = _cfg["Ghostscript:Args"] ??
                       "-dQUIET -dNOPAUSE -dBATCH -sDEVICE=pdfwrite -dPDFSETTINGS=/prepress -dEmbedAllFonts=true -dSubsetFonts=true";

        var inputFull = Path.GetFullPath(inputPdfPath);
        var outPath = outputPdfPath ?? Path.Combine(
            Path.GetDirectoryName(inputFull)!,
            Path.GetFileNameWithoutExtension(inputFull) + ".gs.pdf");

        // Prepare arguments (quote paths!)
        var args = $"{argsBase} -sOutputFile=\"{outPath}\" \"{inputFull}\"";

        var psi = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using var proc = Process.Start(psi);
            if (proc == null)
            {
                _log.LogWarning("Failed to start Ghostscript process; return original.");
                return inputPdfPath;
            }

            // Optional: read outputs to help debugging
            var stdOut = proc.StandardOutput.ReadToEndAsync();
            var stdErr = proc.StandardError.ReadToEndAsync();

            proc.WaitForExit();

            var outText = stdOut.Result;
            var errText = stdErr.Result;

            if (proc.ExitCode != 0)
            {
                _log.LogWarning("Ghostscript exit code {Code}. stderr: {Err}", proc.ExitCode, errText);
                // Nếu lỗi, trả về file gốc
                return inputPdfPath;
            }

            if (!File.Exists(outPath) || new FileInfo(outPath).Length == 0)
            {
                _log.LogWarning("Ghostscript produced no output; stderr: {Err}", errText);
                return inputPdfPath;
            }

            _log.LogInformation("Ghostscript OK. Output: {Out}", outPath);
            return outPath;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Ghostscript failed; return original PDF.");
            return inputPdfPath;
        }
    }

    private string GetGsPathByPlatform()
    {
        if (OperatingSystem.IsWindows())
            return _cfg["Ghostscript:ExePathWindows"] ?? @"C:\Program Files\gs\gswin64c.exe";
        if (OperatingSystem.IsLinux())
            return _cfg["Ghostscript:ExePathLinux"] ?? "/usr/bin/gs";
        if (OperatingSystem.IsMacOS())
            return _cfg["Ghostscript:ExePathMac"] ?? "/opt/homebrew/bin/gs";
        return "";
    }
}
