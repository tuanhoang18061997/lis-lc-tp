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
    public class CybowReader300
    {
        public static string inputData = string.Empty;
        public static string className = typeof(CybowReader300).Name;
        private static readonly ILog log4net = LogManager.GetLogger(typeof(CybowReader300).Name);
        private static string firtLastChar = "~";
        private static string idChar = "ID(";
        public static bool isWriteFile = false;

        public void AnalysisPatient(SerialPort sp, Connect connect, string _sidForUrine)
        {
            try
            {
                string input = sp.ReadExisting();
                if (isWriteFile) Folder_File_Tool.Create_Write_File(className, true, input);
                //inputData += input;
                //var firstChar = inputData.Substring(0, 1);
                //var lastChar = inputData.Substring(inputData.Length - 3, 1);
                //if (firstChar == firtLastChar && lastChar == firtLastChar)
                //{
                //    CybowReader300_TestResult_AnalysisLine(inputData, connect);
                //    inputData = string.Empty;
                //}
            }
            catch (Exception ex)
            {
                log4net.Error(ex);
                inputData = string.Empty;
            }
        }

        public void CybowReader300_TestResult_AnalysisLine(string inputData, Connect connect)
        {
            try
            {
                if (!string.IsNullOrEmpty(inputData))
                {
                    var insertTime = DateTimeServer.Get_DateServerByEntity();
                    var lstMap = MapBL.Get_ListMapByDeviceID(connect.DeviceId);
                    var arrInputData = inputData.Split((char)ProtocolASCII.LF);
                    var dateTime = arrInputData[0].Substring(1).Trim().Replace('/', '-');
                    try
                    {
                        insertTime = DateTime.Parse(dateTime);
                    }
                    catch { }

                    var lineID = arrInputData.Where(p => p.Contains(idChar)).FirstOrDefault();
                    var seq = lineID.Substring(3, 13).Trim();

                    foreach (var item in arrInputData)
                    {
                        if (!string.IsNullOrEmpty(item.Trim()) && item.Length >= 3)
                        {
                            try
                            {
                                var testCodeIn = item.Substring(0, 3).Trim();
                                var posneg = item.Substring(3, 8).Trim();
                                double? result = null;
                                if (IsNumber(item.Substring(11, 11).Trim()))
                                {
                                    result = Folder_File_Tool.ParseToDouble(item.Substring(11, 11).Trim());
                                }

                                var map = lstMap.Where(p => p.TestcodeIn.Trim().ToUpper() == testCodeIn.Trim().ToUpper()).FirstOrDefault();
                                if(map != null)
                                {
                                    ResultStandardBL.Insert(seq, map, result, posneg, insertTime);
                                }
                            }
                            catch (Exception ex)
                            {
                                log4net.Error(ex);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log4net.Error(ex);
            }
        }

        public bool IsNumber(string value)
        {
            Regex regex = new Regex(@"^[-+]?[0-9]*\.?[0-9]+$");
            return regex.IsMatch(value);
        }

        public void Clear()
        {
            inputData = string.Empty;
        }

        public void WriteFile(bool _isWriteFile)
        {
            isWriteFile = _isWriteFile;
        }
    }
}
