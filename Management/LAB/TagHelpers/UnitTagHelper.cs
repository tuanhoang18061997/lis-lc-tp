using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;

namespace LAB.TagHelpers
{
    [HtmlTargetElement("unit", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class UnitTagHelper : TagHelper
    {
        /// <summary>
        /// Chuỗi đơn vị đầu vào. Nếu không truyền, TagHelper sẽ lấy inner text.
        /// Ví dụ: "X10^9/L", "10^-3 mol/L"
        /// </summary>
        public string? Text { get; set; }

        /// <summary>
        /// true = xuất ra dạng Unicode superscript (10⁹/L).
        /// false (mặc định) = HTML với &lt;sup&gt; (10<sup>9</sup>/L).
        /// </summary>
        public bool Unicode { get; set; } = false;

        /// <summary>
        /// Chuẩn hoá 'X' -> '×' phía trước "10^" (thường gặp trong lab: X10^9/L).
        /// </summary>
        public bool NormalizeMultiply { get; set; } = true;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            // Lấy input: ưu tiên thuộc tính Text, fallback là inner content
            var input = Text;
            if (string.IsNullOrWhiteSpace(input))
            {
                var child = await output.GetChildContentAsync();
                input = child.GetContent();
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                // Không có nội dung -> không render gì
                output.SuppressOutput();
                return;
            }

            string result = Unicode ? ToUnicodeSup(input!, NormalizeMultiply)
                                    : ToHtmlSup(input!, NormalizeMultiply);

            // Không bọc thêm thẻ nào, render thẳng nội dung
            output.TagName = null;
            output.Content.SetHtmlContent(result);
        }

        // ====== Render HTML với <sup> ======
        private static string ToHtmlSup(string input, bool normalizeMultiply)
        {
            // Encode toàn bộ input để tránh chèn HTML ngoài ý muốn.
            // Sau đó chạy regex trên chuỗi đã encode.
            string s = HtmlEncoder.Default.Encode(input);

            if (normalizeMultiply)
            {
                // Chuẩn hoá ký hiệu nhân: X|x trước "10^" => ×
                // Làm trên chuỗi đã encode nên 'X' vẫn là 'X'
                s = Regex.Replace(s, @"\b[Xx](?=10\^)", "×");
            }

            // 10^exp -> 10<sup>exp</sup>  (exp có thể + hoặc -)
            s = Regex.Replace(s, @"10\^\s*([-+]?\d+)", "10<sup>$1</sup>");

            return s;
        }

        // ====== Render Unicode superscript ======
        private static readonly Dictionary<char, char> SupMap = new()
        {
            ['0']='⁰', ['1']='¹', ['2']='²', ['3']='³', ['4']='⁴',
            ['5']='⁵', ['6']='⁶', ['7']='⁷', ['8']='⁸', ['9']='⁹',
            ['+']='⁺', ['-']='⁻'
        };

        private static string ToUnicodeSup(string input, bool normalizeMultiply)
        {
            string s = input;

            if (normalizeMultiply)
            {
                s = Regex.Replace(s, @"\b[Xx](?=10\^)", "×");
            }

            // Thay "10^exp" -> "10" + superscript(exp)
            s = Regex.Replace(s, @"10\^\s*([-+]?\d+)", m =>
            {
                var exp = m.Groups[1].Value;
                var sb = new StringBuilder("10");
                foreach (var ch in exp)
                    sb.Append(SupMap.TryGetValue(ch, out var sup) ? sup : ch);
                return sb.ToString();
            });

            // Encode toàn bộ để an toàn khi render (Unicode mũ vẫn hiển thị đúng)
            return HtmlEncoder.Default.Encode(s);
        }
    }
}
