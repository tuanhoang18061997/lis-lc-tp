using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using log4net;
using Connects.BL;
using Connects.Models;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace Connects.Devices
{
    public class ClinitekStatus
    {
        public static string inputData = string.Empty;
        private static readonly ILog log4net = LogManager.GetLogger(typeof(ClinitekStatus).Name);
        public static string sidForUrine = string.Empty;
        private static bool isWriteFile = false;

        public static void AnalysisPatient(SerialPort sp, Connect connect, string _sidForUrine)
        {
            try
            {
                sidForUrine = _sidForUrine;
                string input = sp.ReadExisting();

                if (isWriteFile)
                    Folder_File_Tool.Create_Write_File(typeof(ClinitekStatus).Name, true, input);

                inputData += input;
                byte[] ASCIIValues = Encoding.ASCII.GetBytes(inputData);

                // Check nếu có 12 dấu phẩy liên tục
                if (HasConsecutiveCommas(ASCIIValues, 12))
                {
                    ProcessResult(inputData, connect);
                    inputData = string.Empty;
                }
            }
            catch (Exception ex)
            {
                log4net.Error(ex);
                inputData = string.Empty;
            }
        }

        private static bool HasConsecutiveCommas(byte[] values, int count)
        {
            int consecutive = 0;
            foreach (var val in values)
            {
                if (val == 44) // ASCII của dấu phẩy ','
                {
                    consecutive++;
                    if (consecutive >= count)
                        return true;
                }
                else
                {
                    consecutive = 0; // reset nếu gặp ký tự khác
                }
            }
            return false;
        }

        public static void ProcessResult(string inputData, Connect connect)
        {
            var insertTime = DateTimeServer.Get_DateServerByEntity();
            var lstMap = MapBL.Get_ListMapByDeviceID(connect.DeviceId);
            if (lstMap == null || lstMap.Count == 0) return;
            // 1) Chia record theo 2 dấu phẩy liên tiếp
            var arrayInput = inputData
                .Split(new[] { ",," }, StringSplitOptions.None)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();

            if (arrayInput.Length == 0) return;

            // 2) Lấy seq từ dòng đầu tiên: token đầu tiên sau khi split bằng dấu phẩy đơn
            string seq = arrayInput[1]
                .Split(new[] { ',' }, StringSplitOptions.None)
                .FirstOrDefault()?
                .Trim() ?? string.Empty;
            // Tạo từ điển tra cứu theo TestcodeIn (không phân biệt hoa/thường)
            var mapByCode = lstMap
                .Where(m => !string.IsNullOrWhiteSpace(m.TestcodeIn))
                .ToDictionary(
                    m => m.TestcodeIn.Trim().ToUpperInvariant(),
                    m => m
                );

            foreach (var line in arrayInput)
            {
                try
                {
                    var parts = line.Split(new[] { ',' }, StringSplitOptions.None);
                    if (parts.Length == 0) continue;

                    for (int i = 0; i < parts.Length; i++)
                    {
                        var token = (parts[i] ?? string.Empty).Trim();
                        if (string.IsNullOrEmpty(token)) continue;

                        var key = token.ToUpperInvariant();

                        // Tìm map chính xác theo TestcodeIn
                        var map = lstMap.FirstOrDefault(p =>
                            !string.IsNullOrEmpty(p.TestcodeIn) &&
                            p.TestcodeIn.Trim().Equals(token, StringComparison.OrdinalIgnoreCase));

                        if (map == null) continue;

                        // Lấy value: ưu tiên token kế tiếp (để bắt case "GLU,Negative")
                        string valueToken = string.Empty;
                        if (i + 1 < parts.Length)
                        {
                            valueToken = (parts[i + 1] ?? string.Empty).Trim();
                        }
                        else
                        {
                            // nếu không có, dùng chính token hiện tại (fallback)
                            valueToken = token;
                        }

                        // Chuẩn hoá value: lấy token trước khoảng trắng đầu tiên (bỏ đơn vị nếu có)
                        string resultTmp = ExtractFirstToken(valueToken);

                        double? result = null;
                        var posneg = string.Empty;
                        Folder_File_Tool.ParseResult(resultTmp, out result, out posneg);

                        ResultStandardBL.Insert(seq, map, result, posneg, insertTime);

                        // Nếu đã dùng token kế tiếp làm value thì bỏ qua nó
                        if (i + 1 < parts.Length) i++;
                    }
                }
                catch (Exception ex)
                {
                    log4net.Error(ex);
                }
            }


            //// 3) Duyệt qua từng dòng để xử lý như cũ (nhưng tách theo phẩy)
            //foreach (var line in arrayInput)
            //{
            //    try
            //    {
            //        // Tách theo phẩy đơn cho từng dòng
            //        var parts = line.Split(new[] { ',' }, StringSplitOptions.None);
            //        if (parts.Length == 0) continue;

            //        // Test code nằm ở cột đầu
            //        var testCodeIn = parts[0].Trim();
            //        if (string.IsNullOrEmpty(testCodeIn)) continue;

            //        // Tìm map theo test code
            //        var map = lstMap.FirstOrDefault(p => testCodeIn.Contains(p.TestcodeIn));
            //        if (map == null) continue;

            //        // Phần kết quả = xử lý đặc biệt với parts[1]
            //        string resultTmp = string.Empty;
            //        if (parts.Length > 1)
            //        {
            //            var val = parts[1]?.Trim() ?? string.Empty;
            //            if (!string.IsNullOrEmpty(val))
            //            {
            //                if (val.Contains(" "))
            //                { // sẽ có test code trả về giá trị đi kèm đơn vị và cách nhau bởi space ' ' nên cần phải split ra để lấy đúng giá trị
            //                    var tokens = val.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //                    if (tokens.Length > 0)
            //                    {
            //                        resultTmp = tokens[0]; // lấy nguyên token đầu tiên
            //                    }
            //                }
            //                else
            //                {
            //                    resultTmp = val;
            //                }
            //            }
            //        }

            //        double? result = null;
            //        var posneg = string.Empty;
            //        Folder_File_Tool.ParseResult(resultTmp, out result, out posneg);

            //        ResultStandardBL.Insert(seq, map, result, posneg, insertTime);
            //    }
            //    catch (Exception ex)
            //    {
            //        log4net.Error(ex);
            //    }
            //}
        }

        // Helper: lấy token đầu trước khoảng trắng, rỗng thì trả về ""
        static string ExtractFirstToken(string val)
        {
            val = (val ?? string.Empty).Trim();
            if (val.Length == 0) return string.Empty;

            int spaceIdx = val.IndexOf(' ');
            return spaceIdx > 0 ? val.Substring(0, spaceIdx) : val;
        }

        public static void Clear()
        {
            inputData = string.Empty;
        }

        public static void WriteFile(bool _isWriteFile)
        {
            isWriteFile = _isWriteFile;
        }
    }
}
