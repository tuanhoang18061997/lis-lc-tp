using Connects.Models;
using System;
using System.Data;
using System.Linq;

namespace Connects.BL
{
    public class SettingBL
    {
        private static LABContext db;

        public static Setting Get_Setting(string code)
        {
            db = new LABContext();
            var setting = db.Settings.Where(p => p.Code == code).FirstOrDefault();
            return setting;
        }
    }
}
