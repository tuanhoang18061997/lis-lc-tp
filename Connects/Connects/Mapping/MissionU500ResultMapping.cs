using System;
using System.Collections.Generic;
using System.Linq;

namespace Connects.Mapping
{
    public static class MissionU500ResultMapping
    {
        public class MissionU500ResultMap
        {
            /// <summary>
            /// Mã chỉ số máy trả về. Ví dụ: LEU, NIT, URO, PRO, pH, BLO, SG, KET, BIL, GLU
            /// </summary>
            public string Parameter { get; set; }

            /// <summary>
            /// Giá trị raw máy trả về để map. Ví dụ: -, +, +-, 1+, 2+, 3+, 6.0, 1.005
            /// </summary>
            public string RawCode { get; set; }

            /// <summary>
            /// Giá trị conventional sau khi map. Ví dụ: Negative, Positive, 15, 0.2, 6.0, 1.005
            /// </summary>
            public string Conventional { get; set; }

            /// <summary>
            /// Nếu sau này cần dùng thêm SI thì để sẵn
            /// </summary>
            public string SI { get; set; }
        }

        private static readonly List<MissionU500ResultMap> _maps = new List<MissionU500ResultMap>
        {
            // LEU
            new MissionU500ResultMap { Parameter = "LEU", RawCode = "-",  Conventional = "Negative", SI = "Negative" },
            new MissionU500ResultMap { Parameter = "LEU", RawCode = "+-", Conventional = "Negative", SI = "15" },
            new MissionU500ResultMap { Parameter = "LEU", RawCode = "1+", Conventional = "70",       SI = "70" },
            new MissionU500ResultMap { Parameter = "LEU", RawCode = "2+", Conventional = "125",      SI = "125" },
            new MissionU500ResultMap { Parameter = "LEU", RawCode = "3+", Conventional = "500",      SI = "500" },

            // NIT
            new MissionU500ResultMap { Parameter = "NIT", RawCode = "-", Conventional = "Negative", SI = "Negative" },
            new MissionU500ResultMap { Parameter = "NIT", RawCode = "+", Conventional = "Positive", SI = "Positive" },

            // URO
            new MissionU500ResultMap { Parameter = "URO", RawCode = "-",  Conventional = "0.2",         SI = "3.5" },
            new MissionU500ResultMap { Parameter = "URO", RawCode = "+-", Conventional = "Negative",    SI = "17" },
            new MissionU500ResultMap { Parameter = "URO", RawCode = "1+", Conventional = "2",           SI = "35" },
            new MissionU500ResultMap { Parameter = "URO", RawCode = "2+", Conventional = "4",           SI = "70" },
            new MissionU500ResultMap { Parameter = "URO", RawCode = "3+", Conventional = "8",           SI = "140" },

            // PRO
            new MissionU500ResultMap { Parameter = "PRO", RawCode = "-",  Conventional = "Negative",    SI = "Negative" },
            new MissionU500ResultMap { Parameter = "PRO", RawCode = "+-", Conventional = "Negative",    SI = "0.15" },
            new MissionU500ResultMap { Parameter = "PRO", RawCode = "1+", Conventional = "30",          SI = "0.3" },
            new MissionU500ResultMap { Parameter = "PRO", RawCode = "2+", Conventional = "100",         SI = "1.0" },
            new MissionU500ResultMap { Parameter = "PRO", RawCode = "3+", Conventional = "300",         SI = "3.0" },

            // pH
            new MissionU500ResultMap { Parameter = "PH", RawCode = "5.0", Conventional = "5.0", SI = "5.0" },
            new MissionU500ResultMap { Parameter = "PH", RawCode = "5.5", Conventional = "5.5", SI = "5.5" },
            new MissionU500ResultMap { Parameter = "PH", RawCode = "6.0", Conventional = "6.0", SI = "6.0" },
            new MissionU500ResultMap { Parameter = "PH", RawCode = "6.5", Conventional = "6.5", SI = "6.5" },
            new MissionU500ResultMap { Parameter = "PH", RawCode = "7.0", Conventional = "7.0", SI = "7.0" },
            new MissionU500ResultMap { Parameter = "PH", RawCode = "7.5", Conventional = "7.5", SI = "7.5" },
            new MissionU500ResultMap { Parameter = "PH", RawCode = "8.0", Conventional = "8.0", SI = "8.0" },
            new MissionU500ResultMap { Parameter = "PH", RawCode = "8.5", Conventional = "8.5", SI = "8.5" },

            // BLO
            new MissionU500ResultMap { Parameter = "BLO", RawCode = "-",  Conventional = "Negative", SI = "Negative" },
            new MissionU500ResultMap { Parameter = "BLO", RawCode = "+-", Conventional = "Negative", SI = "10" },
            new MissionU500ResultMap { Parameter = "BLO", RawCode = "1+", Conventional = "25",       SI = "25" },
            new MissionU500ResultMap { Parameter = "BLO", RawCode = "2+", Conventional = "80",       SI = "80" },
            new MissionU500ResultMap { Parameter = "BLO", RawCode = "3+", Conventional = "200",      SI = "200" },

            // SG
            new MissionU500ResultMap { Parameter = "SG", RawCode = "1.000", Conventional = "1.000", SI = "1.000" },
            new MissionU500ResultMap { Parameter = "SG", RawCode = "1.005", Conventional = "1.005", SI = "1.005" },
            new MissionU500ResultMap { Parameter = "SG", RawCode = "1.010", Conventional = "1.010", SI = "1.010" },
            new MissionU500ResultMap { Parameter = "SG", RawCode = "1.015", Conventional = "1.015", SI = "1.015" },
            new MissionU500ResultMap { Parameter = "SG", RawCode = "1.020", Conventional = "1.020", SI = "1.020" },
            new MissionU500ResultMap { Parameter = "SG", RawCode = "1.025", Conventional = "1.025", SI = "1.025" },
            new MissionU500ResultMap { Parameter = "SG", RawCode = "1.030", Conventional = "1.030", SI = "1.030" },

            // KET
            new MissionU500ResultMap { Parameter = "KET", RawCode = "-",  Conventional = "Negative", SI = "Negative" },
            new MissionU500ResultMap { Parameter = "KET", RawCode = "+-", Conventional = "Negative", SI = "0.5" },
            new MissionU500ResultMap { Parameter = "KET", RawCode = "1+", Conventional = "15",       SI = "1.5" },
            new MissionU500ResultMap { Parameter = "KET", RawCode = "2+", Conventional = "40",       SI = "4.0" },
            new MissionU500ResultMap { Parameter = "KET", RawCode = "3+", Conventional = "80",       SI = "8.0" },

            // BIL
            new MissionU500ResultMap { Parameter = "BIL", RawCode = "-",  Conventional = "Negative", SI = "Negative" },
            new MissionU500ResultMap { Parameter = "BIL", RawCode = "1+", Conventional = "1",        SI = "17" },
            new MissionU500ResultMap { Parameter = "BIL", RawCode = "2+", Conventional = "2",        SI = "35" },
            new MissionU500ResultMap { Parameter = "BIL", RawCode = "3+", Conventional = "4",        SI = "70" },

            // GLU
            new MissionU500ResultMap { Parameter = "GLU", RawCode = "-",  Conventional = "Negative", SI = "Negative" },
            new MissionU500ResultMap { Parameter = "GLU", RawCode = "+-", Conventional = "Negative", SI = "5.5" },
            new MissionU500ResultMap { Parameter = "GLU", RawCode = "1+", Conventional = "250",      SI = "14" },
            new MissionU500ResultMap { Parameter = "GLU", RawCode = "2+", Conventional = "500",      SI = "28" },
            new MissionU500ResultMap { Parameter = "GLU", RawCode = "3+", Conventional = "1000",     SI = "55" },
            new MissionU500ResultMap { Parameter = "GLU", RawCode = "4+", Conventional = "2000",     SI = "111" },
        };

