using Connects.BL;
using Connects.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;

namespace Connects.Devices
{
    public  class Panel60
    {
        private static long deviceID = 0;
        private static Connect connect = null;
        private static readonly ILog log4net = LogManager.GetLogger(typeof(Panel60).Name);
        private static System.Timers.Timer timer = null;
        private static string folderPath = string.Empty;

        public static bool Start(IPEndPoint iPEndPoint, long _deviceID)
        {
            try
            {              
                deviceID = _deviceID;
                connect = ConnectBL.Get_ConnectByDevicePKAndComputer(deviceID, Environment.MachineName);
                folderPath = connect.Description;
                InitTimer();
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
            var autoGetResult = SettingBL.Get_Setting("AutoGetResult_Panel60");
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
                if (!Directory.Exists(folderPath)) return;
                DirectoryInfo di = new DirectoryInfo(folderPath);
                FileSystemInfo[] files = di.GetFileSystemInfos();
                var lstFile = files.Where(f => f.Name.EndsWith(".txt")).ToList();
                if (lstFile == null || lstFile.Count <= 0) return;
                var lstMap = MapBL.Get_ListMapByDeviceID(deviceID);
                var seq = string.Empty;

                foreach (var file in lstFile)
                {
                    if(file.Name.Contains(".out"))
                    {
                        var stringOut = File.ReadAllText(file.FullName);
                        var lstLineOut = stringOut.Split((char)ProtocolASCII.LF);
                        foreach (var lineOut in lstLineOut)
                        {
                            try
                            {
                                var arrayLineOut = lineOut.Split("=");
                                if (arrayLineOut != null && arrayLineOut.Length >= 2 && arrayLineOut[0] == "SampleNo")
                                {
                                    seq = arrayLineOut[1].Trim();
                                    var arrayFileNameOut = file.Name.Split("_").ToList();
                                    if(arrayFileNameOut != null && arrayFileNameOut.Count >= 3)
                                    {
                                        var fileNameResult = "Results_" + arrayFileNameOut[1] + "_" + arrayFileNameOut[2].Split(".")[0] + ".txt";
                                        var filePathResult = Path.Combine(folderPath, fileNameResult);
                                        if (File.Exists(filePathResult))
                                        {
                                            var stringResult = File.ReadAllText(Path.Combine(folderPath, fileNameResult));
                                            var lstLineResult = stringResult.Split((char)ProtocolASCII.LF);
                                            foreach (var lineResult in lstLineResult)
                                            {
                                                var arrayLineResult = lineResult.Split(",").ToList();
                                                if(arrayLineResult != null && arrayLineResult.Count >= 3)
                                                {
                                                    var insertTime = DateTimeServer.Get_DateServerByEntity();
                                                    var testCodeIn = lineResult.Contains('"') ? arrayLineResult[2].Trim() : arrayLineResult[1].Trim();
                                                    var value = lineResult.Contains('"') ? arrayLineResult[3].Trim() : arrayLineResult[2].Trim();
                                                    var posneg = string.Empty;
                                                    double? result = null;

                                                    var map = lstMap.Where(p => p.TestcodeIn.Trim() == testCodeIn.Trim()).FirstOrDefault();
                                                    if (map != null)
                                                    {
                                                        try
                                                        {
                                                            var lstValue = value.ToList();
                                                            if (lstValue != null && lstValue.Count > 0 && Char.IsNumber(lstValue[0]))
                                                            {
                                                                result = double.Parse(value);                                                               
                                                            }
                                                            else
                                                            {
                                                                lstValue.RemoveAt(0);
                                                                result = double.Parse(new string(lstValue.ToArray()));
                                                            }
                                                            posneg = result > map?.TestCode?.HigherLimit ? value + " DƯƠNG TÍNH" : value + " ÂM TÍNH";
                                                        }
                                                        catch
                                                        {
                                                            posneg = value;
                                                        }

                                                        ResultStandardBL.Insert(seq, map, null, posneg, insertTime);
                                                    }                                                                                              
                                                }
                                            }
                                            try
                                            {
                                                File.Delete(filePathResult);
                                            }
                                            catch { }
                                        }
                                    }                                  
                                }                               
                            }
                            catch (Exception ex)
                            {
                                log4net.Error(ex);
                            }
                        }
                        try
                        {
                            File.Delete(file.FullName);
                        }
                        catch { }
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
