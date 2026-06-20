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
    public class U120
    {
        public static string inputData = string.Empty;
        private static readonly ILog log4net = LogManager.GetLogger(typeof(U120).Name);
        public static string sidForUrine = string.Empty;
        private static bool isWriteFile = false;

        public static void AnalysisPatient(SerialPort sp, Connect connect, string _sidForUrine)
        {
            try
            {
                sidForUrine = _sidForUrine;
                string input = sp.ReadExisting();
                if (isWriteFile) Folder_File_Tool.Create_Write_File(typeof(U120).Name, true, input);
                inputData += input;
                byte[] ASCIIValues = Encoding.ASCII.GetBytes(inputData);

                if ((ASCIIValues.Contains(ProtocolASCII.STX) && ASCIIValues.Contains(ProtocolASCII.ETX)))
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

        public static void ProcessResult(string inputData, Connect connect)
        {
            var insertTime = DateTimeServer.Get_DateServerByEntity();
            var arrayInput = inputData.Split((char)ProtocolASCII.LF);
            var lstMap = MapBL.Get_ListMapByDeviceID(connect.DeviceId);
            if (lstMap == null || lstMap.Count == 0) return;
            var operatorID = string.Empty;
            var Seq = string.Empty;

            foreach (var line in arrayInput)
            {
                Seq = sidForUrine;
                if (string.IsNullOrEmpty(Seq) && line.Contains("Operator"))
                {
                    var index = line.IndexOf(':');
                    if (index != -1)
                    {
                        operatorID = line.Substring(index + 1).Trim();
                    }
                }
                else if (string.IsNullOrEmpty(Seq) && line.Contains("No."))
                {
                    Seq = double.Parse(line.Trim().Split('.')[1].ToString()).ToString();
                }
                else
                {
                    var arrayItem = line.Trim().Split(' ');
                    var testCodeIn = arrayItem[0].Length == 4 ? arrayItem[0].Substring(1) : arrayItem[0];
                    var map = lstMap.Where(p => p.TestcodeIn == testCodeIn).FirstOrDefault();
                    if (map == null) continue;
                    var lstItem = new List<string>();
                    string posneg = string.Empty;
                    double? result = null;

                    foreach (var item in arrayItem)
                    {
                        if (!string.IsNullOrEmpty(item.Trim()))
                        {
                            lstItem.Add(item.Trim());
                        }
                    }

                    if (lstItem.Count >= 2)
                    {
                        var resultTmp = lstItem[1];
                        if (resultTmp == "-" || resultTmp == "+")
                        {
                            posneg = resultTmp == "-" ? "ÂM TÍNH" : "DƯƠNG TÍNH";
                        }
                        else
                        {
                            Folder_File_Tool.ParseResult(resultTmp, out result, out posneg);
                        }
                            
                        ResultStandardBL.Insert(Seq, map, result, posneg, insertTime);
                    }                 
                }
            }
        }

        public static void Clear()
        {
            inputData = string.Empty;
        }

        public static bool IsNumber(string value)
        {
            Regex regex = new Regex(@"^[-+]?[0-9]*\.?[0-9]+$");
            return regex.IsMatch(value);
        }

        public static void WriteFile(bool _isWriteFile)
        {
            isWriteFile = _isWriteFile;
        }
    }
}
