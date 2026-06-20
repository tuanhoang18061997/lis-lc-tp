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
    public class MispaCountPlus_IP
    {
        private static readonly ILog log4net = LogManager.GetLogger(typeof(MispaCountPlus_IP).Name);
        private static Socket socket;
        private static TcpListener tcpListener;
        private static List<Socket> lstSocket = new List<Socket>();
        private static string inputData = string.Empty;
        private static string EndResult = "END_RESULT";
        private static string SID = "SID";
        private static string PID = "PID";
        private static bool isWriteFile = false;
        private static long deviceID = 0;
        private static System.Timers.Timer timer;
        private static Connect connect = null;
        private static bool socketConnectForUpdateStatus = false;

        public static bool Start(IPEndPoint iPEndPoint, long _deviceID)
        {
            try
            {
                deviceID = _deviceID;
                tcpListener = new TcpListener(iPEndPoint);
                tcpListener.Start();
                Thread thread = new Thread(() =>
                {
                    ServerListen();
                });
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
                if (lstSocket != null)
                {
                    foreach (var socket in lstSocket)
                        if (socket != null) socket.Close();
                }
                if (tcpListener != null) tcpListener.Stop();
                RemoveTimer();
                return true;
            }
            catch (Exception ex)
            {
                log4net.Error(ex);
                return false;
            }
        }

        private static void ServerListen()
        {
            while (true)
            {
                try
                {
                    socket = tcpListener.AcceptSocket();
                    lstSocket.Add(socket);
                    if (lstSocket.Count >= 2)
                    {
                        lstSocket[0].Close();
                        lstSocket.RemoveAt(0);
                    }
                    Thread rec = new Thread(ServerReceive);
                    rec.IsBackground = true;
                    rec.Start(socket);
                }
                catch (Exception ex)
                {
                    log4net.Error(ex);
                    break;
                }
            }
        }

        public static void ServerReceive(System.Object obj)
        {
            try
            {
                Socket socket = obj as Socket;
                while (true)
                {
                    byte[] byteData = new byte[socket.ReceiveBufferSize];
                    socket.Receive(byteData);
                    string dataRemovedByteNull = RemoveByteNull(byteData);                 
                    inputData += dataRemovedByteNull;
                    if (!string.IsNullOrEmpty(dataRemovedByteNull.Trim())) Update_Waiting_Receving();
                    if(isWriteFile) Folder_File_Tool.Create_Write_File(typeof(MispaCountPlus_IP).Name, true, dataRemovedByteNull);
                    byte[] ASCIIline = Encoding.ASCII.GetBytes(inputData);

                    //if (inputData.Contains(EndResult) && ASCIIline[ASCIIline.Length-2] == ProtocolASCII.CR && ASCIIline[ASCIIline.Length - 1] == ProtocolASCII.LF)
                    //{
                    //    ProcessForResult(inputData);
                    //    inputData = string.Empty;
                    //}
                }
            }
            catch (Exception ex)
            {
                log4net.Error(ex);
            }
        }

        public static void HostSend_ACK()
        {
            string stringACK = Encoding.Default.GetString(new byte[1] { ProtocolASCII.ACK });
            byte[] byteData = Encoding.ASCII.GetBytes(stringACK);
            socket.Send(byteData);
        }

        private static void ProcessForResult(string inputData)
        {
            if (string.IsNullOrEmpty(inputData)) return;
            try
            {
                var arrayInputData = inputData.Split((char)ProtocolASCII.LF);
                var seq = string.Empty;
                DateTime insertTime = DateTimeServer.Get_DateServerByEntity();
                var lstMap = MapBL.Get_ListMapByDeviceID(deviceID);
                var lstTestCodeInResult = new List<string>();

                foreach (var line in arrayInputData)
                {
                    try
                    {
                        var arrayLine = line.Split(';');
                        if (arrayLine[0].Trim() == SID)
                        {
                            seq = arrayLine[1].Trim();
                        }
                        if(string.IsNullOrEmpty(seq) && arrayLine[0].Trim() == PID)
                        {
                            seq = arrayLine[1].Trim();
                        }

                        var testCodeIn = arrayLine[0].Trim();
                        var resultTmp = arrayLine[1].Trim();
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
            catch (Exception ex)
            {
                log4net.Error(ex);
            }
        }

        private static string RemoveByteNull(byte[] byteInput)
        {
            string stringData = Encoding.Default.GetString(byteInput);
            int startIndexNull = stringData.IndexOf((char)ProtocolASCII.NUL);
            stringData = stringData.Substring(0, startIndexNull);
            return stringData;
        }

        public static void Clear()
        {
            inputData = string.Empty;
        }

        public static void WriteFile(bool _isWriteFile)
        {
            isWriteFile = _isWriteFile;
        }

        public static bool CheckConnect()
        {
            return true;
        }

        public static void InitTimer()
        {
            if (timer == null)
            {
                timer = new System.Timers.Timer();
                timer.Elapsed += Timer_Elapsed;
                timer.Interval = 1000;
                timer.Start();
            }
            if (connect == null) connect = ConnectBL.Get_ConnectByDevicePKAndComputer(deviceID, Environment.MachineName);
        }

        public static void RemoveTimer()
        {
            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }
        }

        public static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (socketConnectForUpdateStatus != SocketConnect(out socketConnectForUpdateStatus))
            {
                Connects.Program.mainForm.UpdateConnectForServer(connect, socketConnectForUpdateStatus);
            }
        }

        private static bool SocketConnect(out bool connected1)
        {
            connected1 = false;
            try
            {
                if (socket != null && !socket.Poll(10000, SelectMode.SelectRead)) connected1 = true;
                return connected1;
            }
            catch
            {
                return connected1;
            }
        }

        private static void Update_Waiting_Receving()
        {
            Connects.Program.mainForm.Update_Waiting_Receving(connect);
        }
    }
}
