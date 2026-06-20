namespace Management.Services
{
    public interface IBarcodeService
    {
        /// <summary>
        /// Trả về data-url PNG barcode (CODE_128) cho chuỗi text
        /// </summary>
        string Code128PngDataUrl(string text, int height = 80, int scale = 3, int margin = 0);
    }
}
