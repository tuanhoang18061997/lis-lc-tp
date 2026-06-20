using Connects.BL;
using log4net;
using System;
using System.Data;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace Connects
{
    public partial class frmConfigTCP : Form
    {
        private static readonly ILog log4net = LogManager.GetLogger(typeof(frmConfigTCP).Name);
        private int port = 0;

        public frmConfigTCP(int port)
        {
            InitializeComponent();
            this.port = port;
        }

        private void frmConfigCom_Load(object sender, EventArgs e)
        {
            this.LoadToTextbox();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var computerID = SystemInformation.ComputerName;
            var connect = ConnectBL.Get_Connect(this.port, computerID);
            var com = this.txtIPServer.Text;
            var baudRate = double.Parse(this.txtPortServer.Text);
            var description = this.txtDescription.Text;
            if (connect != null)
            {
                ConnectBL.Update(computerID, this.port, com, baudRate, description);
            }
            else
            {
                try
                {
                    ConnectBL.Add(computerID, this.port, com, baudRate, 0, null, 0, false, description);
                }
                catch (Exception ex)
                { }
            }
            this.Close();
        }

        private void LoadToTextbox()
        {
            try
            {
                var connect = ConnectBL.Get_Connect(this.port, SystemInformation.ComputerName);
                if(connect !=null)
                {
                    this.txtIPServer.Text = connect.Com;
                    this.txtPortServer.Text = connect.BaudRate.ToString();
                    this.txtDescription.Text = connect.Description;
                }
            }
            catch(Exception ex)
            {
                log4net.Error(ex);
            }
        }
    }
}
