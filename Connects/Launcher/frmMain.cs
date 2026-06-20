using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace Launcher
{
    public partial class frmMain : Form
    {
        private static string user = "ftpadmin";
        private static string pass = "P@$$wordftp";
        private static string versionServer = @"ftp://192.168.0.250/App/Connect/Version.txt";
        private static string versionLocal = @"System\Version.txt";

        private static string sourceServer_Zip = @"ftp://192.168.0.250/App/Connect/Source.zip";      
        private static string sourceLocal_Zip =  @"Source.zip";

        private static string extractFolder = Directory.GetCurrentDirectory();
        private static string sourceLocal =  @"Source";
        private static string systemLocal = @"System";
        private static string connect = @"Source\Connects";
        private static string fileFTP = @"System\FTP.txt";
        public frmMain()
        {
            InitializeComponent();
        }   

        private void frmMain_Load(object sender, EventArgs e)
        {
            Check_Account();
            Thread thread = new Thread(ThreadUpdateVersion);
            thread.Start();
        }

        private void Check_Account()
        {
            if (File.Exists(fileFTP))
            {
                var key = File.ReadAllText(fileFTP);
                var arrayKey = key.Split(';');
                user = arrayKey[0].Split('=')[1];
                pass = arrayKey[1].Split('=')[1];
                versionServer = arrayKey[2].Split('=')[1];
                sourceServer_Zip = arrayKey[3].Split('=')[1];
            }
            else
            {
                if (!Directory.Exists(systemLocal)) Directory.CreateDirectory(systemLocal);
                var stringKey = "User=" + user + ";Pass=" + pass + ";VersionServer=" + versionServer + ";SourceServer=" + sourceServer_Zip;
                using (FileStream fs = new FileStream(fileFTP, FileMode.Create))
                {
                    StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                    sw.Write(stringKey);
                    sw.Flush();
                }
            }
        }

        public void ThreadUpdateVersion()
        {
            UpdateVersion();
        }    

        private void UpdateVersion()
        {
            try
            {
                if (CheckVersion())
                {
                    Download();
                    ExtractZip_Remove();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Không thể cập nhật phiên bản mới. Lỗi mạng hoặc FTP nên kiểm tra lại!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            try
            {
                OpenConnect();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Không thể mở ứng dụng. Vui lòng kiểm tra lại !", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
        }

        private bool CheckVersion()
        {
            var version = false;
            // Version FTP
            WebClient request = new WebClient();
            request.Credentials = new NetworkCredential(user, pass);
            byte[] newFileData = request.DownloadData(versionServer);
            string contentVersionFTP = Encoding.UTF8.GetString(newFileData);

            // Version local
            if (File.Exists(versionLocal))
            {
                var contentVersionLocal = File.ReadAllText(versionLocal);
                if (contentVersionFTP != contentVersionLocal)
                {
                    version = true;
                }
                File.Delete(versionLocal);
            }
            else
            {
                version = true;
            }

            // Copy file Version.txt từ server
            using (FileStream fs = new FileStream(versionLocal, FileMode.Create))
            {
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                sw.Write(contentVersionFTP);
                sw.Flush();
            }
            return version;
        }

        private void Download()
        {
            NetworkCredential credentials = new NetworkCredential(user, pass);
            lbStatus.BeginInvoke(new Action(() =>
            {
                lbStatus.Text = "Đang cập nhật phiên bản mới của phần mềm ...";
            }));

            WebRequest sizeRequest = WebRequest.Create(sourceServer_Zip);
            sizeRequest.Credentials = credentials;
            sizeRequest.Method = WebRequestMethods.Ftp.GetFileSize;
            int size = (int)sizeRequest.GetResponse().ContentLength;
            progressBar.Invoke((MethodInvoker)(() => progressBar.Maximum = size));

            WebRequest request = WebRequest.Create(sourceServer_Zip);
            request.Credentials = credentials;
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            using (Stream ftpStream = request.GetResponse().GetResponseStream())
            using (Stream fileStream = File.Create(sourceLocal_Zip))
            {
                byte[] buffer = new byte[10240];
                int read;
                while ((read = ftpStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fileStream.Write(buffer, 0, read);
                    int position = (int)fileStream.Position;

                    progressBar.BeginInvoke(new Action(() =>
                    {
                        progressBar.Value = position;
                    }));
                    if (position == size)
                    {
                        lbStatus.BeginInvoke(new Action(() =>
                        {
                            lbStatus.Text = "Cập nhật thành công !";
                        }));
                    }
                }
            }
        }

        private void ExtractZip_Remove()
        {
            if (File.Exists(sourceLocal_Zip))
            {
                if (Directory.Exists(sourceLocal)) Directory.Delete(sourceLocal,true);
                ZipFile.ExtractToDirectory(sourceLocal_Zip, extractFolder);
                File.Delete(sourceLocal_Zip);
            }
        }

        public void OpenConnect()
        {
            System.Diagnostics.Process.Start(connect);
            Application.Exit();
            Close();
        }
    }
}
