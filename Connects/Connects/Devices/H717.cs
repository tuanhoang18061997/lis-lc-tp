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
    public class H717
    {
        public static string inputData = string.Empty;
        private static SerialPort serialPort = new SerialPort();
        private static readonly ILog log4net = LogManager.GetLogger(typeof(H717).Name);
        private static string testcodeCHOL = "07CHOLESTEROL TOTAL";
        private static string testcodeHDL = "09HDL-C";
        private static string testcodeLDL = "10LDL - C";
        private static string testcodeTRYG = "08TRYGLYCERID";
        private static bool isWriteFile = false;

        public static void AnalysisPatient(SerialPort sp, Connect connect, string _sidForUrine)
        {
            try
            {
                serialPort = sp;
                string input = sp.ReadExisting();
                if (isWriteFile) Folder_File_Tool.Create_Write_File(typeof(H717).Name, true, input);
                inputData += input;
                byte[] ASCIIValues = Encoding.ASCII.GetBytes(inputData);

                if ((ASCIIValues.Contains(ProtocolASCII.STX) && ASCIIValues.Contains(ProtocolASCII.ETX)))
                {
                    ProcessResult(inputData, connect);
                    input = string.Empty;
                    inputData = string.Empty;
                    HostSend_ACK();
                }
            }
            catch (Exception ex)
            {
                log4net.Error(ex);
            }
        }

        public static void ProcessResult(string inputData, Connect connect)
        {
            var insertTime = DateTimeServer.Get_DateServerByEntity();
            var lstMap = MapBL.Get_ListMapByDeviceID(connect.DeviceId);
            if (lstMap == null || lstMap.Count == 0) return;

            var indexSTX = inputData.IndexOf((char)ProtocolASCII.STX);
            inputData = inputData.Substring(indexSTX);
            var indexETX = inputData.IndexOf((char)ProtocolASCII.ETX);
            inputData = inputData.Substring(0, indexETX + 1);

            var seq = inputData.Substring(4, 4).Trim();
            inputData = inputData.Substring(25);
            indexETX = inputData.IndexOf((char)ProtocolASCII.ETX);
            inputData = inputData.Substring(0, indexETX);
            string resultCHOL = null;
            string resultHDL = null;
            string resultTRYG = null;
            var insertedLDL = false;

            var count = inputData.Length;
            var lstResult = new List<string>();
            try
            {
                while (count > 0)
                {
                    var result = inputData.Substring(0, 9);
                    inputData = inputData.Remove(0, 9);
                    lstResult.Add(result);
                    count = inputData.Length;
                }
            }
            catch (Exception ex)
            {
                log4net.Error(ex);
            }

            if (lstResult != null)
            {
                foreach (var item in lstResult)
                {
                    try
                    {
                        var testCodeIn = item.Substring(0, 2).Trim();
                        var resultTmp = item.Substring(2, 6).Trim().Replace(" ", "");
                        double? result = null;
                        var posneg = string.Empty;
                        Folder_File_Tool.ParseResult(resultTmp, out result, out posneg);

                        var map = lstMap.Where(p => p.TestcodeIn == testCodeIn).FirstOrDefault();
                        if (map != null)
                        {
                            ResultStandardBL.Insert(seq, map, result, string.Empty, insertTime);
                            if (map.TestCode.Code == testcodeHDL) resultHDL = resultTmp.Replace(" ", "");
                            if (map.TestCode.Code == testcodeCHOL) resultCHOL = resultTmp.Replace(" ", "");
                            if (map.TestCode.Code == testcodeTRYG) resultTRYG = resultTmp.Replace(" ", "");
                        }
                        if (!insertedLDL && !string.IsNullOrEmpty(resultCHOL) && !string.IsNullOrEmpty(resultHDL) && !string.IsNullOrEmpty(resultTRYG))
                        {
                            try
                            {
                                result = (double.Parse(resultCHOL) - double.Parse(resultHDL) - (double.Parse(resultTRYG) / 5));
                            }
                            catch
                            { }

                            map = lstMap.Where(p => p.TestCode.Code == testcodeLDL).FirstOrDefault();
                            ResultStandardBL.Insert(seq, map, result, string.Empty, insertTime);
                            insertedLDL = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        log4net.Error(ex);
                    }
                }
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

        public static void HostSend_ACK()
        {
            string stringACK = Encoding.Default.GetString(new byte[1] { ProtocolASCII.ACK });
            serialPort.Write(stringACK);
        }
    }
}
