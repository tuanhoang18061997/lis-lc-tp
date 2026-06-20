using Connects.BL;
using Connects.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;

namespace Connects.Devices
{
    public  class PCR
    {
        private static long deviceID = 0;
        private static Connect connect = null;
        private static readonly ILog log4net = LogManager.GetLogger(typeof(PCR).Name);
        private static System.Timers.Timer timer = null;
        private static string folderPath = string.Empty;

        public static bool Start(IPEndPoint iPEndPoint, long _deviceID)
        {
            try
            {
                InitTimer();
                deviceID = _deviceID;
                connect = ConnectBL.Get_ConnectByDevicePKAndComputer(deviceID, Environment.MachineName);
                folderPath = connect.Description;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool Close()
        {
            timer.Stop();
            timer.Dispose();
            timer = null;
            return true;
        }

        public static void InitTimer()
        {
            timer = new System.Timers.Timer();
            var autoGetResult = SettingBL.Get_Setting("AutoGetResult");
            int timeTick = 120000;
            if (autoGetResult != null) timeTick = int.Parse(autoGetResult.Value);
            timer.Elapsed += Timer_Elapsed;
            timer.Interval = timeTick;
            timer.Start();
        }

        public static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Get_TestResutFile();
        }

        public static void Get_TestResutFile()
        {
            try
            {
                var listFileXml = Directory.EnumerateFiles(folderPath, "*.*").Where(s => s.EndsWith(".xml")).ToList();
                var lstMap = MapBL.Get_ListMapByDeviceID(deviceID);
                if (listFileXml != null && listFileXml.Count > 0 && lstMap != null && lstMap.Count > 0)
                {
                    foreach(var file in listFileXml.ToList())
                    {
                        try
                        {
                            var insertTime = DateTimeServer.Get_DateServerByEntity();
                            var seq = string.Empty;
                            var testCodeIn = string.Empty;
                            var resultTmp = string.Empty;
                            double? result = null;
                            var posneg = string.Empty;
                            double? resultDouble = null;
                            var resultString = string.Empty;

                            XmlDocument doc = new XmlDocument();
                            XmlNodeList xmlnode;
                            FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
                            doc.Load(fileStream);
                            xmlnode = doc.GetElementsByTagName("plate");
                            for (int i = 0; i < xmlnode[0].ChildNodes.Count; i++)
                            {
                                try
                                {
                                    seq = posneg = testCodeIn = resultTmp = resultString = string.Empty;
                                    result = resultDouble = null;
                                    result = null;
                                   var nodeCell = xmlnode[0].ChildNodes[i];
                                    if (nodeCell.Name == "cell")
                                    {
                                        seq = nodeCell.Attributes["name"].Value;
                                        for (int j = 0; j < nodeCell.ChildNodes.Count; j++)
                                        {
                                            var nodeChild = nodeCell.ChildNodes[j];
                                            if (nodeChild.Name == "channel")
                                            {
                                                var nodeResult = nodeChild.ChildNodes[0];
                                                if (nodeResult.Name == "result")
                                                {
                                                    resultTmp = nodeResult.Attributes["value"].Value;
                                                }
                                            }

                                            if (nodeChild.Name == "test")
                                            {
                                                testCodeIn = nodeChild.Attributes["id"].Value;
                                                var nodeResult = nodeChild.ChildNodes[0];
                                                if (nodeResult.Name == "result")
                                                {
                                                    posneg = nodeResult.Attributes["value"].Value;
                                                }
                                            }
                                        }                                       
                                    }
                                    if (!string.IsNullOrEmpty(seq.Trim()) && !string.IsNullOrEmpty(posneg.Trim()) && !string.IsNullOrEmpty(testCodeIn.Trim()))
                                    {
                                        if (posneg == "-") posneg = "ÂM TÍNH";
                                        else if (posneg == "+") posneg = "DƯƠNG TÍNH";
                                        else posneg = "KHÔNG XÁC ĐỊNH";
                                        Folder_File_Tool.ParseResult(resultTmp, out resultDouble, out resultString);
                                        result = (resultDouble == null || resultDouble == 0) ? null : resultDouble;
                                        var map = lstMap.Where(p => p.TestcodeIn == testCodeIn).FirstOrDefault();
                                        if (map == null) continue;
                                        ResultStandardBL.Insert(seq, map, result, posneg, insertTime);
                                    }
                                }
                                catch { }
                            }
                            listFileXml.Remove(file);
                            fileStream.Close();
                            File.Delete(file);
                        }
                        catch (Exception ex)
                        {
                            log4net.Error(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log4net.Error(ex);
            }
        }

        public static void Clear()
        {
        }
        public static bool CheckConnect()
        {
            return false;
        }
    }
}