        public static List<MissionU500ResultMap> GetAll()
        {
            return _maps;
        }

        //public static MissionU500ResultMap Find(string parameter, string rawCode)
        //{
        //    parameter = NormalizeParameter(parameter);
        //    rawCode = NormalizeRawCode(rawCode);

        //    return _maps.FirstOrDefault(x =>
        //        NormalizeParameter(x.Parameter) == parameter &&
        //        NormalizeRawCode(x.RawCode) == rawCode);
        //}

        public static MissionU500ResultMap Find(string parameter, string rawCode)
        {
            parameter = NormalizeParameter(parameter);
            rawCode = NormalizeRawCode(rawCode);

            var map = _maps.FirstOrDefault(x =>
                NormalizeParameter(x.Parameter) == parameter &&
                NormalizeRawCode(x.RawCode) == rawCode);

            if (map == null)
                return null;

            if (rawCode == "+-")
            {
                return new MissionU500ResultMap
                {
                    Parameter = map.Parameter,
                    RawCode = map.RawCode,
                    Conventional = "Negative",
                    SI = "Negative"
                };
            }

            return map;
        }

        public static string NormalizeParameter(string parameter)
        {
            return (parameter ?? string.Empty)
                .Trim()
                .TrimStart('*')
                .ToUpperInvariant();
        }

        public static string NormalizeRawCode(string rawCode)
        {
            rawCode = (rawCode ?? string.Empty).Trim().ToUpperInvariant();
            rawCode = rawCode.Replace("`", string.Empty);
            rawCode = rawCode.Replace(" ", string.Empty);

            if (rawCode == "NEG")
                return "-";

            if (rawCode == "POS")
                return "+";

            return rawCode;
        }
    }
}