using System;
using System.IO.Ports;
using System.Text;
using log4net;
using Connects.BL;
using Connects.Models;
using System.Linq;

namespace Connects.Devices
{
    public class MindrayBC3000Plus
    {
        public static string inputData = string.Empty;
        private static readonly ILog log4net = LogManager.GetLogger(typeof(MindrayBC3000Plus).Name);
        private static bool isWriteFile = false;

        public static void AnalysisPatient(SerialPort sp, Connect connect, string _sidForUrine)
        {
            try
            {
                string input = sp.ReadExisting();
                inputData += input;

                if (isWriteFile) Folder_File_Tool.Create_Write_File(typeof(MindrayBC3000Plus).Name, true, input);
                byte[] ASCIIline = Encoding.ASCII.GetBytes(inputData);

                if (ASCIIline.Contains(ProtocolASCII.STX) && ASCIIline.Contains(ProtocolASCII.SUB))
                {
                    var indexSTX = inputData.IndexOf((char)ProtocolASCII.STX);
                    var indexSUB = inputData.IndexOf((char)ProtocolASCII.SUB);
                    var result = inputData.Substring(indexSTX, indexSUB+1);
                    inputData = inputData.Substring(indexSUB+1);                
                    Process_TestResult(result, connect);
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
                var seq = int.Parse(inputData.Substring(12,10)).ToString();

                if (lstMap != null && lstMap.Count > 0)
                {
                    foreach(var map in lstMap)
                    {                       
                        if (map != null)
                        {
                            try
                            {
                                var resultTmp = inputData.Substring(int.Parse(map.TestcodeIn), int.Parse(map.TestcodeIn2));
                                var indexDot = map.FormatNumber.IndexOf('.');
                                if (indexDot != -1) resultTmp = resultTmp.Insert(indexDot, ".");
                                var posneg = string.Empty;
                                double? result = null;
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
