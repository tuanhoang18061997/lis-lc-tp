using Connects.BL;
using Connects.Devices;
using Connects.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;
using Type = System.Type;

namespace Connects
{
    public partial class frmConnect : Form
    {
        #region ****************************************************************************************** PROPERTY
        private readonly ILog log4net = LogManager.GetLogger(typeof(frmConnect).Name);
        private long deviceId = 0;
        private SerialPort SerialPort_01 = new SerialPort();
        private SerialPort SerialPort_02 = new SerialPort();
        private SerialPort SerialPort_03 = new SerialPort();
        private SerialPort SerialPort_04 = new SerialPort();
        private SerialPort SerialPort_05 = new SerialPort();
        private SerialPort SerialPort_06 = new SerialPort();

        private bool isConnect01 = false;
        private bool isConnect02 = false;
        private bool isConnect03 = false;
        private bool isConnect04 = false;
        private bool isConnect05 = false;
        private bool isConnect06 = false;

        private string computer = string.Empty;
        private string isCOM = "COM";
        private string nameSpace = "Connects.Devices";
        private string method = "AnalysisPatient";
        private string clear = "Clear";
        private string method_WriteFile = "WriteFile";
        private string methodIP_Start = "Start";
        private string methodIP_Close = "Close";
        private string method_Clear = "Clear";
        private string method_CheckConnect = "CheckConnect";
        private string startTimer = "StartTimer";
        private string stopTimer = "StopTimer";
        private string receving = "Receving ...";
        private string waiting = "Waiting ...";

        private Timer timerAutoUpdateTime = new Timer();
        private Timer timerAutoGetPatientInfo = new Timer();
        private Timer timerAutoPushResultBHYT = new Timer();

        private Timer timer1;
        private decimal count1 = 0;
        private decimal timerCount1 = 0;

        private Timer timer2;
        private decimal count2 = 0;
        private decimal timerCount2 = 0;

        private Timer timer3;
        private decimal count3 = 0;
        private decimal timerCount3 = 0;

        private Timer timer4;
        private decimal count4 = 0;
        private decimal timerCount4 = 0;

        private Timer timer5;
        private decimal count5 = 0;
        private decimal timerCount5 = 0;

        private Timer timer6;
        private decimal count6 = 0;
        private decimal timerCount6 = 0;

        private bool isConnectOk = false;
        #endregion

        public frmConnect()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            this.CheckConnectString();
        }

        #region ****************************************************************************************** EVENT

        private void frmConnect_Load(object sender, EventArgs e)
        {
            if (this.isConnectOk)
            {
                this.lblComputerName.Text = string.Empty;
                this.lbUser.Text = string.Empty;
                this.lbVersion.Text = string.Empty;

                this.computer = SystemInformation.ComputerName;

                // Xóa tất cả cổng COM không còn tồn tại trên phần cứng
                this.DeleteAllComNotExistOnComputer();

                // Cập nhật color - Cập nhật Status
                this.Update_Status_Color();

                // Load thông tin cổng COM và máy xét nghiệm đã chọn
                this.LoadComInfoAndSelectedDevice();

                // Load form login
                this.LoadLogin();
                SessionBL.Reset_Session();
            }
        }

        private void FrmConnectString_FormClosing(object sender, FormClosingEventArgs e)
        {
            frmConnectString frm = sender as frmConnectString;
            if (frm.isOk)
            {
                this.isConnectOk = true;
                this.InitializeEvent();
                this.InitializeTimer();
                this.frmConnect_Load(null, null);
            }
            else
            {
                Application.Exit();
            }
        }

