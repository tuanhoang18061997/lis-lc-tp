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
    public class URIT180
    {
        public static string inputData = string.Empty;
        private static readonly ILog log4net = LogManager.GetLogger(typeof(URIT180).Name);
        public static string sidForUrine = string.Empty;
        private static bool isWriteFile = false;

        public static void AnalysisPatient(SerialPort sp, Connect connect, string _sidForUrine)
        {
            try
            {
                sidForUrine = _sidForUrine;
                string input = sp.ReadExisting();

                if (isWriteFile)
                    Folder_File_Tool.Create_Write_File(typeof(URIT180).Name, true, input);

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
            var arrayInput = inputData.Split((char)ProtocolASCII.LF);                   
            var Seq = sidForUrine;

            foreach (var line in arrayInput)
            {
                try
                {
                    var testCodeIn = line.Substring(0, 4).Trim();
                    var map = lstMap.Where(p => testCodeIn.Contains(p.TestcodeIn)).FirstOrDefault();
                    if (map == null) continue;
                    var resultTmp = line.Substring(5).Trim();
                    double? result = null;
                    var posneg = string.Empty;
                    Folder_File_Tool.ParseResult(resultTmp, out result, out posneg);
                    ResultStandardBL.Insert(Seq, map, result, posneg, insertTime);
                }
                catch
                { }
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
