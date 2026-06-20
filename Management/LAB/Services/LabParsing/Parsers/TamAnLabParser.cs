using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Management.Models;
using static System.Net.Mime.MediaTypeNames;

namespace Management.Services.LabParsing.Parsers
{
    public class TamAnLabParser : IExternalLabParser
    {
        // Giá trị này phải khớp với option trên UI (vd. <option value="TamAnLab">…</option>)
        public string Name => "TamAnLab";

        public List<ExternalLabItem> Parse(string pdfText)
        {
            var result = new List<ExternalLabItem>();
            if (string.IsNullOrWhiteSpace(pdfText)) return result;

            // 1) Chuẩn hoá để phá "dính cột"
            //string s = pdfText;
            //s = Regex.Replace(s, @"([()])", " $1 ");                  // thêm space quanh ( )
            //s = Regex.Replace(s, @"(?<=[A-Za-zµ%])(?=\d)", " ");      // Chữ→Số
            //s = Regex.Replace(s, @"(?<=\d)(?=[A-Za-zµ%])", " ");      // Số→Chữ
            ////s = Regex.Replace(s, @"(?<=[a-z])(?=[A-Z])", " ");        // a→A
            //s = Regex.Replace(s, @"\s+", " ").Trim();                 // gộp trắng

            // 2) Token hoá theo space
            var tokens = pdfText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int n = tokens.Length;

            // 3) Quét cửa sổ trượt để bắt mã xét nghiệm
            for (int i = 0; i < n; i++)
            {
                string code = null; int codeLen = 0;

                // --- Các mã 1 token
                if (Eq(tokens[i], "HBsAg")) { code = "HBsAg"; codeLen = 1; }
                else if (Eq(tokens[i], "CEA")) { code = "CEA"; codeLen = 1; }

                // --- Mã 2 token: CA 125, CA 15-3, Anti HBS
                else if (Eq(tokens[i], "CA") && i + 1 < n && (Eq(tokens[i + 1], "15-") && Eq(tokens[i + 2], "3")))
                { code = $"CA {tokens[i + 1]}" + " " + $"{tokens[i + 2]}"; codeLen = 3; }
                else if (Eq(tokens[i], "CA") && i + 1 < n && (Eq(tokens[i + 1], "125")))
                { code = $"CA {tokens[i + 1]}"; codeLen = 2; }
                else if (Eq(tokens[i], "Anti") && i + 1 < n && Eq(tokens[i + 1], "HBS"))
                { code = "Anti HBS"; codeLen = 2; }

                if (code == null) continue;

                // Bỏ qua STT ngay sau code nếu có (toàn số)
                int k = i + codeLen;
                if (k < n && IsDigits(tokens[k])) k++;

                int t = k;
                string qualifier = null, value = null, unit = null, refRange = null;

                // Check riêng code = HBsAg sẽ có Qualifier nằm ở vị trí t + 2
                if (code == "HBsAg")
                {
                    t = t + 1;

                    // Qualifier: Negative | Positive | Âm | Dương
                    if (t >= 0 && IsQualifier(tokens[t])) { qualifier = tokens[t]; t--; }

                    // Value (số): 16,30 | 1.50 | 0.41
                    if (t >= 0) { value = NormalizeNumber(tokens[t]); t--; }
                }
                else
                {

                    // Qualifier: Negative | Positive | Âm | Dương
                    if (t >= 0 && IsQualifier(tokens[t])) { qualifier = tokens[t]; t--; }

                    // Value (số): 16,30 | 1.50 | 0.41
                    if (t >= 0) { value = NormalizeNumber(tokens[t]); t--; }
                }



                //// Ref range: ... )  ...  (
                //if (t >= 0 && tokens[t] == ")")
                //{
                //    int end = t; t--;
                //    var parts = new List<string>();
                //    while (t >= 0 && tokens[t] != "(") { parts.Add(tokens[t]); t--; }
                //    if (t >= 0 && tokens[t] == "(")
                //    {
                //        parts.Reverse();
                //        refRange = string.Join(" ", parts);
                //        t--; // lùi qua '('
                //    }
                //}

                // Unit: token trước ref hoặc trước value (U/mL, ng/mL, COI, …)
                if (t >= 0) unit = tokens[t];

                // 5) Kết quả hợp lệ thì add
                if (!string.IsNullOrEmpty(code) && (!string.IsNullOrEmpty(value) || !string.IsNullOrEmpty(qualifier)))
                {
                    var finalValue = string.IsNullOrEmpty(qualifier) ? value : $"{value ?? ""} {qualifier}".Trim();
                    result.Add(new ExternalLabItem
                    {
                        Code = code,
                        Name = code,
                        Value = finalValue ?? "", // có thể là số | qualifier | số - qualifier
                        //Unit = unit ?? "",
                        //RefRange = refRange ?? ""
                    });
                }
                // tiếp tục quét; không cần nhảy i vì có thể có nhiều chỉ số trong cùng trang
            }

            return result;
        }

        // ================= helpers =================
        static bool Eq(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        static bool IsDigits(string s) => s.All(char.IsDigit);
        static bool IsNumberLike(string s) => Regex.IsMatch(s, @"^\d+([.,]\d+)?$");
        static string NormalizeNumber(string s) => (s ?? "").Replace(",", ".");
        static bool IsQualifier(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            var x = s.ToLowerInvariant();
            return x == "negative" || x == "positive" || x == "âm" || x == "dương";
        }
        private static readonly Dictionary<string, string> PartnerNameToInternalCode = new(StringComparer.OrdinalIgnoreCase)
        {
            // Map tên hiển thị trong PDF → Code xét nghiệm nội bộ
            // Điều chỉnh theo hệ thống của bạn
            ["HBsAg"] = "HBsAg",
            ["Anti HBS"] = "ANTI_HBS",  // hoặc "Anti-HBs"
            ["CA 15-3"] = "CA15-3",
            ["CA 125"] = "CA-125",
            ["CEA"] = "CEA"
        };
        // Một số chuẩn hoá tên để dễ map
        private static string NormalizeTestName(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return raw?.Trim() ?? "";
            var name = raw.Trim();

            // Fix các biến thể hay gặp ở đối tác
            name = name.Replace("CA 15- 3", "CA 15-3", StringComparison.OrdinalIgnoreCase);
            name = name.Replace("Anti HBS", "Anti HBS", StringComparison.OrdinalIgnoreCase); // giữ nguyên
            name = name.Replace("CA125", "CA 125", StringComparison.OrdinalIgnoreCase);

            return name;
        }
    }
}
