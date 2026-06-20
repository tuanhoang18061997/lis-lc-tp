using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Connects.BL;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using Connects.Models;

namespace Connects.Devices
{
    public class MindrayBC5380_IP_SERVER
    {
        private static string className = typeof(MindrayBC5380_IP_SERVER).Name;
        private static readonly ILog log4net = LogManager.GetLogger(typeof(MindrayBC5380_IP_SERVER).Name);
        private static string inputData = string.Empty;
        private static string line = string.Empty;
        private static string text_O = "O";
        private string text_R = "R";
        private static string text_Result = "N";
        private static string text_QC = "Q";
        private static string linePNG = ".PNG";

        private static Connect connect = null;
        private static IPEndPoint ipe;
        private static TcpClient tcpclient;
        private static NetworkStream stream;
        private static System.Timers.Timer timer;
        private static bool connectedServer = false;
        private static bool isWriteFile = false;
        private static long deviceID = 0;

        public static bool Start(IPEndPoint iPEndPoint, long _deviceID)
        {
            try
            {
                deviceID = _deviceID;
                connect = ConnectBL.Get_ConnectByDeviceId(deviceID);
                ipe = iPEndPoint;
                tcpclient = new TcpClient();
                tcpclient.Connect(ipe);
                stream = tcpclient.GetStream();
                Thread thread = new Thread(Receive);
                thread.IsBackground = true;
                thread.Start();
                InitTimer();
                return true;
            }
            catch (Exception ex)
            {
                log4net.Error(ex);
                return false;
            }
        }

        public static bool Close()
        {
            try
            {
                if (stream != null) stream.Close();
                if (tcpclient != null) tcpclient.Close();
                RemoveTimer();
                return true;
            }
            catch (Exception ex)
            {
                log4net.Error(ex);
                return false;
            }
        }

