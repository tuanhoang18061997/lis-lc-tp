using System;
using System.IO;
using ZXing;
using ZXing.Common;
using SkiaSharp;
using Management.Services;

public class BarcodeService : IBarcodeService
{
    public string Code128PngDataUrl(string text, int height = 80, int scale = 3, int margin = 0)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var options = new EncodingOptions
        {
            Height = height * scale,  // tăng DPI
            Margin = margin,
            PureBarcode = true
        };

        var writer = new BarcodeWriterPixelData
        {
            Format = BarcodeFormat.CODE_128,
            Options = options
        };

        var pixelData = writer.Write(text);

        // Convert pixelData sang SKBitmap
        using var bitmap = new SKBitmap(pixelData.Width, pixelData.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
        unsafe
        {
            fixed (byte* ptr = pixelData.Pixels)
            {
                bitmap.InstallPixels(bitmap.Info, (IntPtr)ptr, bitmap.Info.RowBytes);
            }
        }

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        var base64 = Convert.ToBase64String(data.ToArray());

        return "data:image/png;base64," + base64;
    }
}
