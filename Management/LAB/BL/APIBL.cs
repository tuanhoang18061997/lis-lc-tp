using Management.Controllers;
using Management.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Utilities.Encoders;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using Object = Management.Models.Object;

namespace Management.BL
{
    public class APIBL
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
        public static Setting settingAPICancelService = null;

        public static List<Doctor> lstDoctor = null;
        public static List<Object> lstObject = null;
        public static List<Location> lstLocation = null;

        public static System.Timers.Timer timerAutoUpdateTime;
        public static System.Timers.Timer timerAutoGetPatientInfo;
        public static System.Timers.Timer timerAutoPushResultBHYT;

        public APIBL()
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
                settingAPICancelService = null;

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

            // Thêm vào phương thức Init_Properties()
            //if (settingAPICancelService == null)
            //{
            //    settingAPICancelService = await _db.Settings.Where(p => p.Code == SettingBL.APIUpdateStatus).FirstOrDefaultAsync();
            //    if (settingAPICancelService == null)
            //    {
            //        try
            //        {
            //            settingAPICancelService = new Setting { Code = SettingBL.APIUpdateStatus, Name = SettingBL.APIUpdateStatus, Value = String.Empty };
            //            await _db.Settings.AddAsync(settingAPICancelService);
            //            await _db.SaveChangesAsync();
            //        }
            //        catch { }
            //    }
            //}

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
                if (timerAutoPushResultBHYT == null)
                {
                    timerAutoPushResultBHYT = new System.Timers.Timer();
                    timerAutoPushResultBHYT.Elapsed += TimerAutoPushResultBHYT_Elapsed;
                    timerAutoPushResultBHYT.Interval = settingAutoPushResultBHYT != null ? double.Parse(settingAutoPushResultBHYT.Value) : 1800000;
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
                    var listUpdateStatusTicketItemId = new List<StatusTicketItemId>();
                    var lstSIDForTicketItem = new List<SIDForTicketItem>();
                    if (settingAPIGetPatientAndService != null && !string.IsNullOrEmpty(settingAPIGetPatientAndService.Value))
                    {
                        var response = client.GetStringAsync(settingAPIGetPatientAndService.Value);
                        var lstPatientInfo = JsonConvert.DeserializeObject<List<PatientInfo_Add>>(response.Result);

                        //if (lstObject == null)
                        //{
                        //    lstObject = await _db.Objects.Where(p => p.Active == true).ToListAsync();
                        //}
                        //if (lstDoctor == null)
                        //{
                        //    lstDoctor = await _db.Doctors.Where(p => p.Active == true).ToListAsync();
                        //}
                        //if (lstLocation == null)
                        //{
                        //    lstLocation = await _db.Locations.Where(p => p.Active == true).ToListAsync();
                        //}

                        //string test = @"[
                        //                            {
                        //                              ""PatientId"": ""0002226"",
                        //                              ""TicketId"": ""25627636"",
                        //                              ""Seq"": ""002027"",
                        //                              ""MaBenhAn"": ""BA260216"",
                        //                              ""TicketItemId"": ""25630015"",
                        //                              ""PatientName"": ""Jessica"",
                        //                              ""Sex"": ""female"",
                        //                              ""Address"": ""777 Lý Thường Kiệt"",
                        //                              ""Age"": ""1997-02-05"",
                        //                              ""DoctorName"": ""BS CKI Trần Thành Đông"",
                        //                              ""Diagnostic"": """",
                        //                              ""ObjectName"": ""DV"",
                        //                              ""LocationName"": ""Phòng Tổ chức Hành chính Quản trị"",
                        //                              ""ServiceId"": ""TDCN_DLX"",
                        //                              ""AssignDate"": ""2026-06-15 19:26:03"",
                        //                              ""Type"": ""out"",
                        //                              ""MaDotKham"": ""HD SAISON"",
                        //                              ""Phone"": ""0789657456""
                        //                            },
                        //                            {
                        //                              ""PatientId"": ""0002225"",
                        //                              ""TicketId"": ""25627635"",
                        //                              ""Seq"": ""002026"",
                        //                              ""MaBenhAn"": ""BA260215"",
                        //                              ""TicketItemId"": ""25630015"",
                        //                              ""PatientName"": ""Lâm Thu Thảo"",
                        //                              ""Sex"": ""female"",
                        //                              ""Address"": ""63/6 Trần Hữu Trang, Phường 11, Quận Phú Nhuận, Tp. Hồ Chí Minh"",
                        //                              ""Age"": ""2000-02-05"",
                        //                              ""DoctorName"": ""BS CKI Trần Thành Đông"",
                        //                              ""Diagnostic"": """",
                        //                              ""ObjectName"": ""DV"",
                        //                              ""LocationName"": ""Phòng Tổ chức Hành chính Quản trị"",
                        //                              ""ServiceId"": ""XN331"",
                        //                              ""AssignDate"": ""2026-04-07 16:26:03"",
                        //                              ""Type"": ""out"",
                        //                              ""MaDotKham"": ""HD SAISON"",
                        //                              ""Phone"": ""0789657456""
                        //                            },
                        //                            {
                        //                              ""PatientId"": ""0002225"",
                        //                              ""TicketId"": ""25627635"",
                        //                              ""Seq"": ""002026"",
                        //                              ""MaBenhAn"": ""BA260215"",
                        //                              ""TicketItemId"": ""25630016"",
                        //                              ""PatientName"": ""Lâm Thu Thảo"",
                        //                              ""Sex"": ""female"",
                        //                              ""Address"": ""63/6 Trần Hữu Trang, Phường 11, Quận Phú Nhuận, Tp. Hồ Chí Minh"",
                        //                              ""Age"": ""2000-02-05"",
                        //                              ""DoctorName"": ""BS CKI Trần Thành Đông"",
                        //                              ""Diagnostic"": """",
                        //                              ""ObjectName"": ""DV"",
                        //                              ""LocationName"": ""Phòng Tổ chức Hành chính Quản trị"",
                        //                              ""ServiceId"": ""XN057"",
                        //                              ""AssignDate"": ""2026-04-07 16:26:03"",
                        //                              ""Type"": ""out"",
                        //                              ""MaDotKham"": ""HD SAISON"",
                        //                              ""Phone"": ""0789657456""
                        //                            },
                        //                            {
                        //                              ""PatientId"": ""0002225"",
                        //                              ""TicketId"": ""25627635"",
                        //                              ""Seq"": ""002026"",
                        //                              ""MaBenhAn"": ""BA260215"",
                        //                              ""TicketItemId"": ""25630017"",
                        //                              ""PatientName"": ""Lâm Thu Thảo"",
                        //                              ""Sex"": ""female"",
                        //                              ""Address"": ""63/6 Trần Hữu Trang, Phường 11, Quận Phú Nhuận, Tp. Hồ Chí Minh"",
                        //                              ""Age"": ""2000-02-05"",
                        //                              ""DoctorName"": ""BS CKI Trần Thành Đông"",
                        //                              ""Diagnostic"": """",
                        //                              ""ObjectName"": ""DV"",
                        //                              ""LocationName"": ""Phòng Tổ chức Hành chính Quản trị"",
                        //                              ""ServiceId"": ""XN058"",
                        //                              ""AssignDate"": ""2026-04-07 16:26:03"",
                        //                              ""Type"": ""out"",
                        //                              ""MaDotKham"": ""HD SAISON"",
                        //                              ""Phone"": ""0789657456""
                        //                            },
                        //                            {
                        //                              ""PatientId"": ""0002225"",
                        //                              ""TicketId"": ""25627635"",
                        //                              ""Seq"": ""002026"",
                        //                              ""MaBenhAn"": ""BA260215"",
                        //                              ""TicketItemId"": ""25630018"",
                        //                              ""PatientName"": ""Lâm Thu Thảo"",
                        //                              ""Sex"": ""female"",
                        //                              ""Address"": ""63/6 Trần Hữu Trang, Phường 11, Quận Phú Nhuận, Tp. Hồ Chí Minh"",
                        //                              ""Age"": ""2000-02-05"",
                        //                              ""DoctorName"": ""BS CKI Trần Thành Đông"",
                        //                              ""Diagnostic"": """",
                        //                              ""ObjectName"": ""DV"",
                        //                              ""LocationName"": ""Phòng Tổ chức Hành chính Quản trị"",
                        //                              ""ServiceId"": ""XN059"",
                        //                              ""AssignDate"": ""2026-04-07 16:26:03"",
                        //                              ""Type"": ""out"",
                        //                              ""MaDotKham"": ""HD SAISON"",
                        //                              ""Phone"": ""0789657456""
                        //                            },
                        //{
                        //                              ""PatientId"": ""0002225"",
                        //                              ""TicketId"": ""25627635"",
                        //                              ""Seq"": ""002026"",
                        //                              ""MaBenhAn"": ""BA260215"",
                        //                              ""TicketItemId"": ""25630019"",
                        //                              ""PatientName"": ""Lâm Thu Thảo"",
                        //                              ""Sex"": ""female"",
                        //                              ""Address"": ""63/6 Trần Hữu Trang, Phường 11, Quận Phú Nhuận, Tp. Hồ Chí Minh"",
                        //                              ""Age"": ""2000-02-05"",
                        //                              ""DoctorName"": ""BS CKI Trần Thành Đông"",
                        //                              ""Diagnostic"": """",
                        //                              ""ObjectName"": ""DV"",
                        //                              ""LocationName"": ""Phòng Tổ chức Hành chính Quản trị"",
                        //                              ""ServiceId"": ""SA33"",
                        //                              ""AssignDate"": ""2026-04-07 16:26:03"",
                        //                              ""Type"": ""out"",
                        //                              ""MaDotKham"": ""HD SAISON"",
                        //                              ""Phone"": ""0789657456""
                        //                            }
                        //                        ]";
                        //var lstPatientInfo = JsonConvert.DeserializeObject<List<PatientInfo_Add>>(test);

                        try
                        {
                            if (lstPatientInfo != null && lstPatientInfo.Count() > 0)
                            {
                                var dateTimeServer = DateTime.Now;
                                var groupPatientInfo = lstPatientInfo.GroupBy(p => new
                                {
                                    p.PatientId,
                                    Type = (p.Type ?? "").Trim().ToLower()
                                });
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
                                                var sex = string.Empty;
                                                var objectName = "Thu phí";
                                                try
                                                {
                                                    age = DateTime.Parse(lstGroup[0].Age);
                                                }
                                                catch { }

                                                if (lstGroup[0].Sex.Trim() == "male")
                                                {
                                                    sex = "Nam";
                                                }
                                                else
                                                {
                                                    sex = "Nữ";
                                                }

                                                if (!string.IsNullOrEmpty(lstGroup[0].ObjectName.Trim()))
                                                {
                                                    objectName = lstGroup[0].ObjectName.Trim();
                                                }

                                                try
                                                {
                                                    patient = await AddPatient(null, lstGroup[0].Seq, lstGroup[0].PatientId, lstGroup[0].TicketId, lstGroup[0].PatientName,
                                                        sex, lstGroup[0].Address, age, lstGroup[0].Diagnostic, objectName, lstGroup[0].LocationName,
                                                        lstGroup[0].DoctorName, dateTimeServer, lstGroup[0].AssignDate, lstGroup[0].Type, lstGroup[0].MaBenhAn,
                                                        lstGroup[0].MaDotKham, lstGroup[0].Phone, lstGroup[0].SoCccd);
                                                }
                                                catch (Exception ex)
                                                {
                                                    foreach (var item in lstGroup)
                                                    {
                                                        if (item != null)
                                                        {
                                                            listUpdateStatusTicketItemId.Add(new StatusTicketItemId { ticket_item_id = item.TicketItemId, type = item.Type, status = StatusForHIS.waitting, result = string.Empty });
                                                        }
                                                    }
                                                    continue;
                                                }
                                                if (patient == null) continue;
                                                // 🔹 Tìm keyResultForHis_XN cũ của bệnh nhân, nếu có thì dùng luôn
                                                var keyResultForHis_XN = await _db.ResultXNs
                                                    .Where(p => p.PatientId == patient.Id && !string.IsNullOrEmpty(p.KeyResultForHis))
                                                    .Select(p => p.KeyResultForHis)
                                                    .FirstOrDefaultAsync();

                                                // 🔹 Nếu chưa có key nào thì tạo mới
                                                if (string.IsNullOrEmpty(keyResultForHis_XN))
                                                {
                                                    keyResultForHis_XN = "xn-" + Guid.NewGuid().ToString();
                                                }
                                                var keyResultForHis_CDHA = string.Empty;
                                                foreach (var item in lstGroup)
                                                {
                                                    if (item != null)
                                                    {
                                                        try
                                                        {
                                                            var service = await GetService(item.ServiceId); // ServiceId là mã Code dịch vụ từ HIS
                                                            if (service != null)
                                                            {
                                                                // Biến dùng chung để đẩy về HIS
                                                                string sidToHis = null;
                                                                string ketLuan = null;
                                                                var statusToHis = StatusForHIS.waitting;

                                                                if (service.Category.Group.Code == "XN")
                                                                {
                                                                    // 1) Tạo/giữ SID cho XN
                                                                    sidToHis = keyResultForHis_XN;
                                                                    await AddResultXN(patient.Id, item.TicketItemId, dateTimeServer, service, item.DoctorName, sidToHis, item.Type);

                                                                    // 2) Đẩy SID về HIS (để HIS & LIS cùng trỏ 1 SID)
                                                                    lstSIDForTicketItem.Add(new SIDForTicketItem { ticketItemId = item.TicketItemId, sid = sidToHis, type = item.Type });

                                                                    statusToHis = StatusForHIS.waitting;

                                                                    // 4) Push trạng thái về HIS kèm SID
                                                                    listUpdateStatusTicketItemId.Add(new StatusTicketItemId
                                                                    {
                                                                        ticket_item_id = item.TicketItemId,
                                                                        type = item.Type,
                                                                        status = statusToHis,
                                                                        result = ketLuan ?? string.Empty,
                                                                        sid = sidToHis
                                                                    });
                                                                }
                                                                else if (service.Category.Group.Code == "CDHA")
                                                                {
                                                                    // 1) Tạo SID riêng cho CDHA (vd: 'sa-<guid>' hay theo Category.Code)
                                                                    sidToHis = service.Category.Code.ToLower() + "-" + Guid.NewGuid().ToString();
                                                                    keyResultForHis_CDHA = sidToHis;
                                                                    await AddResultCDHA(patient.Id, item.TicketItemId, dateTimeServer, service, item.DoctorName, sidToHis, item.Type);

                                                                    // 2) Đẩy SID về HIS
                                                                    lstSIDForTicketItem.Add(new SIDForTicketItem { ticketItemId = item.TicketItemId, sid = keyResultForHis_CDHA, type = item.Type });

                                                                    // 3) Lấy kết luận CDHA
                                                                    ketLuan = await _db.ResultCDHAs
                                                                        .Where(p => p.TicketItemId == item.TicketItemId)
                                                                        .Select(p => p.Result)
                                                                        .FirstOrDefaultAsync();

                                                                    statusToHis = string.IsNullOrEmpty(ketLuan) ? StatusForHIS.waitting : StatusForHIS.closed;

                                                                    // 4) Push trạng thái về HIS kèm SID
                                                                    listUpdateStatusTicketItemId.Add(new StatusTicketItemId
                                                                    {
                                                                        ticket_item_id = item.TicketItemId,
                                                                        type = item.Type,
                                                                        status = statusToHis,
                                                                        result = ketLuan ?? string.Empty,
                                                                        sid = sidToHis
                                                                    });
                                                                }
                                                            }
                                                            //listUpdateStatusTicketItemId.Add(new StatusTicketItemId { ticket_item_id = item.TicketItemId, type = item.Type, status = StatusForHIS.waitting });

                                                            // Lấy kết luận cận lâm sàng
                                                            //var ket_luan_cls = await _db.ResultCDHAs
                                                            //    .Where(p => p.TicketItemId == item.TicketItemId)
                                                            //    .Select(p => p.Result)
                                                            //    .FirstOrDefaultAsync();
                                                            //if (!string.IsNullOrEmpty(ket_luan_cls))
                                                            //{
                                                            //    listUpdateStatusTicketItemId.Add(new StatusTicketItemId
                                                            //    {
                                                            //        ticket_item_id = item.TicketItemId,
                                                            //        type = item.Type,
                                                            //        status = StatusForHIS.closed,
                                                            //        result = ket_luan_cls ?? string.Empty
                                                            //    });
                                                            //}
                                                            //else
                                                            //{
                                                            //    listUpdateStatusTicketItemId.Add(new StatusTicketItemId
                                                            //    {
                                                            //        ticket_item_id = item.TicketItemId,
                                                            //        type = item.Type,
                                                            //        status = StatusForHIS.waitting,
                                                            //        result = ket_luan_cls ?? string.Empty
                                                            //    });
                                                            //}

                                                        }
                                                        catch
                                                        {
                                                            //listUpdateStatusTicketItemId.Add(new StatusTicketItemId { ticket_item_id = item.TicketItemId, type = item.Type, status = StatusForHIS.waitting });
                                                            var ket_luan_cls = await _db.ResultCDHAs
                                                                .Where(p => p.TicketItemId == item.TicketItemId)
                                                                .Select(p => p.Result) // giả sử cột kết luận tên là KetLuan
                                                                .FirstOrDefaultAsync();
                                                            listUpdateStatusTicketItemId.Add(new StatusTicketItemId
                                                            {
                                                                ticket_item_id = item.TicketItemId,
                                                                type = item.Type,
                                                                status = StatusForHIS.waitting,
                                                                result = ket_luan_cls ?? string.Empty
                                                            });
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
                    if (listUpdateStatusTicketItemId != null && listUpdateStatusTicketItemId.Count > 0)
                    {
                        await UpdateStatusTicketItemId(listUpdateStatusTicketItemId);
                    }
                    if (lstSIDForTicketItem != null && lstSIDForTicketItem.Count > 0)
                    {
                        await UpdateSIDForTicketItemId(lstSIDForTicketItem);
                    }
                }
            }
            catch { }
        }

        public async Task<Patient> AddPatient(string sid, string seq, string pid, string ticket_id, string patientName, string sex,
            string address, DateTime? age, string diagnostic, string objectName, string locationName,
            string doctorName, DateTime dateTimeServer, string assignDate, string benhan, string maBenhAn, string maDotKham, string phone, string soCccd)
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

                // loại bệnh án pakage / out
                var normalizedType = (benhan ?? string.Empty).Trim().ToLower();
                if (string.IsNullOrEmpty(normalizedType))
                {
                    normalizedType = "pakage"; // hoặc giữ rỗng nếu bạn muốn an toàn theo dữ liệu HIS
                }

                Patient patientOld = null;
                try
                {
                    using (var context = new LABContext())
                    {
                        patientOld = await context.Patients.FirstOrDefaultAsync(p =>
                               p.PatientId == pid
                            && p.TicketId == ticket_id
                            && p.DoctorId == doctorId
                            && p.InsertTime > assignDateTmp.AddMinutes(-15)
                            && p.InsertTime.Value.Day == assignDateTmp.Day
                            && p.InsertTime.Value.Month == assignDateTmp.Month
                            && p.InsertTime.Value.Year == assignDateTmp.Year
                            && (p.BenhAn ?? "").Trim().ToLower() == normalizedType);
                    }
                }
                catch (Exception ex)
                { }

                if (patientOld == null)
                {
                    if (string.IsNullOrEmpty(seq))
                    {
                        seq = await GetSeq();
                    }
                    try
                    {
                        var sidSuffix = normalizedType == "out" ? "O" : "P";

                        sid = assignDateTmp.Day.ToString().PadLeft(2, '0')
                            + assignDateTmp.Month.ToString().PadLeft(2, '0')
                            + assignDateTmp.Year.ToString().Substring(2, 2)
                            + "-" + seq
                            + "-" + sidSuffix;
                    }
                    catch
                    {
                        var sidSuffix = normalizedType == "out" ? "O" : "P";

                        sid = assignDateTmp.Day.ToString().PadLeft(2, '0')
                            + assignDateTmp.Month.ToString().PadLeft(2, '0')
                            + assignDateTmp.Year.ToString().Substring(2, 2)
                            + "-" + seq
                            + "-" + sidSuffix;
                    }

                    // ★ THÊM ĐOẠN NÀY: nếu SID đã tồn tại rồi thì dùng luôn record đó, khỏi tạo mới
                    using (var context = new LABContext())
                    {
                        var existedBySid = await context.Patients.FirstOrDefaultAsync(p =>
                            p.Sid == sid
                            && (p.BenhAn ?? "").Trim().ToLower() == normalizedType
                        );

                        if (existedBySid != null)
                        {
                            // có thể update diagnostic nếu cái mới dài hơn
                            if (!string.IsNullOrEmpty(diagnostic))
                            {
                                var oldDiag = existedBySid.Diagnostic ?? string.Empty;
                                if (oldDiag.Length < diagnostic.Length)
                                {
                                    existedBySid.Diagnostic = diagnostic;
                                    await context.SaveChangesAsync();
                                }
                            }
                            return existedBySid;
                        }
                    }
                    // ★ Hết đoạn thêm

                    using (var context = new LABContext())
                    {
                        patientOld = new Patient();
                        patientOld.Sid = sid;
                        patientOld.Seq = seq;
                        patientOld.PatientId = pid;
                        patientOld.TicketId = ticket_id;
                        patientOld.PatientName = patientName;
                        patientOld.Sex = sex;
                        patientOld.Address = address;
                        patientOld.Age = age;
                        patientOld.Diagnostic = diagnostic;
                        patientOld.DoctorId = doctorId;
                        patientOld.ObjectId = objectId;
                        patientOld.LocationId = locationId;
                        patientOld.InsertTime = assignDateTmp;
                        patientOld.BenhAn = benhan;
                        patientOld.Active = true;
                        patientOld.MaBenhAn = maBenhAn;
                        patientOld.MaDotKham = maDotKham;
                        patientOld.Phone = phone;
                        patientOld.SoCccd = soCccd;

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

        public async Task<bool> AddResultXN(long? patientId, string ticket_item_id, DateTime dateTimeServer, Service service, string doctorName, string keyResultForHis, string typeBenhAn)
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
                                    result.TypeBenhAn = typeBenhAn;
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

        public async Task<bool> AddResultCDHA(
            long? patientId,
            string ticket_item_id,
            DateTime dateTimeServer,
            Service service,
            string doctorName,
            string keyResultForHis,
            string typeBenhAn)
        {
            try
            {
                var doctor = lstDoctor
                  .Where(p => p.Name.Equals(doctorName))
                  .FirstOrDefault();

                long? doctorId = null;

                if (doctor != null)
                    doctorId = doctor.Id;

                using (var context = new LABContext())
                {
                    var patient = await context.Patients
                      .Where(p => p.Id == patientId)
                      .FirstOrDefaultAsync();

                    if (patient == null)
                        return false;

                    // =========================================================
                    // Một chỉ định HIS được xác định theo TicketItemId + ServiceId.
                    //
                    // Không chỉ kiểm tra ServiceId, vì cùng một bệnh nhân có thể
                    // được chỉ định lại cùng dịch vụ ở lần/ngày khác.
                    // =========================================================
                    var resultOld = await context.ResultCDHAs
                      .FirstOrDefaultAsync(p =>
                        p.PatientId == patientId &&
                        p.ServiceId == service.Id &&
                        p.TicketItemId == ticket_item_id &&
                        p.Active == true);

                    if (resultOld == null)
                    {
                        // Nếu đây là dịch vụ đầu tiên của module thì bật Wait*
                        // theo workflow hiện tại.
                        var hasResultInModule = await context.ResultCDHAs
                          .AnyAsync(p =>
                            p.Active == true &&
                            p.PatientId == patientId &&
                            p.Service.Category.Code == service.Category.Code);

                        if (!hasResultInModule)
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
                            else if (service.Category.Code == "NSCTC")
                                patient.WaitNSCTC = true;
                            else if (service.Category.Code == "TDCN")
                                patient.WaitTDCN = true;
                        }

                        var result = new ResultCDHA
                        {
                            PatientId = patientId,
                            ServiceId = service.Id,
                            TicketItemId = ticket_item_id,

                            InsertTime = dateTimeServer,
                            DoctorId = doctorId,

                            KeyResultForHis = keyResultForHis,
                            TypeBenhAn = typeBenhAn,

                            Active = true,

                            // ===============================================
                            // Service-level validation state
                            // Chỉ định mới từ HIS chưa từng Valid.
                            // ===============================================
                            IsValidated = false,
                            LastValidatedAt = null,
                            LastValidatedByUserId = null
                        };

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

        public async Task UpdateStatusTicketItemId(List<StatusTicketItemId> lstValue)
        {
            try
            {
                if (settingAPIUpdateStatus != null && !string.IsNullOrEmpty(settingAPIUpdateStatus.Value))
                {
                    HttpResponseMessage response = new HttpResponseMessage();
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(settingAPIUpdateStatus.Value);
                        var myContent = JsonConvert.SerializeObject(lstValue);
                        var buffer = Encoding.UTF8.GetBytes(myContent);
                        var byteContent = new ByteArrayContent(buffer);
                        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        response = client.PostAsync("", byteContent).Result;
                    }
                }
            }
            catch { }
        }

        public async Task UpdateSIDForTicketItemId(List<SIDForTicketItem> value)
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
                        var seq = resultStandard.Seq;
                        var sid = insertTime.Day.ToString().PadLeft(2, '0') + insertTime.Month.ToString().PadLeft(2, '0') +
                                  insertTime.Year.ToString().Substring(2, 2) + "-" + resultStandard.Seq;
                        var preSid = preInserTime.Day.ToString().PadLeft(2, '0') + preInserTime.Month.ToString().PadLeft(2, '0') +
                                 preInserTime.Year.ToString().Substring(2, 2) + "-" + resultStandard.Seq;

                        await UpdateResultXN(sid, preSid, seq, resultStandard.TestCodeId, resultStandard.Result, resultStandard.PosNeg, resultStandard);
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

        public async Task UpdateResultXN(string sid, string preSid, string seq, long? testCodeId, double? result, string posneg, ResultStandard resultStandard)
        {
            if (result != null || !string.IsNullOrEmpty(posneg.Trim()))
            {
                try
                {
                    using (var context = new LABContext())
                    {
                        var now = DateTime.Now;
                        var from = now.AddDays(-30);
                        //var objResult = await context.ResultXNs.Where(p => p.Patient.Sid == sid && p.TestCodeId == testCodeId && p.Patient.WaitXN == false && p.Patient.ProcessXN == true && p.Patient.ValidXN == false).FirstOrDefaultAsync();
                        //if (objResult == null) objResult = await context.ResultXNs.Where(p => p.Patient.Sid == preSid && p.TestCodeId == testCodeId && p.Patient.WaitXN == false && p.Patient.ProcessXN == true && p.Patient.ValidXN == false).FirstOrDefaultAsync();
                        // 1) Tìm ResultXN theo SID hiện tại
                        var objResult = await context.ResultXNs
                            .Include(r => r.Patient)
                            .FirstOrDefaultAsync(p =>
                                p.Patient.Sid == sid &&
                                p.TestCodeId == testCodeId &&
                                p.Patient.WaitXN == false &&
                                p.Patient.ProcessXN == true &&
                                p.Patient.ValidXN == false);

                        // 2) Fallback: thử preSid (SID của ngày trước)
                        if (objResult == null)
                        {
                            objResult = await context.ResultXNs
                                .Include(r => r.Patient)
                                .FirstOrDefaultAsync(p =>
                                    (p.Patient.Sid == preSid || p.Patient.Seq == seq) &&
                                    p.TestCodeId == testCodeId &&
                                    p.Patient.WaitXN == false &&
                                    p.Patient.ProcessXN == true &&
                                    p.Patient.ValidXN == false &&
                                    p.InsertTime >= from && p.InsertTime <= now);
                        }
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

                        // ========== NHÁNH 1: Có cả result (numeric) + posneg (định tính) ==========
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
                        // ========== NHÁNH 2: Chỉ có posneg (định tính) ==========
                        else if (result == null && !string.IsNullOrEmpty(posneg))
                        {
                            if (objTestCode != null && objTestCode.NormalResult != null)
                            {
                                // có case lưu tạm kết quả đổ từ máy nước tiểu về tỷ trọng SG 1.020 nếu lưu theoi kiểu double sẽ mất số 0 cuối
                                if (posneg != "Positive" && posneg != "Negative")
                                {
                                    double? lower;
                                    double? upper;
                                    // Kiểm tra xem posneg có phải là một số hợp lệ không
                                    double resultTmp;
                                    bool isNumeric = double.TryParse(posneg, out resultTmp);

                                    if (isNumeric)
                                    {
                                        lower = objTestCode?.LowerLimit;
                                        upper = objTestCode?.HigherLimit;

                                        if (lower == null && upper != null)
                                        {

                                            objResult.Status = (resultTmp > upper) ? 2 : 0;
                                        }
                                        else if (lower != null && upper == null)
                                        {
                                            objResult.Status = (resultTmp < lower) ? 1 : 0;
                                        }
                                        else if (lower != null && upper != null)
                                        {
                                            if (resultTmp < lower) objResult.Status = 1;
                                            else if (resultTmp > upper) objResult.Status = 2;
                                            else objResult.Status = 0;
                                        }
                                        else
                                        {
                                            // Không có giới hạn tham chiếu -> coi là bình thường
                                            objResult.Status = 0;
                                        }
                                        objResult.PosNeg = posneg;
                                        objResult.Result = null;
                                    }
                                    else
                                    {
                                        var isStarPattern = System.Text.RegularExpressions.Regex.IsMatch(posneg.Trim(), @"^[\*\.]+$");

                                        if (isStarPattern)
                                        {
                                            // Nếu là pattern dấu * thì gán vào Result thay vì PosNeg
                                            objResult.Result = posneg;
                                            objResult.PosNeg = null;
                                            objResult.Status = 0;
                                        }
                                        else
                                        {
                                            // Nếu posneg không phải là một số hợp lệ, gán giá trị mặc định
                                            objResult.PosNeg = posneg;
                                            objResult.Result = null;
                                            objResult.Status = 0;
                                            if (posneg == "Positive")
                                            {
                                                objResult.Status = 2;
                                            }
                                        }
                                    }
                                }
                                else if (posneg == objTestCode.NormalResult.Trim())
                                {
                                    objResult.PosNeg = posneg;
                                    objResult.Result = null;
                                    objResult.Status = 0;
                                }
                                else
                                {
                                    objResult.PosNeg = posneg;
                                    objResult.Result = null;
                                    objResult.Status = 0; // LÚC ĐẦU BẰNG 2 NHƯNG SỬA LẠI ĐỂ HIỂN THỊ MÀU ĐEN THÔI
                                }
                            }
                            else
                            {
                                objResult.PosNeg = posneg;
                                objResult.Result = null;
                                objResult.Status = 0;
                            }
                        }
                        // ========== NHÁNH 3: Chỉ có result (numeric) -> SO SÁNH THEO GIỚI TÍNH ==========
                        else if (result != null && string.IsNullOrEmpty(posneg))
                        {
                            // Lấy giới tính từ Patient (Nam/Nữ)
                            var sex = (objResult?.Patient?.Sex ?? string.Empty).Trim();

                            // Chọn cặp giới hạn theo giới tính
                            double? lower;
                            double? upper;

                            // Nếu là Nữ: ưu tiên dùng LowerLimitF/HigherLimitF; nếu null thì fallback về LowerLimit/HigherLimit
                            if (string.Equals(sex, "Nữ", StringComparison.OrdinalIgnoreCase))
                            {
                                lower = objTestCode?.LowerLimitF ?? objTestCode?.LowerLimit;
                                upper = objTestCode?.HigherLimitF ?? objTestCode?.HigherLimit;
                            }
                            else
                            {
                                // Mặc định coi là Nam (hoặc không xác định) -> dùng giới hạn Nam
                                lower = objTestCode?.LowerLimit;
                                upper = objTestCode?.HigherLimit;
                            }

                            // So sánh và gán Status
                            if (lower == null && upper != null)
                            {
                                objResult.Status = (result > upper) ? 2 : 0;
                            }
                            else if (lower != null && upper == null)
                            {
                                objResult.Status = (result < lower) ? 1 : 0;
                            }
                            else if (lower != null && upper != null)
                            {
                                if (result < lower) objResult.Status = 1;
                                else if (result > upper) objResult.Status = 2;
                                else objResult.Status = 0;
                            }
                            else
                            {
                                // Không có giới hạn tham chiếu -> coi là bình thường
                                objResult.Status = 0;
                            }

                            // Ghi lại kết quả định dạng
                            objResult.Result = Format_Decimal(result);
                        }

                        if (resultStandard != null && resultStandard.Device != null)
                            objResult.DeviceCodeBHYT = resultStandard.Device.CodeBHYT;
                        await context.SaveChangesAsync();
                        // Lấy SID thực tế từ bản ghi tìm được
                        var actualSid = objResult?.Patient?.Sid;
                        if (string.IsNullOrWhiteSpace(actualSid))
                        {
                            // Fallback an toàn nếu vì lý do gì đó Patient chưa load
                            // (trong code hiện tại đã Include đầy đủ nên nhánh này hiếm khi xảy ra)
                            actualSid = sid;
                        }
                        await UpdateStatus_ResultStandard(resultStandard, StatusForResultStandard.Updated);
                        await UpdateStatus_Result(actualSid, testCodeId, StatusForResult.HaveResult);
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
                               .AddDays(-30);
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
                    //var lstResult = await context.ResultXNs.Where(p => p.Active == true && p.PushBHYT == false && 
                    //    p.Patient.ValidXN == true && p.Patient.InsertTime > from && p.Patient.InsertTime < to).ToListAsync();

                    var lstResult = await context.ResultXNs.Where(p => p.Active == true && p.PushBHYT == false &&
                        (p.Patient.ValidXN == true || (p.Patient.NotFullResultXN == true && p.Patient.ProcessXN == true)) &&
                        p.Patient.InsertTime > from && p.Patient.InsertTime < to).ToListAsync();

                    var lstResult_BHYT = new List<Result_BHYT>();
                    var lstStatusTicketItemId = new List<StatusTicketItemId>();

                    if (lstResult != null)
                    {
                        foreach (var item in lstResult)
                        {
                            if (item.TestCode.IsTestHead) continue;

                            //var result_BHYT = lstResult_BHYT.Where(p => p.ticket_item_id == item.TicketItemId).FirstOrDefault();
                            //if (result_BHYT == null)
                            //{
                            //    var nguoiThucHien = string.Empty;
                            //    if(!string.IsNullOrEmpty(item?.Patient?.UserXN?.Name))
                            //    {
                            //        nguoiThucHien = item?.Patient?.UserXN?.Name.Split('.').Length > 1 ? item?.Patient?.UserXN?.Name.Split('.')[1].Trim() : item?.Patient?.UserXN?.Name.Trim();
                            //    }                              
                            //    var lstResult_XN = new List<Result_XN> { new Result_XN { param_code = item.TestCode.CodeBHYT, param_name = item.TestCode.NameBHYT, param_value = !string.IsNullOrEmpty(item.Result) ? item.Result : item.PosNeg, param_unit = item?.TestCode?.Unit } };
                            //    var result_BHYT_New = new Result_BHYT { patient_id = item?.Patient?.PatientId, ticket_id = item?.Patient?.TicketId, service_id = item?.Service?.Code, ticket_item_id = item.TicketItemId, type = item.Patient.BenhAn, mechine_code = item.DeviceCodeBHYT, result = lstResult_XN, date = item.Patient.ReturnResultTimeXN ?? DateTime.Now, nguoi_thuc_hien = nguoiThucHien, bs_doc_kq = item?.Patient?.UserXN?.MaBHYT };
                            //    lstResult_BHYT.Add(result_BHYT_New);
                            //}
                            //else
                            //{
                            //    var result_XN = new Result_XN { param_code = item.TestCode.CodeBHYT, param_name = item.TestCode.NameBHYT, param_value = !string.IsNullOrEmpty(item.Result) ? item.Result : item.PosNeg, param_unit = item?.TestCode?.Unit };
                            //    result_BHYT.result.Add(result_XN);
                            //}

                            // Chỉ gửi kết quả BHYT khi bệnh nhân đã ValidXN
                            if (item.Patient.ValidXN == true)
                            {
                                var result_BHYT = lstResult_BHYT.Where(p => p.ticket_item_id == item.TicketItemId).FirstOrDefault();
                                if (result_BHYT == null)
                                {
                                    var nguoiThucHien = string.Empty;
                                    if (!string.IsNullOrEmpty(item?.Patient?.UserXN?.Name))
                                    {
                                        nguoiThucHien = item?.Patient?.UserXN?.Name.Split('.').Length > 1 ? item?.Patient?.UserXN?.Name.Split('.')[1].Trim() : item?.Patient?.UserXN?.Name.Trim();
                                    }
                                    var lstResult_XN = new List<Result_XN> { new Result_XN { param_code = item.TestCode.CodeBHYT, param_name = item.TestCode.NameBHYT, param_value = !string.IsNullOrEmpty(item.Result) ? item.Result : item.PosNeg, param_unit = item?.TestCode?.Unit } };
                                    var result_BHYT_New = new Result_BHYT { patient_id = item?.Patient?.PatientId, ticket_id = item?.Patient?.TicketId, service_id = item?.Service?.Code, ticket_item_id = item.TicketItemId, type = item.Patient.BenhAn, mechine_code = item.DeviceCodeBHYT, result = lstResult_XN, date = item.Patient.ReturnResultTimeXN ?? DateTime.Now, nguoi_thuc_hien = nguoiThucHien, bs_doc_kq = item?.Patient?.UserXN?.MaBHYT };
                                    lstResult_BHYT.Add(result_BHYT_New);
                                }
                                else
                                {
                                    var result_XN = new Result_XN { param_code = item.TestCode.CodeBHYT, param_name = item.TestCode.NameBHYT, param_value = !string.IsNullOrEmpty(item.Result) ? item.Result : item.PosNeg, param_unit = item?.TestCode?.Unit };
                                    result_BHYT.result.Add(result_XN);
                                }
                            }

                            // Set status cho Ticket_Item_Id (cả 2 trường hợp ValidXN và NotFullResultXN)
                            var value = lstStatusTicketItemId.Where(p => p.ticket_item_id == item.TicketItemId).FirstOrDefault();
                            if (value == null)
                            {
                                lstStatusTicketItemId.Add(new StatusTicketItemId { ticket_item_id = item.TicketItemId, type = item.TypeBenhAn, status = StatusForHIS.closed, sid = item.KeyResultForHis });
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

                        if (lstStatusTicketItemId != null && lstStatusTicketItemId.Count > 0)
                        {
                            await UpdateStatusTicketItemId(lstStatusTicketItemId);
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
                    // Các module CDHA đã chuyển sang service-level validation.
                    // Record mới: chỉ push khi chính ResultCDHA đó IsValidated = true.
                    // Legacy: IsValidated = null thì fallback Patient.Valid* để không làm mất dữ liệu cũ.
                    var lstResultImage = await context.ResultCDHAs
                        .Where(p => p.Active == true &&
                                    p.PushBHYT == false &&
                                    p.Patient.InsertTime > from &&
                                    p.Patient.InsertTime < to &&
                                    p.Service.Category.Code != "TDCN" &&
                                    !string.IsNullOrWhiteSpace(p.Result) &&
                                    (
                                        p.IsValidated == true ||
                                        (
                                            p.IsValidated == null &&
                                            (
                                                (p.Patient.ValidSA == true && p.Service.Category.Code == "SA") ||
                                                (p.Patient.ValidSAT == true && p.Service.Category.Code == "SAT") ||
                                                (p.Patient.ValidXQ == true && p.Service.Category.Code == "XQ") ||
                                                (p.Patient.ValidNS == true && p.Service.Category.Code == "NS") ||
                                                (p.Patient.ValidNSCTC == true && p.Service.Category.Code == "NSCTC") ||
                                                (p.Patient.ValidDDT == true && p.Service.Category.Code == "DDT")
                                            )
                                        )
                                    ))
                        .ToListAsync();

                    // TDCN không bắt buộc có Result.
                    // Record mới vẫn phải IsValidated = true; legacy fallback Patient.ValidTDCN.
                    var lstResultTDCN = await context.ResultCDHAs
                        .Where(p => p.Active == true &&
                                    p.PushBHYT == false &&
                                    p.Patient.InsertTime > from &&
                                    p.Patient.InsertTime < to &&
                                    p.Service.Category.Code == "TDCN" &&
                                    (p.IsValidated == true || (p.IsValidated == null && p.Patient.ValidTDCN == true)))
                        .ToListAsync();

                    if (lstResultTDCN.Count > 0)
                    {
                        lstResultImage.AddRange(lstResultTDCN);
                    }

                    var lstResult_BHYT = new List<Result_BHYT>();
                    var lstStatusTicketItemId = new List<StatusTicketItemId>();

                    if (lstResultImage != null)
                    {
                        foreach (var item in lstResultImage)
                        {
                            // Record service-level dùng LastValidatedAt riêng của chính dịch vụ.
                            // Legacy hoặc fail-safe thiếu timestamp sẽ fallback Patient.ReturnResultTime* như trước.
                            DateTime? legacyDate = item?.Service?.Category?.Code == "SA" ? item?.Patient?.ReturnResultTimeSA :
                                                   (item?.Service?.Category?.Code == "SAT" ? item?.Patient?.ReturnResultTimeSAT :
                                                   (item?.Service?.Category?.Code == "XQ" ? item?.Patient?.ReturnResultTimeXQ :
                                                   (item?.Service?.Category?.Code == "DDT" ? item?.Patient?.ReturnResultTimeDDT :
                                                   (item?.Service?.Category?.Code == "NSCTC" ? item?.Patient?.ReturnResultTimeNSCTC :
                                                   (item?.Service?.Category?.Code == "TDCN" ? item?.Patient?.ReturnResultTimeTDCN : item?.Patient?.ReturnResultTimeNS)))));

                            var date = item.IsValidated.HasValue && item.LastValidatedAt.HasValue ? item.LastValidatedAt : legacyDate;

                            var nguoiThucHien = item?.Service?.Category?.Code == "SA" ? item?.Patient?.UserSA?.Name :
                                              (item?.Service?.Category?.Code == "SAT" ? item?.Patient?.UserSAT?.Name :
                                              (item?.Service?.Category?.Code == "XQ" ? item?.Patient?.UserReturnXQ?.Name :
                                              (item?.Service?.Category?.Code == "DDT" ? item?.Patient?.UserDDT?.Name :
                                              (item?.Service?.Category?.Code == "NSCTC" ? item?.Patient?.UserNSCTC?.Name :
                                              (item?.Service?.Category?.Code == "TDCN" ? item?.Patient?.UserTDCN?.Name : item?.Patient?.UserNS?.Name)))));

                            var bsDocKQ = item?.Service?.Category?.Code == "SA" ? item?.Patient?.UserSA?.MaBHYT :
                                         (item?.Service?.Category?.Code == "SAT" ? item?.Patient?.UserSAT?.MaBHYT :
                                         (item?.Service?.Category?.Code == "XQ" ? item?.Patient?.UserReturnXQ?.MaBHYT :
                                         (item?.Service?.Category?.Code == "DDT" ? item?.Patient?.UserDDT?.MaBHYT :
                                         (item?.Service?.Category?.Code == "NSCTC" ? item?.Patient?.UserNSCTC?.MaBHYT :
                                         (item?.Service?.Category?.Code == "TDCN" ? item?.Patient?.UserTDCN?.MaBHYT : item?.Patient?.UserNS?.MaBHYT)))));

                            if (!string.IsNullOrEmpty(nguoiThucHien))
                            {
                                nguoiThucHien = nguoiThucHien.Split('.').Length > 1 ? nguoiThucHien.Split('.')[1].Trim() : nguoiThucHien.Trim();
                            }

                            lstResult_BHYT.Add(new Result_BHYT { patient_id = item?.Patient?.PatientId, ticket_id = item?.Patient?.TicketId, service_id = item?.Service?.Code, ticket_item_id = item.TicketItemId, type = item.Patient.BenhAn, mechine_code = item.DeviceCodeBHYT, describe = item.Description, conclusion = item.Result, date = date ?? DateTime.Now, nguoi_thuc_hien = nguoiThucHien, bs_doc_kq = bsDocKQ });
                            lstStatusTicketItemId.Add(new StatusTicketItemId { ticket_item_id = item.TicketItemId, type = item.TypeBenhAn, status = StatusForHIS.closed, sid = item.KeyResultForHis, result = item.Result });
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

                    if (lstStatusTicketItemId != null && lstStatusTicketItemId.Count > 0)
                    {
                        await UpdateStatusTicketItemId(lstStatusTicketItemId);
                    }
                }
            }
            catch { }
        }

        public async Task Send_Result_BHYT(List<Result_BHYT> lstValue)
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
            catch { }
        }

        public async Task<string> GetSetting_APIStartAutoTask()
        {
            using (var context = new LABContext())
            {
                var _setting = await context.Settings.Where(p => p.Code == SettingBL.SelectAPIStartAutoTask).FirstOrDefaultAsync();
                if (_setting != null)
                {
                    return _setting.Value;
                }
                return null;
            }
        }

        /*
         ** Cập nhật thông tin bệnh nhân từ HIS
         */
        public async Task<ApiResponse> UpdatePatientFromHIS(UpdatePatientRequest request)
        {
            try
            {
                using (var context = new LABContext())
                {
                    // Tìm tất cả bản ghi bệnh nhân theo PatientId (và TicketId nếu có)
                    IQueryable<Patient> query = context.Patients.Where(p => p.PatientId == request.PatientId);

                    // Nếu có TicketId thì lọc thêm theo TicketId
                    if (!string.IsNullOrEmpty(request.TicketId))
                    {
                        query = query.Where(p => p.TicketId == request.TicketId);
                    }

                    var patients = await query.ToListAsync();

                    if (patients == null || patients.Count == 0)
                    {
                        return new ApiResponse
                        {
                            Success = false,
                            Message = "Không tìm thấy bệnh nhân",
                            ErrorCode = "PATIENT_NOT_FOUND"
                        };
                    }

                    // Cập nhật thông tin cho tất cả bản ghi tìm được
                    foreach (var patient in patients)
                    {
                        // Cập nhật thông tin bệnh nhân
                        if (!string.IsNullOrEmpty(request.PatientName))
                            patient.PatientName = request.PatientName;

                        if (!string.IsNullOrEmpty(request.Sex))
                            patient.Sex = request.Sex == "male" ? "Nam" : "Nữ";

                        if (!string.IsNullOrEmpty(request.Age))
                        {
                            try
                            {
                                patient.Age = DateTime.Parse(request.Age);
                            }
                            catch { }
                        }

                        if (!string.IsNullOrEmpty(request.Address))
                            patient.Address = request.Address;

                        if (!string.IsNullOrEmpty(request.Diagnostic))
                            patient.Diagnostic = request.Diagnostic;

                        if (!string.IsNullOrEmpty(request.MaBenhAn))
                            patient.MaBenhAn = request.MaBenhAn;

                        if (!string.IsNullOrEmpty(request.MaDotKham))
                            patient.MaDotKham = request.MaDotKham;

                        // Cập nhật Doctor nếu có
                        if (!string.IsNullOrEmpty(request.DoctorName))
                        {
                            var doctor = lstDoctor?.FirstOrDefault(p => p.Name.Equals(request.DoctorName));
                            if (doctor != null)
                            {
                                patient.DoctorId = doctor.Id;
                            }
                            else
                            {
                                patient.DoctorId = await AddDoctorFromHIS(request.DoctorName);
                            }
                        }

                        // Cập nhật Object nếu có
                        if (!string.IsNullOrEmpty(request.ObjectName))
                        {
                            var obj = lstObject?.FirstOrDefault(p => p.Name.Equals(request.ObjectName));
                            if (obj != null)
                            {
                                patient.ObjectId = obj.Id;
                            }
                            else
                            {
                                patient.ObjectId = await AddObjectFromHIS(request.ObjectName);
                            }
                        }

                        // Cập nhật Location nếu có
                        if (!string.IsNullOrEmpty(request.LocationName))
                        {
                            var location = lstLocation?.FirstOrDefault(p => p.Name.Equals(request.LocationName));
                            if (location != null)
                            {
                                patient.LocationId = location.Id;
                            }
                            else
                            {
                                patient.LocationId = await AddLocationFromHIS(request.LocationName);
                            }
                        }

                        patient.UpdateTime = DateTime.Now;
                    }

                    await context.SaveChangesAsync();

                    return new ApiResponse
                    {
                        Success = true,
                        Message = $"Cập nhật thông tin thành công cho {patients.Count} bản ghi bệnh nhân",
                        Data = new
                        {
                            PatientId = request.PatientId,
                            TicketId = request.TicketId,
                            UpdatedRecordsCount = patients.Count,
                            UpdateTime = DateTime.Now,
                            UpdatedPatients = patients.Select(p => new
                            {
                                p.Sid,
                                p.PatientId,
                                p.TicketId,
                                p.PatientName,
                                p.UpdateTime
                            }).ToList()
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Lỗi khi cập nhật: {ex.Message}",
                    ErrorCode = "UPDATE_ERROR"
                };
            }
        }

        /// <summary>
        /// Cập nhật dịch vụ không thực hiện từ HIS
        /// </summary>
        public async Task<ApiResponse> UpdateServiceNotPerformed(UpdateServiceNotPerformedRequest request)
        {
            try
            {
                using (var context = new LABContext())
                {
                    // 1. Tìm Service theo ServiceCode
                    var service = await context.Services
                        .Include(s => s.Category)
                            .ThenInclude(c => c.Group)
                        .FirstOrDefaultAsync(s => s.Code == request.ServiceCode);

                    if (service == null)
                    {
                        return new ApiResponse
                        {
                            Success = false,
                            Message = $"Không tìm thấy dịch vụ với ServiceCode: {request.ServiceCode}",
                            ErrorCode = "SERVICE_CODE_NOT_FOUND"
                        };
                    }

                    // 2. Lấy thông tin Category và GroupCategory
                    var categoryCode = service.Category?.Code;
                    var groupCategoryCode = service.Category?.Group?.Code;

                    if (string.IsNullOrEmpty(groupCategoryCode))
                    {
                        return new ApiResponse
                        {
                            Success = false,
                            Message = $"Không xác định được nhóm dịch vụ cho ServiceCode: {request.ServiceCode}",
                            ErrorCode = "GROUP_CATEGORY_NOT_FOUND"
                        };
                    }

                    // 3. Tìm Doctor theo DoctorName (nếu có)
                    long? doctorId = null;
                    if (!string.IsNullOrEmpty(request.DoctorName))
                    {
                        var doctor = lstDoctor?.FirstOrDefault(p => p.Name.Equals(request.DoctorName));
                        if (doctor != null)
                        {
                            doctorId = doctor.Id;
                        }
                        else
                        {
                            // Nếu không tìm thấy thì tạo mới
                            doctorId = await AddDoctorFromHIS(request.DoctorName);
                        }
                    }

                    var updatedCount = 0;

                    // 4. Xử lý theo GroupCategory
                    if (groupCategoryCode == "XN")
                    {
                        // Tìm và cập nhật ResultXN theo TicketItemId và ServiceId
                        var resultXN = await context.ResultXNs
                            .Where(p => p.TicketItemId == request.TicketItemId
                                     && p.ServiceId == service.Id
                                     && p.Active == true)
                            .ToListAsync();

                        if (resultXN != null && resultXN.Count > 0)
                        {
                            foreach (var item in resultXN)
                            {
                                item.Active = false;
                                item.UpdateTime = DateTime.Now;
                                if (doctorId.HasValue)
                                {
                                    item.DoctorId = doctorId.Value;
                                }
                                updatedCount++;
                            }
                        }
                    }
                    else if (groupCategoryCode == "CDHA")
                    {
                        // Tìm và cập nhật ResultCDHA theo TicketItemId và ServiceId
                        var resultCDHA = await context.ResultCDHAs
                            .Where(p => p.TicketItemId == request.TicketItemId
                                     && p.ServiceId == service.Id
                                     && p.Active == true)
                            .ToListAsync();

                        if (resultCDHA != null && resultCDHA.Count > 0)
                        {
                            foreach (var item in resultCDHA)
                            {
                                item.Active = false;
                                item.UpdateTime = DateTime.Now;
                                if (doctorId.HasValue)
                                {
                                    item.DoctorId = doctorId.Value;
                                }
                                updatedCount++;
                            }
                        }
                    }
                    else
                    {
                        return new ApiResponse
                        {
                            Success = false,
                            Message = $"Nhóm dịch vụ '{groupCategoryCode}' không được hỗ trợ. Chỉ hỗ trợ 'XN' hoặc 'CDHA'",
                            ErrorCode = "UNSUPPORTED_GROUP_CATEGORY"
                        };
                    }

                    // 5. Kiểm tra nếu không tìm thấy bản ghi nào
                    if (updatedCount == 0)
                    {
                        return new ApiResponse
                        {
                            Success = false,
                            Message = $"Không tìm thấy kết quả dịch vụ với TicketItemId: {request.TicketItemId} và ServiceCode: {request.ServiceCode}",
                            ErrorCode = "RESULT_NOT_FOUND",
                            Data = new
                            {
                                TicketItemId = request.TicketItemId,
                                ServiceCode = request.ServiceCode,
                                CategoryCode = categoryCode,
                                GroupCategoryCode = groupCategoryCode,
                                DoctorName = request.DoctorName,
                                UpdatedCount = updatedCount
                            }
                        };
                    }

                    await context.SaveChangesAsync();

                    return new ApiResponse
                    {
                        Success = true,
                        Message = "Cập nhật dịch vụ thành công",
                        Data = new
                        {
                            TicketItemId = request.TicketItemId,
                            ServiceCode = request.ServiceCode,
                            ServiceName = service.Name,
                            CategoryCode = categoryCode,
                            GroupCategoryCode = groupCategoryCode,
                            DoctorName = request.DoctorName,
                            DoctorId = doctorId,
                            UpdatedCount = updatedCount,
                            UpdatedAt = DateTime.Now
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Lỗi khi cập nhật: {ex.Message}",
                    ErrorCode = "UPDATE_ERROR"
                };
            }
        }

        /// <summary>
        /// Cập nhật nhiều dịch vụ không thực hiện cùng lúc
        /// </summary>
        public async Task<ApiResponse> UpdateMultipleServicesNotPerformed(List<UpdateServiceNotPerformedRequest> requests)
        {
            try
            {
                var results = new List<object>();
                var successCount = 0;
                var failCount = 0;

                foreach (var request in requests)
                {
                    var result = await UpdateServiceNotPerformed(request);
                    if (result.Success)
                    {
                        successCount++;
                    }
                    else
                    {
                        failCount++;
                    }
                    results.Add(new
                    {
                        TicketItemId = request.TicketItemId,
                        ServiceCode = request.ServiceCode,
                        Success = result.Success,
                        Message = result.Message,
                        ErrorCode = result.ErrorCode
                    });
                }

                return new ApiResponse
                {
                    Success = true,
                    Message = $"Xử lý hoàn tất: {successCount} thành công, {failCount} thất bại",
                    Data = new
                    {
                        TotalRequests = requests.Count,
                        SuccessCount = successCount,
                        FailCount = failCount,
                        Details = results
                    }
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Lỗi khi xử lý batch: {ex.Message}",
                    ErrorCode = "BATCH_UPDATE_ERROR"
                };
            }
        }

        // Thêm phương thức mới để gọi API hủy dịch vụ
        public async Task UpdateCancelServiceStatus(string ticketItemId, string type)
        {
            try
            {
                if (settingAPIUpdateStatus != null && !string.IsNullOrEmpty(settingAPIUpdateStatus.Value))
                {
                    var cancelData = new List<StatusTicketItemId>
                    {
                        new StatusTicketItemId
                        {
                            ticket_item_id = ticketItemId,
                            type = type,
                            status = StatusForHIS.canceled
                        }
                    };

                    HttpResponseMessage response = new HttpResponseMessage();
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(settingAPIUpdateStatus.Value);
                        var myContent = JsonConvert.SerializeObject(cancelData);
                        var buffer = Encoding.UTF8.GetBytes(myContent);
                        var byteContent = new ByteArrayContent(buffer);
                        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        response = client.PostAsync("", byteContent).Result;
                    }
                }
            }
            catch { }
        }
    }

    public class StatusTicketItemId
    {
        public string ticket_item_id { get; set; }
        public string sid { get; set; }
        public string type { get; set; }
        public string status { get; set; }
        public string result { get; set; } // Thêm để đẩy KQ cho HIS
        //public string doctor { get; set; } // Thêm để đẩy Bác Sĩ thực hiện và trả KQ cho HIS
    }

    public class SIDForTicketItem
    {
        public string ticketItemId { get; set; }
        public string sid { get; set; }
        public string type { get; set; }
    }

    public class PatientInfo_Add
    {
        public string PatientId { get; set; }
        public string TicketId { get; set; }
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
        public string ServiceName { get; set; }
        public string AssignDate { get; set; }
        public string Type { get; set; }
        public string Seq { get; set; }
        public string MaBenhAn { get; set; }
        public string MaDotKham { get; set; }
        public string Phone { get; set; }
        public string? SoCccd { get; set; }
    }

    public class PatientInfo_Del
    {
        public string TicketItemId { get; set; }
        public string ServiceId { get; set; }
    }

    public class StatusForHIS
    {
        public static string open = "open"; // open : Bên HIS khi chưa đẩy qua
        public static string waitting = "waiting"; // Đã nhận từ HIS
        public static string in_progress = "in_progress"; // Lab đang làm
        public static string closed = "closed"; // Lab đã làm xong 
        public static string canceled = "canceled"; // đã hủy chỉ định
    }

    public class StatusForResultStandard
    {
        public static string NotUpdate = "NOT";
        public static string Updated = "UPD";
        public static string Erorr = "ERO";
    }

    public class StatusForResult
    {
        public static int NotResult = 0;
        public static int HaveResult = 1;
        public static int Sended = 2;
        public static int Erorr = 3;
    }

    public class Result_BHYT
    {
        public string patient_id { get; set; }
        public string ticket_id { get; set; }
        public string ticket_item_id { get; set; }
        public string service_id { get; set; }
        public string type { get; set; }
        public string mechine_code { get; set; }
        public string describe { get; set; }
        public string conclusion { get; set; }
        public List<Result_XN> result { get; set; }
        public DateTime date { get; set; }
        public string nguoi_thuc_hien { get; set; }
        public string bs_doc_kq { get; set; }
    }

    public class Result_XN
    {
        public string param_code { get; set; }
        public string param_name { get; set; }
        public string param_value { get; set; }
        public string param_unit { get; set; }
    }
}
