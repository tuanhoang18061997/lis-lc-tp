using Connects.BL;
using System;
using System.Windows.Forms;


namespace Connects
{
    public partial class frmLogin : Form
    {
        public bool isLogin = false;
        public bool isConnect = false;
        public frmLogin( bool isConnect)
        {
            InitializeComponent();
            this.isConnect = isConnect;
        }

        #region ************************************************************************* EVENT
        private void btnLogin_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(txtUserCode.Text.Trim()))
            {
                MessageBox.Show("Nhập tên đăng nhập", "Thông báo", MessageBoxButtons.OK);
                this.txtUserCode.Focus();
                return;
            }
            if (string.IsNullOrEmpty(txtPassword.Text.Trim()))
            {
                MessageBox.Show("Nhập mật khẩu", "Thông báo", MessageBoxButtons.OK);
                this.txtPassword.Focus();
                return;
            }

            var user = UserBL.Get_UserByUserCode(this.txtUserCode.Text);

            if(user == null)
            {
                MessageBox.Show("Tên đăng nhập không tồn tại", "Thông báo", MessageBoxButtons.OK);
                this.txtUserCode.Focus();
                return;
            }
            if(user.Password != txtPassword.Text)
            {
                MessageBox.Show("Mật khẩu không đúng", "Thông báo", MessageBoxButtons.OK);
                this.txtPassword.Focus();
                return;
            }

            SessionBL.ResetStatusAllUserOnCumputerID(SystemInformation.ComputerName);
            SessionBL.InsertOrUpdate(user.Id, SystemInformation.ComputerName);
            this.isLogin = true;
            this.Close();         
        }

        private void chkShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            if(this.chkShowPassword.Checked)
                this.txtPassword.PasswordChar = '\0';
            else
                this.txtPassword.PasswordChar = '*';
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void txtUserID_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                this.txtPassword.Focus();
            }
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.btnLogin.Focus();
            }
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
            this.txtUserCode.Focus();
        }
        #endregion
    }
}