        private void SerialPort_01_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            this.CallMethodProcess_1(sp, 1);
            this.count1++;
        }

        private void SerialPort_02_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            this.CallMethodProcess_2(sp, 2);
            this.count2++;
        }

        private void SerialPort_03_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            this.CallMethodProcess_3(sp, 3);
            this.count3++;
        }

        private void SerialPort_04_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            this.CallMethodProcess_4(sp, 4);
            this.count4++;
        }

        private void SerialPort_05_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            this.CallMethodProcess_5(sp, 5);
            this.count5++;
        }

        private void SerialPort_06_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            this.CallMethodProcess_6(sp, 6);
            this.count6++;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.count1 == 0 || this.timerCount1 == this.count1)
            {
                this.txtStatus1.Text = "Waiting ...";
                this.timerCount1 = this.count1 = 0;
            }
            else
                this.txtStatus1.Text = "Receving ...";
            this.timerCount1 = this.count1;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (this.count2 == 0 || this.timerCount2 == this.count2)
            {
                this.txtStatus2.Text = "Waiting ...";
                this.timerCount2 = this.count2 = 0;
            }
            else
                this.txtStatus2.Text = "Receving ...";
            this.timerCount2 = this.count2;
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            if (this.count3 == 0 || this.timerCount3 == this.count3)
            {
                this.txtStatus3.Text = "Waiting ...";
                this.timerCount3 = this.count3 = 0;
            }
            else
                this.txtStatus3.Text = "Receving ...";
            this.timerCount3 = this.count3;
        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            if (this.count4 == 0 || this.timerCount4 == this.count4)
            {
                this.txtStatus4.Text = "Waiting ...";
                this.timerCount4 = this.count4 = 0;
            }
            else
                this.txtStatus4.Text = "Receving ...";
            this.timerCount4 = this.count4;
        }

        private void timer5_Tick(object sender, EventArgs e)
        {
            if (this.count5 == 0 || this.timerCount5 == this.count5)
            {
                this.txtStatus5.Text = "Waiting ...";
                this.timerCount5 = this.count5 = 0;
            }
            else
                this.txtStatus5.Text = "Receving ...";
            this.timerCount5 = this.count5;
        }

        private void timer6_Tick(object sender, EventArgs e)
        {
            if (this.count6 == 0 || this.timerCount6 == this.count6)
            {
                this.txtStatus6.Text = "Waiting ...";
                this.timerCount6 = this.count6 = 0;
            }
            else
                this.txtStatus6.Text = "Receving ...";
            this.timerCount6 = this.count6;
        }

        private void btnCauHinhCOM_Click(object sender, EventArgs e)
        {
            string computer = SystemInformation.ComputerName;
            string[] lstCom = SerialPort.GetPortNames();
            bool existConnect = false;
            int port = this.GetPort();

            if (port != 0)
            {
                var connect = ConnectBL.Get_Connect(port, computer);
                if (connect != null)
                {
                    if (connect.Status)
                    {
                        MessageBox.Show("Vui lòng ngắt kết nối !", "Thông báo", MessageBoxButtons.OK);
                        return;
                    }
                }
            }

            // Kiểm tra xem tất cả cổng COM đã được kết nối hay chưa
            var lstConnect = ConnectBL.Get_ListConnect(computer, true);
            if (lstCom != null && lstCom.Length > 0)
            {
                foreach (var com in lstCom)
                {
                    var exit = lstConnect.Where(p => p.Com == com).FirstOrDefault();
                    if (exit == null)
                    {
                        existConnect = true;
                    }
                }
            }

            if (existConnect)
            {
                try
                {
                    if (port != 0)
                    {
                        var connect = ConnectBL.Get_Connect(port, computer);
                        if (connect != null)
                        {
                            if (connect.Status)
                            {
                                MessageBox.Show("Vui lòng ngắt kết nối !", "Thông báo", MessageBoxButtons.OK);
                            }
                            else
                            {
                                frmConfigCom frmConfigCom = new frmConfigCom(port);
                                frmConfigCom.FormClosing += frmConfigCom_FormClosing;
                                frmConfigCom.Show();
                                this.Enabled = false;
                            }
                        }
                        else
                        {
                            frmConfigCom frmConfigCom = new frmConfigCom(port);
                            frmConfigCom.FormClosing += frmConfigCom_FormClosing;
                            frmConfigCom.Show();
                            this.Enabled = false;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Vui lòng chọn cổng !", "Thông báo", MessageBoxButtons.OK);
                    }
                }
                catch
                {
                    MessageBox.Show("Cổng COM không tồn tại !", "Thông báo", MessageBoxButtons.OK);
                }
            }
            else
            {
                MessageBox.Show("Không thể cấu hình. Vì Tất cả cổng COM đã được kết nối !", "Thông báo", MessageBoxButtons.OK);
            }
        }

        private void frmConfigCom_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Enabled = true;
            this.computer = SystemInformation.ComputerName;
            int port = this.GetPort();

            if (port != 0)
            {
                var connect = ConnectBL.Get_Connect(port, computer);
                if (connect != null)
                {
                    string setting = connect.Com + " Setting : " + connect.BaudRate + ", " + connect.Parity + ", " + connect.DataBit + ", " + connect.StopBit;

                    if (rdoPort1.Checked)
                    {
                        txtParametersCom1.Text = setting;
                    }
                    else if (rdoPort2.Checked)
                    {
                        txtParametersCom2.Text = setting;
                    }
                    else if (rdoPort3.Checked)
                    {
                        txtParametersCom3.Text = setting;
                    }
                    else if (rdoPort4.Checked)
                    {
                        txtParametersCom4.Text = setting;
                    }
                    else if (rdoPort5.Checked)
                    {
                        txtParametersCom5.Text = setting;
                    }
                    else if (rdoPort6.Checked)
                    {
                        txtParametersCom6.Text = setting;
                    }
                }
            }
        }

        private void btnChonMayXN_Click(object sender, EventArgs e)
        {
            this.computer = SystemInformation.ComputerName;
            try
            {
                int port = this.GetPort();
                if (port != 0)
                {
                    var connect = ConnectBL.Get_Connect(port, computer);
                    if (connect != null)
                    {
                        if (connect.Status)
                        {
                            MessageBox.Show("Vui lòng ngắt kết nối !", "Thông báo", MessageBoxButtons.OK);
                        }
                        else
                        {
                            frmDevice frmDevice = new frmDevice(port, this.computer);
                            frmDevice.FormClosing += frmDevice_FormClosing;
                            frmDevice.Show();
                            this.Enabled = false;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Vui lòng cấu hình COM !", "Thông báo", MessageBoxButtons.OK);
                    }
                }
                else
                {
                    MessageBox.Show("Vui lòng chọn cổng !", "Thông báo", MessageBoxButtons.OK);
                }
            }
            catch
            {
                MessageBox.Show("Cổng COM không tồn tại !", "Thông báo", MessageBoxButtons.OK);
            }
        }

        private void frmDevice_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Enabled = true;
            var frmDevice = sender as frmDevice;
            this.deviceId = frmDevice.idDevice;
            this.computer = SystemInformation.ComputerName;
            int port = 0;
            if (!frmDevice.isSelect)
            {
                var device = DeviceBL.Get_Device(this.deviceId);
                if (device == null)
                {
                    if (rdoPort1.Checked)
                    {
                        txtDeviceName1.Text = string.Empty;
                        txtDeviceID1.Text = string.Empty;
                        txtProtocol1.Text = string.Empty;
                    }
                    if (rdoPort2.Checked)
                    {
                        txtDeviceName2.Text = string.Empty;
                        txtDeviceID2.Text = string.Empty;
                        txtProtocol2.Text = string.Empty;
                    }
                    if (rdoPort3.Checked)
                    {
                        txtDeviceName3.Text = string.Empty;
                        txtDeviceID3.Text = string.Empty;
                        txtProtocol3.Text = string.Empty;
                    }
                    if (rdoPort4.Checked)
                    {
                        txtDeviceName4.Text = string.Empty;
                        txtDeviceID4.Text = string.Empty;
                        txtProtocol4.Text = string.Empty;
                    }
                    if (rdoPort5.Checked)
                    {
                        txtDeviceName5.Text = string.Empty;
                        txtDeviceID5.Text = string.Empty;
                        txtProtocol5.Text = string.Empty;
                    }
                    if (rdoPort6.Checked)
                    {
                        txtDeviceName6.Text = string.Empty;
                        txtDeviceID6.Text = string.Empty;
                        txtProtocol6.Text = string.Empty;
                    }
                }
                return;
            }

            var device01 = DeviceBL.Get_Device(deviceId);
            if (device01 != null)
            {
                // Lưu Id máy xét nghiệm vào table COM
                port = this.GetPort();
                if (port != 0)
                {
                    var connect = ConnectBL.Get_Connect(port, computer);
                    if (connect != null)
                    {
                        ConnectBL.Update_DeviceIdAndStatus(connect.ComputerId, connect.PortId, device01.Id, false);
                    }
                }

                // Load thông tin máy xét nghiệm đã chọn lên form kết nối
                if (rdoPort1.Checked)
                {
                    txtDeviceName1.Text = device01.Name;
                    txtDeviceID1.Text = device01.Id.ToString();
                    txtStatus1.Text = string.Empty;
                    pStatus1.BackColor = Color.Red;
                    txtProtocol1.Text = device01.Protocol;
                }
                if (rdoPort2.Checked)
                {
                    txtDeviceName2.Text = device01.Name;
                    txtDeviceID2.Text = device01.Id.ToString();
                    txtStatus2.Text = string.Empty;
                    pStatus2.BackColor = Color.Red;
                    txtProtocol2.Text = device01.Protocol;
                }
                if (rdoPort3.Checked)
                {
                    txtDeviceName3.Text = device01.Name;
                    txtDeviceID3.Text = device01.Id.ToString();
                    txtStatus3.Text = string.Empty;
                    pStatus3.BackColor = Color.Red;
                    txtProtocol3.Text = device01.Protocol;
                }
                if (rdoPort4.Checked)
                {
                    txtDeviceName4.Text = device01.Name;
                    txtDeviceID4.Text = device01.Id.ToString();
                    txtStatus4.Text = string.Empty;
                    pStatus4.BackColor = Color.Red;
                    txtProtocol4.Text = device01.Protocol;

                }
                if (rdoPort5.Checked)
                {
                    txtDeviceName5.Text = device01.Name;
                    txtDeviceID5.Text = device01.Id.ToString();
                    txtStatus5.Text = string.Empty;
                    pStatus5.BackColor = Color.Red;
                    txtProtocol5.Text = device01.Protocol;
                }
                if (rdoPort6.Checked)
                {
                    txtDeviceName6.Text = device01.Name;
                    txtDeviceID6.Text = device01.Id.ToString();
                    txtStatus6.Text = string.Empty;
                    pStatus6.BackColor = Color.Red;
                    txtProtocol6.Text = device01.Protocol;
                }
            }
            else
            {
                // Xóa text
                if (rdoPort1.Checked)
                {
                    txtDeviceName1.Text = string.Empty;
                    txtDeviceID1.Text = string.Empty;
                    txtProtocol1.Text = string.Empty;
                }
                if (rdoPort2.Checked)
                {
                    txtDeviceName2.Text = string.Empty;
                    txtDeviceID2.Text = string.Empty;
                    txtProtocol2.Text = string.Empty;
                }
                if (rdoPort3.Checked)
                {
                    txtDeviceName3.Text = string.Empty;
                    txtDeviceID3.Text = string.Empty;
                    txtProtocol3.Text = string.Empty;
                }
                if (rdoPort4.Checked)
                {
                    txtDeviceName4.Text = string.Empty;
                    txtDeviceID4.Text = string.Empty;
                    txtProtocol4.Text = string.Empty;
                }
                if (rdoPort5.Checked)
                {
                    txtDeviceName5.Text = string.Empty;
                    txtDeviceID5.Text = string.Empty;
                    txtProtocol5.Text = string.Empty;
                }
                if (rdoPort6.Checked)
                {
                    txtDeviceName6.Text = string.Empty;
                    txtDeviceID6.Text = string.Empty;
                    txtProtocol6.Text = string.Empty;
                }

                // Xóa IdMayXN trong table COM
                port = this.GetPort();
                if (port != 0)
                {
                    var connect = ConnectBL.Get_Connect(port, computer);
                    if (connect != null)
                    {
                        ConnectBL.Update_DeviceIdAndStatus(connect.ComputerId, connect.PortId, 0, false);
                    }
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnClose2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            this.computer = SystemInformation.ComputerName;
            int port = this.GetPort();

            if (port == 0)
            {
                MessageBox.Show("Vui lòng chọn cổng !", "Thông báo", MessageBoxButtons.OK);
            }
            else
            {
                var connect = ConnectBL.Get_Connect(port, computer);
                if (connect != null)
                {
                    if (rdoPort1.Checked)
                    {
                        if (connect.Com.Contains(isCOM))
                            this.Connect(SerialPort_01, connect);
                        else
                            this.Open_Server_Or_Client(connect);

                        this.isConnect01 = true;
                    }
                    if (rdoPort2.Checked)
                    {
                        if (connect.Com.Contains(isCOM))
                            this.Connect(SerialPort_02, connect);
                        else
                            this.Open_Server_Or_Client(connect);

                        this.isConnect02 = true;
                    }
                    if (rdoPort3.Checked)
                    {
                        if (connect.Com.Contains(isCOM))
                            this.Connect(SerialPort_03, connect);
                        else
                            this.Open_Server_Or_Client(connect);

                        this.isConnect03 = true;
                    }
                    if (rdoPort4.Checked)
                    {
                        if (connect.Com.Contains(isCOM))
                            this.Connect(SerialPort_04, connect);
                        else
                            this.Open_Server_Or_Client(connect);

                        this.isConnect04 = true;
                    }
                    if (rdoPort5.Checked)
                    {
                        if (connect.Com.Contains(isCOM))
                            this.Connect(SerialPort_05, connect);
                        else
                            this.Open_Server_Or_Client(connect);

                        this.isConnect05 = true;
                    }
                    if (rdoPort6.Checked)
                    {
                        if (connect.Com.Contains(isCOM))
                            this.Connect(SerialPort_06, connect);
                        else
                            this.Open_Server_Or_Client(connect);

                        this.isConnect06 = true;
                    }
                }
            }
        }

        private void btnDisConnect_Click(object sender, EventArgs e)
        {
            int port = this.GetPort();
            DialogResult result = MessageBox.Show("Bạn muốn ngắt kết nối cổng " + port + " ?", "Thông báo !", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (DialogResult.Yes == result)
            {

                this.computer = SystemInformation.ComputerName;
                try
                {
                    if (port == 0)
                    {
                        MessageBox.Show("Vui lòng chọn cổng !", "Thông báo", MessageBoxButtons.OK);
                    }
                    else
                    {
                        var connect = ConnectBL.Get_Connect(port, computer);
                        if (connect != null)
                        {
                            if (rdoPort1.Checked)
                            {
                                if (connect.Com.Contains(isCOM))
                                    this.DisConnect(SerialPort_01, connect);
                                else
                                    this.Close_Server_Or_Client(connect);

                                this.isConnect01 = false;
                            }
                            if (rdoPort2.Checked)
                            {
                                if (connect.Com.Contains(isCOM))
                                    this.DisConnect(SerialPort_02, connect);
                                else
                                    this.Close_Server_Or_Client(connect);

                                this.isConnect02 = false;
                            }
                            if (rdoPort3.Checked)
                            {
                                if (connect.Com.Contains(isCOM))
                                    this.DisConnect(SerialPort_03, connect);
                                else
                                    this.Close_Server_Or_Client(connect);

                                this.isConnect03 = false;
                            }
                            if (rdoPort4.Checked)
                            {
                                if (connect.Com.Contains(isCOM))
                                    this.DisConnect(SerialPort_04, connect);
                                else
                                    this.Close_Server_Or_Client(connect);

                                this.isConnect04 = false;
                            }
                            if (rdoPort5.Checked)
                            {
                                if (connect.Com.Contains(isCOM))
                                    this.DisConnect(SerialPort_05, connect);
                                else
                                    this.Close_Server_Or_Client(connect);

                                this.isConnect05 = false;
                            }
                            if (rdoPort6.Checked)
                            {
                                if (connect.Com.Contains(isCOM))
                                    this.DisConnect(SerialPort_06, connect);
                                else
                                    this.Close_Server_Or_Client(connect);

                                this.isConnect06 = false;
                            }
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("Không thể đóng kết nối. Vui lòng thử lại !", "Thông báo", MessageBoxButtons.OK);
                }
            }
            else
                return;
        }

        private void rdoPort_CheckedChanged(object sender, EventArgs e)
        {
            this.computer = SystemInformation.ComputerName;
            int port = this.GetPort();

            var connect = ConnectBL.Get_Connect(port, computer);
            if (connect != null)
            {
                if (connect.Status)
                {
                    btnConnect.Enabled = false;
                    btnDisconnect.Enabled = true;
                }
                else
                {
                    btnConnect.Enabled = true;
                    btnDisconnect.Enabled = false;
                }
            }
        }

        private void btnConnectAll_Click(object sender, EventArgs e)
        {
            var lstConnect = ConnectBL.Get_ListConnectAll(SystemInformation.ComputerName);
            if (lstConnect == null || lstConnect.Count == 0) return;

            foreach (var connect in lstConnect)
            {
                if (connect != null)
                {
                    if (connect.PortId == int.Parse(rdoPort1.Tag.ToString()) && this.isConnect01 == false)
                    {
                        if (connect.Com.Contains(isCOM))
                            this.ConnectAll(SerialPort_01, connect);
                        else
                            this.Open_Server_Or_Client(connect);

                        this.isConnect01 = true;
                    }
                    if (connect.PortId == int.Parse(rdoPort2.Tag.ToString()) && this.isConnect02 == false)
                    {
                        if (connect.Com.Contains(isCOM))
                            this.ConnectAll(SerialPort_02, connect);
                        else
                            this.Open_Server_Or_Client(connect);

                        this.isConnect02 = true;
                    }
                    if (connect.PortId == int.Parse(rdoPort3.Tag.ToString()) && this.isConnect03 == false)
                    {
                        if (connect.Com.Contains(isCOM))
                            this.ConnectAll(SerialPort_03, connect);
                        else
                            this.Open_Server_Or_Client(connect);

                        this.isConnect03 = true;
                    }
                    if (connect.PortId == int.Parse(rdoPort4.Tag.ToString()) && this.isConnect04 == false)
                    {
                        if (connect.Com.Contains(isCOM))
                            this.ConnectAll(SerialPort_04, connect);
                        else
                            this.Open_Server_Or_Client(connect);

                        this.isConnect04 = true;
                    }
                    if (connect.PortId == int.Parse(rdoPort5.Tag.ToString()) && this.isConnect05 == false)
                    {
                        if (connect.Com.Contains(isCOM))
                            this.ConnectAll(SerialPort_05, connect);
                        else
                            this.Open_Server_Or_Client(connect);

                        this.isConnect05 = true;
                    }
                    if (connect.PortId == int.Parse(rdoPort6.Tag.ToString()) && this.isConnect06 == false)
                    {
                        if (connect.Com.Contains(isCOM))
                            this.ConnectAll(SerialPort_06, connect);
                        else
                            this.Open_Server_Or_Client(connect);

                        this.isConnect06 = true;
                    }

                    this.btnConnect.Enabled = false;
                    this.btnDisconnect.Enabled = true;
                }
            }
        }

        private void btnDisConnectAll_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Bạn muốn ngắt kết nối tất cả các cổng ?", "Thông báo !", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (DialogResult.Yes == result)
            {
                var lstConnect = ConnectBL.Get_ListConnectAll(SystemInformation.ComputerName);
                if (lstConnect == null) return;

                foreach (var connect in lstConnect)
                {
                    if (connect != null)
                    {
                        if (connect.PortId == int.Parse(rdoPort1.Tag.ToString()) && this.isConnect01 == true)
                        {
                            if (connect.Com.Contains(isCOM))
                                this.DisConnectAll(SerialPort_01, connect);
                            else
                                this.Close_Server_Or_Client(connect);

                            this.isConnect01 = false;
                        }                          
                        if (connect.PortId == int.Parse(rdoPort2.Tag.ToString()) && this.isConnect02 == true)
                        {
                            if (connect.Com.Contains(isCOM))
                                this.DisConnectAll(SerialPort_02, connect);
                            else
                                this.Close_Server_Or_Client(connect);

                            this.isConnect02 = false;
                        }
                        if (connect.PortId == int.Parse(rdoPort3.Tag.ToString()) && this.isConnect03 == true)
                        {
                            if (connect.Com.Contains(isCOM))
                                this.DisConnectAll(SerialPort_03, connect);
                            else
                                this.Close_Server_Or_Client(connect);

                            this.isConnect03 = false;
                        }
                        if (connect.PortId == int.Parse(rdoPort4.Tag.ToString()) && this.isConnect04 == true)
                        {
                            if (connect.Com.Contains(isCOM))
                                this.DisConnectAll(SerialPort_04, connect);
                            else
                                this.Close_Server_Or_Client(connect);

                            this.isConnect04 = false;
                        }
                        if (connect.PortId == int.Parse(rdoPort5.Tag.ToString()) && this.isConnect05 == true)
                        {
                            if (connect.Com.Contains(isCOM))
                                this.DisConnectAll(SerialPort_05, connect);
                            else
                                this.Close_Server_Or_Client(connect);

                            this.isConnect05 = false;
                        }
                        if (connect.PortId == int.Parse(rdoPort6.Tag.ToString()) && this.isConnect06 == true)
                        {
                            if (connect.Com.Contains(isCOM))
                                this.DisConnectAll(SerialPort_06, connect);
                            else
                                this.Close_Server_Or_Client(connect);

                            this.isConnect06 = false;
                        }

                        this.btnConnect.Enabled = true;
                        this.btnDisconnect.Enabled = false;
                    }
                }
            }
            else
                return;
        }

        private void frmConnect_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.isConnectOk)
            {
                var lstConnect = ConnectBL.Get_ListConnectAll(SystemInformation.ComputerName);
                if (lstConnect != null)
                {
                    var connect = lstConnect.Where(p => p.Status == true).FirstOrDefault();
                    if (connect != null)
                    {
                        MessageBox.Show("Vui lòng đóng tất cả các kết nối !", "Thông báo", MessageBoxButtons.OK);
                        e.Cancel = true;
                    }
                    else
                    {
                        SessionBL.UpdateStatus(SystemInformation.ComputerName, false);
                    }
                }
            }
            else
            {
                SessionBL.UpdateStatus(SystemInformation.ComputerName, false);
            }
        }

        private void FrmLogin_FormClosing(object sender, FormClosingEventArgs e)
        {
            var frmLogin = sender as frmLogin;
            if (frmLogin.isLogin)
            {
                this.Enabled = true;
                this.Set_User();
                this.HideConfigFunction();

                // Log sau khi login thành công
                try
                {
                    var session = SessionBL.Get_SessionByComputerIDForConnect(SystemInformation.ComputerName);
                    var user = session != null ? UserBL.Get_UserByUserID(session.UserId) : null;
                    string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] LOGIN SUCCESS - Computer: {SystemInformation.ComputerName}, User: {user?.Code ?? "Unknown"} ({user?.Name ?? "N/A"})\r\n";

                    log4net.Info($"User {user?.Code} logged in successfully on computer {SystemInformation.ComputerName}");
                    Folder_File_Tool.Create_Write_File("SystemLog", true, logMessage);
                }
                catch (Exception ex)
                {
                    log4net.Error("Error logging login information", ex);
                }
            }
            else
            {
                Application.Exit();
            }
        }

        private void btnConfigTCP_Click(object sender, EventArgs e)
        {
            this.computer = SystemInformation.ComputerName;

            try
            {
                int port = this.GetPort();
                if (port != 0)
                {
                    var connect = ConnectBL.Get_Connect(port, computer);
                    if (connect != null)
                    {
                        if (connect.Status)
                        {
                            MessageBox.Show("Vui lòng ngắt kết nối !", "Thông báo", MessageBoxButtons.OK);
                        }
                        else
                        {
                            frmConfigTCP frmConfigForIStat = new frmConfigTCP(port);
                            frmConfigForIStat.FormClosing += FrmConfigTCP_FormClosing;
                            frmConfigForIStat.Show();
                            this.Enabled = false;
                        }
                    }
                    else
                    {
                        frmConfigTCP frmConfigForIStat = new frmConfigTCP(port);
                        frmConfigForIStat.FormClosing += FrmConfigTCP_FormClosing;
                        frmConfigForIStat.Show();
                        this.Enabled = false;
                    }
                }
                else
                {
                    MessageBox.Show("Vui lòng chọn cổng !", "Thông báo", MessageBoxButtons.OK);
                }
            }
            catch
            {
                MessageBox.Show(" Cấu hình chưa đúng !", "Thông báo", MessageBoxButtons.OK);
            }
        }

        private void FrmConfigTCP_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Enabled = true;
            this.computer = SystemInformation.ComputerName;
            int port = this.GetPort();

            if (port != 0)
            {
                var connect = ConnectBL.Get_Connect(port, computer);
                if (!connect.Com.Contains(isCOM))
                {
                    if (connect != null)
                    {
                        string setting = "IP : " + connect.Com + " : " + connect.BaudRate;

                        if (rdoPort1.Checked)
                        {
                            txtParametersCom1.Text = setting;
                        }
                        else if (rdoPort2.Checked)
                        {
                            txtParametersCom2.Text = setting;
                        }
                        else if (rdoPort3.Checked)
                        {
                            txtParametersCom3.Text = setting;
                        }
                        else if (rdoPort4.Checked)
                        {
                            txtParametersCom4.Text = setting;
                        }
                        else if (rdoPort5.Checked)
                        {
                            txtParametersCom5.Text = setting;
                        }
                        else if (rdoPort6.Checked)
                        {
                            txtParametersCom6.Text = setting;
                        }
                    }
                }
            }
        }

        private void btnReset1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Bạn muốn xóa dữ liệu tạm ? ", "Thông báo !", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (DialogResult.Yes == result)
            {
                this.Reset(1);
            }
            else
                return;
        }

        private void btnReset2_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Bạn muốn xóa dữ liệu tạm ? ", "Thông báo !", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (DialogResult.Yes == result)
            {
                this.Reset(2);
            }
            else
                return;
        }

        private void btnReset3_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Bạn muốn xóa dữ liệu tạm ? ", "Thông báo !", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (DialogResult.Yes == result)
            {
                this.Reset(3);
            }
            else
                return;
        }

        private void btnReset4_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Bạn muốn xóa dữ liệu tạm ? ", "Thông báo !", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (DialogResult.Yes == result)
            {
                this.Reset(4);
            }
            else
                return;
        }

        private void btnReset5_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Bạn muốn xóa dữ liệu tạm ? ", "Thông báo !", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (DialogResult.Yes == result)
            {
                this.Reset(5);
            }
            else
                return;
        }

        private void btnReset6_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Bạn muốn xóa dữ liệu tạm ? ", "Thông báo !", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (DialogResult.Yes == result)
            {
                this.Reset(6);
            }
            else
                return;
        }

        private void chkQC1_CheckedChanged(object sender, EventArgs e)
        {
            var processQC = this.chkQC1.Checked;
            var deviceID = long.Parse(this.txtDeviceID1.Text);
            this.ProcessQC(deviceID, processQC);   
        }

        private void chkQC2_CheckedChanged(object sender, EventArgs e)
        {
            var processQC = this.chkQC2.Checked;
            var deviceID = long.Parse(this.txtDeviceID2.Text);
            this.ProcessQC(deviceID, processQC);
        }

        private void chkQC3_CheckedChanged(object sender, EventArgs e)
        {
            var processQC = this.chkQC3.Checked;
            var deviceID = long.Parse(this.txtDeviceID3.Text);
            this.ProcessQC(deviceID, processQC);
        }

        private void chkQC4_CheckedChanged(object sender, EventArgs e)
        {
            var processQC = this.chkQC4.Checked;
            var deviceID = long.Parse(this.txtDeviceID4.Text);
            this.ProcessQC(deviceID, processQC);
        }

        private void chkQC5_CheckedChanged(object sender, EventArgs e)
        {
            var processQC = this.chkQC5.Checked;
            var deviceID = long.Parse(this.txtDeviceID5.Text);
            this.ProcessQC(deviceID, processQC);
        }

        private void chkQC6_CheckedChanged(object sender, EventArgs e)
        {
            var processQC = this.chkQC6.Checked;
            var deviceID = long.Parse(this.txtDeviceID6.Text);
            this.ProcessQC(deviceID, processQC);
        }

        private void btnViewLog_Click(object sender, EventArgs e)
        {
            try
            {
                string logFilePath = Path.Combine(Folder_File_Tool.folder, "SystemLog", Folder_File_Tool.fileDevice);

                if (!File.Exists(logFilePath))
                {
                    MessageBox.Show("File log chưa tồn tại!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Tạo form để hiển thị log
                Form logForm = new Form
                {
                    Text = "System Log - " + logFilePath,
                    Size = new Size(1000, 600),
                    StartPosition = FormStartPosition.CenterScreen,
                    Icon = this.Icon
                };

                // TextBox để hiển thị nội dung log
                TextBox txtLog = new TextBox
                {
                    Multiline = true,
                    ScrollBars = ScrollBars.Both,
                    Dock = DockStyle.Fill,
                    Font = new Font("Courier New", 9),
                    ReadOnly = true,
                    WordWrap = false
                };

                // Panel chứa các nút
                Panel pnlButtons = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 50
                };

                // Nút làm mới
                Button btnRefresh = new Button
                {
                    Text = "Làm mới",
                    Location = new Point(10, 10),
                    Size = new Size(100, 30)
                };
                btnRefresh.Click += (s, ev) =>
                {
                    try
                    {
                        txtLog.Text = File.ReadAllText(logFilePath, Encoding.UTF8);
                        txtLog.SelectionStart = txtLog.Text.Length;
                        txtLog.ScrollToCaret();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi đọc file log: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                // Nút xóa log
                Button btnClearLog = new Button
                {
                    Text = "Xóa log",
                    Location = new Point(120, 10),
                    Size = new Size(100, 30)
                };
                btnClearLog.Click += (s, ev) =>
                {
                    DialogResult result = MessageBox.Show("Bạn có chắc muốn xóa toàn bộ log?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        try
                        {
                            File.WriteAllText(logFilePath, string.Empty, Encoding.UTF8);
                            txtLog.Text = string.Empty;
                            MessageBox.Show("Đã xóa log thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Lỗi khi xóa log: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                };

                // Nút mở thư mục
                Button btnOpenFolder = new Button
                {
                    Text = "Mở thư mục",
                    Location = new Point(230, 10),
                    Size = new Size(100, 30)
                };
                btnOpenFolder.Click += (s, ev) =>
                {
                    try
                    {
                        string folderPath = Path.GetDirectoryName(logFilePath);
                        System.Diagnostics.Process.Start("explorer.exe", folderPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi mở thư mục: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                // Nút đóng
                Button btnCloseLog = new Button
                {
                    Text = "Đóng",
                    Location = new Point(340, 10),
                    Size = new Size(100, 30)
                };
                btnCloseLog.Click += (s, ev) => logForm.Close();

                pnlButtons.Controls.Add(btnRefresh);
                pnlButtons.Controls.Add(btnClearLog);
                pnlButtons.Controls.Add(btnOpenFolder);
                pnlButtons.Controls.Add(btnCloseLog);

                logForm.Controls.Add(txtLog);
                logForm.Controls.Add(pnlButtons);

                // Đọc nội dung file log
                txtLog.Text = File.ReadAllText(logFilePath, Encoding.UTF8);

                // Cuộn xuống cuối
                txtLog.SelectionStart = txtLog.Text.Length;
                txtLog.ScrollToCaret();

                logForm.ShowDialog();

                log4net.Info("Viewed system log file");
            }
            catch (Exception ex)
            {
                log4net.Error("Error viewing log file", ex);
                MessageBox.Show($"Lỗi khi mở file log: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDeleteConnect_Click(object sender, EventArgs e)
        {
            this.computer = SystemInformation.ComputerName;
            int port = this.GetPort();

            DialogResult result = MessageBox.Show("Bạn muốn xóa kết nối này ?", "Thông báo !", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (DialogResult.Yes == result)
            {
                if (port == 0)
                {
                    MessageBox.Show("Vui lòng chọn kết nối!", "Thông báo", MessageBoxButtons.OK);
                }
                else
                {
                    var connect = ConnectBL.Get_Connect(port, computer);
                    if (connect.Status == true)
                    {
                        MessageBox.Show("Kết nối đang mở nên không thể xóa. Vui lòng đóng kết nối!", "Thông báo", MessageBoxButtons.OK);
                    }
                    else
                    {
                        if (ConnectBL.DeleteConect(port, this.computer))
                        {
                            this.DeleteContentControls(port);
                        }
                        else
                        {
                            MessageBox.Show("Không thể xóa kết nối này. Vui lòng kiểm tra lại!", "Thông báo", MessageBoxButtons.OK);
                        }
                    }
                }
            }
        }

        private void chkWriteResult1_CheckedChanged(object sender, EventArgs e)
        {
            Update_WriteFile(this.txtProtocol1.Text, this.chkWriteResult1.Checked);
        }

        private void chkWriteResult2_CheckedChanged(object sender, EventArgs e)
        {
            Update_WriteFile(this.txtProtocol2.Text, this.chkWriteResult2.Checked);
        }

        private void chkWriteResult3_CheckedChanged(object sender, EventArgs e)
        {
            Update_WriteFile(this.txtProtocol3.Text, this.chkWriteResult3.Checked);
        }

        private void chkWriteResult4_CheckedChanged(object sender, EventArgs e)
        {
            Update_WriteFile(this.txtProtocol4.Text, this.chkWriteResult4.Checked);
        }

        private void chkWriteResult5_CheckedChanged(object sender, EventArgs e)
        {
            Update_WriteFile(this.txtProtocol5.Text, this.chkWriteResult5.Checked);
        }

        private void chkWriteResult6_CheckedChanged(object sender, EventArgs e)
        {
            Update_WriteFile(this.txtProtocol6.Text, this.chkWriteResult6.Checked);
        }

        #endregion


        #region ****************************************************************************************** METHOD

        private void CheckConnectString()
        {
            try
            {
                if (ConfigConnectString.SetConnectString())
                {
                    this.isConnectOk = true;
                    this.InitializeEvent();
                    this.InitializeTimer();
                }
                else
                {
                    this.OpenFormConnectString();
                }
            }
            catch
            {
                this.OpenFormConnectString();
            }
        }

        private void OpenFormConnectString()
        {
            frmConnectString frm = new frmConnectString();
            frm.FormClosing += FrmConnectString_FormClosing;
            frm.Show();
            this.Enabled = false;
        }

        private void HideConfigFunction()
        {
            var sesion = SessionBL.Get_SessionByComputerIDForConnect(SystemInformation.ComputerName);
            if (sesion != null)
            {
                var user = UserBL.Get_UserByUserID(sesion.UserId);
                if (user != null)
                {
                    if(user.Code == "Admin")
                    {
                        this.btnConfigCom.Enabled = true;
                        this.btnSelectDevice.Enabled = true;
                        this.btnConfigTCP.Enabled = true;
                        this.btnDeleteConnect.Enabled = true;
                        this.HideQC(true);
                    }
                    else
                    {
                        this.btnConfigCom.Enabled = false;
                        this.btnSelectDevice.Enabled = false;
                        this.btnConfigTCP.Enabled = false;
                        this.btnDeleteConnect.Enabled = false;
                        this.HideQC(false);
                    }
                }
            }
        }

        private void HideQC(bool hide)
        {
            this.chkQC1.Enabled = hide;
            this.chkQC2.Enabled = hide;
            this.chkQC3.Enabled = hide;
            this.chkQC4.Enabled = hide;
            this.chkQC5.Enabled = hide;
            this.chkQC6.Enabled = hide;
        }

        private void InitializeEvent()
        {
            this.SerialPort_01.DataReceived += new SerialDataReceivedEventHandler(SerialPort_01_DataReceived);
            this.SerialPort_02.DataReceived += new SerialDataReceivedEventHandler(SerialPort_02_DataReceived);
            this.SerialPort_03.DataReceived += new SerialDataReceivedEventHandler(SerialPort_03_DataReceived);
            this.SerialPort_04.DataReceived += new SerialDataReceivedEventHandler(SerialPort_04_DataReceived);
            this.SerialPort_05.DataReceived += new SerialDataReceivedEventHandler(SerialPort_05_DataReceived);
            this.SerialPort_06.DataReceived += new SerialDataReceivedEventHandler(SerialPort_06_DataReceived);

            this.rdoPort1.CheckedChanged += rdoPort_CheckedChanged;
            this.rdoPort2.CheckedChanged += rdoPort_CheckedChanged;
            this.rdoPort3.CheckedChanged += rdoPort_CheckedChanged;
            this.rdoPort4.CheckedChanged += rdoPort_CheckedChanged;
            this.rdoPort5.CheckedChanged += rdoPort_CheckedChanged;
            this.rdoPort6.CheckedChanged += rdoPort_CheckedChanged;

            this.FormClosing += frmConnect_FormClosing;

        }

        private void InitializeTimer()
        {
            this.timer1 = new Timer();
            this.timer1.Tick += timer1_Tick;
            this.timer1.Interval = 3000;

            this.timer2 = new Timer();
            this.timer2.Tick += timer2_Tick;
            this.timer2.Interval = 3000;

            this.timer3 = new Timer();
            this.timer3.Tick += timer3_Tick;
            this.timer3.Interval = 3000;

            this.timer4 = new Timer();
            this.timer4.Tick += timer4_Tick;
            this.timer4.Interval = 3000;

            this.timer5 = new Timer();
            this.timer5.Tick += timer5_Tick;
            this.timer5.Interval = 3000;

            this.timer6 = new Timer();
            this.timer6.Tick += timer6_Tick;
            this.timer6.Interval = 3000;

        }

        private void DeleteAllComNotExistOnComputer()
        {
            try
            {
                string currentComputer = SystemInformation.ComputerName;
                string[] lstCom = SerialPort.GetPortNames();
                var lstConnect = ConnectBL.Get_ListConnectAll(currentComputer);

                StringBuilder logBuilder = new StringBuilder();
                logBuilder.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] DeleteAllComNotExistOnComputer - Computer: {currentComputer}");
                logBuilder.AppendLine($"Available COM ports: {(lstCom != null && lstCom.Length > 0 ? string.Join(", ", lstCom) : "None")}");
                logBuilder.AppendLine($"Total connections in database: {lstConnect?.Count ?? 0}");

                if (lstConnect != null && lstConnect.Count > 0)
                {
                    foreach (var connect in lstConnect)
                    {
                        // Chỉ xử lý kết nối của máy hiện tại
                        if (connect.ComputerId != currentComputer)
                        {
                            logBuilder.AppendLine($"  SKIP: Port {connect.PortId} - {connect.Com} - Wrong computer ({connect.ComputerId})");
                            continue;
                        }

                        bool shouldKeep = false;
                        string reason = "";

                        // 1. Kiểm tra nếu là kết nối IP (không phải COM port)
                        if (!string.IsNullOrEmpty(connect.Com) && !connect.Com.Contains(isCOM))
                        {
                            shouldKeep = true;
                            reason = "IP connection (not COM port)";
                        }
                        // 2. Kiểm tra nếu COM port tồn tại trong danh sách phần cứng
                        else if (lstCom != null && lstCom.Length > 0 && lstCom.Contains(connect.Com))
                        {
                            shouldKeep = true;
                            reason = "COM port exists in hardware";
                        }
                        // 3. Giữ lại nếu không có danh sách COM port (trường hợp hệ thống chưa sẵn sàng)
                        else if (lstCom == null || lstCom.Length == 0)
                        {
                            shouldKeep = true;
                            reason = "No COM ports detected - keeping connection for safety";
                        }

                        if (shouldKeep)
                        {
                            logBuilder.AppendLine($"  KEEP: Port {connect.PortId} - {connect.Com} - Reason: {reason}");
                        }
                        else
                        {
                            logBuilder.AppendLine($"  DELETE: Port {connect.PortId} - {connect.Com} - Reason: COM port not found in hardware");
                            ConnectBL.Delete(connect.ComputerId, connect.PortId);
                        }
                    }
                }
                else
                {
                    logBuilder.AppendLine("  No connections found in database");
                }

                logBuilder.AppendLine();
                Folder_File_Tool.Create_Write_File("SystemLog", true, logBuilder.ToString());
                log4net.Info($"DeleteAllComNotExistOnComputer completed for {currentComputer}");
            }
            catch (Exception ex)
            {
                string errorMsg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR in DeleteAllComNotExistOnComputer\r\n  Message: {ex.Message}\r\n  StackTrace: {ex.StackTrace}\r\n\r\n";
                log4net.Error("Error in DeleteAllComNotExistOnComputer", ex);
                Folder_File_Tool.Create_Write_File("SystemLog", true, errorMsg);
            }
        }

        private void LoadComInfoAndSelectedDevice()
        {
            var lstConnect = ConnectBL.Get_ListConnectAll(SystemInformation.ComputerName);
            // Log danh sách máy
            try
            {
                StringBuilder logBuilder = new StringBuilder();
                logBuilder.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] LOAD DEVICE LIST - Computer: {SystemInformation.ComputerName}");
                logBuilder.AppendLine($"Total connections found: {lstConnect?.Count ?? 0}");

                if (lstConnect != null && lstConnect.Count > 0)
                {
                    foreach (var conn in lstConnect)
                    {
                        var device = conn.DeviceId.HasValue ? DeviceBL.Get_Device(conn.DeviceId.Value) : null;
                        logBuilder.AppendLine($"  Port {conn.PortId}: {conn.Com} - Device: {device?.Name ?? "None"} (ID: {conn.DeviceId ?? 0}) - Status: {(conn.Status ? "Connected" : "Disconnected")}");
                    }
                }
                else
                {
                    logBuilder.AppendLine("  No connections configured");
                }

                logBuilder.AppendLine();
                log4net.Info("Device list loaded");
                Folder_File_Tool.Create_Write_File("SystemLog", true, logBuilder.ToString());
            }
            catch (Exception ex)
            {
                log4net.Error("Error logging device list", ex);
            }
            if (lstConnect != null)
            {
                for (int i = 0; i < lstConnect.Count; i++)
                {
                    if (lstConnect[i] != null)
                    {
                        string setting = string.Empty;
                        if (lstConnect[i].Com.Contains(isCOM))
                            setting = lstConnect[i].Com + " Setting : " + lstConnect[i].BaudRate + ", " + lstConnect[i].Parity + ", " + lstConnect[i].DataBit + ", " + lstConnect[i].StopBit;
                        else
                            setting = "IP : " + lstConnect[i].Com + " : " + lstConnect[i].BaudRate;
                        long deviceID = lstConnect[i].DeviceId??0;
                        var device = DeviceBL.Get_Device(deviceID);
                        switch (i)
                        {
                            case 0:
                                {
                                    txtParametersCom1.Text = setting;
                                    txtStatus1.Text = lstConnect[i].Status ? "Waiting ..." : string.Empty;
                                    pStatus1.BackColor = lstConnect[i].Status ? Color.Green : Color.Red;
                                    if (device != null)
                                    {
                                        txtDeviceName1.Text = device.Name;
                                        txtDeviceID1.Text = device.Id.ToString();
                                        chkQC1.Checked = device.ProcessQc;
                                        txtProtocol1.Text = device.Protocol;
                                    }
                                    break;
                                }
                            case 1:
                                {
                                    txtParametersCom2.Text = setting;
                                    txtStatus2.Text = lstConnect[i].Status ? "Waiting ..." : string.Empty;
                                    pStatus2.BackColor = lstConnect[i].Status ? Color.Green : Color.Red;
                                    if (device != null)
                                    {
                                        txtDeviceName2.Text = device.Name;
                                        txtDeviceID2.Text = device.Id.ToString();
                                        chkQC2.Checked = device.ProcessQc;
                                        txtProtocol2.Text = device.Protocol;
                                    }
                                    break;
                                }
                            case 2:
                                {
                                    txtParametersCom3.Text = setting;
                                    txtStatus3.Text = lstConnect[i].Status ? "Waiting ..." : string.Empty;
                                    pStatus3.BackColor = lstConnect[i].Status ? Color.Green : Color.Red;
                                    if (device != null)
                                    {
                                        txtDeviceName3.Text = device.Name;
                                        txtDeviceID3.Text = device.Id.ToString();
                                        chkQC3.Checked = device.ProcessQc;
                                        txtProtocol3.Text = device.Protocol;
                                    }
                                    break;
                                }
                            case 3:
                                {
                                    txtParametersCom4.Text = setting;
                                    txtStatus4.Text = lstConnect[i].Status ? "Waiting ..." : string.Empty;
                                    pStatus4.BackColor = lstConnect[i].Status ? Color.Green : Color.Red;
                                    if (device != null)
                                    {
                                        txtDeviceName4.Text = device.Name;
                                        txtDeviceID4.Text = device.Id.ToString();
                                        chkQC4.Checked = device.ProcessQc;
                                        txtProtocol4.Text = device.Protocol;
                                    }
                                    break;
                                }
                            case 4:
                                {
                                    txtParametersCom5.Text = setting;
                                    txtStatus5.Text = lstConnect[i].Status ? "Waiting ..." : string.Empty;
                                    pStatus5.BackColor = lstConnect[i].Status ? Color.Green : Color.Red;
                                    if (device != null)
                                    {
                                        txtDeviceName5.Text = device.Name;
                                        txtDeviceID5.Text = device.Id.ToString();
                                        chkQC5.Checked = device.ProcessQc;
                                        txtProtocol5.Text = device.Protocol;
                                    }
                                    break;
                                }
                            case 5:
                                {
                                    txtParametersCom6.Text = setting;
                                    txtStatus6.Text = lstConnect[i].Status ? "Waiting ..." : string.Empty;
                                    pStatus6.BackColor = lstConnect[i].Status ? Color.Green : Color.Red;
                                    if (device != null)
                                    {
                                        txtDeviceName6.Text = device.Name;
                                        txtDeviceID6.Text = device.Id.ToString();
                                        chkQC6.Checked = device.ProcessQc;
                                        txtProtocol6.Text = device.Protocol;
                                    }
                                    break;
                                }
                            default: break;                                                                                                                                                                                     
                        }
                    }
                }
            }
        }

        private void LoadLogin()
        {
            this.Enabled = false;
            frmLogin frmLogin = new frmLogin(true);
            frmLogin.FormClosing += FrmLogin_FormClosing;
            frmLogin.Show();

        }

        private void Set_User()
        {
            this.lblComputerName.Text = "Máy - " + SystemInformation.ComputerName;
            this.lbVersion.Text = "Phiên bản phần mềm - " + Application.ProductVersion;
            var sesion = SessionBL.Get_SessionByComputerIDForConnect(SystemInformation.ComputerName);
            var hostPital = HostpitalBL.Get_Hostpital();
            if (sesion != null)
            {
                var user = UserBL.Get_UserByUserID(sesion.UserId);
                if (user != null)
                {
                    this.lbUser.Text = string.Concat("User - ", user.Code, " ( ", user.Name, " )");
                }
            }
            if (hostPital != null)
            {
                this.txtHospital.Text = hostPital.Name.ToUpper();
            }
        }

        private void Connect(SerialPort serialPort, Connect connect)
        {
            var device = connect.DeviceId.HasValue ? DeviceBL.Get_Device(connect.DeviceId.Value) : null;
            string logPrefix = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] CONNECT - Port {connect.PortId} ({connect.Com})";
            log4net.Info($"Attempting to connect Port {connect.PortId} - {connect.Com} - Device: {device?.Name ?? "None"}");

            StringBuilder logBuilder = new StringBuilder();
            logBuilder.AppendLine($"{logPrefix} - Device: {device?.Name ?? "None"} (ID: {connect.DeviceId ?? 0})");
            logBuilder.AppendLine($"  Settings: BaudRate={connect.BaudRate}, DataBits={connect.DataBit}, Parity={connect.Parity}, StopBits={connect.StopBit}, RTS={connect.Rts}");

            serialPort.PortName = connect.Com;
            serialPort.BaudRate = Convert.ToInt32(connect.BaudRate);
            serialPort.DataBits = Convert.ToInt32(connect.DataBit);
            serialPort.RtsEnable = connect.Rts;

            switch (connect.Parity)
            {
                case "Odd":
                    serialPort.Parity = Parity.Odd;
                    break;
                case "None":
                    serialPort.Parity = Parity.None;
                    break;
                case "Even":
                    serialPort.Parity = Parity.Even;
                    break;
            }

            switch (connect.StopBit.ToString())
            {
                case "1":
                    serialPort.StopBits = StopBits.One;
                    break;
                case "1.5":
                    serialPort.StopBits = StopBits.OnePointFive;
                    break;
                case "2":
                    serialPort.StopBits = StopBits.Two;
                    break;
            }

            try
            {
                serialPort.Open();

                logBuilder.AppendLine($"  Result: SUCCESS - Port opened successfully");
                log4net.Info($"Port {connect.PortId} connected successfully");

                btnConnect.Enabled = false;
                btnDisconnect.Enabled = true;

                if (rdoPort1.Checked)
                {
                    pStatus1.BackColor = Color.Green;
                    txtStatus1.Text = "Waiting ...";
                    this.timer1.Start();
                    this.btnReset1.Tag = connect.DeviceId;
                }
                if (rdoPort2.Checked)
                {
                    pStatus2.BackColor = Color.Green;
                    txtStatus2.Text = "Waiting ...";
                    this.timer2.Start();
                    this.btnReset2.Tag = connect.DeviceId;
                }
                if (rdoPort3.Checked)
                {
                    pStatus3.BackColor = Color.Green;
                    txtStatus3.Text = "Waiting ...";
                    this.timer3.Start();
                    this.btnReset3.Tag = connect.DeviceId;
                }
                if (rdoPort4.Checked)
                {
                    pStatus4.BackColor = Color.Green;
                    txtStatus4.Text = "Waiting ...";
                    this.timer4.Start();
                    this.btnReset4.Tag = connect.DeviceId;
                }
                if (rdoPort5.Checked)
                {
                    pStatus5.BackColor = Color.Green;
                    txtStatus5.Text = "Waiting ...";
                    this.timer5.Start();
                    this.btnReset5.Tag = connect.DeviceId;
                }
                if (rdoPort6.Checked)
                {
                    pStatus6.BackColor = Color.Green;
                    txtStatus6.Text = "Waiting ...";
                    this.timer6.Start();
                    this.btnReset6.Tag = connect.DeviceId;
                }

                ConnectBL.Update_Status(connect.ComputerId, connect.PortId, true);
                InitTimerInConnectCOM(connect, serialPort);
                Reset(connect);
                logBuilder.AppendLine();
                Folder_File_Tool.Create_Write_File("SystemLog", true, logBuilder.ToString());
            }
            catch(Exception ex)
            {
                string errorMsg = $"{logPrefix}\r\n  Result: FAILED - {ex.Message}\r\n  StackTrace: {ex.StackTrace}\r\n\r\n";
                log4net.Error($"Failed to connect Port {connect.PortId}", ex);
                Folder_File_Tool.Create_Write_File("SystemLog", true, errorMsg);

                MessageBox.Show("Không thể kết nối. Vui lòng thử lại !", "Thông báo", MessageBoxButtons.OK);
            }
        }

        private void DisConnect(SerialPort serialport, Connect connect)
        {
            try
            {
                serialport.Close();

                btnConnect.Enabled = true;
                btnDisconnect.Enabled = false;

                if (rdoPort1.Checked)
                {
                    this.timer1.Stop();
                    pStatus1.BackColor = Color.Red;
                    txtStatus1.Text = string.Empty;
                    this.btnReset1.Tag = string.Empty;
                }
                if (rdoPort2.Checked)
                {
                    this.timer2.Stop();
                    pStatus2.BackColor = Color.Red;
                    txtStatus2.Text = string.Empty;
                    this.btnReset2.Tag = string.Empty;
                }
                if (rdoPort3.Checked)
                {
                    this.timer3.Stop();
                    pStatus3.BackColor = Color.Red;
                    txtStatus3.Text = string.Empty;
                    this.btnReset3.Tag = string.Empty;
                }
                if (rdoPort4.Checked)
                {
                    this.timer4.Stop();
                    pStatus4.BackColor = Color.Red;
                    txtStatus4.Text = string.Empty;
                    this.btnReset4.Tag = string.Empty;
                }
                if (rdoPort5.Checked)
                {
                    this.timer5.Stop();
                    pStatus5.BackColor = Color.Red;
                    txtStatus5.Text = string.Empty;
                    this.btnReset5.Tag = string.Empty;
                }
                if (rdoPort6.Checked)
                {
                    this.timer6.Stop();
                    pStatus6.BackColor = Color.Red;
                    txtStatus6.Text = string.Empty;
                    this.btnReset6.Tag = string.Empty;
                }

                ConnectBL.Update_Status(connect.ComputerId, connect.PortId, false);
                this.Auto_Check_Write_File(connect.PortId, false);
                RemoveTimerInConnectCOM(connect);
            }
            catch(Exception ex)
            {
                log4net.Error(ex);
                MessageBox.Show("Không thể đóng kết nối. Vui lòng thử lại !", "Thông báo", MessageBoxButtons.OK);
            }
        }

        private void ConnectAll(SerialPort serialPort, Connect connect)
        {
            serialPort.PortName = connect.Com;
            serialPort.BaudRate = Convert.ToInt32(connect.BaudRate);
            serialPort.DataBits = Convert.ToInt32(connect.DataBit);
            this.computer = SystemInformation.ComputerName;

            switch (connect.Parity)
            {
                case "Odd":
                    serialPort.Parity = Parity.Odd;
                    break;
                case "None":
                    serialPort.Parity = Parity.None;
                    break;
                case "Even":
                    serialPort.Parity = Parity.Even;
                    break;
            }

            switch (connect.StopBit.ToString())
            {
                case "1":
                    serialPort.StopBits = StopBits.One;
                    break;
                case "1.5":
                    serialPort.StopBits = StopBits.OnePointFive;
                    break;
                case "2":
                    serialPort.StopBits = StopBits.Two;
                    break;
            }

            try
            {
                serialPort.Open();

                btnConnect.Enabled = false;
                btnDisconnect.Enabled = true;

                if (connect.PortId == int.Parse(rdoPort1.Tag.ToString()) && connect.ComputerId == computer)
                {
                    pStatus1.BackColor = Color.Green;
                    txtStatus1.Text = "Waiting ...";
                    this.timer1.Start();
                    this.btnReset1.Tag = connect.DeviceId;
                }
                if (connect.PortId == int.Parse(rdoPort2.Tag.ToString()) && connect.ComputerId == computer)
                {
                    pStatus2.BackColor = Color.Green;
                    txtStatus2.Text = "Waiting ...";
                    this.timer2.Start();
                    this.btnReset2.Tag = connect.DeviceId;
                }
                if (connect.PortId == int.Parse(rdoPort3.Tag.ToString()) && connect.ComputerId == computer)
                {
                    pStatus3.BackColor = Color.Green;
                    txtStatus3.Text = "Waiting ...";
                    this.timer3.Start();
                    this.btnReset3.Tag = connect.DeviceId;
                }
                if (connect.PortId == int.Parse(rdoPort4.Tag.ToString()) && connect.ComputerId == computer)
                {
                    pStatus4.BackColor = Color.Green;
                    txtStatus4.Text = "Waiting ...";
                    this.timer4.Start();
                    this.btnReset4.Tag = connect.DeviceId;
                }
                if (connect.PortId == int.Parse(rdoPort5.Tag.ToString()) && connect.ComputerId == computer)
                {
                    pStatus5.BackColor = Color.Green;
                    txtStatus5.Text = "Waiting ...";
                    this.timer5.Start();
                    this.btnReset5.Tag = connect.DeviceId;
                }
                if (connect.PortId == int.Parse(rdoPort6.Tag.ToString()) && connect.ComputerId == computer)
                {
                    pStatus6.BackColor = Color.Green;
                    txtStatus6.Text = "Waiting ...";
                    this.timer6.Start();
                    this.btnReset6.Tag = connect.DeviceId;
                }

                ConnectBL.Update_Status(connect.ComputerId, connect.PortId, true);
                Auto_Check_Write_File(connect.PortId, true);
                InitTimerInConnectCOM(connect, serialPort);
                Reset(connect);
            }
            catch(Exception ex)
            {
                log4net.Error(ex);
            }
        }

        private void DisConnectAll(SerialPort serialPort, Connect connect)
        {
            try
            {
                serialPort.Close();

                if (connect.PortId == int.Parse(rdoPort1.Tag.ToString()) && connect.ComputerId == computer)
                {
                    this.timer1.Stop();
                    pStatus1.BackColor = Color.Red;
                    txtStatus1.Text = string.Empty;
                    this.btnReset1.Tag = string.Empty;
                }
                if (connect.PortId == int.Parse(rdoPort2.Tag.ToString()) && connect.ComputerId == computer)
                {
                    this.timer2.Stop();
                    pStatus2.BackColor = Color.Red;
                    txtStatus2.Text = string.Empty;
                    this.btnReset2.Tag = string.Empty;
                }
                if (connect.PortId == int.Parse(rdoPort3.Tag.ToString()) && connect.ComputerId == computer)
                {
                    this.timer3.Stop();
                    pStatus3.BackColor = Color.Red;
                    txtStatus3.Text = string.Empty;
                    this.btnReset3.Tag = string.Empty;
                }
                if (connect.PortId == int.Parse(rdoPort4.Tag.ToString()) && connect.ComputerId == computer)
                {
                    this.timer4.Stop();
                    pStatus4.BackColor = Color.Red;
                    txtStatus4.Text = string.Empty;
                    this.btnReset4.Tag = string.Empty;
                }
                if (connect.PortId == int.Parse(rdoPort5.Tag.ToString()) && connect.ComputerId == computer)
                {
                    this.timer5.Stop();
                    pStatus5.BackColor = Color.Red;
                    txtStatus5.Text = string.Empty;
                    this.btnReset5.Tag = string.Empty;
                }
                if (connect.PortId == int.Parse(rdoPort6.Tag.ToString()) && connect.ComputerId == computer)
                {
                    this.timer6.Stop();
                    pStatus6.BackColor = Color.Red;
                    txtStatus6.Text = string.Empty;
                    this.btnReset6.Tag = string.Empty;
                }

                ConnectBL.Update_Status(connect.ComputerId, connect.PortId, false);
                this.Auto_Check_Write_File(connect.PortId, false);
                RemoveTimerInConnectCOM(connect);
            }
            catch(Exception ex)
            {
                log4net.Error(ex);
            }
        }

        private int GetPort()
        {
            if (rdoPort1.Checked)
                return int.Parse(rdoPort1.Tag.ToString());
            if (rdoPort2.Checked)
                return int.Parse(rdoPort2.Tag.ToString());
            if (rdoPort3.Checked)
                return int.Parse(rdoPort3.Tag.ToString());
            if (rdoPort4.Checked)
                return int.Parse(rdoPort4.Tag.ToString());
            if (rdoPort5.Checked)
                return int.Parse(rdoPort5.Tag.ToString());
            if (rdoPort6.Checked)
                return int.Parse(rdoPort6.Tag.ToString());
            return 0;
        }

        public void Reset(int portID)
        {
            string computer = SystemInformation.ComputerName;
            var connect = ConnectBL.Get_Connect(portID, computer);
            var device = DeviceBL.Get_Device(connect.DeviceId??0);

            Type type = Type.GetType(this.nameSpace + "." + device.Protocol);
            MethodInfo method = type.GetMethod(this.clear); 
            var obj = Activator.CreateInstance(type); 
            method.Invoke(obj, new object[] {}); 
        }

        public void Reset(Connect connect)
        {
            Type type = Type.GetType(this.nameSpace + "." + connect.Device.Protocol);
            MethodInfo method = type.GetMethod(this.clear);
            var obj = Activator.CreateInstance(type);
            method.Invoke(obj, new object[] { });
        }

        public void Update_Status_Color()
        {
            var lstConnect = ConnectBL.Get_ListConnectAll(SystemInformation.ComputerName);
            if (lstConnect == null) return;

            foreach (var connect in lstConnect)
            {
                if (connect != null)
                {
                    if (connect.PortId == int.Parse(rdoPort1.Tag.ToString()))
                    {
                        pStatus1.BackColor = Color.Red;
                        ConnectBL.Update_Status(connect.ComputerId, connect.PortId, false);
                    }
                    if (connect.PortId == int.Parse(rdoPort2.Tag.ToString()))
                    {
                        pStatus2.BackColor = Color.Red;
                        ConnectBL.Update_Status(connect.ComputerId, connect.PortId, false);
                    }
                    if (connect.PortId == int.Parse(rdoPort3.Tag.ToString()))
                    {
                        pStatus3.BackColor = Color.Red;
                        ConnectBL.Update_Status(connect.ComputerId, connect.PortId, false);
                    }
                    if (connect.PortId == int.Parse(rdoPort4.Tag.ToString()))
                    {
                        pStatus4.BackColor = Color.Red;
                        ConnectBL.Update_Status(connect.ComputerId, connect.PortId, false);
                    }
                    if (connect.PortId == int.Parse(rdoPort5.Tag.ToString()))
                    {
                        pStatus5.BackColor = Color.Red;
                        ConnectBL.Update_Status(connect.ComputerId, connect.PortId, false);
                    }
                    if (connect.PortId == int.Parse(rdoPort6.Tag.ToString()))
                    {
                        pStatus6.BackColor = Color.Red;
                        ConnectBL.Update_Status(connect.ComputerId, connect.PortId, false);
                    }

                    this.btnConnect.Enabled = true;
                    this.btnDisconnect.Enabled = false;
                }
            }
        }

        private void Open_Server_Or_Client(Connect connect)
        {
            var device = connect.DeviceId.HasValue ? DeviceBL.Get_Device(connect.DeviceId.Value) : null;
            string logPrefix = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] CONNECT IP - Port {connect.PortId}";

            try
            {
                log4net.Info($"Attempting to connect Port {connect.PortId} - IP: {connect.Com}:{connect.BaudRate} - Device: {device?.Name ?? "None"}");

                StringBuilder logBuilder = new StringBuilder();
                logBuilder.AppendLine($"{logPrefix} - Device: {device?.Name ?? "None"} (ID: {connect.DeviceId ?? 0})");
                logBuilder.AppendLine($"  IP Address: {connect.Com}:{connect.BaudRate}");
                logBuilder.AppendLine($"  Protocol: {device?.Protocol ?? "Unknown"}");

                //IPAddress ipServer = IPAddress.Parse(connect.Com);
                //int portServer = int.Parse(connect.BaudRate.ToString());
                //IPEndPoint iPEndPointServer = new IPEndPoint(ipServer, portServer);
                //bool isStart = false;

                //Type type = Type.GetType(this.nameSpace + "." + connect.Device.Protocol);
                //MethodInfo method = type.GetMethod(this.methodIP_Start);
                //var obj = Activator.CreateInstance(type);
                //isStart = bool.Parse(method.Invoke(obj, new object[] { iPEndPointServer, connect.DeviceId }).ToString());

                //method = type.GetMethod(this.method_CheckConnect);
                //obj = Activator.CreateInstance(type);
                //var checkConnect = bool.Parse(method.Invoke(obj, new object[] { }).ToString());

                // Validate IP Address
                IPAddress ipServer;
                if (!IPAddress.TryParse(connect.Com, out ipServer))
                {
                    string errorMsg = $"{logPrefix}\r\n  Result: FAILED - Invalid IP Address format: {connect.Com}\r\n  Reason: IP address parsing failed\r\n\r\n";
                    log4net.Error($"Port {connect.PortId} - Invalid IP address: {connect.Com}");
                    Folder_File_Tool.Create_Write_File("SystemLog", true, errorMsg);
                    MessageBox.Show($"Địa chỉ IP không hợp lệ: {connect.Com}", "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Validate Port
                int portServer;
                if (!int.TryParse(connect.BaudRate.ToString(), out portServer) || portServer < 1 || portServer > 65535)
                {
                    string errorMsg = $"{logPrefix}\r\n  Result: FAILED - Invalid Port number: {connect.BaudRate}\r\n  Reason: Port must be between 1 and 65535\r\n\r\n";
                    log4net.Error($"Port {connect.PortId} - Invalid port number: {connect.BaudRate}");
                    Folder_File_Tool.Create_Write_File("SystemLog", true, errorMsg);
                    MessageBox.Show($"Số cổng không hợp lệ: {connect.BaudRate}", "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                logBuilder.AppendLine($"  IP Address validated: {ipServer}");
                logBuilder.AppendLine($"  Port validated: {portServer}");

                IPEndPoint iPEndPointServer = new IPEndPoint(ipServer, portServer);

                // Validate Device Protocol
                if (device == null || string.IsNullOrEmpty(device.Protocol))
                {
                    string errorMsg = $"{logPrefix}\r\n  Result: FAILED - Device or Protocol not configured\r\n  Reason: Device is null or Protocol is empty\r\n  Device: {device?.Name ?? "NULL"}\r\n\r\n";
                    log4net.Error($"Port {connect.PortId} - Device or Protocol not configured");
                    Folder_File_Tool.Create_Write_File("SystemLog", true, errorMsg);
                    MessageBox.Show("Thiết bị hoặc giao thức chưa được cấu hình!", "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Validate Type exists
                Type type = Type.GetType(this.nameSpace + "." + device.Protocol);
                if (type == null)
                {
                    string errorMsg = $"{logPrefix}\r\n  Result: FAILED - Protocol class not found\r\n  Reason: Type '{this.nameSpace}.{device.Protocol}' does not exist\r\n  Expected namespace: {this.nameSpace}\r\n  Expected protocol: {device.Protocol}\r\n\r\n";
                    log4net.Error($"Port {connect.PortId} - Protocol class not found: {this.nameSpace}.{device.Protocol}");
                    Folder_File_Tool.Create_Write_File("SystemLog", true, errorMsg);
                    MessageBox.Show($"Không tìm thấy class giao thức: {device.Protocol}", "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                logBuilder.AppendLine($"  Protocol type loaded: {type.FullName}");

                // Validate Start method exists
                MethodInfo methodStart = type.GetMethod(this.methodIP_Start);
                if (methodStart == null)
                {
                    string errorMsg = $"{logPrefix}\r\n  Result: FAILED - Start method not found\r\n  Reason: Method '{this.methodIP_Start}' not found in type '{type.FullName}'\r\n  Available methods: {string.Join(", ", type.GetMethods().Select(m => m.Name))}\r\n\r\n";
                    log4net.Error($"Port {connect.PortId} - Start method '{this.methodIP_Start}' not found in {type.FullName}");
                    Folder_File_Tool.Create_Write_File("SystemLog", true, errorMsg);
                    MessageBox.Show($"Phương thức Start không tồn tại trong {device.Protocol}", "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                logBuilder.AppendLine($"  Start method found: {methodStart.Name}");

                // Try to invoke Start method
                bool isStart = false;
                var obj = Activator.CreateInstance(type);

                try
                {
                    //object result = methodStart.Invoke(obj, new object[] { iPEndPointServer, connect.DeviceId });
                    //isStart = bool.Parse(result.ToString());
                    //logBuilder.AppendLine($"  Start method invoked: returned {isStart}");
                    var startParams = methodStart.GetParameters();
                    object secondArg;

                    // Some device protocols (e.g. CobasE411_IP) expect a Connect object,
                    // others expect a long deviceId.
                    if (startParams.Length >= 2 && startParams[1].ParameterType == typeof(Connect))
                    {
                        secondArg = connect;
                    }
                    else
                    {
                        secondArg = connect.DeviceId.HasValue ? connect.DeviceId.Value : 0L;
                    }

                    object result = methodStart.Invoke(obj, new object[] { iPEndPointServer, secondArg });
                    isStart = bool.Parse(result.ToString());
                    logBuilder.AppendLine($"  Start method invoked: returned {isStart}");
                }
                catch (Exception invokeEx)
                {
                    string errorMsg = $"{logPrefix}\r\n  Result: FAILED - Start method invocation failed\r\n  Reason: {invokeEx.GetType().Name}: {invokeEx.Message}\r\n  InnerException: {invokeEx.InnerException?.Message ?? "None"}\r\n  StackTrace: {invokeEx.StackTrace}\r\n\r\n";
                    log4net.Error($"Port {connect.PortId} - Start method invocation failed", invokeEx);
                    Folder_File_Tool.Create_Write_File("SystemLog", true, errorMsg);
                    MessageBox.Show($"Lỗi khi gọi phương thức Start: {invokeEx.InnerException?.Message ?? invokeEx.Message}", "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Validate CheckConnect method exists
                MethodInfo methodCheckConnect = type.GetMethod(this.method_CheckConnect);
                if (methodCheckConnect == null)
                {
                    logBuilder.AppendLine($"  WARNING: CheckConnect method not found - skipping connection check");
                    log4net.Warn($"Port {connect.PortId} - CheckConnect method '{this.method_CheckConnect}' not found in {type.FullName}");
                }

                bool checkConnect = false;
                if (methodCheckConnect != null)
                {
                    try
                    {
                        object checkResult = methodCheckConnect.Invoke(obj, new object[] { });
                        checkConnect = bool.Parse(checkResult.ToString());
                        logBuilder.AppendLine($"  CheckConnect method invoked: returned {checkConnect}");
                    }
                    catch (Exception checkEx)
                    {
                        logBuilder.AppendLine($"  WARNING: CheckConnect invocation failed: {checkEx.Message}");
                        log4net.Warn($"Port {connect.PortId} - CheckConnect invocation failed", checkEx);
                    }
                }

                if (isStart)
                {
                    logBuilder.AppendLine($"  Result: SUCCESS - Connection established");
                    logBuilder.AppendLine($"  Connection status: {(checkConnect ? "Server mode (waiting for client)" : "Client mode (connected)")}");
                    log4net.Info($"Port {connect.PortId} IP connection established successfully");

                    ConnectBL.Update_Status(connect.ComputerId, connect.PortId, true);
                    this.Auto_Check_Write_File(connect.PortId, true);
                    Reset(connect);

                    btnConnect.Enabled = false;
                    btnDisconnect.Enabled = true;

                    if (connect.PortId == int.Parse(rdoPort1.Tag.ToString()) && connect.ComputerId == computer)
                    {
                        pStatus1.BackColor = checkConnect ? Color.Yellow : Color.Green;
                        txtStatus1.Text = "Waiting ...";
                        this.btnReset1.Tag = connect.DeviceId;
                    }
                    if (connect.PortId == int.Parse(rdoPort2.Tag.ToString()) && connect.ComputerId == computer)
                    {
                        pStatus2.BackColor = checkConnect ? Color.Yellow : Color.Green;
                        txtStatus2.Text = "Waiting ...";
                        this.btnReset2.Tag = connect.DeviceId;
                    }
                    if (connect.PortId == int.Parse(rdoPort3.Tag.ToString()) && connect.ComputerId == computer)
                    {
                        pStatus3.BackColor = checkConnect ? Color.Yellow : Color.Green;
                        txtStatus3.Text = "Waiting ...";
                        this.btnReset3.Tag = connect.DeviceId;
                    }
                    if (connect.PortId == int.Parse(rdoPort4.Tag.ToString()) && connect.ComputerId == computer)
                    {
                        pStatus4.BackColor = checkConnect ? Color.Yellow : Color.Green;
                        txtStatus4.Text = "Waiting ...";
                        this.btnReset4.Tag = connect.DeviceId;
                    }
                    if (connect.PortId == int.Parse(rdoPort5.Tag.ToString()) && connect.ComputerId == computer)
                    {
                        pStatus5.BackColor = checkConnect ? Color.Yellow : Color.Green;
                        txtStatus5.Text = "Waiting ...";
                        this.btnReset5.Tag = connect.DeviceId;
                    }
                    if (connect.PortId == int.Parse(rdoPort6.Tag.ToString()) && connect.ComputerId == computer)
                    {
                        pStatus6.BackColor = checkConnect ? Color.Yellow : Color.Green;
                        txtStatus6.Text = "Waiting ...";
                        this.btnReset6.Tag = connect.DeviceId;
                    }
                }
                else
                {
                    logBuilder.AppendLine($"  Result: FAILED - Connection could not be established");
                    logBuilder.AppendLine($"  Reason: Start method returned false");
                    logBuilder.AppendLine($"  Possible causes:");
                    logBuilder.AppendLine($"    - Server/Device is not running or not reachable");
                    logBuilder.AppendLine($"    - IP address or port is incorrect");
                    logBuilder.AppendLine($"    - Firewall blocking the connection");
                    logBuilder.AppendLine($"    - Network cable disconnected");
                    logBuilder.AppendLine($"    - Device is already connected to another client");
                    log4net.Warn($"Port {connect.PortId} IP connection failed to start - Start method returned false");
                    MessageBox.Show($"Không thể kết nối đến {connect.Com}:{connect.BaudRate}\n\nVui lòng kiểm tra:\n- Thiết bị đã bật và sẵn sàng\n- Địa chỉ IP và cổng đúng\n- Firewall không chặn kết nối\n- Cáp mạng được kết nối", "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                logBuilder.AppendLine();
                Folder_File_Tool.Create_Write_File("SystemLog", true, logBuilder.ToString());
            }
            catch (SocketException sEx)
            {
                string errorMsg = $"{logPrefix}\r\n  Result: FAILED - Socket Exception\r\n  Error Code: {sEx.ErrorCode}\r\n  Socket Error: {sEx.SocketErrorCode}\r\n  Reason: {sEx.Message}\r\n  Possible causes:\r\n";

                switch (sEx.SocketErrorCode)
                {
                    case SocketError.ConnectionRefused:
                        errorMsg += "    - Connection refused: Target device/server is not listening on this port\r\n";
                        errorMsg += "    - Device may be turned off or the service is not running\r\n";
                        break;
                    case SocketError.TimedOut:
                        errorMsg += "    - Connection timeout: Device is not responding\r\n";
                        errorMsg += "    - Network is too slow or device is unreachable\r\n";
                        break;
                    case SocketError.HostNotFound:
                        errorMsg += "    - Host not found: IP address does not exist on network\r\n";
                        break;
                    case SocketError.NetworkUnreachable:
                        errorMsg += "    - Network unreachable: No route to host\r\n";
                        errorMsg += "    - Check network configuration and routing\r\n";
                        break;
                    case SocketError.AddressAlreadyInUse:
                        errorMsg += "    - Address already in use: Port is being used by another application\r\n";
                        break;
                    default:
                        errorMsg += $"    - Socket error code: {sEx.SocketErrorCode}\r\n";
                        break;
                }

                errorMsg += $"  IP: {connect.Com}:{connect.BaudRate}\r\n  StackTrace: {sEx.StackTrace}\r\n\r\n";
                log4net.Error($"Failed to connect IP Port {connect.PortId} - Socket error", sEx);
                Folder_File_Tool.Create_Write_File("SystemLog", true, errorMsg);
                MessageBox.Show($"Lỗi kết nối mạng: {sEx.Message}\n\nMã lỗi: {sEx.SocketErrorCode}", "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (TargetInvocationException tiEx)
            {
                string errorMsg = $"{logPrefix}\r\n  Result: FAILED - Target Invocation Exception\r\n  Reason: Error occurred in the invoked method\r\n  Message: {tiEx.Message}\r\n  InnerException: {tiEx.InnerException?.GetType().Name}: {tiEx.InnerException?.Message}\r\n  InnerException StackTrace: {tiEx.InnerException?.StackTrace}\r\n  StackTrace: {tiEx.StackTrace}\r\n\r\n";
                log4net.Error($"Failed to connect IP Port {connect.PortId} - Method invocation failed", tiEx);
                Folder_File_Tool.Create_Write_File("SystemLog", true, errorMsg);
                MessageBox.Show($"Lỗi trong phương thức kết nối: {tiEx.InnerException?.Message ?? tiEx.Message}", "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                string errorMsg = $"{logPrefix}\r\n  Result: FAILED - Unexpected Exception\r\n  Exception Type: {ex.GetType().Name}\r\n  Message: {ex.Message}\r\n  Source: {ex.Source}\r\n  TargetSite: {ex.TargetSite}\r\n";

                if (ex.InnerException != null)
                {
                    errorMsg += $"  InnerException: {ex.InnerException.GetType().Name}\r\n  InnerException Message: {ex.InnerException.Message}\r\n  InnerException StackTrace: {ex.InnerException.StackTrace}\r\n";
                }

                errorMsg += $"  StackTrace: {ex.StackTrace}\r\n\r\n";
                log4net.Error($"Failed to connect IP Port {connect.PortId} - Unexpected error", ex);
                Folder_File_Tool.Create_Write_File("SystemLog", true, errorMsg);
                MessageBox.Show($"Lỗi không xác định: {ex.Message}", "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Close_Server_Or_Client(Connect connect)
        {
            try
            {
                bool isStop = false;
                Type type = Type.GetType(this.nameSpace + "." + connect.Device.Protocol);
                MethodInfo method = type.GetMethod(this.methodIP_Close);
                var obj = Activator.CreateInstance(type);
                isStop = bool.Parse(method.Invoke(obj, new object[] { }).ToString());

                if (isStop)
                {
                    ConnectBL.Update_Status(connect.ComputerId, connect.PortId, false);
                    this.Auto_Check_Write_File(connect.PortId, false);
                    btnConnect.Enabled = true;
                    btnDisconnect.Enabled = false;

                    if (connect.PortId == int.Parse(rdoPort1.Tag.ToString()) && connect.ComputerId == computer)
                    {
                        pStatus1.BackColor = Color.Red;
                        txtStatus1.Text = string.Empty;
                        this.btnReset1.Tag = string.Empty;
                    }
                    if (connect.PortId == int.Parse(rdoPort2.Tag.ToString()) && connect.ComputerId == computer)
                    {
                        pStatus2.BackColor = Color.Red;
                        txtStatus2.Text = string.Empty;
                        this.btnReset2.Tag = string.Empty;
                    }
                    if (connect.PortId == int.Parse(rdoPort3.Tag.ToString()) && connect.ComputerId == computer)
                    {
                        pStatus3.BackColor = Color.Red;
                        txtStatus3.Text = string.Empty;
                        this.btnReset3.Tag = string.Empty;
                    }
                    if (connect.PortId == int.Parse(rdoPort4.Tag.ToString()) && connect.ComputerId == computer)
                    {
                        pStatus4.BackColor = Color.Red;
                        txtStatus4.Text = string.Empty;
                        this.btnReset4.Tag = string.Empty;
                    }
                    if (connect.PortId == int.Parse(rdoPort5.Tag.ToString()) && connect.ComputerId == computer)
                    {
                        pStatus5.BackColor = Color.Red;
                        txtStatus5.Text = string.Empty;
                        this.btnReset5.Tag = string.Empty;
                    }
                    if (connect.PortId == int.Parse(rdoPort6.Tag.ToString()) && connect.ComputerId == computer)
                    {
                        pStatus6.BackColor = Color.Red;
                        txtStatus6.Text = string.Empty;
                        this.btnReset6.Tag = string.Empty;
                    }
                }
            }
            catch(Exception ex)
            {
                log4net.Error(ex);
            }
        }

        private void CallMethodProcess_1(SerialPort sp, int portID)
        {
            string computer = SystemInformation.ComputerName;
            var connect = ConnectBL.Get_Connect(portID, computer);
            var device = DeviceBL.Get_Device(connect.DeviceId ?? 0);
            Type type = Type.GetType(this.nameSpace + "." + device.Protocol);
            MethodInfo method = type.GetMethod(this.method);
            var obj = Activator.CreateInstance(type);
            method.Invoke(obj, new object[] { sp, connect, this.txtSID.Text });
        }

        private void CallMethodProcess_2(SerialPort sp, int portID)
        {
            string computer = SystemInformation.ComputerName;
            var connect = ConnectBL.Get_Connect(portID, computer);
            var device = DeviceBL.Get_Device(connect.DeviceId??0);
            Type type = Type.GetType(this.nameSpace + "." + device.Protocol);
            MethodInfo method = type.GetMethod(this.method);
            var obj = Activator.CreateInstance(type);
            method.Invoke(obj, new object[] { sp, connect, this.txtSID.Text });
        }

        private void CallMethodProcess_3(SerialPort sp, int portID)
        {
            string computer = SystemInformation.ComputerName;
            var connect = ConnectBL.Get_Connect(portID, computer);
            var device = DeviceBL.Get_Device(connect.DeviceId??0);
            Type type = Type.GetType(this.nameSpace + "." + device.Protocol);
            MethodInfo method = type.GetMethod(this.method);
            var obj = Activator.CreateInstance(type);
            method.Invoke(obj, new object[] { sp, connect, this.txtSID.Text });
        }

        private void CallMethodProcess_4(SerialPort sp, int portID)
        {
            string computer = SystemInformation.ComputerName;
            var connect = ConnectBL.Get_Connect(portID, computer);
            var device = DeviceBL.Get_Device(connect.DeviceId??0);
            Type type = Type.GetType(this.nameSpace + "." + device.Protocol);
            MethodInfo method = type.GetMethod(this.method);
            var obj = Activator.CreateInstance(type);
            method.Invoke(obj, new object[] { sp, connect, this.txtSID.Text });
        }

        private void CallMethodProcess_5(SerialPort sp, int portID)
        {
            string computer = SystemInformation.ComputerName;
            var connect = ConnectBL.Get_Connect(portID, computer);
            var device = DeviceBL.Get_Device(connect.DeviceId??0);
            Type type = Type.GetType(this.nameSpace + "." + device.Protocol);
            MethodInfo method = type.GetMethod(this.method);
            var obj = Activator.CreateInstance(type);
            method.Invoke(obj, new object[] { sp, connect, this.txtSID.Text });
        }

        private void CallMethodProcess_6(SerialPort sp, int portID)
        {
            string computer = SystemInformation.ComputerName;
            var connect = ConnectBL.Get_Connect(portID, computer);
            var device = DeviceBL.Get_Device(connect.DeviceId??0);
            Type type = Type.GetType(this.nameSpace + "." + device.Protocol);
            MethodInfo method = type.GetMethod(this.method);
            var obj = Activator.CreateInstance(type);
            method.Invoke(obj, new object[] { sp, connect, this.txtSID.Text });
        }

        private void ProcessQC(long deviceID, bool processQC)
        {
            try
            {
                DeviceBL.UpdateProcessQC(deviceID, processQC);
            }
            catch (Exception ex)
            {
                log4net.Error(ex);
                MessageBox.Show("Không thể xử lý QC cho máy này", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void DeleteContentControls(int port)
        {
            if (port == 1)
            {
                this.txtDeviceName1.Text = string.Empty;
                this.txtDeviceID1.Text = string.Empty;
                this.txtParametersCom1.Text = string.Empty;
                this.txtStatus1.Text = string.Empty;
                this.pStatus1.BackColor = this.BackColor;
                this.chkWriteResult1.Checked = false;
                this.chkQC1.Checked = false;
                this.txtProtocol1.Text = string.Empty;
            }
            else if (port == 2)
            {
                this.txtDeviceName2.Text = string.Empty;
                this.txtDeviceID2.Text = string.Empty;
                this.txtParametersCom2.Text = string.Empty;
                this.txtStatus2.Text = string.Empty;
                this.pStatus2.BackColor = this.BackColor;
                this.chkWriteResult2.Checked = false;
                this.chkQC2.Checked = false;
                this.txtProtocol2.Text = string.Empty;
            }
            else if (port == 3)
            {
                this.txtDeviceName3.Text = string.Empty;
                this.txtDeviceID3.Text = string.Empty;
                this.txtParametersCom3.Text = string.Empty;
                this.txtStatus3.Text = string.Empty;
                this.pStatus3.BackColor = this.BackColor;
                this.chkWriteResult3.Checked = false;
                this.chkQC3.Checked = false;
                this.txtProtocol3.Text = string.Empty;
            }
            else if (port == 4)
            {
                this.txtDeviceName4.Text = string.Empty;
                this.txtDeviceID4.Text = string.Empty;
                this.txtParametersCom4.Text = string.Empty;
                this.txtStatus4.Text = string.Empty;
                this.pStatus4.BackColor = this.BackColor;
                this.chkWriteResult4.Checked = false;
                this.chkQC4.Checked = false;
                this.txtProtocol4.Text = string.Empty;
            }
            else if (port == 5)
            {
                this.txtDeviceName5.Text = string.Empty;
                this.txtDeviceID5.Text = string.Empty;
                this.txtParametersCom5.Text = string.Empty;
                this.txtStatus5.Text = string.Empty;
                this.pStatus5.BackColor = this.BackColor;
                this.chkWriteResult5.Checked = false;
                this.chkQC5.Checked = false;
                this.txtProtocol5.Text = string.Empty;
            }
            else if (port == 6)
            {
                this.txtDeviceName6.Text = string.Empty;
                this.txtDeviceID6.Text = string.Empty;
                this.txtParametersCom6.Text = string.Empty;
                this.txtStatus6.Text = string.Empty;
                this.pStatus6.BackColor = this.BackColor;
                this.chkWriteResult6.Checked = false;
                this.chkQC6.Checked = false;
                this.txtProtocol6.Text = string.Empty;
            }
        }

        private void Update_WriteFile(string prtocol, bool writeFile)
        {
            Type type = Type.GetType(this.nameSpace + "." + prtocol);
            MethodInfo method = type.GetMethod(this.method_WriteFile);
            var obj = Activator.CreateInstance(type);
            method.Invoke(obj, new object[] { writeFile });
        }

        public void Update_Waiting_Receving(Connect connect)
        {
            if (connect != null)
            {
                switch (connect.PortId)
                {
                    case 1:
                        this.count1++;
                        break;
                    case 2:
                        this.count2++;
                        break;
                    case 3:
                        this.count3++;
                        break;
                    case 4:
                        this.count4++;
                        break;
                    case 5:
                        this.count5++;
                        break;
                    case 6:
                        this.count6++;
                        break;
                }
            }
        }

        public void UpdateConnectForServer(Connect connect, bool checkConnect)
        {
            if (connect.PortId == int.Parse(rdoPort1.Tag.ToString()) && connect.ComputerId == computer)
            {
                pStatus1.BackColor = checkConnect ? Color.Green : Color.Yellow;
                txtStatus1.Text = this.waiting;
                this.btnReset1.Tag = connect.DeviceId;
            }
            if (connect.PortId == int.Parse(rdoPort2.Tag.ToString()) && connect.ComputerId == computer)
            {
                pStatus2.BackColor = checkConnect ? Color.Green : Color.Yellow;
                txtStatus2.Text = this.waiting;
                this.btnReset2.Tag = connect.DeviceId;
            }
            if (connect.PortId == int.Parse(rdoPort3.Tag.ToString()) && connect.ComputerId == computer)
            {
                pStatus3.BackColor = checkConnect ? Color.Green : Color.Yellow;
                txtStatus3.Text = this.waiting;
                this.btnReset3.Tag = connect.DeviceId;
            }
            if (connect.PortId == int.Parse(rdoPort4.Tag.ToString()) && connect.ComputerId == computer)
            {
                pStatus4.BackColor = checkConnect ? Color.Green : Color.Yellow;
                txtStatus4.Text = this.waiting;
                this.btnReset4.Tag = connect.DeviceId;
            }
            if (connect.PortId == int.Parse(rdoPort5.Tag.ToString()) && connect.ComputerId == computer)
            {
                pStatus5.BackColor = checkConnect ? Color.Green : Color.Yellow;
                txtStatus5.Text = this.waiting;
                this.btnReset5.Tag = connect.DeviceId;
            }
            if (connect.PortId == int.Parse(rdoPort6.Tag.ToString()) && connect.ComputerId == computer)
            {
                pStatus6.BackColor = checkConnect ? Color.Green : Color.Yellow;
                txtStatus6.Text = this.waiting;
                this.btnReset6.Tag = connect.DeviceId;
            }
        }

        public void UpdateConnectForClient(Connect connect, bool checkConnect)
        {
            long? deviceId = 0;
            if (connect.Device != null)
            {
                deviceId = connect.DeviceId;
            }
            if (connect.PortId == int.Parse(rdoPort1.Tag.ToString()))
            {
                pStatus1.BackColor = checkConnect ? Color.Green : Color.Red;
                txtStatus1.Text = this.waiting;
                this.btnReset1.Tag = deviceId;
            }
            if (connect.PortId == int.Parse(rdoPort2.Tag.ToString()))
            {
                pStatus2.BackColor = checkConnect ? Color.Green : Color.Red;
                txtStatus2.Text = this.waiting;
                this.btnReset2.Tag = deviceId;
            }
            if (connect.PortId == int.Parse(rdoPort3.Tag.ToString()))
            {
                pStatus3.BackColor = checkConnect ? Color.Green : Color.Red;
                txtStatus3.Text = this.waiting;
                this.btnReset3.Tag = deviceId;
            }
            if (connect.PortId == int.Parse(rdoPort4.Tag.ToString()))
            {
                pStatus4.BackColor = checkConnect ? Color.Green : Color.Red;
                txtStatus4.Text = this.waiting;
                this.btnReset4.Tag = deviceId;
            }
            if (connect.PortId == int.Parse(rdoPort5.Tag.ToString()))
            {
                pStatus5.BackColor = checkConnect ? Color.Green : Color.Red;
                txtStatus5.Text = this.waiting;
                this.btnReset5.Tag = deviceId;
            }
            if (connect.PortId == int.Parse(rdoPort6.Tag.ToString()))
            {
                pStatus6.BackColor = checkConnect ? Color.Green : Color.Red;
                txtStatus6.Text = this.waiting;
                this.btnReset6.Tag = deviceId;
            }
        }

        #endregion

        #region Tạo Timer
        public void InitTimerInConnectCOM(Connect connect, SerialPort serialPort)
        {
            try
            {
                Type type = Type.GetType(this.nameSpace + "." + connect.Device.Protocol);
                MethodInfo method = type.GetMethod("InitTimer");
                var obj = Activator.CreateInstance(type);
                method.Invoke(obj, new object[] { serialPort });
            }
            catch { }
        }

        public void RemoveTimerInConnectCOM(Connect connect)
        {
            try
            {
                Type type = Type.GetType(this.nameSpace + "." + connect.Device.Protocol);
                MethodInfo method = type.GetMethod("RemoveTimer");
                var obj = Activator.CreateInstance(type);
                method.Invoke(obj, new object[] { });
            }
            catch { }
        }
        #endregion

        private void Auto_Check_Write_File(int port, bool check)
        {
            switch (port)
            {
                case 1:
                    {
                        this.chkWriteResult1.Checked = check;
                        break;
                    }
                case 2:
                    {
                        this.chkWriteResult2.Checked = check;
                        break;
                    }
                case 3:
                    {
                        this.chkWriteResult3.Checked = check;
                        break;
                    }
                case 4:
                    {
                        this.chkWriteResult4.Checked = check;
                        break;
                    }
                case 5:
                    {
                        this.chkWriteResult5.Checked = check;
                        break;
                    }
                case 6:
                    {
                        this.chkWriteResult6.Checked = check;
                        break;
                    }
            }
        }
        private void btnViewProtocolLog1_Click(object sender, EventArgs e)
        {
            ViewProtocolLog(txtProtocol1.Text, 1);
            //CobasE411MockServer_Interactive.SendNextStep();

        }

        private void btnViewProtocolLog2_Click(object sender, EventArgs e)
        {
            ViewProtocolLog(txtProtocol2.Text, 2);
            //string response = CobasE411MockServer_Interactive.ReadResponse();
            //MessageBox.Show($"Response: {response}");
        }

        private void btnViewProtocolLog3_Click(object sender, EventArgs e)
        {
            ViewProtocolLog(txtProtocol3.Text, 3);
            //CobasE411MockServer_Interactive.Stop();

        }

        private async void btnViewProtocolLog4_Click(object sender, EventArgs e)
        {
            ViewProtocolLog(txtProtocol4.Text, 4);
            #region Mở phía dưới test máy COBAS E411
            /*
                // 1. Start mock server
                CobasE411MockServer_Interactive.Start(9998);

                // 2. Đợi 1 giây cho server sẵn sàng
                await Task.Delay(1000);

                // 4. Đợi 1 giây cho kết nối ổn định
                await Task.Delay(1000);

                // 5. Gửi tất cả các bước tự động (chạy trong thread riêng để không block UI)
                await Task.Run(() =>
                {
                    bool success = CobasE411MockServer_Interactive.SendAllStepsAuto(500);

                    // Hiển thị kết quả trên UI thread
                    this.Invoke((Action)(() =>
                    {
                        MessageBox.Show(success
                            ? "Test hoàn thành! Kiểm tra DB xem kết quả đã được lưu."
                            : "Test thất bại! Xem log để biết chi tiết.");
                    }));
                });
             */
            #endregion
        }

        private void btnViewProtocolLog5_Click(object sender, EventArgs e)
        {
            ViewProtocolLog(txtProtocol5.Text, 5);
            //CobasE411MockServer.Start();
        }

        private void btnViewProtocolLog6_Click(object sender, EventArgs e)
        {
            ViewProtocolLog(txtProtocol6.Text, 6);
        }

        private void ViewProtocolLog(string protocol, int portNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(protocol))
                {
                    MessageBox.Show($"Cổng {portNumber} chưa có giao thức được cấu hình!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string logFilePath = Path.Combine(Folder_File_Tool.folder, protocol, Folder_File_Tool.fileDateDevice);

                if (!File.Exists(logFilePath))
                {
                    MessageBox.Show($"File log của {protocol} chưa tồn tại!\n\nĐường dẫn: {logFilePath}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Tạo form để hiển thị log
                Form logForm = new Form
                {
                    Text = $"Protocol Log - {protocol} (Cổng {portNumber})",
                    Size = new Size(1000, 600),
                    StartPosition = FormStartPosition.CenterScreen,
                    Icon = this.Icon
                };

                // TextBox để hiển thị nội dung log
                TextBox txtLog = new TextBox
                {
                    Multiline = true,
                    ScrollBars = ScrollBars.Both,
                    Dock = DockStyle.Fill,
                    Font = new Font("Courier New", 9),
                    ReadOnly = true,
                    WordWrap = false
                };

                // Panel chứa các nút
                Panel pnlButtons = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 50
                };

                // Label hiển thị thông tin
                Label lblInfo = new Label
                {
                    Text = $"Protocol: {protocol} | Port: {portNumber} | File: {logFilePath}",
                    Location = new Point(10, 5),
                    AutoSize = true,
                    ForeColor = Color.Blue
                };
                pnlButtons.Controls.Add(lblInfo);

                // Nút làm mới
                Button btnRefresh = new Button
                {
                    Text = "Làm mới",
                    Location = new Point(10, 25),
                    Size = new Size(100, 30)
                };
                btnRefresh.Click += (s, ev) =>
                {
                    try
                    {
                        txtLog.Text = File.ReadAllText(logFilePath, Encoding.UTF8);
                        txtLog.SelectionStart = txtLog.Text.Length;
                        txtLog.ScrollToCaret();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi đọc file log: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                // Nút xóa log
                Button btnClearLog = new Button
                {
                    Text = "Xóa log",
                    Location = new Point(120, 25),
                    Size = new Size(100, 30)
                };
                btnClearLog.Click += (s, ev) =>
                {
                    DialogResult result = MessageBox.Show($"Bạn có chắc muốn xóa toàn bộ log của {protocol}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        try
                        {
                            File.WriteAllText(logFilePath, string.Empty, Encoding.UTF8);
                            txtLog.Text = string.Empty;
                            MessageBox.Show("Đã xóa log thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            log4net.Info($"Cleared log for protocol {protocol} on port {portNumber}");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Lỗi khi xóa log: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                };

                // Nút mở thư mục
                Button btnOpenFolder = new Button
                {
                    Text = "Mở thư mục",
                    Location = new Point(230, 25),
                    Size = new Size(100, 30)
                };
                btnOpenFolder.Click += (s, ev) =>
                {
                    try
                    {
                        string folderPath = Path.GetDirectoryName(logFilePath);
                        System.Diagnostics.Process.Start("explorer.exe", folderPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi mở thư mục: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                // Nút tìm kiếm
                Button btnSearch = new Button
                {
                    Text = "Tìm kiếm",
                    Location = new Point(340, 25),
                    Size = new Size(100, 30)
                };
                btnSearch.Click += (s, ev) =>
                {
                    string searchText = Microsoft.VisualBasic.Interaction.InputBox("Nhập từ khóa cần tìm:", "Tìm kiếm", "");
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        int index = txtLog.Text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);
                        if (index >= 0)
                        {
                            txtLog.Select(index, searchText.Length);
                            txtLog.ScrollToCaret();
                            txtLog.Focus();
                        }
                        else
                        {
                            MessageBox.Show($"Không tìm thấy '{searchText}'", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                };

                // Nút đóng
                Button btnCloseLog = new Button
                {
                    Text = "Đóng",
                    Location = new Point(450, 25),
                    Size = new Size(100, 30)
                };
                btnCloseLog.Click += (s, ev) => logForm.Close();

                pnlButtons.Controls.Add(btnRefresh);
                pnlButtons.Controls.Add(btnClearLog);
                pnlButtons.Controls.Add(btnOpenFolder);
                pnlButtons.Controls.Add(btnSearch);
                pnlButtons.Controls.Add(btnCloseLog);

                logForm.Controls.Add(txtLog);
                logForm.Controls.Add(pnlButtons);

                // Đọc nội dung file log
                txtLog.Text = File.ReadAllText(logFilePath, Encoding.UTF8);

                // Cuộn xuống cuối
                txtLog.SelectionStart = txtLog.Text.Length;
                txtLog.ScrollToCaret();

                logForm.ShowDialog();

                log4net.Info($"Viewed protocol log for {protocol} on port {portNumber}");
            }
            catch (Exception ex)
            {
                log4net.Error($"Error viewing protocol log for port {portNumber}", ex);
                MessageBox.Show($"Lỗi khi mở file log: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
