using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Management.Models;

namespace Management.Services.LabParsing
{
    /// <summary>
    /// Ánh xạ RIÊNG PDF công thức máu vào 22 TestCodeId thuộc ServiceId=12216.
    /// Tuyệt đối không thay đổi mã TestCode trong database, không map TestHead=706.
    /// </summary>
    public static class Cbc12216ResultMapper
    {
        public const long ServiceId = 12216;
        public static readonly IReadOnlyDictionary<long, string> ExpectedCodes =
            new Dictionary<long, string>
            {
                [652] = "WBC_LC",   [653] = "NEUT#_LC", [654] = "LYM#_LC",
                [655] = "MONO#_LC", [656] = "EOS#_LC",  [657] = "BASO#_LC",
                [658] = "NEUT%_LC", [659] = "LYM%_LC",  [660] = "MONO%_LC",
                [661] = "EOS%_LC",  [662] = "BASO%_LC", [663] = "RBC_LC",
                [664] = "HGB_LC",   [665] = "HCT_LC",   [666] = "MCV_LC",
                [667] = "MCH_LC",   [668] = "MCHC_LC",  [669] = "RDW_LC",
                [670] = "PLT_LC",   [671] = "MPV_LC",   [672] = "PDW_LC",
                [673] = "PCT_LC"
            };

        // RDW-CV trên PDF đích danh tương ứng với RDW_LC của dịch vụ này.
        private static readonly IReadOnlyDictionary<string, long> PdfNameToId =
            new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase)
            {
                ["WBC"] = 652,   ["NEUT#"] = 653, ["LYM#"] = 654,
                ["MONO#"] = 655, ["EOS#"] = 656,  ["BASO#"] = 657,
                ["NEUT%"] = 658, ["LYM%"] = 659,  ["MONO%"] = 660,
                ["EOS%"] = 661,  ["BASO%"] = 662, ["RBC"] = 663,
                ["HGB"] = 664,   ["HCT"] = 665,   ["MCV"] = 666,
                ["MCH"] = 667,   ["MCHC"] = 668,  ["RDW-CV"] = 669,
                ["PLT"] = 670,   ["MPV"] = 671,   ["PDW"] = 672,
                ["PCT"] = 673
            };

        private static readonly Regex NumericCell = new Regex(
            @"^\s*(?<value>\d+(?:[.,]\d+)?)\s*(?:\((?<flag>H|L|CV)\))?\s*$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public static List<Cbc12216ParsedResult> ParseStrict(IEnumerable<ExternalLabItem> tableItems)
        {
            if (tableItems == null)
                throw new InvalidDataException("Không trích xuất được bảng kết quả CBC.");

            var mapped = new Dictionary<long, List<Cbc12216ParsedResult>>();
            foreach (var item in tableItems.Where(x => x != null))
            {
                var label = item.Name ?? item.Code ?? string.Empty;
                var m = Regex.Match(label, @"^\s*(?<key>[A-Za-z][A-Za-z0-9#%\-]*)");
                if (!m.Success || !PdfNameToId.TryGetValue(m.Groups["key"].Value, out var id))
                    continue; // IG, NRBC, RDW-SD, MacroR, MicroR, P-LCR không thuộc 22 mã

                var cell = NumericCell.Match(item.Value ?? string.Empty);
                if (!cell.Success || !decimal.TryParse(
                    cell.Groups["value"].Value.Replace(',', '.'), NumberStyles.Number,
                    CultureInfo.InvariantCulture, out var number))
                {
                    throw new InvalidDataException(
                        $"Giá trị CBC không hợp lệ ở '{m.Groups["key"].Value}': '{item.Value}'.");
                }

                var flag = cell.Groups["flag"].Value.ToUpperInvariant();
                var parsed = new Cbc12216ParsedResult
                {
                    TestCodeId = id,
                    PdfName = m.Groups["key"].Value,
                    Value = cell.Groups["value"].Value.Replace(',', '.'),
                    Number = number,
                    PdfStatus = flag == "L" ? 1 : flag == "H" ? 2 : (int?)null
                };
                if (!mapped.TryGetValue(id, out var entries))
                    mapped[id] = entries = new List<Cbc12216ParsedResult>();
                entries.Add(parsed);
            }

            // Fail closed: thiếu 1 chỉ số hoặc có 2 kết quả không giống nhau đều không ghi DB.
            var missing = ExpectedCodes.Keys.Except(mapped.Keys).ToList();
            if (missing.Count != 0)
                throw new InvalidDataException("PDF CBC thiếu chỉ số: " +
                    string.Join(", ", missing.Select(id => ExpectedCodes[id])) +
                    ". Kiểm tra đủ cả 2 trang PDF.");

            var result = new List<Cbc12216ParsedResult>();
            foreach (var id in ExpectedCodes.Keys.OrderBy(x => x))
            {
                var distinct = mapped[id]
                    .GroupBy(x => x.Value + "|" + (x.PdfStatus?.ToString() ?? ""),
                        StringComparer.OrdinalIgnoreCase)
                    .ToList();
                if (distinct.Count != 1)
                    throw new InvalidDataException("Có nhiều giá trị khác nhau cho " + ExpectedCodes[id]);
                result.Add(distinct[0].First());
            }
            return result;
        }
    }

    public sealed class Cbc12216ParsedResult
    {
        public long TestCodeId { get; set; }
        public string PdfName { get; set; }
        public string Value { get; set; }
        public decimal Number { get; set; }
        public int? PdfStatus { get; set; }
    }
}