        public static void Receive()
        {
            try
            {
                while (true)
                {
                    try
                    {
                        byte[] byteData = new byte[tcpclient.ReceiveBufferSize];
                        stream.Read(byteData, 0, byteData.Length);
                        string input = RemoveByteNull(byteData);
                        if (!string.IsNullOrEmpty(input.Trim())) Update_Waiting_Receving();
                        if (isWriteFile) Folder_File_Tool.Create_Write_File(className, true, input);

                        //byte[] arrayByteInput = Encoding.ASCII.GetBytes(input);
                        //if (arrayByteInput[0] == ProtocolASCII.ENQ)
                        //{
                        //    HostSend_ACK();
                        //}
                        //else if (arrayByteInput[0] == ProtocolASCII.EOT)
                        //{
                        //    Process_TestResult(inputData, connect);
                        //    inputData = string.Empty;
                        //    line = string.Empty;
                        //}
                        //else
                        //{
                        //    inputData += input;
                        //    line += input;
                        //    byte[] ASCIIline = Encoding.ASCII.GetBytes(line);
                        //    if (ASCIIline[0] == ProtocolASCII.STX && ASCIIline[ASCIIline.Length - 2] == ProtocolASCII.CR && ASCIIline[ASCIIline.Length - 1] == ProtocolASCII.LF)
                        //    {
                        //        HostSend_ACK();
                        //        line = string.Empty;
                        //    }
                        //}
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                log4net.Error(ex);
            }
        }

        public static void Process_TestResult(string inputData, Connect connect)
        {
            //try
            //{
            //    DateTime insertTime = new DateTime();
            //    var arrayInputData = ConcatLine(inputData);
            //    var seq = string.Empty;
            //    var resultType = string.Empty;
            //    List<ResultStandardItemParam> listParamInsert = new List<ResultStandardItemParam>();

            //    if (arrayInputData != null && arrayInputData.Length > 0)
            //    {
            //        foreach (var line in arrayInputData)
            //        {
            //            try
            //            {
            //                var arrayLine = line.Split('|');
            //                if (arrayLine != null)
            //                {
            //                    if (arrayLine[0].Contains(text_O) && arrayLine[0].Length == 2)
            //                    {
            //                        resultType = arrayLine[11];
            //                        var arrayText = arrayLine[3].Split('^');
            //                        if (arrayText != null)
            //                        {
            //                            if (resultType == text_Result)
            //                                seq = arrayText[2].Trim();
            //                            else if (resultType == text_QC)
            //                                seq = arrayText[2].Trim().Substring(0, 2) + arrayText[2].Trim().Substring(arrayText[2].Trim().Length - 2, 2);
            //                        }
            //                    }

            //                    var testCodeIn = string.Empty;
            //                    var resultTmp = string.Empty;
            //                    var posneg = string.Empty;
            //                    double? result = null;

            //                    if (arrayLine[0].Contains(text_R) && arrayLine[0].Length == 2)
            //                    {
            //                        resultTmp = arrayLine[3];
            //                        Folder_File_Tool.ParseResult(resultTmp, out result, out posneg);
            //                        var arrayText = arrayLine[2].Split('^');
            //                        if (arrayText != null)
            //                        {
            //                            testCodeIn = arrayText[4];
            //                        }
            //                        try
            //                        {
            //                            var year = arrayLine[12].Substring(0, 4);
            //                            var month = arrayLine[12].Substring(4, 2);
            //                            var date = arrayLine[12].Substring(6, 2);
            //                            var hours = arrayLine[12].Substring(8, 2);
            //                            var minute = arrayLine[12].Substring(10, 2);
            //                            insertTime = DateTime.Parse(year + "-" + month + "-" + date + " " + hours + ":" + minute);
            //                        }
            //                        catch
            //                        {
            //                            insertTime = DateTimeServer.Now();
            //                        }

            //                        if (!resultTmp.Contains(linePNG))
            //                        {
            //                            listParamInsert.Add(new ResultStandardItemParam() { seq = seq, testCodeIn = testCodeIn, result = result, posneg = posneg, insertTime = insertTime });
            //                        }
            //                    }
            //                }
            //            }
            //            catch (Exception ex)
            //            {
            //                log4net.Error(ex);
            //            }
            //        }
            //    }
            //    if (listParamInsert.Count > 0)
            //    {
            //        ResultStandardBL.Insert(new ResultStandardParam() { items = listParamInsert, deviceIds = connect.getDeviceIds() }, connect);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    log4net.Error(ex);
            //}
        }

        public static string[] ConcatLine(string inputData)
        {
            try
            {
                var arrayLine = inputData.Split((char)ProtocolASCII.STX);

                for (int i = 0; i < arrayLine.Length; i++)
                {
                    if (arrayLine[i].Contains(((char)ProtocolASCII.ETB).ToString()))
                    {
                        var line = arrayLine[i].Substring(0, arrayLine[i].Length - 5) + arrayLine[i + 1];
                        arrayLine[i] = line;
                    }
                }

                return arrayLine;
            }
            catch (Exception ex)
            {
                log4net.Error(ex);
                return null;
            }
        }

        public static void HostSend_ACK()
        {
            byte[] byteData = Encoding.ASCII.GetBytes(Encoding.Default.GetString(new byte[1] { ProtocolASCII.ACK }));
            if (isWriteFile) Folder_File_Tool.Create_Write_File(className, true, Encoding.Default.GetString(new byte[1] { ProtocolASCII.ACK }));
            stream.Write(byteData, 0, byteData.Length);
        }

        public static bool CheckConnect()
        {
            return false;
        }

        public static void Clear()
        {
            inputData = string.Empty;
            line = string.Empty;
        }

        public static void WriteFile(bool _isWriteFile)
        {
            isWriteFile = _isWriteFile;
        }

        public static void InitTimer()
        {
            connectedServer = true;
            if (timer == null)
            {
                timer = new System.Timers.Timer();
                timer.Elapsed += Timer_Elapsed;
                timer.Interval = 1000;
                timer.Start();
            }
        }

        public static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (connectedServer != tcpclient.Connected)
            {
                connectedServer = tcpclient.Connected;
                Connects.Program.mainForm.UpdateConnectForClient(connect, connectedServer);
            }
            if (!connectedServer) ReStart();
        }

        public static void ReStart()
        {
            try
            {
                CloseTmp();
                Connects.Program.mainForm.UpdateConnectForClient(connect, StartTmp());
            }
            catch (Exception ex)
            {
                log4net.Error(ex);
            }
        }

        public static bool CloseTmp()
        {
            try
            {
                if (stream != null) stream.Close();
                if (tcpclient != null) tcpclient.Close();
                return true;
            }
            catch (Exception ex)
            {
                log4net.Error(ex);
                return false;
            }
        }

        public static void RemoveTimer()
        {
            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }
        }

        public static bool StartTmp()
        {
            try
            {
                tcpclient = new TcpClient();
                tcpclient.Connect(ipe);
                stream = tcpclient.GetStream();
                Thread thread = new Thread(Receive);
                thread.IsBackground = true;
                thread.Start();
                return true;
            }
            catch (Exception ex)
            {
                log4net.Error(ex);
                return false;
            }
        }

        private static string RemoveByteNull(byte[] byteInput)
        {
            string stringData = Encoding.Default.GetString(byteInput);
            int startIndexNull = stringData.IndexOf((char)ProtocolASCII.NUL);
            stringData = stringData.Substring(0, startIndexNull);
            return stringData;
        }

        private static void Update_Waiting_Receving()
        {
            Program.mainForm.Update_Waiting_Receving(connect);
        }
    }
}
