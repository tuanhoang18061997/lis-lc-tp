using System;
using System.IO.Ports;
using System.Text;
using log4net;
using Connects.BL;
using Connects.Models;
using System.Linq;
using System.Threading;
using System.Net.Sockets;

namespace Connects.Devices
{
    public class MispaCXL
    {
        private static string inputData = string.Empty;
        private static string line = string.Empty;
        private static SerialPort serialPort = new SerialPort();
        private static string className = typeof(MispaCXL).Name;
        private static readonly ILog log4net = LogManager.GetLogger(className);        
        private static string text_O = "O";
        private static string text_R = "R";
        private static bool isWriteFile = false;
        private static System.Timers.Timer timer;
        private static DateTime dateTime = new DateTime();

        public void InitTimer(SerialPort _serialPort)
        {
            if (timer == null)
            {
                serialPort = _serialPort;
                timer = new System.Timers.Timer();
                timer.Elapsed += Timer_Elapsed;
                timer.Interval = 5000;
                timer.Start();
            }
        }

        public void RemoveTimer()
        {
            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }
        }

        public void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if(string.IsNullOrEmpty(inputData.Trim()))
            {
                HostSend_ENQ();
            }
        }

        public static void AnalysisPatient(SerialPort sp, Connect connect, string _sidForUrine)
        {
            try
            {
                string input = sp.ReadExisting();
                inputData += input;
                serialPort = sp;
                if (isWriteFile) Folder_File_Tool.Create_Write_File(className, true, input);
                if (input.Contains((char)ProtocolASCII.ENQ))
                {
                    HostSend_ACK();
                }
                else if (input.Contains((char)ProtocolASCII.EOT))
                {
                    Process_TestResult(inputData, connect);
                    Clear();
                }
                else if (input.Contains((char)ProtocolASCII.ACK))
                {
                    Clear();
                    HostSend_EOT();                   
                }
                else
                {                   
                    line += input;
                    if (line.Contains((char)ProtocolASCII.STX) && (line.Contains((char)ProtocolASCII.LF)))
                    {
                        HostSend_ACK();
                        line = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                log4net.Error(ex);
            }
        }

        //public static void AnalysisPatient(SerialPort sp, Connect connect, string _sidForUrine)
        //{
        //    try
        //    {
        //        string input = sp.ReadExisting();
        //        serialPort = sp;
        //        if (isWriteFile) Folder_File_Tool.Create_Write_File(className, true, input);
        //        byte[] ENQ_EOT_ACK = Encoding.ASCII.GetBytes(input);
        //        if (ENQ_EOT_ACK[0] == ProtocolASCII.ENQ)
        //        {
        //            HostSend_ACK();
        //        }
        //        else if (ENQ_EOT_ACK[0] == ProtocolASCII.EOT)
        //        {
        //            Process_TestResult(inputData, connect);
        //            inputData = string.Empty;
        //            line = string.Empty;
        //        }
        //        else
        //        {
        //            inputData += input;
        //            line += input;
        //            byte[] ASCIIline = Encoding.ASCII.GetBytes(line);
        //            if (ASCIIline[0] == ProtocolASCII.STX && ASCIIline[ASCIIline.Length - 2] == ProtocolASCII.CR && ASCIIline[ASCIIline.Length - 1] == ProtocolASCII.LF)
        //            {
        //                HostSend_ACK();
        //                line = string.Empty;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        log4net.Error(ex);
        //    }
        //}

        public static void Process_TestResult(string inputData, Connect connect)
        {
            if (string.IsNullOrEmpty(inputData.Trim())) return;
            var lstMap = MapBL.Get_ListMapByDeviceID(connect.DeviceId);
            var arrayInputData = inputData.Split((char)ProtocolASCII.STX);
            DateTime insertTime = DateTimeServer.Get_DateServerByEntity();
            string seq = string.Empty;

            foreach (var line in arrayInputData)
            {
                try
                {
                    var arrayLine = line.Split('|');
                    if (arrayLine != null)
                    {
                        if (arrayLine[0].Contains(text_O))
                        {
                            seq = arrayLine[2].Split('^')[0];
                        }
                        if (arrayLine[0].Contains(text_R))
                        {
                            var testCodeIn = arrayLine[2].Split('^')[3];
                            var resultTmp = arrayLine[3];
                            double? result = null;
                            var posneg = string.Empty;
                            Folder_File_Tool.ParseResult(resultTmp, out result, out posneg);
                            var map = lstMap.Where(p => p.TestcodeIn == testCodeIn).FirstOrDefault();
                            if (map != null)
                            {
                                ResultStandardBL.Insert(seq, map, result, posneg, insertTime);
                            }
                        }                        
                    }
                }
                catch (Exception ex)
                {
                    log4net.Error(ex);
                }
            }
        }

        public static void HostSend_ACK()
        {
            string stringACK = Encoding.Default.GetString(new byte[1] { ProtocolASCII.ACK });
            if (isWriteFile) Folder_File_Tool.Create_Write_File(className, true, stringACK);
            serialPort.Write(stringACK);           
        }

        public static void HostSend_ENQ()
        {
            string stringENQ = Encoding.Default.GetString(new byte[1] { ProtocolASCII.ENQ });
            if (isWriteFile) Folder_File_Tool.Create_Write_File(className, true, stringENQ);
            serialPort.Write(stringENQ);
        }

        public static void HostSend_EOT()
        {
            string stringEOT = Encoding.Default.GetString(new byte[1] { ProtocolASCII.EOT });
            if (isWriteFile) Folder_File_Tool.Create_Write_File(className, true, stringEOT);
            serialPort.Write(stringEOT);
        }

        public static void Clear()
        {
            inputData = string.Empty;
            line = string.Empty;
        }

        public static void WriteFile(bool _isWriteFile)
        {
            isWriteFile = _isWriteFile;
        }
    }
}
