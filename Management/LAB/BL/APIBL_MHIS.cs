using Management.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using Object = Management.Models.Object;

namespace Management.BL
{
    public class APIBL_MHIS
    {
        public LABContext _db;

        public static Setting settingAutoGetPatientInfo = null;
        public static Setting settingAPIGetPatientAndService = null;
        public static Setting settingAPIUpdateStatus = null;
        public static Setting settingAPIUpdateSid = null;

        public static Setting settingAPISendResult = null;
        public static Setting settingAutoPushResultBHYT = null;

        public static Setting settingAutoUpdateTime = null;
        public static Setting settingPreSearchTime = null;
        public static Setting settingDayCreateSID = null;

        public static List<Doctor> lstDoctor = null;
        public static List<Object> lstObject = null;
        public static List<Location> lstLocation = null;

        public static System.Timers.Timer timerAutoUpdateTime;
        public static System.Timers.Timer timerAutoGetPatientInfo;
        public static System.Timers.Timer timerAutoPushResultBHYT;

        public APIBL_MHIS()
        {
            _db = new LABContext();
        }

        public async Task<bool> StartAutoTask()
        {
            try
            {
                await Refresh_Properties();
                await Init_Properties();
                await Init_Timer();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Refresh_Properties()
        {
            try
            {
                await RemoveTimer_Auto_GetPatientAndService();
                await RemoveTimer_Auto_PushResultBHYT();
                await RemoveTimer_Auto_Update_ResultStandard_To_ResultXN();

                settingAutoGetPatientInfo = null;
                settingAPIGetPatientAndService = null;
                settingAPIUpdateStatus = null;
                settingAPIUpdateSid = null;
                settingAPISendResult = null;
                settingAutoPushResultBHYT = null;
                settingAutoUpdateTime = null;
                settingPreSearchTime = null;
                settingDayCreateSID = null;

                lstDoctor = null;
                lstObject = null;
                lstLocation = null;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task Init_Properties()
        {
            // Patient Info And Service
            if (settingAutoGetPatientInfo == null)
            {
                settingAutoGetPatientInfo = await _db.Settings.Where(p => p.Code == SettingBL.AutoGetPatientInfo).FirstOrDefaultAsync();
                if (settingAutoGetPatientInfo == null)
                {
                    try
                    {
                        settingAutoGetPatientInfo = new Setting { Code = SettingBL.AutoGetPatientInfo, Name = SettingBL.AutoGetPatientInfo, Value = "120000" };
                        await _db.Settings.AddAsync(settingAutoGetPatientInfo);
                        await _db.SaveChangesAsync();
                    }
                    catch { }
                }
            }
            if (settingAPIGetPatientAndService == null)
            {
                settingAPIGetPatientAndService = await _db.Settings.Where(p => p.Code == SettingBL.APIGetPatientAndService).FirstOrDefaultAsync();
                if (settingAPIGetPatientAndService == null)
                {
                    try
                    {
                        settingAPIGetPatientAndService = new Setting { Code = SettingBL.APIGetPatientAndService, Name = SettingBL.APIGetPatientAndService, Value = String.Empty };
                        await _db.Settings.AddAsync(settingAPIGetPatientAndService);
                        await _db.SaveChangesAsync();
                    }
                    catch { }
                }
            }
            if (settingAPIUpdateStatus == null)
            {
                settingAPIUpdateStatus = await _db.Settings.Where(p => p.Code == SettingBL.APIUpdateStatus).FirstOrDefaultAsync();
                if (settingAPIUpdateStatus == null)
                {
                    try
                    {
                        settingAPIUpdateStatus = new Setting { Code = SettingBL.APIUpdateStatus, Name = SettingBL.APIUpdateStatus, Value = String.Empty };
                        await _db.Settings.AddAsync(settingAPIUpdateStatus);
                        await _db.SaveChangesAsync();
                    }
                    catch { }
                }
            }
            if (settingAPIUpdateSid == null)
            {
                settingAPIUpdateSid = await _db.Settings.Where(p => p.Code == SettingBL.APIUpdateSid).FirstOrDefaultAsync();
                if (settingAPIUpdateSid == null)
                {
                    try
                    {
                        settingAPIUpdateSid = new Setting { Code = SettingBL.APIUpdateSid, Name = SettingBL.APIUpdateSid, Value = String.Empty };
                        await _db.Settings.AddAsync(settingAPIUpdateSid);
                        await _db.SaveChangesAsync();
                    }
                    catch { }
                }
            }

            // Push Result BHYT
            if (settingAutoPushResultBHYT == null)
            {
                settingAutoPushResultBHYT = await _db.Settings.Where(p => p.Code == SettingBL.AutoPushResultBHYT).FirstOrDefaultAsync();
                if (settingAutoPushResultBHYT == null)
                {
                    try
                    {
                        settingAutoPushResultBHYT = new Setting { Code = SettingBL.AutoPushResultBHYT, Name = SettingBL.AutoPushResultBHYT, Value = "300000" };
                        await _db.Settings.AddAsync(settingAutoPushResultBHYT);
                        await _db.SaveChangesAsync();
                    }
                    catch { }
                }
            }
            if (settingAPISendResult == null)
            {
                settingAPISendResult = await _db.Settings.Where(p => p.Code == SettingBL.APISendResult).FirstOrDefaultAsync();
                if (settingAPISendResult == null)
                {
                    try
                    {
                        settingAPISendResult = new Setting { Code = SettingBL.APISendResult, Name = SettingBL.APISendResult, Value = String.Empty };
                        await _db.Settings.AddAsync(settingAPISendResult);
                        await _db.SaveChangesAsync();
                    }
                    catch { }
                }
            }

            // Auto Update Result From ResultStandard To ResultXN
            if (settingAutoUpdateTime == null)
            {
                settingAutoUpdateTime = await _db.Settings.Where(p => p.Code == SettingBL.AutoUpdateTime).FirstOrDefaultAsync();
                if (settingAutoUpdateTime == null)
                {
                    try
                    {
                        settingAutoUpdateTime = new Setting { Code = SettingBL.AutoUpdateTime, Name = SettingBL.AutoUpdateTime, Value = "15000" };
                        await _db.Settings.AddAsync(settingAutoUpdateTime);
                        await _db.SaveChangesAsync();
                    }
                    catch { }
                }
            }
            if (settingPreSearchTime == null)
            {
                settingPreSearchTime = await _db.Settings.Where(p => p.Code == SettingBL.PreSearchTime).FirstOrDefaultAsync();
                if (settingPreSearchTime == null)
                {
                    try
                    {
                        settingPreSearchTime = new Setting { Code = SettingBL.PreSearchTime, Name = SettingBL.PreSearchTime, Value = "2" };
                        await _db.Settings.AddAsync(settingPreSearchTime);
                        await _db.SaveChangesAsync();
                    }
                    catch { }
                }
            }
            if (settingDayCreateSID == null)
            {
                settingDayCreateSID = await _db.Settings.Where(p => p.Code == SettingBL.DayCreateSID).FirstOrDefaultAsync();
                if (settingDayCreateSID == null)
                {
                    try
                    {
                        settingDayCreateSID = new Setting { Code = SettingBL.DayCreateSID, Name = SettingBL.DayCreateSID, Value = "1" };
                        await _db.Settings.AddAsync(settingDayCreateSID);
                        await _db.SaveChangesAsync();
                    }
                    catch { }
                }
            }

            // Get Object - Doctor - Location
            if (lstObject == null)
            {
                lstObject = await _db.Objects.Where(p => p.Active == true).ToListAsync();
            }
            if (lstDoctor == null)
            {
                lstDoctor = await _db.Doctors.Where(p => p.Active == true).ToListAsync();
            }
            if (lstLocation == null)
            {
                lstLocation = await _db.Locations.Where(p => p.Active == true).ToListAsync();
            }
        }

        public async Task Init_Timer()
        {
            await InitTimer_Auto_GetPatientAndService();
            await InitTimer_Auto_Update_ResultStandard_To_ResultXN();
            await InitTimer_Auto_PushResultBHYT();
        }

        public async Task InitTimer_Auto_GetPatientAndService()
        {
            try
            {
                if (timerAutoGetPatientInfo == null)
                {
                    timerAutoGetPatientInfo = new System.Timers.Timer();
                    timerAutoGetPatientInfo.Elapsed += TimerAutoGetPatientInfo_Elapsed;
                    timerAutoGetPatientInfo.Interval = settingAutoGetPatientInfo != null ? double.Parse(settingAutoGetPatientInfo.Value) : 120000;
                    timerAutoGetPatientInfo.Start();
                }
            }
            catch { }
        }

        public async Task RemoveTimer_Auto_GetPatientAndService()
        {
            if (timerAutoGetPatientInfo != null)
            {
                timerAutoGetPatientInfo.Stop();
                timerAutoGetPatientInfo = null;
            }
        }

        public async Task InitTimer_Auto_Update_ResultStandard_To_ResultXN()
        {
            try
            {
                if (timerAutoUpdateTime == null)
                {
                    timerAutoUpdateTime = new System.Timers.Timer();
                    timerAutoUpdateTime.Elapsed += TimerAutoUpdateTime_Elapsed;
                    timerAutoUpdateTime.Interval = settingAutoUpdateTime != null ? double.Parse(settingAutoUpdateTime.Value) : 10000;
                    timerAutoUpdateTime.Start();
                }
            }
            catch { }
        }

        public async Task RemoveTimer_Auto_Update_ResultStandard_To_ResultXN()
        {
            if (timerAutoUpdateTime != null)
            {
                timerAutoUpdateTime.Stop();
                timerAutoUpdateTime = null;
            }
        }

        public async Task InitTimer_Auto_PushResultBHYT()
        {
            try
            {
                if (timerAutoPushResultBHYT == null && settingAutoPushResultBHYT != null && !string.IsNullOrEmpty(settingAutoPushResultBHYT.Value))
                {
                    timerAutoPushResultBHYT = new System.Timers.Timer();
                    timerAutoPushResultBHYT.Elapsed += TimerAutoPushResultBHYT_Elapsed;
                    timerAutoPushResultBHYT.Interval = double.Parse(settingAutoPushResultBHYT.Value);
                    timerAutoPushResultBHYT.Start();
                }
            }
            catch { }
        }

        public async Task RemoveTimer_Auto_PushResultBHYT()
        {
            if (timerAutoPushResultBHYT != null)
            {
                timerAutoPushResultBHYT.Stop();
                timerAutoPushResultBHYT = null;
            }
        }

        public async void TimerAutoGetPatientInfo_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            await Auto_GetPatientAndService();
        }

        public async void TimerAutoUpdateTime_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            await Auto_Update_ResultStandard_To_ResultXN();
        }

        public async void TimerAutoPushResultBHYT_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            await Auto_PushResultBHYT();
        }


        // ************************************************************** Get Patient From His
        public async Task Auto_GetPatientAndService()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var lstSIDForTicketItem = new List<SIDForTicketItem_MHIS>();
                    if (settingAPIGetPatientAndService != null && !string.IsNullOrEmpty(settingAPIGetPatientAndService.Value))
                    {
                        var response = client.GetStringAsync(settingAPIGetPatientAndService.Value);
                        var lstPatientInfo = JsonConvert.DeserializeObject<List<PatientInfo_MHIS>>(response.Result);
                        //string test = @"[{'PatientId':'044273','TicketItemId':'7157135','PatientName':'TEST','Sex':'1','Address':'T\u1ed5 4, Ph\u01b0\u1eddng Ngh\u0129a \u0110\u1ee9c, Th\u00e0nh Ph\u1ed1 Gia Ngh\u0129a, \u00d0\u1eafk N\u00f4ng','Age':'2016','DoctorName':'Đoàn Công Danh','Diagnostic':'S\u1ed1t CRNN - TD th\u1ee7y \u0111\u1eadu','ObjectName':'Dịch vụ','LocationName':'Khoa Ngoại tổng hợp','ServiceId':'2000000252','AssignDate':'2023-03-16 15:50:00'},{'PatientId':'044273','TicketItemId':'7157136','PatientName':'TEST','Sex':'1','Address':'T\u1ed5 4, Ph\u01b0\u1eddng Ngh\u0129a \u0110\u1ee9c, Th\u00e0nh Ph\u1ed1 Gia Ngh\u0129a, \u00d0\u1eafk N\u00f4ng','Age':'2016','DoctorName':'Đoàn Công Danh','Diagnostic':'S\u1ed1t CRNN - TD th\u1ee7y \u0111\u1eadu','ObjectName':'Dịch vụ','LocationName':'Khoa Ngoại tổng hợp','ServiceId':'2000000425','AssignDate':'2023-03-16 15:50:00'}]";
                        //var lstPatientInfo = JsonConvert.DeserializeObject<List<PatientInfo_MHIS>>(test);
                        try
                        {
                            if (lstPatientInfo != null && lstPatientInfo.Count() > 0)
                            {
                                var dateTimeServer = DateTime.Now;
                                var groupPatientInfo = lstPatientInfo.GroupBy(p => p.PatientId);
                                if (groupPatientInfo != null)
                                {
                                    foreach (var group in groupPatientInfo)
                                    {
                                        try
                                        {
                                            var lstGroup = group.ToList();
                                            if (lstGroup != null && lstGroup.Count > 0)
                                            {
                                                Patient patient = null;
                                                DateTime? age = null;
                                                bool isYear = false;
                                                var sex = lstGroup[0].Sex.Trim() == "1" ? "Nam" : "Nữ";
                                                try
                                                {
                                                    if (lstGroup[0].Age.Contains("/"))
                                                    {
                                                        age = DateTime.Parse(lstGroup[0].Age);
                                                    }
                                                    else
                                                    {
                                                        isYear = true;
                                                        age = DateTime.Parse("01/01/" + lstGroup[0].Age);
                                                    }
                                                }
                                                catch { }

                                                try
                                                {
                                                    patient = await AddPatient(null, null, lstGroup[0].PatientId, lstGroup[0].PatientName, sex, lstGroup[0].Address, age, isYear, lstGroup[0].Diagnostic, lstGroup[0].ObjectName, lstGroup[0].LocationName, lstGroup[0].DoctorName, dateTimeServer, lstGroup[0].AssignDate);
                                                }
                                                catch
                                                {
                                                    foreach (var item in lstGroup)
                                                    {
                                                        if (item != null)
                                                        {
                                                            lstSIDForTicketItem.Add(new SIDForTicketItem_MHIS { ticketItemId = item.TicketItemId, sid = String.Empty, status = StatusForHIS.waitting });
                                                        }
                                                    }
                                                    continue;
                                                }
                                                if (patient == null) continue;
                                                var keyResultForHis_XN = "xn-" + Guid.NewGuid().ToString();
                                                var keyResultForHis_CDHA = string.Empty;
                                                foreach (var item in lstGroup)
                                                {
                                                    if (item != null)
                                                    {
                                                        try
                                                        {
                                                            var service = await GetService(item.ServiceId);
                                                            if (service != null)
                                                            {
                                                                if (service.Category.Group.Code == "XN")
                                                                {
                                                                    await AddResultXN(patient.Id, item.TicketItemId, dateTimeServer, service, item.DoctorName, keyResultForHis_XN);
                                                                    lstSIDForTicketItem.Add(new SIDForTicketItem_MHIS { ticketItemId = item.TicketItemId, sid = keyResultForHis_XN, status = StatusForHIS.waitting });
                                                                }
                                                                else if (service.Category.Group.Code == "CDHA")
                                                                {
                                                                    keyResultForHis_CDHA = service.Category.Code.ToLower() + "-" + Guid.NewGuid().ToString();
                                                                    await AddResultCDHA(patient.Id, item.TicketItemId, dateTimeServer, service, item.DoctorName, keyResultForHis_CDHA);
                                                                    lstSIDForTicketItem.Add(new SIDForTicketItem_MHIS { ticketItemId = item.TicketItemId, sid = keyResultForHis_CDHA, status = StatusForHIS.waitting });
                                                                }
                                                            }
                                                            else
                                                            {

                                                                lstSIDForTicketItem.Add(new SIDForTicketItem_MHIS { ticketItemId = item.TicketItemId, sid = keyResultForHis_CDHA, status = StatusForHIS.waitting });
                                                            }
                                                        }
                                                        catch
                                                        {
                                                            lstSIDForTicketItem.Add(new SIDForTicketItem_MHIS { ticketItemId = item.TicketItemId, sid = keyResultForHis_CDHA, status = StatusForHIS.waitting });
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        catch { }
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                    if (lstSIDForTicketItem != null && lstSIDForTicketItem.Count > 0)
                    {
                        await UpdateSIDForTicketItemId(lstSIDForTicketItem);
                    }
                }
            }
            catch { }
        }

        public async Task SaveService(string code, string name)
        {
            try
            {
                using (var context = new LABContext())
                {
                    var _obj = new Service();
                    _obj.Code = code;
                    _obj.Name = name;
                    _obj.PrintOrder = 255;
                    _obj.ProcessType = false;
                    _obj.Active = true;
                    await context.Services.AddAsync(_obj);
                    await context.SaveChangesAsync();
                }
            }
            catch{}
        }

        public async Task<Patient> AddPatient(string sid, string seq, string pid, string patientName, string sex,
                                  string address, DateTime? age, bool isYear, string diagnostic, string objectName, string locationName,
                                  string doctorName, DateTime dateTimeServer, string assignDate)
        {
            try
            {
                var obj = lstObject.Where(p => p.Name.Equals(objectName)).FirstOrDefault();
                long? objectId = null;
                if (obj != null)
                {
                    objectId = obj.Id;
                }
                else
                {
                    objectId = await AddObjectFromHIS(objectName);
                }

                var location = lstLocation.Where(p => p.Name.Equals(locationName)).FirstOrDefault();
                long? locationId = null;
                if (location != null)
                {
                    locationId = location.Id;
                }
                else
                {
                    locationId = await AddLocationFromHIS(locationName);
                }

                var doctor = lstDoctor.Where(p => p.Name.Equals(doctorName)).FirstOrDefault();
                long? doctorId = null;
                if (doctor != null)
                {
                    doctorId = doctor.Id;
                }
                else
                {
                    doctorId = await AddDoctorFromHIS(doctorName);
                }

                var assignDateTmp = DateTime.Parse(assignDate);
                Patient patientOld = null;
                try
                {
                    using (var context = new LABContext())
                    {
                        patientOld = await context.Patients.FirstOrDefaultAsync(p => p.PatientId == pid && p.DoctorId == doctorId && p.InsertTime > assignDateTmp.AddMinutes(-15) &&
                                                                                p.InsertTime.Value.Day == assignDateTmp.Day && p.InsertTime.Value.Month == assignDateTmp.Month &&
                                                                                p.InsertTime.Value.Year == assignDateTmp.Year);
                    }
                }
                catch (Exception ex)
                { }

                if (patientOld == null)
                {
                    seq = await GetSeq();
                    try
                    {
                        sid = assignDateTmp.Day.ToString().PadLeft(2, '0') + assignDateTmp.Month.ToString().PadLeft(2, '0') +
                               assignDateTmp.Year.ToString().Substring(2, 2) + "-" + seq;
                    }
                    catch
                    {
                        sid = dateTimeServer.Day.ToString().PadLeft(2, '0') + dateTimeServer.Month.ToString().PadLeft(2, '0') +
                              dateTimeServer.Year.ToString().Substring(2, 2) + "-" + seq;
                    }

                    using (var context = new LABContext())
                    {
                        patientOld = new Patient();
                        patientOld.Sid = sid;
                        patientOld.Seq = seq;
                        try
                        {
                            patientOld.PatientId = pid.Substring(4);
                        }
                        catch
                        {
                            patientOld.PatientId = pid;
                        }
                        patientOld.PatientName = patientName;
                        patientOld.Sex = sex;
                        patientOld.Address = address;
                        patientOld.Age = age;
                        patientOld.IsYear = isYear;
                        patientOld.Diagnostic = diagnostic;
                        patientOld.DoctorId = doctorId;
                        patientOld.ObjectId = objectId;
                        patientOld.LocationId = locationId;
                        patientOld.InsertTime = assignDateTmp;
                        patientOld.Active = true;

                        await context.Patients.AddAsync(patientOld);
                        await context.SaveChangesAsync();
                        return patientOld;
                    }
                }
                else
                {
                    if (patientOld.Diagnostic.Length < diagnostic.Length)
                    {
                        using (var context = new LABContext())
                        {
                            patientOld.Diagnostic = diagnostic;
                            await context.SaveChangesAsync();
                        }
                    }
                    return patientOld;
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> AddResultXN(long? patientId, string ticket_item_id, DateTime dateTimeServer, Service service, string doctorName, string keyResultForHis)
        {
            try
            {
                var doctor = lstDoctor.Where(p => p.Name.Equals(doctorName)).FirstOrDefault();
                long? doctorId = null;
                if (doctor != null) doctorId = doctor.Id;
                using (var context = new LABContext())
                {
                    var lstResultOld = await context.ResultXNs.Where(p => p.PatientId == patientId).ToListAsync();
                    var patient = await context.Patients.Where(p => p.Id == patientId).FirstOrDefaultAsync();
                    ResultXN resultOld = null;
                    if (lstResultOld != null && lstResultOld.Count > 0)
                    {
                        resultOld = lstResultOld.Where(p => p.ServiceId == service.Id).FirstOrDefault();
                    }
                    else
                    {
                        patient.WaitXN = true;
                    }

                    if (resultOld == null)
                    {
                        var lstTestCodeId = await context.ServiceTests.Where(p => p.ServiceId == service.Id).Select(p => p.TestCodeId).ToListAsync();
                        if (lstTestCodeId != null && lstTestCodeId.Count > 0)
                        {
                            var lstTestCode = await context.TestCodes.Where(p => lstTestCodeId.Contains(p.Id)).ToListAsync();
                            if (lstTestCode != null && lstTestCode.Count > 0)
                            {
                                foreach (var item in lstTestCode)
                                {
                                    ResultXN result = new ResultXN();
                                    result.PatientId = patientId;
                                    result.TicketItemId = ticket_item_id;
                                    result.TestCodeId = item.Id;
                                    result.ServiceId = service.Id;
                                    result.ValidPrint = true;
                                    result.Status = 0;
                                    result.StatusResult = StatusForResult.NotResult;
                                    result.Result = item.IsTestHead ? "." : string.Empty;
                                    result.InsertTime = dateTimeServer;
                                    result.DoctorId = doctorId;
                                    result.KeyResultForHis = keyResultForHis;
                                    result.Active = true;
                                    await context.ResultXNs.AddAsync(result);
                                }
                                await context.SaveChangesAsync();
                            }
                        }
                    }
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddResultCDHA(long? patientId, string ticket_item_id, DateTime dateTimeServer, Service service, string doctorName, string keyResultForHis)
        {
            try
            {
                var doctor = lstDoctor.Where(p => p.Name.Equals(doctorName)).FirstOrDefault();
                long? doctorId = null;
                if (doctor != null) doctorId = doctor.Id;
                using (var context = new LABContext())
                {

                    var lstResultOld = await context.ResultCDHAs.Where(p => p.PatientId == patientId && p.Service.Category.Code == service.Category.Code).ToListAsync();
                    var patient = await context.Patients.Where(p => p.Id == patientId).FirstOrDefaultAsync();
                    ResultCDHA resultOld = null;
                    if (lstResultOld != null && lstResultOld.Count > 0)
                    {
                        resultOld = lstResultOld.Where(p => p.ServiceId == service.Id).FirstOrDefault();
                    }
                    else
                    {
                        if (service.Category.Code == "SA")
                            patient.WaitSA = true;
                        else if (service.Category.Code == "SAT")
                            patient.WaitSAT = true;
                        else if (service.Category.Code == "DDT")
                            patient.WaitDDT = true;
                        else if (service.Category.Code == "NS")
                            patient.WaitNS = true;
                        else if (service.Category.Code == "XQ")
                            patient.WaitXQ = true;
                    }

                    if (resultOld == null)
                    {
                        ResultCDHA result = new ResultCDHA();
                        result.PatientId = patientId;
                        result.ServiceId = service.Id;
                        result.TicketItemId = ticket_item_id;
                        result.InsertTime = dateTimeServer;
                        result.DoctorId = doctorId;
                        result.KeyResultForHis = keyResultForHis;
                        result.Active = true;
                        await context.ResultCDHAs.AddAsync(result);
                        await context.SaveChangesAsync();
                    }
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<long?> AddObjectFromHIS(string name)
        {
            try
            {
                using (var context = new LABContext())
                {
                    var _object = await context.Objects.Where(p => p.Name.Equals(name)).FirstOrDefaultAsync();
                    if (_object == null)
                    {
                        _object = new Object() { Code = name, Name = name, Active = true };
                        await context.Objects.AddAsync(_object);
                        await context.SaveChangesAsync();
                        lstObject = await context.Objects.Where(p => p.Active == true).ToListAsync();
                    }
                    return _object.Id;
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<long?> AddLocationFromHIS(string name)
        {
            try
            {
                using (var context = new LABContext())
                {
                    var _location = await context.Locations.Where(p => p.Name.Equals(name)).FirstOrDefaultAsync();
                    if (_location == null)
                    {
                        _location = new Location() { Code = name, Name = name, Active = true };
                        await context.Locations.AddAsync(_location);
                        await context.SaveChangesAsync();
                        lstLocation = await context.Locations.Where(p => p.Active == true).ToListAsync();
                    }
                    return _location.Id;
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<long?> AddDoctorFromHIS(string name)
        {
            try
            {
                using (var context = new LABContext())
                {
                    var _doctor = await context.Doctors.Where(p => p.Name.Equals(name)).FirstOrDefaultAsync();
                    if (_doctor == null)
                    {
                        _doctor = new Doctor() { Code = name, Name = name, Active = true };
                        await context.Doctors.AddAsync(_doctor);
                        await context.SaveChangesAsync();
                        lstDoctor = await context.Doctors.Where(p => p.Active == true).ToListAsync();
                    }
                    return _doctor.Id;
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<Service> GetService(string code)
        {
            return await _db.Services.Where(p => p.Code == code).FirstOrDefaultAsync();
        }

        public async Task<string> GetSeq()
        {
            using (var context = new LABContext())
            {
                var _setting = await context.Settings.Where(p => p.Code == SettingBL.Sequence).FirstOrDefaultAsync();
                if (_setting == null)
                {
                    _setting = new Setting() { Code = SettingBL.Sequence, Name = SettingBL.Sequence, Value = "1" };
                    await context.Settings.AddAsync(_setting);
                }
                else if (int.Parse(_setting.Value) == 9999)
                {
                    _setting.Value = "1";
                }
                else
                {
                    _setting.Value = (int.Parse(_setting.Value) + 1).ToString();
                }
                await context.SaveChangesAsync();
                return _setting.Value;
            }
        }

        public async Task UpdateSIDForTicketItemId(List<SIDForTicketItem_MHIS> value)
        {
            try
            {
                if (settingAPIUpdateSid != null && !string.IsNullOrEmpty(settingAPIUpdateSid.Value))
                {
                    HttpResponseMessage response = new HttpResponseMessage();
                    using (var client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(10);
                        client.BaseAddress = new Uri(settingAPIUpdateSid.Value);
                        var myContent = JsonConvert.SerializeObject(value);
                        var buffer = Encoding.UTF8.GetBytes(myContent);
                        var byteContent = new ByteArrayContent(buffer);
                        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        response = client.PostAsync("", byteContent).Result;
                    }
                }
            }
            catch { }
        }


        // *************************************************************** Update Result From ResultStandard to ResultXN
        public async Task Auto_Update_ResultStandard_To_ResultXN()
        {
            try
            {
                var preSearchTime = 2;
                if (settingPreSearchTime != null)
                {
                    preSearchTime = int.Parse(settingPreSearchTime.Value);
                }
                var dayCreateSID = 1;
                if (settingDayCreateSID != null)
                {
                    dayCreateSID = int.Parse(settingDayCreateSID.Value);
                }

                var dateTimeServer = DateTime.Now;
                var minDateTimeServer = dateTimeServer.AddHours(-preSearchTime);
                var maxDateTimeServer = dateTimeServer;
                var lstResultStandard = await GetList_ResultStandard(minDateTimeServer, maxDateTimeServer);
                var lstSid = new List<string>();

                if (lstResultStandard != null)
                {
                    foreach (var resultStandard in lstResultStandard)
                    {
                        var insertTime = resultStandard.InsertTime;
                        var preInserTime = insertTime.AddDays(-dayCreateSID);
                        var sid = insertTime.Day.ToString().PadLeft(2, '0') + insertTime.Month.ToString().PadLeft(2, '0') +
                                  insertTime.Year.ToString().Substring(2, 2) + "-" + resultStandard.Seq;
                        var preSid = preInserTime.Day.ToString().PadLeft(2, '0') + preInserTime.Month.ToString().PadLeft(2, '0') +
                                 preInserTime.Year.ToString().Substring(2, 2) + "-" + resultStandard.Seq;

                        await UpdateResultXN(sid, preSid, resultStandard.TestCodeId, resultStandard.Result, resultStandard.PosNeg, resultStandard);
                        if (!lstSid.Contains(sid)) lstSid.Add(sid);
                    }

                    foreach (var sid in lstSid)
                    {
                        await FullResultXN(sid);
                    }
                }
            }
            catch { }
        }

        public async Task<List<ResultStandard>> GetList_ResultStandard(DateTime minDateTimeServer, DateTime maxDateTimeServer)
        {
            _db = new LABContext();
            return await _db.ResultStandards.Where(p => p.Status == StatusForResultStandard.NotUpdate && p.InsertTime > minDateTimeServer && p.InsertTime <= maxDateTimeServer).ToListAsync();
        }

        public async Task UpdateResultXN(string sid, string preSid, long? testCodeId, double? result, string posneg, ResultStandard resultStandard)
        {
            if (result != null || !string.IsNullOrEmpty(posneg.Trim()))
            {
                try
                {
                    using (var context = new LABContext())
                    {
                        var objResult = await context.ResultXNs.Where(p => p.Patient.Sid == sid && p.TestCodeId == testCodeId && p.Patient.WaitXN == false && p.Patient.ProcessXN == true && p.Patient.ValidXN == false).FirstOrDefaultAsync();
                        if (objResult == null) objResult = await context.ResultXNs.Where(p => p.Patient.Sid == preSid && p.TestCodeId == testCodeId && p.Patient.WaitXN == false && p.Patient.ProcessXN == true && p.Patient.ValidXN == false).FirstOrDefaultAsync();
                        if (objResult == null && resultStandard.Device.Code == "Elisa")
                        {
                            for (int i = 1; i <= 9; i++)
                            {
                                var sidTmp = sid.Split('-')[0] + "-" + i + sid.Split('-')[1];
                                objResult = await context.ResultXNs.Where(p => p.Patient.Sid == sidTmp && p.TestCodeId == testCodeId && p.Patient.WaitXN == false && p.Patient.ProcessXN == true && p.Patient.ValidXN == false).FirstOrDefaultAsync();
                                if (objResult == null)
                                {
                                    var preSidTmp = preSid.Split('-')[0] + "-" + i + preSid.Split('-')[1];
                                    objResult = await context.ResultXNs.Where(p => p.Patient.Sid == preSidTmp && p.TestCodeId == testCodeId && p.Patient.WaitXN == false && p.Patient.ProcessXN == true && p.Patient.ValidXN == false).FirstOrDefaultAsync();
                                }
                                if (objResult != null) break;
                            }
                        }
                        if (objResult == null)
                        {
                            await UpdateStatus_ResultStandard(resultStandard, StatusForResultStandard.Erorr);
                            return;
                        }
                        var objTestCode = await context.TestCodes.Where(p => p.Id == testCodeId).FirstOrDefaultAsync();
                        if (result != null && !string.IsNullOrEmpty(posneg))
                        {
                            if (objTestCode != null && objTestCode.NormalResult != null)
                            {
                                if (posneg == objTestCode.NormalResult.Trim())
                                {
                                    objResult.Result = Format_Decimal(result);
                                    objResult.PosNeg = posneg;
                                    objResult.Status = 0;
                                }
                                else
                                {
                                    objResult.Result = Format_Decimal(result);
                                    objResult.PosNeg = posneg;
                                    objResult.Status = 2;
                                }
                            }
                            else
                            {
                                objResult.Result = Format_Decimal(result);
                                objResult.PosNeg = posneg;
                                objResult.Status = 0;
                            }
                        }
                        else if (result == null && !string.IsNullOrEmpty(posneg))
                        {
                            if (objTestCode != null && objTestCode.NormalResult != null)
                            {
                                if (posneg == objTestCode.NormalResult.Trim())
                                {
                                    objResult.PosNeg = posneg;
                                    objResult.Status = 0;
                                }
                                else
                                {
                                    objResult.PosNeg = posneg;
                                    objResult.Status = 2;
                                }
                            }
                            else
                            {
                                objResult.PosNeg = posneg;
                                objResult.Status = 0;
                            }
                        }
                        else if (result != null && string.IsNullOrEmpty(posneg))
                        {
                            if (objTestCode != null)
                            {
                                if (objTestCode.LowerLimit == null && objTestCode.HigherLimit != null)
                                {
                                    if (result > objTestCode.HigherLimit)
                                    {
                                        objResult.Status = 2;
                                    }
                                    else
                                    {
                                        objResult.Status = 0;
                                    }
                                }
                                else if (objTestCode.LowerLimit != null && objTestCode.HigherLimit == null)
                                {
                                    if (result < objTestCode.LowerLimit)
                                    {
                                        objResult.Status = 1;
                                    }
                                    else
                                    {
                                        objResult.Status = 0;
                                    }
                                }
                                else if (objTestCode.LowerLimit != null && objTestCode.HigherLimit != null)
                                {
                                    if (result < objTestCode.LowerLimit)
                                    {
                                        objResult.Status = 1;
                                    }
                                    else if (result > objTestCode.HigherLimit)
                                    {
                                        objResult.Status = 2;
                                    }
                                    else
                                    {
                                        objResult.Status = 0;
                                    }
                                }

                                objResult.Result = Format_Decimal(result);
                            }
                            else
                            {
                                objResult.Result = Format_Decimal(result);
                            }
                        }

                        if (resultStandard != null && resultStandard.Device != null)
                            objResult.DeviceCodeBHYT = resultStandard.Device.CodeBHYT;
                        await context.SaveChangesAsync();
                        await UpdateStatus_ResultStandard(resultStandard, StatusForResultStandard.Updated);
                        await UpdateStatus_Result(sid, testCodeId, StatusForResult.HaveResult);
                    }
                }
                catch (Exception ex)
                {
                    await UpdateStatus_ResultStandard(resultStandard, StatusForResultStandard.Erorr);
                }
            }
        }

        public async Task UpdateStatus_Result(string sid, long? testCodeId, int status)
        {
            using (var context = new LABContext())
            {
                var result = await context.ResultXNs.Where(p => p.Patient.Sid == sid && p.TestCodeId == testCodeId).FirstOrDefaultAsync();
                if (result != null)
                {
                    result.StatusResult = status;
                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task UpdateStatus_ResultStandard(ResultStandard resultStandard, string status)
        {
            using (var context = new LABContext())
            {
                var objResultStandard = await context.ResultStandards.Where(p => p.Id == resultStandard.Id).FirstOrDefaultAsync();
                if (objResultStandard != null)
                {
                    objResultStandard.Status = status;
                    await context.SaveChangesAsync();
                }
            }
        }

        public string Format_Decimal(double? result)
        {
            try
            {
                var stringResult = result?.ToString("0.0##", GetCultureInfo());
                return stringResult;
            }
            catch (Exception ex)
            {
                return result.ToString();
            }
        }

        public CultureInfo GetCultureInfo()
        {
            CultureInfo cultureInfo = new CultureInfo("en-US");
            cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
            return cultureInfo;
        }

        public async Task FullResultXN(string sid)
        {
            try
            {
                using (var context = new LABContext())
                {
                    var _item = await context.ResultXNs.Where(p => p.Active == true && p.Patient.Sid == sid && string.IsNullOrEmpty(p.Result.Trim()) && string.IsNullOrEmpty(p.PosNeg.Trim())).FirstOrDefaultAsync();
                    if (_item == null)
                    {
                        var _patient = await context.Patients.Where(p => p.Sid == sid).FirstOrDefaultAsync();
                        if (_patient != null)
                        {
                            _patient.FullResultXN = true;
                            await context.SaveChangesAsync();
                        }
                    }
                }
            }
            catch { }
        }

        // *************************************************************** Push Result BHYT
        public async Task Auto_PushResultBHYT()
        {
            var serverTime = DateTime.Now;
            var fromTime = new DateTime(serverTime.Year, serverTime.Month, serverTime.Day, 23, 59, 59)
                               .AddDays(-1);
            var toTime = new DateTime(serverTime.Year, serverTime.Month, serverTime.Day, 23, 59, 59);
            await Get_ResultXN_PushBHYT(fromTime, toTime);
            await Get_ResultCDHA_PushBHYT(fromTime, toTime);
        }

        public async Task Get_ResultXN_PushBHYT(DateTime from, DateTime to)
        {
            try
            {
                using (var context = new LABContext())
                {
                    var lstResult = await context.ResultXNs.Where(p => p.Active == true && p.PushBHYT == false && p.Patient.ValidXN == true && p.Patient.InsertTime > from && p.Patient.InsertTime < to).ToListAsync();
                    var lstResult_BHYT = new List<Result_BHYT_MHIS>();
                    var lstSIDForTicketItem = new List<SIDForTicketItem_MHIS>();

                    if (lstResult != null)
                    {
                        foreach (var item in lstResult)
                        {
                            if (item.TestCode.IsTestHead || string.IsNullOrEmpty(item.TestCode.CodeBHYT) || string.IsNullOrEmpty(item.TestCode.NameBHYT)) continue;
                            var result_BHYT = lstResult_BHYT.Where(p => p.ticket_item_id == item.TicketItemId).FirstOrDefault();
                            if (result_BHYT == null)
                            {
                                var lstResult_XN = new List<Result_XN_MHIS> { new Result_XN_MHIS { param_code = item.TestCode.CodeBHYT, param_name = item.TestCode.NameBHYT, param_value = !string.IsNullOrEmpty(item.Result) ? item.Result : item.PosNeg } };
                                var result_BHYT_New = new Result_BHYT_MHIS { ticket_item_id = item.TicketItemId, mechine_code = item.DeviceCodeBHYT, result = lstResult_XN, date = item.Patient.ReturnResultTimeXN ?? DateTime.Now };
                                lstResult_BHYT.Add(result_BHYT_New);
                            }
                            else
                            {
                                var result_XN = new Result_XN_MHIS { param_code = item.TestCode.CodeBHYT, param_name = item.TestCode.NameBHYT, param_value = !string.IsNullOrEmpty(item.Result) ? item.Result : item.PosNeg };
                                result_BHYT.result.Add(result_XN);
                            }

                            // Set status cho Ticket_Item_Id
                            var value = lstSIDForTicketItem.Where(p => p.ticketItemId == item.TicketItemId).FirstOrDefault();
                            if (value == null)
                            {
                                lstSIDForTicketItem.Add(new SIDForTicketItem_MHIS { ticketItemId = item.TicketItemId, sid = item.KeyResultForHis, status = StatusForHIS.closed });
                            }
                        }

                        if (lstResult_BHYT != null && lstResult_BHYT.Count > 0)
                        {
                            await Send_Result_BHYT(lstResult_BHYT);
                            foreach (var item in lstResult)
                            {
                                item.PushBHYT = true;
                            }
                            await context.SaveChangesAsync();
                        }

                        if (lstSIDForTicketItem != null && lstSIDForTicketItem.Count > 0)
                        {
                            await UpdateSIDForTicketItemId(lstSIDForTicketItem);
                        }
                    }
                }
            }
            catch { }
        }

        public async Task Get_ResultCDHA_PushBHYT(DateTime from, DateTime to)
        {
            try
            {
                using (var context = new LABContext())
                {
                    var lstResultImage = await context.ResultCDHAs.Where(p => p.Active == true && p.PushBHYT == false && p.Patient.InsertTime > from && p.Patient.InsertTime < to &&
                                                             ((p.Patient.ValidSA == true && p.Service.Category.Code == "SA") ||
                                                             (p.Patient.ValidXQ == true && p.Service.Category.Code == "XQ") ||
                                                             (p.Patient.ValidNS == true && p.Service.Category.Code == "NS"))).ToListAsync();
                    var lstResult_BHYT = new List<Result_BHYT_MHIS>();
                    var lstSIDForTicketItem = new List<SIDForTicketItem_MHIS>();
                    if (lstResultImage != null)
                    {
                        foreach (var item in lstResultImage)
                        {
                            var date = item.Service.Category.Code == "SA" ? item.Patient.ReturnResultTimeSA : (item.Service.Category.Code == "XQ" ? item.Patient.ReturnResultTimeXQ : item.Patient.ReturnResultTimeNS);
                            lstResult_BHYT.Add(new Result_BHYT_MHIS { ticket_item_id = item.TicketItemId, mechine_code = item.DeviceCodeBHYT, describe = item.Description, conclusion = item.Result, date = date ?? DateTime.Now });
                            lstSIDForTicketItem.Add(new SIDForTicketItem_MHIS { ticketItemId = item.TicketItemId, sid = item.KeyResultForHis, status = StatusForHIS.closed });
                        }
                    }

                    if (lstResult_BHYT != null && lstResult_BHYT.Count > 0)
                    {
                        await Send_Result_BHYT(lstResult_BHYT);
                        foreach (var item in lstResultImage)
                        {
                            item.PushBHYT = true;
                        }
                        await context.SaveChangesAsync();
                    }

                    if (lstSIDForTicketItem != null && lstSIDForTicketItem.Count > 0)
                    {
                        await UpdateSIDForTicketItemId(lstSIDForTicketItem);
                    }
                }
            }
            catch { }
        }

        public async Task Send_Result_BHYT(List<Result_BHYT_MHIS> lstValue)
        {
            try
            {
                if (settingAPISendResult != null && !string.IsNullOrEmpty(settingAPISendResult.Value))
                {
                    HttpResponseMessage response = new HttpResponseMessage();
                    using (var client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(10);
                        client.BaseAddress = new Uri(settingAPISendResult.Value);
                        var myContent = JsonConvert.SerializeObject(lstValue);
                        var buffer = Encoding.UTF8.GetBytes(myContent);
                        var byteContent = new ByteArrayContent(buffer);
                        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        response = client.PostAsync("", byteContent).Result;
                    }
                }
            }
            catch (Exception ex)
            { }
        }
    }

    public class PatientInfo_MHIS
    {
        public string PatientId { get; set; }
        public string TicketItemId { get; set; }
        public string PatientName { get; set; }
        public string Sex { get; set; }
        public string Address { get; set; }
        public string Age { get; set; }
        public string Diagnostic { get; set; }
        public string DoctorName { get; set; }
        public string ObjectName { get; set; }
        public string LocationName { get; set; }
        public string ServiceId { get; set; }
        public string AssignDate { get; set; }
    }

    public class SIDForTicketItem_MHIS
    {
        public string ticketItemId { get; set; }
        public string sid { get; set; }
        public string status { get; set; }
    }

    public class Result_BHYT_MHIS
    {
        public string ticket_item_id { get; set; }
        public string mechine_code { get; set; }
        public string describe { get; set; }
        public string conclusion { get; set; }
        public List<Result_XN_MHIS> result { get; set; }
        public DateTime date { get; set; }
    }

    public class Result_XN_MHIS
    {
        public string param_code { get; set; }
        public string param_name { get; set; }
        public string param_value { get; set; }
    }
}
