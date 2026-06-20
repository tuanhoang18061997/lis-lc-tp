using System;
using System.Configuration;
using System.IO;

namespace Connects.BL
{
    public class ConfigConnectString
    {
        public static string folder = @"System";
        public static string file = @"System\ConnectString.txt";

        public static bool SetConnectString()
        {
            try
            {
                if (File.Exists(file))
                {
                    string note = File.ReadAllText(file);
                    string param = Note.Decrypt(note);
                    string[] arrParam = param.Split(';');
                    string server = arrParam[0].Split('=')[1].ToString();
                    string data = arrParam[1].Split('=')[1].ToString();
                    string user = arrParam[2].Split('=')[1].ToString();
                    string pass = arrParam[3].Split('=')[1].ToString();
                    string connectString = "metadata=res://*/;provider=System.Data.SqlClient;provider connection string='data source=" + server + ";initial catalog=" + data + ";user id=" + user + ";password=" + pass + ";MultipleActiveResultSets=True;App=EntityFramework'";

                    var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    var connectionStringsSection = (ConnectionStringsSection)config.GetSection("connectionStrings");
                    if (connectionStringsSection != null)
                    {
                        connectionStringsSection.ConnectionStrings[0].Name = "LABContext";
                        connectionStringsSection.ConnectionStrings[0].ConnectionString = connectString;
                        connectionStringsSection.ConnectionStrings[0].ProviderName = "System.Data.EntityClient";
                        config.Save();
                        ConfigurationManager.RefreshSection("connectionStrings");
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public static string SetConnectString_SQL()
        {
            try
            {
                if (File.Exists(file))
                {
                    string param = Note.Decrypt(File.ReadAllText(file));
                    string[] arrParam = param.Split(';');
                    string server = arrParam[0].Split('=')[1].ToString();
                    string data = arrParam[1].Split('=')[1].ToString();
                    string user = arrParam[2].Split('=')[1].ToString();
                    string pass = arrParam[3].Split('=')[1].ToString();
                    string sqlString = "Data Source=" + server + ";Initial Catalog=" + data + ";Persist Security Info=True;User ID=" + user + ";Password=" + pass + "";
                    return sqlString;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
