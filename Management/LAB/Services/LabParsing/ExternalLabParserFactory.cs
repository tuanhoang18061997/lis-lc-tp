using System.Collections.Generic;
using System.Linq;
using Management.Models;

namespace Management.Services.LabParsing
{
    public class ExternalLabParserFactory
    {
        private readonly Dictionary<string, IExternalLabParser> _byName;

        public ExternalLabParserFactory(IEnumerable<IExternalLabParser> parsers)
        {
            _byName = parsers.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
        }

        public List<string> GetRegisteredVendors() => _byName.Keys.OrderBy(x => x).ToList();

        public List<ExternalLabItem> ParseByName(string vendorName, string pdfText)
        {
            if (string.IsNullOrWhiteSpace(vendorName))
                throw new ArgumentException("Vui lòng chọn Vendor.", nameof(vendorName));

            if (!_byName.TryGetValue(vendorName, out var parser))
                throw new KeyNotFoundException($"Chưa đăng ký parser cho vendor '{vendorName}'.");

            return parser.Parse(pdfText) ?? new List<ExternalLabItem>();
        }

        // Tuỳ chọn: fallback nếu vendor null → dùng parser mặc định
        public List<ExternalLabItem> TryParseOrDefault(string vendorName, string pdfText, string defaultVendor = "TamAnLab")
        {
            if (!string.IsNullOrWhiteSpace(vendorName) && _byName.TryGetValue(vendorName, out var p))
                return p.Parse(pdfText) ?? new List<ExternalLabItem>();

            if (_byName.TryGetValue(defaultVendor, out var def))
                return def.Parse(pdfText) ?? new List<ExternalLabItem>();

            return new List<ExternalLabItem>();
        }
    }
}
