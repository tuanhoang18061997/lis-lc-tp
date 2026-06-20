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
    public class Elisa
    {
        private static string inputData = string.Empty;
        private static string className = typeof(Elisa).Name;
        private static readonly ILog log4net = LogManager.GetLogger(className);        
        private static bool isWriteFile = false;
        private static Setting settingKhongGanCungMaXN = null;

        public static void AnalysisPatient(SerialPort sp, Connect connect, string _sidForUrine)
        {
            if (settingKhongGanCungMaXN == null) settingKhongGanCungMaXN = SettingBL.Get_Setting("Elisa");
            try
            {
                string input = sp.ReadExisting();
                inputData += input;
                if (isWriteFile) Folder_File_Tool.Create_Write_File(className, true, input);
                if(inputData.Contains((char)ProtocolASCII.LF))
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
            if (string.IsNullOrEmpty(inputData.Trim())) return;
            var lstData = inputData.Split(',').ToList();
            var lstMap = MapBL.Get_ListMapByDeviceID(connect.DeviceId);
            if (lstMap == null || lstMap.Count == 0) return;
            DateTime insertTime = DateTimeServer.Get_DateServerByEntity();
            string seq = string.Empty, testCodeIn = string.Empty, posneg = string.Empty, resultTmp = string.Empty;
            double? resultDouble = null;
            double? result = null;

            for (int i = 0; i < lstData.Count; i++)
            {
                try
                {
                    foreach (var map in lstMap)
                    {
                        if (map == null) continue;
                        if (lstData[i] == map.TestcodeIn)
                        {
                            testCodeIn = map.TestcodeIn;
                            seq = lstData[i + 4];
                            resultTmp = Regex.Match(lstData[i + 5], @"\d+.+\d").Value;
                            Folder_File_Tool.ParseResult(resultTmp, out resultDouble, out posneg);
                            if(resultDouble == null)
                            {
                                result = null;
                            }
                            else
                            {
                                if (settingKhongGanCungMaXN != null && settingKhongGanCungMaXN.Value == "1")
                                {
                                    if (map.TestCode.HigherLimit != null)
                                    {
                                        if (resultDouble < map.TestCode.HigherLimit.Value)
                                            posneg = "ÂM TÍNH (" + resultDouble.Value.ToString(Folder_File_Tool.GetCultureInfo()) + ")";
                                        else
                                            posneg = "DƯƠNG TÍNH (" + resultDouble.Value.ToString(Folder_File_Tool.GetCultureInfo()) + ")";
                                    }
                                }
                                else
                                {
                                    if (map.TestCode.Code == "216TOX")
                                    {
                                        if (resultDouble < 0.36)
                                            posneg = "ÂM TÍNH (" + resultDouble.Value.ToString(Folder_File_Tool.GetCultureInfo()) + ")";
                                        else
                                            posneg = "DƯƠNG TÍNH (" + resultDouble.Value.ToString(Folder_File_Tool.GetCultureInfo()) + ")";
                                    }
                                    else if (map.TestCode.Code == "217ECH")
                                    {
                                        if (resultDouble < 0.36)
                                            posneg = "ÂM TÍNH (" + resultDouble.Value.ToString(Folder_File_Tool.GetCultureInfo()) + ")";
                                        else
                                            posneg = "DƯƠNG TÍNH (" + resultDouble.Value.ToString(Folder_File_Tool.GetCultureInfo()) + ")";
                                    }
                                    else if (map.TestCode.Code == "218STR")
                                    {
                                        if (resultDouble < 0.24)
                                            posneg = "ÂM TÍNH (" + resultDouble.Value.ToString(Folder_File_Tool.GetCultureInfo()) + ")";
                                        else
                                            posneg = "DƯƠNG TÍNH (" + resultDouble.Value.ToString(Folder_File_Tool.GetCultureInfo()) + ")";
                                    }
                                    else if (map.TestCode.Code == "219FAS")
                                    {
                                        if (resultDouble < 0.24)
                                            posneg = "ÂM TÍNH (" + resultDouble.Value.ToString(Folder_File_Tool.GetCultureInfo()) + ")";
                                        else
                                            posneg = "DƯƠNG TÍNH (" + resultDouble.Value.ToString(Folder_File_Tool.GetCultureInfo()) + ")";
                                    }
                                    else if (map.TestCode.Code == "223MYC")
                                    {
                                        if (resultDouble < 0.36)
                                            posneg = "ÂM TÍNH (" + resultDouble.Value.ToString(Folder_File_Tool.GetCultureInfo()) + ")";
                                        else
                                            posneg = "DƯƠNG TÍNH (" + resultDouble.Value.ToString(Folder_File_Tool.GetCultureInfo()) + ")";
                                    }
                                }
                            }

                            ResultStandardBL.Insert(seq, map, result, posneg, insertTime);
                            i = i + 5;
                        }
                    }
                }
                catch { }
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
