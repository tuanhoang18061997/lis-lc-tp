using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace Client
{
    public partial class Client : Form
    {
        IPEndPoint ipe;
        TcpClient tcpclient;
        NetworkStream stream;
        public byte ACK = 6;
        public Client()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }
           
        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                ipe = new IPEndPoint(IPAddress.Parse(txtIP.Text), int.Parse(txtPort.Text));
                tcpclient = new TcpClient();
                tcpclient.Connect(ipe);
                stream = tcpclient.GetStream();
                Thread thread = new Thread(Receive);
                thread.IsBackground = true;
                thread.Start();
            }
            catch (Exception ex)
            {
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (stream != null)
                    stream.Close();
                if (tcpclient != null)
                    tcpclient.Close();
            }
            catch(Exception ex)
            {
            }
        }

        public void Receive()
        {
            try
            {
                while (true)
                {
                    byte[] byteData = new byte[tcpclient.ReceiveBufferSize];
                    stream.Read(byteData, 0, byteData.Length);
                    string data = Encoding.UTF8.GetString(byteData);

                    //Create_Write_File("CellDynRuby.txt", "Oanh");
                    Create_Write_File("CellDynRuby.txt", data);

                    //byte[] byteDataACK = Encoding.ASCII.GetBytes(Encoding.Default.GetString(new byte[1] { ACK }));
                    //stream.Write(byteDataACK, 0, byteDataACK.Length);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] byteData = File.ReadAllBytes(@"D:\LISs\testcode\MISSION_U500_IP.txt");
                //byte[] byteData = Encoding.ASCII.GetBytes("adasdasd");
                stream.Write(byteData, 0, byteData.Length);
            }
            catch (Exception ex)
            {
            }
        }

        public void Create_Write_File(string file, string data)
        {
            if (!File.Exists(file))
            {
                FileStream fs = new FileStream(file, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                sw.Write(data);
                sw.Flush();
                fs.Close();
            }
            else
            {
                string currentData = File.ReadAllText(file);
                string newData = currentData + data;
                File.WriteAllText(file, newData);
            }
        }

        private void Client_Load(object sender, EventArgs e)
        {

        }
    }
}
