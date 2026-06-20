using HtmlAgilityPack;
using Management.Models;
using SelectPdf;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using static System.Net.Mime.MediaTypeNames;
using Image = System.Drawing.Image;

namespace Management.BL
{
    public class ToolBL
    {
        private readonly LABContext _db;
        private readonly SettingBL _settingBL;
        public ToolBL(LABContext db, SettingBL settingBL)
        {
            _db = db;
            _settingBL = settingBL;
        }

        public static DateTime Get_DateNow()
        {
            return DateTime.Now;
        }

        public async Task<string> ExportPdf_Result(string folder, string file, string headerHtml, string contentHtml, string footerHtml)
        {
            try
            {
                var baseUrl = await _settingBL.GetSetting(SettingBL.BaseUrl);
                if (string.IsNullOrEmpty(baseUrl)) return null;
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                HtmlToPdf converter = new HtmlToPdf();
                converter.Options.PdfPageSize = PdfPageSize.A4;
                converter.Options.MarginTop = 20;
                converter.Options.MarginLeft = 30;
                converter.Options.MarginRight = 20;
                converter.Options.MarginBottom = 10;   // <--- thêm dòng này

                var headerPdf = new PdfHtmlSection(headerHtml, baseUrl);
                headerPdf.AutoFitHeight = HtmlToPdfPageFitMode.AutoFit;
                converter.Header.Add(headerPdf);
                converter.Options.DisplayHeader = true;
                converter.Header.Height = 140;

                // DÙNG FOOTER CHỈ ĐỂ HIỂN THỊ SỐ TRANG
                converter.Options.DisplayFooter = true;   // <--- bật footer
                converter.Footer.Height = 25;            // chiều cao footer
                                                         // Không sử dụng footer của PDF nữa
                                                         //converter.Options.DisplayFooter = false;

                // Page numbering ở center (nếu cần)
                var pageText = new PdfTextSection(0, 0,
                    "Trang {page_number}/{total_pages}",
                    new System.Drawing.Font("Arial", 10));

                pageText.HorizontalAlign = PdfTextHorizontalAlign.Center;
                pageText.VerticalAlign = PdfTextVerticalAlign.Middle; // không cần set Y
                converter.Footer.Add(pageText);

                // Thêm footerHtml vào cuối content với CSS để đảm bảo nó ở trang cuối
                var finalContent = contentHtml;
                if (!string.IsNullOrEmpty(footerHtml))
                {
                    finalContent += $@"
                        <div style='page-break-before: auto; margin-top: 30px; page-break-inside: avoid; position: relative; clear: both;'>
                            {footerHtml}
                        </div>";
                }

                // Convert content 
                PdfDocument doc = converter.ConvertHtmlString(finalContent, baseUrl);
                doc.Save(file);
                doc.Close();
                return Convert.ToBase64String(File.ReadAllBytes(file));
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<string> ExportPdf_Result_XN(string folder, string file, string headerHtml, string contentHtml, string footerHtml)
        {
            try
            {
                var baseUrl = await _settingBL.GetSetting(SettingBL.BaseUrl);
                if (string.IsNullOrEmpty(baseUrl)) return null;
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                HtmlToPdf converter = new HtmlToPdf();
                converter.Options.PdfPageSize = PdfPageSize.A4;
                converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;

                converter.Options.MarginTop = 20;
                converter.Options.MarginLeft = 30;
                converter.Options.MarginRight = 20;

                // QUAN TRỌNG:
                // chừa khoảng trống cuối mỗi trang để chứa footer
                converter.Options.MarginBottom = 30;

                // ================= HEADER =================
                converter.Options.DisplayHeader = true;
                converter.Header.Height = 140;
                converter.Header.DisplayOnFirstPage = true;
                converter.Header.DisplayOnOddPages = true;
                converter.Header.DisplayOnEvenPages = true;

                var headerPdf = new PdfHtmlSection(headerHtml, baseUrl);
                headerPdf.AutoFitHeight = HtmlToPdfPageFitMode.AutoFit;
                converter.Header.Add(headerPdf);

                // ================= FOOTER =================
                converter.Options.DisplayFooter = true;

                // tăng height để không bị mất chữ ký + tên người ký
                converter.Footer.Height = 110;
                converter.Footer.DisplayOnFirstPage = true;
                converter.Footer.DisplayOnOddPages = true;
                converter.Footer.DisplayOnEvenPages = true;

                if (!string.IsNullOrEmpty(footerHtml))
                {
                    var footerPdf = new PdfHtmlSection(footerHtml, baseUrl);
                    footerPdf.AutoFitHeight = HtmlToPdfPageFitMode.AutoFit;
                    converter.Footer.Add(footerPdf);
                }

                //Số trang
                var pageText = new PdfTextSection(
                    0,
                    95,
                    "Trang {page_number}/{total_pages}",
                    new System.Drawing.Font("Arial", 9)
                );

                pageText.HorizontalAlign = PdfTextHorizontalAlign.Center;
                pageText.VerticalAlign = PdfTextVerticalAlign.Top;
                converter.Footer.Add(pageText);

                // KHÔNG nối footerHtml vào content nữa
                PdfDocument doc = converter.ConvertHtmlString(contentHtml, baseUrl);

                doc.Save(file);
                doc.Close();
                return Convert.ToBase64String(File.ReadAllBytes(file));
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<string> ExportPdf_Result_No_Footer_Header(string folder, string file, string contentHtml)
        {
            try
            {
                var baseUrl = await _settingBL.GetSetting(SettingBL.BaseUrl);
                if (string.IsNullOrEmpty(baseUrl)) return null;
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                HtmlToPdf converter = new HtmlToPdf();
                converter.Options.PdfPageSize = PdfPageSize.A4;
                converter.Options.MarginTop = 20;
                converter.Options.MarginLeft = 30;
                converter.Options.MarginRight = 20;
                converter.Options.MarginBottom = 10;
                                // DÙNG FOOTER CHỈ ĐỂ HIỂN THỊ SỐ TRANG
                converter.Options.DisplayFooter = true;   // <--- bật footer
                converter.Footer.Height = 40;            // chiều cao footer
                                                         // Không sử dụng footer của PDF nữa
                                                         //converter.Options.DisplayFooter = false;

                // Page numbering ở center (nếu cần)
                var pageText = new PdfTextSection(0, 0,
                    "Trang {page_number}/{total_pages}",
                    new System.Drawing.Font("Arial", 10));

                pageText.HorizontalAlign = PdfTextHorizontalAlign.Center;
                pageText.VerticalAlign = PdfTextVerticalAlign.Middle; // không cần set Y
                converter.Footer.Add(pageText);
                PdfDocument doc = converter.ConvertHtmlString(contentHtml, baseUrl);

                doc.Save(file);
                doc.Close();
                return Convert.ToBase64String(File.ReadAllBytes(file));
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<string> ExportPdf_Report(string folder, string file, string headerHtml, string contentHtml, string footerHtml)
        {
            try
            {
                var baseUrl = await _settingBL.GetSetting("BaseUrl");
                if (string.IsNullOrEmpty(baseUrl)) return null;
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                HtmlToPdf converter = new HtmlToPdf();
                converter.Options.PdfPageSize = PdfPageSize.A4;
                converter.Options.MarginTop = 20;
                converter.Options.MarginLeft = 30;
                converter.Options.MarginRight = 20;

                var headerPdf = new PdfHtmlSection(headerHtml, baseUrl);
                headerPdf.AutoFitHeight = HtmlToPdfPageFitMode.AutoFit;
                converter.Header.Add(headerPdf);
                converter.Options.DisplayHeader = true;
                converter.Header.Height = 110;

                var footerPdf = new PdfHtmlSection(footerHtml, baseUrl);
                footerPdf.AutoFitHeight = HtmlToPdfPageFitMode.AutoFit;
                converter.Footer.Add(footerPdf);
                converter.Options.DisplayFooter = true;
                converter.Footer.Height = 135;

                PdfDocument doc = converter.ConvertHtmlString(contentHtml, baseUrl);

                doc.Save(file);
                doc.Close();
                return Convert.ToBase64String(File.ReadAllBytes(file));
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<string> ExportPdf_Report_Landscape(string folder, string file, string headerHtml, string contentHtml, string footerHtml)
        {
            try
            {
                var baseUrl = await _settingBL.GetSetting(SettingBL.BaseUrl);
                if (string.IsNullOrEmpty(baseUrl)) return null;
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                HtmlToPdf converter = new HtmlToPdf();
                converter.Options.PdfPageSize = PdfPageSize.A4;
                converter.Options.MarginTop = 5;
                converter.Options.MarginLeft = 30;
                converter.Options.PdfPageOrientation = PdfPageOrientation.Landscape;

                var headerPdf = new PdfHtmlSection(headerHtml, baseUrl);
                headerPdf.AutoFitHeight = HtmlToPdfPageFitMode.AutoFit;
                converter.Header.Add(headerPdf);
                converter.Options.DisplayHeader = true;
                converter.Header.Height = 110;

                var footerPdf = new PdfHtmlSection(footerHtml, baseUrl);
                footerPdf.AutoFitHeight = HtmlToPdfPageFitMode.AutoFit;
                converter.Footer.Add(footerPdf);
                converter.Options.DisplayFooter = false;
                converter.Footer.Height = 123;

                // Đánh số trang => Nhưng không hiển thị được ở cuối trang
                //var totalPage = converter.Footer.TotalPagesOffset;
                //var pageNumber = converter.Footer.FirstPageNumber;
                //var text = new PdfTextSection(pageNumber, totalPage, "Trang {page_number}/{total_pages}  ", new System.Drawing.Font("Arial", 8));
                //text.HorizontalAlign = PdfTextHorizontalAlign.Center;
                //text.VerticalAlign = PdfTextVerticalAlign.Bottom;
                //converter.Footer.Add(text);

                PdfDocument doc = converter.ConvertHtmlString(contentHtml, baseUrl);

                doc.Save(file);
                doc.Close();
                return Convert.ToBase64String(File.ReadAllBytes(file));
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<string> ExportPdf_Result_Landscape_OnePage_FromFiles(
            string folder,
            string file,
            string headerHtml,
            string contentResultHtml,
            string contentImageHtml,
            string footerHtml,
            string templatePath /* ví dụ: Path.Combine(_env.ContentRootPath, "PdfTemplates", "Landscape2ColRaw.html") */
        )
        {
        try
        {
            var baseUrl = await _settingBL.GetSetting(SettingBL.BaseUrl);
            if (string.IsNullOrEmpty(baseUrl)) return null;

            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            if (!System.IO.File.Exists(templatePath)) throw new FileNotFoundException("Template not found", templatePath);

            // 1) Load template và thay thế placeholder (không dựng HTML/CSS trong code)
            var template = System.IO.File.ReadAllText(templatePath);
            var html = template
                .Replace("{{BASE_URL}}", baseUrl)
                .Replace("{{HEADER}}", headerHtml ?? string.Empty)
                .Replace("{{CONTENT_RESULT}}", contentResultHtml ?? string.Empty)
                .Replace("{{CONTENT_IMAGE}}", contentImageHtml ?? string.Empty)
                .Replace("{{FOOTER}}", footerHtml ?? string.Empty);

            // 2) Convert HTML -> PDF: A4 Landscape + ép 1 trang (ShrinkOnly)
            var converter = new HtmlToPdf
            {
                Options =
            {
                PdfPageSize = PdfPageSize.A4,
                PdfPageOrientation = PdfPageOrientation.Landscape,

                // Khớp @page margin
                MarginTop = 10,
                MarginBottom = 10,
                MarginLeft = 10,
                MarginRight = 10,

                // Co nội dung để vừa 1 trang (không sinh trang 2)
                AutoFitWidth = HtmlToPdfPageFitMode.ShrinkOnly,
                AutoFitHeight =HtmlToPdfPageFitMode.NoAdjustment,

                // Không dùng Header/Footer của PDF (vì chiếm full-width)
                DisplayHeader = false,
                DisplayFooter = false
            }
                };
                
            var pdf = converter.ConvertHtmlString(html, baseUrl);
            pdf.Save(file);
            pdf.Close();

            return Convert.ToBase64String(System.IO.File.ReadAllBytes(file));
        }
        catch
        {
            return null;
        }
    }


    public Image Base64ToImage(string base64String)
        {
            base64String = base64String.Replace("data:image/png;base64,", "");
            byte[] imageBytes = Convert.FromBase64String(base64String);
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                Image image = Image.FromStream(ms, true);
                return image;
            }
        }

        public bool SaveImage(string folder, string file, Image image)
        {
            try
            {
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                image.Save(file);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteImage(string file)
        {
            try
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> ExportPdf_Result_FooterLastPageOnly(string folder, string file, string headerHtml, string contentHtml, string footerHtml)
        {
            try
            {
                var baseUrl = await _settingBL.GetSetting(SettingBL.BaseUrl);
                if (string.IsNullOrEmpty(baseUrl)) return null;
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                HtmlToPdf converter = new HtmlToPdf();
                converter.Options.PdfPageSize = PdfPageSize.A4;
                converter.Options.MarginTop = 20;
                converter.Options.MarginLeft = 30;
                converter.Options.MarginRight = 20;

                // Header
                var headerPdf = new PdfHtmlSection(headerHtml, baseUrl);
                headerPdf.AutoFitHeight = HtmlToPdfPageFitMode.AutoFit;
                converter.Header.Add(headerPdf);
                converter.Options.DisplayHeader = true;
                converter.Header.Height = 170;

                //var footerPdf = new PdfHtmlSection(footerHtml, baseUrl);
                //footerPdf.AutoFitHeight = HtmlToPdfPageFitMode.AutoFit;
                //converter.Footer.Add(footerPdf);
                //converter.Options.DisplayFooter = true;
                //converter.Footer.Height = 105;

                // Page numbering (không có footer HTML)
                var totalPage = converter.Footer.TotalPagesOffset;
                var pageNumber = converter.Footer.FirstPageNumber;
                var pageText = new PdfTextSection(pageNumber, totalPage, "Trang {page_number}/{total_pages}", new System.Drawing.Font("Arial", 9));
                pageText.HorizontalAlign = PdfTextHorizontalAlign.Center;
                pageText.VerticalAlign = PdfTextVerticalAlign.Bottom;
                pageText.Y = 30;
                converter.Footer.Add(pageText);
                converter.Footer.Height = 120;

                // Ghép footer vào cuối content với CSS đặc biệt
                var finalContent = contentHtml;
                if (!string.IsNullOrEmpty(footerHtml))
                {
                    finalContent += $@"
                        <div style='page-break-before: auto; margin-top: 40px; page-break-inside: avoid; 
                                    position: relative; clear: both;'>
                            {footerHtml}
                        </div>";
                }

                PdfDocument doc = converter.ConvertHtmlString(finalContent, baseUrl);
                doc.Save(file);
                doc.Close();
                return Convert.ToBase64String(File.ReadAllBytes(file));
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        string CleanHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return html ?? "";
            return html
                .Replace("\u00A0", " ")     // NBSP -> space
                .Replace("\u00AD", "");     // Soft Hyphen -> remove
        }
    }
}
