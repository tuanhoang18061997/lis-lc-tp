using Management.Models;
using Microsoft.EntityFrameworkCore;
using System;
using Object = Management.Models.Object;

namespace Management.BL
{
    public class SettingBL
    {
        private readonly LABContext _db;

        public static string AutoGetPatientInfo { get; set; } = "AutoGetPatientInfo";
        public static string APIGetPatientAndService { get; set; } = "APIGetPatientAndService";
        public static string APIUpdateStatus { get; set; } = "APIUpdateStatus";
        public static string APIUpdateSid { get; set; } = "APIUpdateSID";

        public static string AutoPushResultBHYT { get; set; } = "AutoPushResultBHYT";
        public static string APISendResult { get; set; } = "APISendResult";
        public static string APICancelService { get; set; } = "APICancelService";

        public static string FTP { get; set; } = "FTP";
        public static string FTPUser { get; set; } = "FTPUser";
        public static string FTPPass { get; set; } = "FTPPass";

        public static string SelectAPIStartAutoTask { get; set; } = "SelectAPIStartAutoTask";
        public static string AutoUpdateTime { get; set; } = "AutoUpdateTime";
        public static string PreSearchTime { get; set; } = "PreSearchTime";
        public static string DayCreateSID { get; set; } = "DayCreateSID";
        public static string Sequence { get; set; } = "Sequence";
        public static string BaseUrl { get; set; } = "BaseUrl";

        public SettingBL(LABContext db)
        {
            _db = db;
        }

        public async Task Init()
        {
            try
            {
                var lstSetting = await _db.Settings.ToListAsync();
                if (lstSetting == null || lstSetting.Count == 0)
                {
                    lstSetting = new List<Setting> 
                    {
                        new Setting { Code = AutoGetPatientInfo, Name = AutoGetPatientInfo, Value = "120000"},
                        new Setting { Code = APIGetPatientAndService, Name = APIGetPatientAndService, Value = ""},
                        new Setting { Code = APIUpdateStatus, Name = APIUpdateStatus, Value = ""},
                        new Setting { Code = APIUpdateSid, Name = APIUpdateSid, Value = ""},
                        new Setting { Code = AutoPushResultBHYT, Name = AutoPushResultBHYT, Value = "1800000"},
                        new Setting { Code = APISendResult, Name = APISendResult, Value = ""},
                        new Setting { Code = APICancelService, Name = APICancelService, Value = ""},
                        new Setting { Code = FTP, Name = FTP, Value = ""},
                        new Setting { Code = FTPUser, Name = FTPUser, Value = ""},
                        new Setting { Code = FTPPass, Name = FTPPass, Value = ""},
                        new Setting { Code = SelectAPIStartAutoTask, Name = SelectAPIStartAutoTask, Value = ""}, // 1 là API Đức Tâm, 2 là API MHIS
                        new Setting { Code = AutoUpdateTime, Name = AutoUpdateTime, Value = "10000"},
                        new Setting { Code = PreSearchTime, Name = PreSearchTime, Value = "12"},
                        new Setting { Code = DayCreateSID, Name = DayCreateSID, Value = "1"},
                        new Setting { Code = Sequence, Name = Sequence, Value = "0"},
                        new Setting { Code = BaseUrl, Name = BaseUrl, Value = ""}
                    };
                    await _db.AddRangeAsync(lstSetting);
                    await _db.SaveChangesAsync();
                }
            }
            catch { }
        }

        public async Task<string> GetSeq()
        {
            var _setting = await _db.Settings.Where(p => p.Code == Sequence).FirstOrDefaultAsync();
            if (_setting == null)
            {
                _setting = new Setting() { Code = Sequence, Name = Sequence, Value = "1" };
                await _db.Settings.AddAsync(_setting);
            }
            else if (int.Parse(_setting.Value) == 9999)
            {
                _setting.Value = "1";
            }
            else
            {
                _setting.Value = (int.Parse(_setting.Value) + 1).ToString();
            }
            await _db.SaveChangesAsync();
            return _setting.Value;
        }

        public async Task<string> GetSetting(string code)
        {
            var _setting = await _db.Settings.Where(p => p.Code == code).FirstOrDefaultAsync();
            if (_setting != null) return _setting.Value;
            return null;
        }

        public async Task<Setting> GetSettingByCode(string code)
        {
            return await _db.Settings.Where(p => p.Code == code).FirstOrDefaultAsync();
        }

        public async Task<List<Setting>> GetList()
        {
            return await _db.Settings.ToListAsync();
        }

        public async Task<List<Setting>> GetListByValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return await _db.Settings.ToListAsync();
            }
            else
            {
                return await _db.Settings.Where(p => p.Name.Contains(value)).ToListAsync();
            }
        }

        public async Task<bool> Save(SettingModel setting)
        {
            try
            {
                var _obj = await _db.Settings.Where(p => p.Code == setting.code).FirstOrDefaultAsync();
                if (_obj == null)
                {
                    _obj = new Setting() { Code = setting.code, Name = setting.name, Value = setting.value };
                    await _db.Settings.AddAsync(_obj);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    _obj.Name = setting.name;
                    _obj.Value = setting.value;
                    await _db.SaveChangesAsync();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Setting> Add(string code, string name, string value)
        {
            try
            {
                var _obj = new Setting() { Code = code, Name = name, Value = value };
                await _db.Settings.AddAsync(_obj);
                await _db.SaveChangesAsync();
                return _obj;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> Delete(string code)
        {
            try
            {
                var _obj = await _db.Settings.Where(p => p.Code == code).FirstOrDefaultAsync();
                if (_obj != null)
                {
                    _db.Settings.Remove(_obj);
                    await _db.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
