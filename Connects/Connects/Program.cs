namespace Connects
{
    internal static class Program
    {
        private static string appGuid = "c0a76b5a-12ab-45c5-b9d9-d693faa6e7b9";
        public static frmConnect mainForm;
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using (Mutex mutex = new Mutex(false, "Global\\" + appGuid))
            {
                if (!mutex.WaitOne(0, false))
                {
                    MessageBox.Show("Ứng dụng đã được mở. Không cho phép mở thêm !");
                    return;
                }

                mainForm = new frmConnect();
                Application.Run(mainForm);
            }
        }
    }
}