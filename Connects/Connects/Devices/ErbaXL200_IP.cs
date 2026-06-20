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
    public class ErbaXL200_IP
    {
        private static readonly ILog log4net = LogManager.GetLogger(typeof(ErbaXL200_IP).Name);
        private static Socket socket;
        private static TcpListener tcpListener;
        private static List<Socket> lstSocket = new List<Socket>();
        private static string stringData = string.Empty;
        private static byte[] byteStringData = null;
        private static string O = "O";
        private static string R = "R";
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
                    stringData += dataRemovedByteNull;
                    if (isWriteFile) Folder_File_Tool.Create_Write_File(typeof(ErbaXL200_IP).Name, true, dataRemovedByteNull);
                    byte[] ENQ_EOT = Encoding.ASCII.GetBytes(dataRemovedByteNull);
                    if (ENQ_EOT[0] == ProtocolASCII.ENQ)
                    {
                        //GẮN TEST
                        //ProcessResult(stringData);
                        //stringData = string.Empty;
                        HostSend_ACK();
                    }
                    else if (ENQ_EOT[0] == ProtocolASCII.EOT)
                    {
                        ProcessResult(stringData);
                        stringData = string.Empty;
                    }
                    else
                    {
                        byte[] ASCIIline = Encoding.ASCII.GetBytes(stringData);
                        if (ASCIIline.Contains(ProtocolASCII.STX) && (ASCIIline.Contains(ProtocolASCII.ETX) || ASCIIline.Contains(ProtocolASCII.ETB)) &&
                            ASCIIline[ASCIIline.Length - 2] == ProtocolASCII.CR && ASCIIline[ASCIIline.Length - 1] == ProtocolASCII.LF)
                        {
                            HostSend_ACK();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log4net.Error(ex);
                stringData = string.Empty;
            }
        }

        private static void ProcessResult(string inputData)
        {
            if (string.IsNullOrEmpty(inputData)) return;
            try
            {
                var arrayInputData = inputData.Split((char)ProtocolASCII.CR);
                var insertTime = DateTimeServer.Get_DateServerByEntity();
                string seq = string.Empty;

                foreach (var line in arrayInputData)
                {
                    var arrayLine = line.Split('|');
                    if (arrayLine[0] == O)
                    {
                        seq = !string.IsNullOrEmpty(arrayLine[2]) ? arrayLine[2] : arrayLine[3];
                    }
                    if (arrayLine[0] == R)
                    {
                        var testCodeIn = arrayLine[2].Split('^', StringSplitOptions.RemoveEmptyEntries)[^1];
                        var resultTmp = arrayLine[3].Trim();
                        double? result = null;
                        var posneg = string.Empty;
                        Folder_File_Tool.ParseResult(resultTmp, out result, out posneg);
                        var map = MapBL.Get_MapByTestCodeIn(deviceID, testCodeIn);
                        if (map != null)
                        {
                            ResultStandardBL.Insert(seq, map, result, posneg, insertTime);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log4net.Error(ex);
            }
        }

        //public static void HostSend_ACK(string stringData)
        //{
        //    var time = stringData.Split('|')[6];
        //    var controlID = stringData.Split('|')[9];
        //    string stringACK = (char)ProtocolASCII.VT + @"MSH|^~\&|||||"+ time + "||ACK^R01|"+ controlID + "|P|2.3.1||||0||ASCII|||" + (char)ProtocolASCII.CR;
        //    stringACK += @"MSA|AA|" + controlID + "|Message accepted|||0|" + (char)ProtocolASCII.CR + (char)ProtocolASCII.FS + (char)ProtocolASCII.CR;
        //    byte[] byteData = Encoding.ASCII.GetBytes(stringACK);
        //    if (isWriteFile) Folder_File_Tool.Create_Write_File(typeof(MindrayBC20s_IP_Server).Name, true, stringACK);
        //    socket.Send(byteData);
        //}

        public static void HostSend_ACK()
        {
            string stringACK = Encoding.Default.GetString(new byte[1] { ProtocolASCII.ACK });
            byte[] byteData = Encoding.ASCII.GetBytes(stringACK);
            socket.Send(byteData);
        }

        private static string RemoveByteNull(byte[] byteInput)
        {
            string stringData = Encoding.Default.GetString(byteInput);
            int startIndexNull = stringData.IndexOf((char)ProtocolASCII.NUL);
            if(startIndexNull != -1)
                stringData = stringData.Substring(0, startIndexNull);
            return stringData;
        }

        public static void Clear()
        {
            stringData = string.Empty;
            byteStringData = null;
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
