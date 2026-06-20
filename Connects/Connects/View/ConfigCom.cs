using Connects.BL;
using System;
using System.Data;
using System.IO.Ports;
using System.Linq;
using System.Windows.Forms;

namespace Connects
{
    public partial class frmConfigCom : Form
    {
        SerialPort sp = new SerialPort();
        int port;

        public frmConfigCom(int port)
        {
            InitializeComponent();
            this.InitializeEvent();           
            this.port = port;
        }

        #region ************************************************************************* METHOD
        private void frmConfigCom_Load(object sender, EventArgs e)
        {           
            this.LoadCOM();
            this.LoadBaudRate();
            this.LoadDataBits();
            this.LoadParity();
            this.LoadStopBit();
            this.ShowHideButton(false, false);
        }
        private void cbStopBit_Click(object sender, EventArgs e)
        {
            this.ShowHideButton(true, true);
        }

        private void cbParirty_Click(object sender, EventArgs e)
        {
            this.ShowHideButton(true, true);
        }

        private void cbDataBit_Click(object sender, EventArgs e)
        {
            this.ShowHideButton(true, true);
        }

        private void cbBaudRate_Click(object sender, EventArgs e)
        {
            this.ShowHideButton(true, true);
        }
        private void cbCOM_Click(object sender, EventArgs e)
        {
            this.ShowHideButton(true, true);
        }

        private void txtDescription_TextChanged(object sender, EventArgs e)
        {
            this.ShowHideButton(true, true);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cbCOM.Text))
            {
                MessageBox.Show("Vui lòng chọn COM !", "Thông báo", MessageBoxButtons.OK);
                return;
            }
            if (string.IsNullOrEmpty(cbBaudRate.Text))
            {
                MessageBox.Show("Vui lòng chọn Baud rate !", "Thông báo", MessageBoxButtons.OK);
                return;
            }
            if (string.IsNullOrEmpty(cbDataBit.Text))
            {
                MessageBox.Show("Vui lòng chọn Data bits !", "Thông báo", MessageBoxButtons.OK);
                return;
            }
            if (string.IsNullOrEmpty(cbParirty.Text))
            {
                MessageBox.Show("Vui lòng chọn Parity !", "Thông báo", MessageBoxButtons.OK);
                return;
            }
            if (string.IsNullOrEmpty(cbStopBit.Text))
            {
                MessageBox.Show("Vui lòng chọn top bit !", "Thông báo", MessageBoxButtons.OK);
                return;
            }

            // Luu
            string computerID = SystemInformation.ComputerName;
            var connect = ConnectBL.Get_Connect(this.port, computerID);

            if (connect != null)
            {
                try
                {
                    string com = cbCOM.Text;
                    double baudRate = double.Parse(cbBaudRate.Text);
                    double dataBit = double.Parse(cbDataBit.Text);
                    string parity = cbParirty.Text;
                    double stopBit = double.Parse(cbStopBit.Text);
                    bool rts = this.chkRTS.Checked;
                    string description = this.txtDescription.Text;
                    ConnectBL.Update(connect.ComputerId, connect.PortId, com, baudRate, dataBit, parity, stopBit, rts, description);
                }
                catch { }          
            }
            else
            {
                try
                {
                    var portID = this.port;
                    var com = cbCOM.Text;
                    var baudRate = double.Parse(cbBaudRate.Text);
                    var dataBit = double.Parse(cbDataBit.Text);
                    var parity = cbParirty.Text;
                    var stopBit = double.Parse(cbStopBit.Text);
                    bool rts = this.chkRTS.Checked;
                    string description = this.txtDescription.Text;

                    ConnectBL.Add(computerID, portID, com, baudRate, dataBit, parity, stopBit, rts, description);
                }
                catch(Exception ex)
                { }
            }

            btnSave.Enabled = false;
            btnCancel.Enabled = false;
            this.Close();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            string id = cbCOM.Text;
            string computerID = SystemInformation.ComputerName;
            var connect = ConnectBL.Get_Connect(this.port, computerID);
            if (connect != null)
            {
                cbCOM.Text = connect.Com;
                cbBaudRate.Text = connect.BaudRate.ToString();
                cbDataBit.Text = connect.DataBit.ToString();
                cbParirty.Text = connect.Parity.ToString();
                cbStopBit.Text = connect.StopBit.ToString();
                txtDescription.Text = connect.Description;
            }
            else
            {
                this.DeleteText();
            }

            this.ShowHideButton(false, false);
        }
        #endregion

        #region ************************************************************************* EVENT

        private void InitializeEvent()
        {
            cbCOM.Click += cbCOM_Click;
            cbBaudRate.Click += cbBaudRate_Click;
            cbDataBit.Click += cbDataBit_Click;
            cbParirty.Click += cbParirty_Click;
            cbStopBit.Click += cbStopBit_Click;
        }

        private void LoadCOM()
        {
            this.ShowHideButton(false, false);
            string computer = SystemInformation.ComputerName;

            string[] lstCom = SerialPort.GetPortNames();
            string portsTmp = string.Empty;

            // Xóa tất cả cổng COM đang kết nối
            var lstConnect = ConnectBL.Get_ListConnect(computer, true);
            if(lstCom != null && lstCom.Length > 0)
            {
                foreach(var com in lstCom)
                {
                    var exit = lstConnect.Where(p => p.Com == com).FirstOrDefault();
                    if (exit == null)
                    {
                        if (string.IsNullOrEmpty(portsTmp))
                            portsTmp += com;
                        else
                            portsTmp += "," + com;
                    }
                }
                lstCom = portsTmp.Split(',');
            }           
            cbCOM.Items.AddRange(lstCom);

            var connect = ConnectBL.Get_Connect(this.port, computer);
            if (connect != null)
            {
                cbCOM.Text = connect.Com;
                cbBaudRate.Text = connect.BaudRate?.ToString();
                cbDataBit.Text = connect.DataBit?.ToString();
                cbParirty.Text = connect.Parity?.ToString();
                cbStopBit.Text = connect.StopBit?.ToString();
                chkRTS.Checked = connect.Rts;
                txtDescription.Text = connect.Description?.ToString();
            }
            else
            {
                this.DeleteText();
            }

        }

        private void LoadBaudRate()
        {
            string[] baudRate = { "50", "75", "110", "150", "300", "600", "1200", "2400", "4800", "9600",
                                  "19200", "28800", "38400", "56000", "115200" };
            cbBaudRate.Items.AddRange(baudRate);
        }

        private void LoadDataBits()
        {
            string[] dataBits = { "5", "6", "7", "8", "9" };
            cbDataBit.Items.AddRange(dataBits);
        }

        private void LoadParity()
        {
            string[] parity = { "None", "Odd", "Even" };
            cbParirty.Items.AddRange(parity);
        }

        private void LoadStopBit()
        {
            string[] stopBit = { "1", "1.5", "2", "2.5" };
            cbStopBit.Items.AddRange(stopBit);
        }

        private void DeleteText()
        {
            cbBaudRate.Text = string.Empty;
            cbDataBit.Text = string.Empty;
            cbParirty.Text = string.Empty;
            cbStopBit.Text = string.Empty;
            txtDescription.Text = string.Empty;
            chkRTS.Checked = false;
        }

        private void ShowHideButton(bool save, bool cancel)
        {
            btnSave.Enabled = save;
            btnCancel.Enabled = cancel;
        }

        private void chkRTS_CheckedChanged(object sender, EventArgs e)
        {
            this.ShowHideButton(true, true);
        }
        #endregion

       
    }
}
