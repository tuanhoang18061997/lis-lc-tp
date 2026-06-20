using Connects.BL;
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
    public class ClinitekStatus_IP
    {
        private static string className = typeof(ClinitekStatus_IP).Name;
        private static readonly ILog log4net = LogManager.GetLogger(typeof(ClinitekStatus_IP).Name);
        private static string inputData = string.Empty;
        private static string line = string.Empty;
        private static string text_O = "O";
        private string text_R = "R";
        private static string text_Result = "N";
        private static string text_QC = "Q";
        private static string linePNG = ".PNG";

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
            }
            catch (Exception ex)
            {
                log4net.Error(ex);
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
                        // Xử lý riêng Bạch cầu (Testcode: LEU)
                        if (key == "LEU")
                        {
                            if (valueToken == "Large")
                            {
                                result = 500;
                            }
                            else if (valueToken == "Moderate")
                            {
                                result = 125;
                            }
                            else if (valueToken == "Small")
                            {
                                result = 70;
                            }
                            else if (valueToken.Contains("Trace"))
                            {
                                posneg = "Negative";
                            }
                            else if (valueToken == "Negative")
                            {
                                posneg = "Negative";
                            }
                        }
                        // Xử lý riêng cầu (Testcode: KET)
                        else if (key == "KET")
                        {
                            if (valueToken == "Large")
                            {
                                result = 80;
                            }
                            else if (valueToken == "Moderate")
                            {
                                result = 40;
                            }
                            else if (valueToken == "Small")
                            {
                                result = 15;
                            }
                            else if (valueToken.Contains("Trace"))
                            {
                                posneg = "Negative";
                            }
                            else if (valueToken == "Negative")
                            {
                                posneg = "Negative";
                            }
                        }
                        // Xử lý riêng (Testcode: BLO) Erythrocytes
                        else if (key == "BLO")
                        {
                            if (valueToken == "Large")
                            {
                                result = 200;
                            }
                            else if (valueToken == "Moderate")
                            {
                                result = 80;
                            }
                            else if (valueToken == "Small")
                            {
                                result = 25;
                            }
                            else if (valueToken.Contains("Trace"))
                            {
                                posneg = "Negative";
                            }
                            else if (valueToken == "Negative")
                            {
                                posneg = "Negative";
                            }
                        }
                        // Xử lý riêng (Testcode: BIL)
                        else if (key == "BIL")
                        {
                            if (valueToken == "Negative")
                            {
                                posneg = "Negative";
                            }
                            else
                            {
                                posneg = "Positive";
                            }
                        }
                        else if (key == "SG")
                        {
                            posneg = resultTmp; // vì kiểu double không lưu được số 0 cuối nếu result = 1.020 thì double sẽ lưu 1.02 nên sẽ lưu sang dạng chuỗi để lấy đc hết
                        }
                        else if (key == "PRO")
                        {
                            if (valueToken.Contains("Trace"))
                            {
                                posneg = "Negative";
                            }
                            else
                            {
                                Folder_File_Tool.ParseResult(resultTmp, out result, out posneg);
                            }
                        }
                        else
                        {
                            Folder_File_Tool.ParseResult(resultTmp, out result, out posneg);
                        }

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
