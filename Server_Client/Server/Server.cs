using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public partial class Server : Form
    {
        IPEndPoint ipe;
        Socket socket;
        TcpListener tcplisten;
        List<Socket> lstSocket = new List<Socket>();
        public byte FS = 28;
        public byte CR = 13;
        public byte VT = 11;
        public byte ACK = 6;

        public Server()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                var ip = this.txtIP.Text;
                var port = int.Parse(this.txtPort.Text);
                ipe = new IPEndPoint(IPAddress.Parse(ip), port);
                tcplisten = new TcpListener(ipe);
                tcplisten.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                tcplisten.Start();
                Thread thread = new Thread(() =>
                {
                    while (true)
                    {
                        try
                        {
                            socket = tcplisten.AcceptSocket();
                            lstSocket.Add(socket);

                            if (lstSocket.Count >= 2)
                            {
                                lstSocket[0].Close();
                                lstSocket.RemoveAt(0);
                            }
                            Thread rec = new Thread(Receive);
                            rec.IsBackground = true;
                            rec.Start(socket);
                        }
                        catch (Exception ex)
                        {
                            break;
                        }
                    }
                });
                thread.IsBackground = true;
                thread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể start server", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public void Receive(Object obj)
        {
            try
            {
                while (true)
                {
                    byte[] byteData = new byte[socket.ReceiveBufferSize];
                    socket.Receive(byteData);
                    string data = Encoding.ASCII.GetString(byteData);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            try
            {
                if (lstSocket != null)
                {
                    foreach (var socket in lstSocket)
                        if (socket != null) socket.Close();
                }
                if (tcplisten != null)
                    tcplisten.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể stop server", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            byte[] byteData = File.ReadAllBytes(@"D:\LISs\testcode\MISSION_U500.txt");
            //byte[] byteData = Encoding.ASCII.GetBytes("Oanh");
            socket.Send(byteData);
        }

        private void Server_Load(object sender, EventArgs e)
        {

        }
    }
}
