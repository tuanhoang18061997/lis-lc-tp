using Connects.BL;
using Connects.Mapping;
using Connects.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Connects.Devices
{
    public class MissionU500_IP
    {
        private static string className = typeof(MissionU500_IP).Name;
        private static readonly ILog log4net = LogManager.GetLogger(typeof(MissionU500_IP).Name);
        private static string inputData = string.Empty;

        private static Connect connect = null;
        private static IPEndPoint ipe;
        private static TcpClient tcpclient;
        private static NetworkStream stream;
        private static System.Timers.Timer timer;
        private static bool connectedServer = false;
        private static bool isWriteFile = false;
        private static long deviceID = 0;

        public static bool Start(IPEndPoint iPEndPoint, long _deviceID)
        {
            try
            {
                deviceID = _deviceID;
                connect = ConnectBL.Get_ConnectByDeviceId(deviceID);
                ipe = iPEndPoint;
                tcpclient = new TcpClient();
                tcpclient.Connect(ipe);
                stream = tcpclient.GetStream();
                Thread thread = new Thread(Receive);
                thread.IsBackground = true;
                thread.Start();
                InitTimer();
                return true;
            }
            catch (Exception ex)
            {
                log4net.Error(ex);
                return false;
            }
        }

        public static bool Close()
        {
            try
            {
                if (stream != null) stream.Close();
                if (tcpclient != null) tcpclient.Close();
                RemoveTimer();
                return true;
            }
            catch (Exception ex)
            {
                log4net.Error(ex);
                return false;
            }
        }

        public static void Receive()
        {
            try
            {
                while (true)
                {
                    try
                    {
                        byte[] byteData = new byte[tcpclient.ReceiveBufferSize];
                        stream.Read(byteData, 0, byteData.Length);
                        string input = RemoveByteNull(byteData);
                        if (!string.IsNullOrEmpty(input.Trim())) Update_Waiting_Receving();
                        if (isWriteFile) Folder_File_Tool.CreateWriteFileByDate(className, true, input);

                        inputData += input;
                        byte[] ASCIIValues = Encoding.ASCII.GetBytes(inputData);

                        if ((ASCIIValues.Contains(ProtocolASCII.STX) && ASCIIValues.Contains(ProtocolASCII.ETX)))
                        {
                            ProcessResult_Production(inputData, connect);
                            inputData = string.Empty;
                        }
                    }
                    catch (Exception ex)
                    {
                        log4net.Error(ex);
                        inputData = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                log4net.Error(ex);
            }
        }

        //public static void ProcessResult(string inputData, Connect connect)
        //{
        //    var insertTime = DateTimeServer.Get_DateServerByEntity();
        //    var Seq = string.Empty;

        //    var lstMap = MapBL.Get_ListMapByDeviceID(connect.DeviceId);
        //    if (lstMap == null || lstMap.Count == 0) return;

        //    var arrayInput = inputData.Split((char)ProtocolASCII.LF);

        //    if (arrayInput.Length == 0) return;

        //    // 2) Lấy seq từ dòng đầu tiên: token đầu tiên sau khi split bằng dấu phẩy đơn
        //    string seq = arrayInput[1]
        //        .Split(new[] { ',' }, StringSplitOptions.None)
        //        .FirstOrDefault()?
        //        .Trim() ?? string.Empty;
        //    // Tạo từ điển tra cứu theo TestcodeIn (không phân biệt hoa/thường)
        //    var mapByCode = lstMap
        //        .Where(m => !string.IsNullOrWhiteSpace(m.TestcodeIn))
        //        .ToDictionary(
        //            m => m.TestcodeIn.Trim().ToUpperInvariant(),
        //            m => m
        //        );

        //    foreach (var line in arrayInput)
        //    {
        //        Seq = sidForUrine;
        //        if (string.IsNullOrEmpty(Seq) && line.Contains("Operator"))
        //        {
        //            var index = line.IndexOf(':');
        //            if (index != -1)
        //            {
        //                operatorID = line.Substring(index + 1).Trim();
        //            }
        //        }
        //        else if (string.IsNullOrEmpty(Seq) && line.Contains("No."))
        //        {
        //            Seq = double.Parse(line.Trim().Split('.')[1].ToString()).ToString();
        //        }
        //        else
        //        {
        //            var arrayItem = line.Trim().Split(' ');
        //            var testCodeIn = arrayItem[0].Length == 4 ? arrayItem[0].Substring(1) : arrayItem[0];
        //            var map = lstMap.Where(p => p.TestcodeIn == testCodeIn).FirstOrDefault();
        //            if (map == null) continue;
        //            var lstItem = new List<string>();
        //            string posneg = string.Empty;
        //            double? result = null;

        //            foreach (var item in arrayItem)
        //            {
        //                if (!string.IsNullOrEmpty(item.Trim()))
        //                {
        //                    lstItem.Add(item.Trim());
        //                }
        //            }

        //            if (lstItem.Count >= 2)
        //            {
        //                var resultTmp = lstItem[1];
        //                if (resultTmp == "-" || resultTmp == "+")
        //                {
        //                    posneg = resultTmp == "-" ? "ÂM TÍNH" : "DƯƠNG TÍNH";
        //                }
        //                else
        //                {
        //                    Folder_File_Tool.ParseResult(resultTmp, out result, out posneg);
        //                }

        //                ResultStandardBL.Insert(Seq, map, result, posneg, insertTime);
        //            }
        //        }
        //    }
        //}
        public static void ProcessResult_Production(string inputData, Connect connect)
        {
            try
            {
                var insertTime = DateTimeServer.Get_DateServerByEntity();

                var lstMap = MapBL.Get_ListMapByDeviceID(connect.DeviceId);
                if (lstMap == null || lstMap.Count == 0)
                    return;

                string seq = string.Empty;

                var lines = inputData
                    .Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();

                foreach (var rawLine in lines)
                {
                    string line = rawLine
                        .Replace(((char)ProtocolASCII.STX).ToString(), "")
                        .Replace(((char)ProtocolASCII.ETX).ToString(), "")
                        .Replace(((char)ProtocolASCII.CR).ToString(), "")
                        .Replace(((char)ProtocolASCII.LF).ToString(), "")
                        .Trim();

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    // Lấy Seq
                    if (line.StartsWith("ID", StringComparison.OrdinalIgnoreCase))
                    {
                        var matchSeq = Regex.Match(line, @"ID\s*:\s*([0-9]+)");

                        if (matchSeq.Success)
                        {
                            seq = matchSeq.Groups[1].Value.Trim();
                        }

                        continue;
                    }

                    // bỏ metadata
                    if (line.StartsWith("ID:", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (line.StartsWith("Other", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (Regex.IsMatch(line, @"^\d{2}-\d{2}-\d{4}"))
                        continue;

                    var tokens = line
                        .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();

                    if (tokens.Count < 2)
                        continue;

                    string parameter = MissionU500ResultMapping.NormalizeParameter(tokens[0]);
                    string rawCode = MissionU500ResultMapping.NormalizeRawCode(tokens[1]);

                    var resultMap = MissionU500ResultMapping.Find(parameter, rawCode);

                    if (resultMap == null)
                        continue;

                    //var mapLis = lstMap.FirstOrDefault(x =>
                    //    !string.IsNullOrWhiteSpace(x.TestcodeIn) &&
                    //    x.TestcodeIn.Trim().ToUpperInvariant() == parameter);

                    var mapLis = lstMap.FirstOrDefault(x =>
                        !string.IsNullOrWhiteSpace(x.TestcodeIn) &&
                        MissionU500ResultMapping.NormalizeParameter(x.TestcodeIn) == parameter);

                    if (mapLis == null)
                        continue;

                    double? result = null;
                    string posneg = string.Empty;

                    // ===== XỬ LÝ RIÊNG PH và SG =====

                    if (parameter == "PH" )
                    {
                        // giữ nguyên decimal
                        if (double.TryParse(resultMap.Conventional,
                            NumberStyles.Any,
                            CultureInfo.InvariantCulture,
                            out double value))
                        {
                            result = value;
                        }
                        else
                        {
                            result = null;
                        }

                        posneg = "";
                    }
                    else if (parameter == "SG") // Nếu có mapping SI cho SG thì ưu tiên parse theo SI
                    {
                        posneg = resultMap.Conventional;
                    }
                    else
                    {
                        // test bình thường
                        Folder_File_Tool.ParseResult(
                            resultMap.Conventional,
                            out result,
                            out posneg);
                    }

                    ResultStandardBL.Insert(
                        seq,
                        mapLis,
                        result,
                        posneg,
                        insertTime);
                }
            }
            catch (Exception ex)
            {
                log4net.Error(ex);
            }
        }
        public static void ProcessResult(string inputData, Connect connect)
        {
            try
            {
                WriteProcessLog("===== START PROCESS RESULT =====");

                var insertTime = DateTimeServer.Get_DateServerByEntity();
                var lstMap = MapBL.Get_ListMapByDeviceID(connect.DeviceId);

                if (lstMap == null || lstMap.Count == 0)
                {
                    WriteProcessLog("MapBL.Get_ListMapByDeviceID khong co du lieu.");
                    return;
                }

                string seq = string.Empty;

                var lines = inputData
                    .Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();

                WriteProcessLog("Tong so line = " + lines.Count);

                foreach (var rawLine in lines)
                {
                    WriteProcessLog("----------------------------------------");
                    WriteProcessLog("RAW LINE = [" + rawLine + "]");

                    string line = rawLine
                        .Replace(((char)ProtocolASCII.STX).ToString(), "")
                        .Replace(((char)ProtocolASCII.ETX).ToString(), "")
                        .Replace(((char)ProtocolASCII.CR).ToString(), "")
                        .Replace(((char)ProtocolASCII.LF).ToString(), "")
                        .Trim();

                    WriteProcessLog("CLEAN LINE = [" + line + "]");

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        WriteProcessLog("Bo qua line rong.");
                        continue;
                    }

                    // Lấy seq từ dòng No.
                    if (line.StartsWith("No.", StringComparison.OrdinalIgnoreCase) ||
                        line.StartsWith("No.:", StringComparison.OrdinalIgnoreCase))
                    {
                        var matchSeq = Regex.Match(line, @"No\.\s*:\s*([0-9]+)");
                        if (matchSeq.Success)
                        {
                            seq = matchSeq.Groups[1].Value.Trim();
                            WriteProcessLog("Lay duoc SEQ = [" + seq + "]");
                        }
                        else
                        {
                            WriteProcessLog("Khong tach duoc SEQ tu dong No.");
                        }

                        continue;
                    }

                    // Bỏ qua các dòng metadata
                    if (line.StartsWith("ID:", StringComparison.OrdinalIgnoreCase) ||
                        line.StartsWith("Other", StringComparison.OrdinalIgnoreCase) ||
                        Regex.IsMatch(line, @"^\d{2}-\d{2}-\d{4}"))
                    {
                        WriteProcessLog("Bo qua dong metadata.");
                        continue;
                    }

                    var tokens = line
                        .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();

                    if (tokens.Count < 2)
                    {
                        WriteProcessLog("Bo qua line do khong du token.");
                        continue;
                    }

                    string parameter = tokens[0].Trim();
                    string rawCode = tokens[1].Trim();

                    parameter = MissionU500ResultMapping.NormalizeParameter(parameter);
                    rawCode = MissionU500ResultMapping.NormalizeRawCode(rawCode);

                    WriteProcessLog("PARAMETER = [" + parameter + "]");
                    WriteProcessLog("RAW CODE  = [" + rawCode + "]");

                    var resultMap = MissionU500ResultMapping.Find(parameter, rawCode);

                    if (resultMap == null)
                    {
                        WriteProcessLog("Khong tim thay mapping noi bo cho parameter=[" + parameter + "], rawCode=[" + rawCode + "]");
                        continue;
                    }

                    WriteProcessLog("MAP NOI BO -> Conventional = [" + resultMap.Conventional + "], SI = [" + resultMap.SI + "]");

                    var mapLis = lstMap.FirstOrDefault(x =>
                        !string.IsNullOrWhiteSpace(x.TestcodeIn) &&
                        x.TestcodeIn.Trim().ToUpperInvariant() == parameter);

                    if (mapLis == null)
                    {
                        WriteProcessLog("Khong tim thay map LIS theo TestcodeIn = [" + parameter + "]");
                        continue;
                    }

                    WriteProcessLog("MAP LIS OK -> TestcodeIn = [" + mapLis.TestcodeIn + "]");

                    // Tạm thời test log trước
                    double? result = null;
                    string posneg = string.Empty;

                    if (!string.IsNullOrWhiteSpace(resultMap.Conventional))
                    {
                        Folder_File_Tool.ParseResult(resultMap.Conventional, out result, out posneg);
                    }

                    WriteProcessLog("SAU PARSE -> result = [" + (result.HasValue ? result.Value.ToString() : "null") + "], posneg = [" + posneg + "]");

                    // Khi test log ổn thì mở dòng insert này ra
                    // ResultStandardBL.Insert(seq, mapLis, result, posneg, insertTime);

                    WriteProcessLog("TEST OK line nay, CHUA insert.");
                }

                WriteProcessLog("===== END PROCESS RESULT =====");
            }
            catch (Exception ex)
            {
                log4net.Error(ex);
                WriteProcessLog("ERROR: " + ex.Message);
            }
        }

        private static void WriteProcessLog(string message)
        {
            log4net.Info("[MissionU500][ProcessResult] " + message);

            if (isWriteFile)
            {
                Folder_File_Tool.CreateWriteFileByDate(className, true,
                    "[MissionU500][ProcessResult] " + message + Environment.NewLine);
            }
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
        public static bool CheckConnect()
        {
            return false;
        }

        public static void InitTimer()
        {
            connectedServer = true;
            if (timer == null)
            {
                timer = new System.Timers.Timer();
                timer.Elapsed += Timer_Elapsed;
                timer.Interval = 1000;
                timer.Start();
            }
        }

        public static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (connectedServer != tcpclient.Connected)
            {
                connectedServer = tcpclient.Connected;
                Connects.Program.mainForm.UpdateConnectForClient(connect, connectedServer);
            }
            if (!connectedServer) ReStart();
        }

        public static void ReStart()
        {
            try
            {
                CloseTmp();
                Connects.Program.mainForm.UpdateConnectForClient(connect, StartTmp());
            }
            catch (Exception ex)
            {
                log4net.Error(ex);
            }
        }

        public static bool CloseTmp()
        {
            try
            {
                if (stream != null) stream.Close();
                if (tcpclient != null) tcpclient.Close();
                return true;
            }
            catch (Exception ex)
            {
                log4net.Error(ex);
                return false;
            }
        }

        public static void RemoveTimer()
        {
            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }
        }

        public static bool StartTmp()
        {
            try
            {
                tcpclient = new TcpClient();
                tcpclient.Connect(ipe);
                stream = tcpclient.GetStream();
                Thread thread = new Thread(Receive);
                thread.IsBackground = true;
                thread.Start();
                return true;
            }
            catch (Exception ex)
            {
                log4net.Error(ex);
                return false;
            }
        }

        private static string RemoveByteNull(byte[] byteInput)
        {
            string stringData = Encoding.Default.GetString(byteInput);
            int startIndexNull = stringData.IndexOf((char)ProtocolASCII.NUL);
            stringData = stringData.Substring(0, startIndexNull);
            return stringData;
        }

        private static void Update_Waiting_Receving()
        {
            Program.mainForm.Update_Waiting_Receving(connect);
        }
    }
}
