using System;
using System.IO.Ports;
using System.Text;
using log4net;
using Connects.BL;
using Connects.Models;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace Connects.Devices
{
    public class G600II
    {
        public static string inputData = string.Empty;
        private static readonly ILog log4net = LogManager.GetLogger(typeof(G600II).Name);
        private static bool isWriteFile = false;
        private static SerialPort serialPort;

        public static void AnalysisPatient(SerialPort sp, Connect connect, string _sidForUrine)
        {
            try
            {
                serialPort = sp;
                string input = sp.ReadExisting();
                inputData += input;
                if (isWriteFile) Folder_File_Tool.Create_Write_File(typeof(G600II).Name, true, input);
                byte[] ASCIIValues = Encoding.ASCII.GetBytes(inputData);

                if (ASCIIValues[0] == ProtocolASCII.ENQ)
                {
                    HostSend_ACK();
                    inputData = string.Empty;
                }
                else if(ASCIIValues[0] == ProtocolASCII.STX && ASCIIValues[ASCIIValues.Length - 2] == ProtocolASCII.ETX)
                {
                    HostSend_ACK();
                }
                else if (ASCIIValues[ASCIIValues.Length - 1] == ProtocolASCII.EOT)
                {
                    Process_TestResult(inputData, connect);
                    inputData = string.Empty;
                }
            }
            catch (Exception ex)
            {
                log4net.Error(ex);
                inputData = string.Empty;
            }
        }

        public static void HostSend_ACK()
        {
            string stringACK = Encoding.Default.GetString(new byte[1] { ProtocolASCII.ACK });
            if (isWriteFile) Folder_File_Tool.Create_Write_File(typeof(G600II).Name, true, stringACK);
            serialPort.Write(stringACK);
        }

        public static void Process_TestResult(string inputData, Connect connect)
        {
            if (string.IsNullOrEmpty(inputData)) return;
            try
            {
                var arrayInputData = inputData.Split('|');
                DateTime insertTime = DateTimeServer.Get_DateServerByEntity();
                var lstMap = MapBL.Get_ListMapByDeviceID(connect.DeviceId);
                var lstTestCodeInResult = new List<string>();

                var seq = arrayInputData[6].Trim();
                var testCodeIn = arrayInputData[13];
                var resultTmp = arrayInputData[17];
                double? result = null;
                var posneg = string.Empty;
                Folder_File_Tool.ParseResult(resultTmp, out result, out posneg);
                var map = lstMap.Where(p => p.TestcodeIn == testCodeIn).FirstOrDefault();
                if (map != null)
                {
                    ResultStandardBL.Insert(seq, map, result, posneg, insertTime);
                }
            }
            catch (Exception ex)
            {
                log4net.Error(ex);
            }
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
