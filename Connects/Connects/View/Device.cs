using Connects.BL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Connects
{
    public partial class frmDevice : Form
    {
        List<ComboboxItemString> lstComboItem = new List<ComboboxItemString>();
        public long idDevice = 0;
        public bool isSelect = false;
        public int port = 0;
        public string computer = string.Empty;

        public frmDevice(int cong, string computer)
        {
            InitializeComponent();
            this.port = cong;
            this.computer = computer;
        }

        private void frmDevice_Load(object sender, EventArgs e)
        {
            this.LoadDevice(string.Empty);
            this.ShowHideButton(true, true, false, false, true);
            this.txtCode.Enabled = false;
            this.cbDevice.Focus();
        }


        #region ************************************************************************* METHOD

        private void LoadDevice(string code)
        {
            this.lstComboItem.Clear();
            this.cbDevice.Items.Clear();
            var lstDevice = DeviceBL.Get_ListDevice();

            if (lstDevice != null && lstDevice.Count > 0)
            {
                foreach (var device in lstDevice)
                {
                    ComboboxItemString item = new ComboboxItemString();
                    item.Value = device.Id;
                    item.Text = device.Name;
                    item.Code = device.Code;

                    this.lstComboItem.Add(item);
                }

                if (this.lstComboItem.Count > 0)
                {
                    cbDevice.Items.AddRange(this.lstComboItem.ToArray());

                    var connect = ConnectBL.Get_Connect(this.port, this.computer);
                    if (connect != null)
                    {
                        var item = this.lstComboItem.Where(p => p.Value == connect.DeviceId).FirstOrDefault();
                        if (item != null)
                        {
                            cbDevice.SelectedItem = item;
                            this.idDevice = item.Value;
                        }
                    }
                    else
                    {
                        var item = this.lstComboItem.Where(p => p.Code == code).FirstOrDefault();
                        if (item != null)
                            cbDevice.SelectedItem = item;
                    }
                }
            }
            else
            {
                cbDevice.Items.Clear();
                this.lstComboItem.Clear();
            }
        }

        private void DeleteText()
        {
            txtCode.Text = string.Empty;
            txtName.Text = string.Empty;
            txtProtocol.Text = string.Empty;
        }

        private void ShowHideButton(bool them, bool xoa, bool luu, bool huy, bool chon)
        {
            btnNew.Visible = them;
            btnDelete.Enabled = xoa;
            btnSave.Visible = luu;
            btnCancel.Enabled = huy;
            btnSelect.Enabled = chon;
        }

        #endregion

        #region ************************************************************************* EVENT
        private void cbDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = cbDevice.SelectedItem as ComboboxItemString;
            if(item != null)
            {
                var device = DeviceBL.Get_Device(item.Value);
                if (device != null)
                {
                    txtId.Text = device.Id.ToString();
                    txtCode.Text = device.Code;
                    txtName.Text = device.Name;
                    txtProtocol.Text = device.Protocol;
                }              
            }
            this.ShowHideButton(true, true, false, false, true);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCode.Text.Trim()))
                MessageBox.Show("Nhập mã máy", "Thông báo", MessageBoxButtons.OK);

            if (string.IsNullOrEmpty(txtName.Text.Trim()))
                MessageBox.Show("Nhập tên máy", "Thông báo",MessageBoxButtons.OK);

            try
            {
                var model = DeviceBL.Get_Device(txtCode.Text);
                var computerID = SystemInformation.ComputerName;

                if (model == null)
                {
                    DeviceBL.Add(txtCode.Text, txtName.Text, txtProtocol.Text, computerID, this.chkActive.Checked);
                }
                else
                {
                    DeviceBL.Update(model.Id, txtCode.Text, txtName.Text, txtProtocol.Text, computerID, this.chkActive.Checked);
                }

                this.txtCode.Enabled = false;
                this.LoadDevice(txtCode.Text);
                this.ShowHideButton(true, true, false, false, true);
            }
            catch { }
            
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            long deviceId = long.Parse(txtId.Text);

            // Kiểm tra nếu máy đang kết nối với COM thì ko cho xóa
            var connect = ConnectBL.Get_ConnectByDeviceId(deviceId);
            if (connect != null && connect.Status)
            {
                MessageBox.Show("Máy đang kết nối. Không thể xóa !", "Thông báo", MessageBoxButtons.OK);
            }
            else
            {
                ConnectBL.Update_DeviceId(deviceId);
                DeviceBL.Delete(deviceId);              
                this.cbDevice.Text = string.Empty;
                this.idDevice = 0;
                this.DeleteText();
                this.LoadDevice(string.Empty);
                btnDelete.Enabled = true;
                btnSelect.Enabled = false;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            long deviceId = long.Parse(txtCode.Text);
            var device = DeviceBL.Get_Device(deviceId);

            if (device == null)
            {
                this.DeleteText();
                this.ShowHideButton(true, false, false, false, false);
            }
            else
            {
                txtCode.Text = device.Id.ToString();
                txtName.Text = device.Name;
                txtProtocol.Text = device.Protocol;
                this.ShowHideButton(true, false, false, false, true);
            }         
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            ComboboxItemString item = cbDevice.SelectedItem as ComboboxItemString;
            if (item != null)
            {
                this.idDevice = item.Value;
            }
            this.isSelect = true;
            this.Close();
        }

        private void txtDeviceID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.txtName.Focus();
            }
        }

        private void txtDeviceName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.txtProtocol.Focus();
            }
        }

        private void txtProtocol_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.btnSave.Focus();
            }
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            this.txtCode.Enabled = true;

            txtCode.Text = string.Empty;
            txtName.Text = string.Empty;
            txtProtocol.Text = string.Empty;

            this.ShowHideButton(false, false, true, true, false);
            cbDevice.Text = string.Empty;
            txtCode.Focus();
        }

        private void txtDeviceID_Leave(object sender, EventArgs e)
        {
            var code = this.txtCode.Text;
            if (!string.IsNullOrEmpty(code))
            {
                var isExist = DeviceBL.CheckExist(code);
                if (isExist)
                {
                    MessageBox.Show("Mã máy đã tồn tại. Vui lòng nhập lại !", "Thông báo", MessageBoxButtons.OK);
                    this.txtCode.Focus();
                }
            }
        }

        #endregion
    }
}
