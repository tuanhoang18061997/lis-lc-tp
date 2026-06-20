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
using System.Collections;

namespace Connects.Devices
{
    public class SwelabAlfa
    {
        public static string inputData = string.Empty;
        public static string stringData = string.Empty;
        private static readonly ILog log4net = LogManager.GetLogger(typeof(SwelabAlfa).Name);
        public static string sidForUrine = string.Empty;
        private static SerialPort serialPort = new SerialPort();
        public static Connect connect = null;
        private static bool isWriteFile = false;
        private string text_OBR = "OBR";
        private string text_OBX = "OBX";
        private string text_NM = "NM";

        public static void AnalysisPatient(SerialPort sp, Connect _connect, string _sidForUrine)
        {
            try
            {
                string input = sp.ReadExisting();
                sidForUrine = _sidForUrine;
                serialPort = sp;
                connect = _connect;
                inputData += input;
                stringData += input;

                if (isWriteFile)
                    Folder_File_Tool.Create_Write_File(typeof(SwelabAlfa).Name, true, input);
                byte[] ASCIIValues = Encoding.ASCII.GetBytes(stringData);

                if (ASCIIValues[0] == ProtocolASCII.VT)
                {
                    ProcessResult(inputData, connect);
                    Clear();
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
            var insertTime = DateTimeServer.Get_DateServerByEntity(); // Lấy thời gian hiện tại từ server
            var lstMap = MapBL.Get_ListMapByDeviceID(connect.DeviceId); // Lấy danh sách mã xn đã map theo DeviceID
            if (lstMap == null || lstMap.Count == 0) return;
            var arrayInput = inputData.Split((char)ProtocolASCII.CR);
            var seq = sidForUrine;

            if (arrayInput != null && arrayInput.Length > 0)
            {
                foreach (var line in arrayInput)
                {
                    var arrayLine = line.Split('|');

                    if (arrayLine[0].Contains("OBR"))
                    {
                        seq = arrayLine[4];
                    }

                    if (arrayLine[0].Contains("OBX") && arrayLine[2].Contains("NM"))
                    {
                        try
                        {
                            var testCodeIn = arrayLine[3];
                            var resultTmp = arrayLine[5];
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

        private void Update_Waiting_Receving()
        {
            Connects.Program.mainForm.Update_Waiting_Receving(connect);
        }
    }
}
