using System;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Data.SqlClient;
using Connects.BL;

namespace Connects
{
    public partial class frmConnectString : Form
    {
        private string Server_Key = "Server";
        private string Data_Key = "Data";
        private string User_Key = "User";
        private string Pass_Key = "Pass";
        public bool isOk = false;
        public bool fromMenu = false;

        public frmConnectString(bool fromMenu = false)
        {
            InitializeComponent();
            this.fromMenu = fromMenu;
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists(ConfigConnectString.file))
                {
                    string param = File.ReadAllText(ConfigConnectString.file);
                    string[] arrParam = param.Split(';');
                    string server = arrParam[0].Split('=')[1].ToString();
                    string data = arrParam[1].Split('=')[1].ToString();
                    string user = arrParam[2].Split('=')[1].ToString();
                    string pass = arrParam[3].Split('=')[1].ToString();

                    this.txtServer.Text = server;
                    this.txtData.Text = data;
                    this.txtUserId.Text = user;
                    this.txtPassword.Text = pass;
                }
            }
            catch { }
            this.txtServer.Focus();
        }


        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtServer.Text.Trim()))
            {
                MessageBox.Show("Nhập server", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.txtServer.Focus();
                return;
            }
            if (string.IsNullOrEmpty(txtData.Text.Trim()))
            {
                MessageBox.Show("Nhập data name", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.txtData.Focus();
                return;
            }
            if (string.IsNullOrEmpty(txtUserId.Text.Trim()))
            {
                MessageBox.Show("Nhập user", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.txtUserId.Focus();
                return;
            }

            if (string.IsNullOrEmpty(txtPassword.Text.Trim()))
            {
                MessageBox.Show("Nhập password", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.txtPassword.Focus();
                return;
            }

            string server = this.txtServer.Text;
            string data = this.txtData.Text;
            string user = this.txtUserId.Text;
            string pass = this.txtPassword.Text;

            if (!this.VerifyConnectivity(server, data, user, pass))
            {
                MessageBox.Show("Không thể kết nối đến cơ sở dữ liệu " + data + ". Vui lòng cấu hình lại kết nối!", "Thông báo!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.txtServer.Focus();
                return;
            }
            else
            {
                if(Directory.Exists(ConfigConnectString.folder))
                {
                    if (File.Exists(ConfigConnectString.file)) File.Delete(ConfigConnectString.file);
                }
                else
                {
                    Directory.CreateDirectory(ConfigConnectString.folder);
                }
                FileStream fs = new FileStream(ConfigConnectString.file, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                string connectString = Server_Key + "=" + server + ";" + Data_Key + "=" + data + ";" + User_Key + "=" + user + ";" + Pass_Key + "=" + pass;
                var note = Note.Encrypt(connectString);
                sw.Write(note);
                sw.Flush();
                fs.Close();
                if (ConfigConnectString.SetConnectString())
                {
                    if(this.fromMenu)
                    {
                        MessageBox.Show("Kết nối đến cơ sở dữ liệu " + data + " thành công.\nTắt ứng dụng và mở lại để kết nối tới cơ sở dữ liệu vừa thiết lập ", "Thông báo!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.isOk = true;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Kết nối đến cơ sở dữ liệu " + data + " thành công!", "Thông báo!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.isOk = true;
                        this.Close();
                    }                   
                }
                else
                {
                    MessageBox.Show("Không thể kết nối đến cơ sở dữ liệu " + data + ". Vui lòng cấu hình lại kết nối!", "Thông báo!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.txtServer.Focus();
                    return;
                }
            }
        }

        private bool VerifyConnectivity(string server, string data, string user, string pass)
        {
            string connectString = "data source=" + server + ";initial catalog=" + data + ";user id=" + user + ";password=" + pass + ";MultipleActiveResultSets=True;App=EntityFramework";
            using (SqlConnection connection = new SqlConnection())
            {
                connection.ConnectionString = connectString;
                try
                {
                    connection.Open();
                }
                catch
                {
                    return false;
                }
                return true;
            }
        }

        private void chkShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chkShowPassword.Checked)
                this.txtPassword.PasswordChar = '\0';
            else
                this.txtPassword.PasswordChar = '*';
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void txtServer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.txtData.Focus();
            }
        }

        private void txtdata_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.txtUserId.Focus();
            }
        }

        private void txtUserId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.txtPassword.Focus();
            }
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.btnSave.Focus();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (File.Exists(ConfigConnectString.file))
            {
                string param = File.ReadAllText(ConfigConnectString.file);
                string[] arrParam = param.Split(';');
                string server = arrParam[0].Split('=')[1].ToString();
                string data = arrParam[1].Split('=')[1].ToString();
                string user = arrParam[2].Split('=')[1].ToString();
                string pass = arrParam[3].Split('=')[1].ToString();

                this.txtServer.Text = server;
                this.txtData.Text = data;
                this.txtUserId.Text = user;
                this.txtPassword.Text = pass;
            }
            this.txtServer.Focus();
        }
    }
}
