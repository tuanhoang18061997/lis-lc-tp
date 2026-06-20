using System;
using System.IO.Ports;
using System.Text;
using log4net;
using Connects.BL;
using Connects.Models;
using System.Linq;
using System.Text.RegularExpressions;

namespace Connects.Devices
{
    public class Caretium
    {
        public static string inputData = string.Empty;
        private static readonly ILog log4net = LogManager.GetLogger(typeof(Caretium).Name);
        private static bool isWriteFile = false;

        public static void AnalysisPatient(SerialPort sp, Connect connect, string _sidForUrine)
        {
            try
            {
                string input = sp.ReadExisting();
                inputData += input;

                if (isWriteFile) Folder_File_Tool.Create_Write_File(typeof(Caretium).Name, true, input);
                byte[] ASCIIline = Encoding.ASCII.GetBytes(inputData);

                if (ASCIIline.Contains(ProtocolASCII.CR) && ASCIIline.Contains(ProtocolASCII.LF))
                {
                    Process_TestResult(inputData, connect);
                    inputData = string.Empty;
                }
            }
            catch (Exception ex)
            {
                log4net.Error(ex);
            }
        }
       
        public static void Process_TestResult(string inputData, Connect connect)
        {
            if(!string.IsNullOrEmpty(inputData))
            {
                var insertTime = DateTimeServer.Get_DateServerByEntity();
                var lstMap = MapBL.Get_ListMapByDeviceID(connect.DeviceId);
                inputData = Regex.Replace(inputData, @"\s+", " ");
                var arrayInputData = inputData.Split(' ');
                var seq = int.Parse(arrayInputData[1]).ToString();

                if (lstMap != null && lstMap.Count > 0)
                {
                    foreach(var map in lstMap)
                    {                       
                        if (map != null)
                        {
                            try
                            {
                                var resultTmp = arrayInputData[int.Parse(map.TestcodeIn)];
                                double? result = null;
                                var posneg = string.Empty;
                                Folder_File_Tool.ParseResult(resultTmp, out result, out posneg);
                                ResultStandardBL.Insert(seq, map, result, posneg, insertTime);
                            }
                            catch(Exception ex)
                            {
                                log4net.Error(ex);
                            }
                        }
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
    }
}
