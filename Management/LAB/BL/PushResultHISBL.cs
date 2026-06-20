using Management.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections;
using System.Net.Http.Headers;
using System.Text;

namespace Management.BL
{
    public class PushResultHISBL
    {
        private readonly LABContext _db;
        public PushResultHISBL(LABContext db)
        {
            _db = db;
        }

        public async Task<List<PushResultHIS>> Get_ListPatient(DateTime fromDate, DateTime toDate, string patientId)
        {
            var lstPatient = new List<Patient>();
            var lstPushResultHIS = new List<PushResultHIS>();

            if (string.IsNullOrEmpty(patientId))
                lstPatient = await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate &&
                    ((p.WaitXN == false && p.ProcessXN == false && p.ValidXN == true) ||
                     (p.WaitSA == false && p.ProcessSA == false && p.ValidSA == true) ||
                     (p.WaitNS == false && p.ProcessNS == false && p.ValidNS == true) ||
                     (p.WaitXQ == false && p.ProcessXQ == false && p.ValidXQ == true) ||
                     (p.WaitDDT == false && p.ProcessDDT == false && p.ValidDDT == true) ||
                     (p.WaitSAT == false && p.ProcessSAT == false && p.ValidSAT == true))).OrderByDescending(p => p.InsertTime).ToListAsync();
            else
                lstPatient =  await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate && p.PatientId == patientId &&
                    ((p.WaitXN == false && p.ProcessXN == false && p.ValidXN == true) ||
                     (p.WaitSA == false && p.ProcessSA == false && p.ValidSA == true) ||
                     (p.WaitNS == false && p.ProcessNS == false && p.ValidNS == true) ||
                     (p.WaitXQ == false && p.ProcessXQ == false && p.ValidXQ == true) ||
                     (p.WaitDDT == false && p.ProcessDDT == false && p.ValidDDT == true) ||
                     (p.WaitSAT == false && p.ProcessSAT == false && p.ValidSAT == true))).OrderByDescending(p => p.InsertTime).ToListAsync();

            if(lstPatient != null && lstPatient.Count > 0)
            {
                foreach(var patient in lstPatient)
                {
                    var lstXN = patient.ResultXNs.DistinctBy(p => p.TicketItemId).ToList();
                    var lstCDHA = patient.ResultCDHAs;
                    if(lstXN != null && lstXN.Count > 0)
                    {
                        foreach(var xn in lstXN)
                        {
                            var o = new PushResultHIS();
                            o.Seq = patient.Seq;
                            o.Sid = patient.Sid;
                            o.PatientId = patient.PatientId;
                            o.PatientName = patient.PatientName;
                            o.TicketId = patient.TicketId;
                            o.TicketItemId = xn.TicketItemId;
                            o.Service = xn.Service.Name;
                            o.ServiceCode = xn.Service.Code;
                            o.KeyResultForHis = xn.KeyResultForHis;
                            o.InsertTime = patient.InsertTime;
                            o.Push = xn.PushBHYT ? "1" : "0";
                            lstPushResultHIS.Add(o);
                        }                        
                    }

                    if (lstCDHA != null && lstCDHA.Count > 0)
                    {
                        foreach (var ha in lstCDHA)
                        {
                            var o = new PushResultHIS();
                            o.Seq = patient.Seq;
                            o.Sid = patient.Sid;
                            o.PatientId = patient.PatientId;
                            o.PatientName = patient.PatientName;
                            o.TicketId = patient.TicketId;
                            o.TicketItemId = ha.TicketItemId;
                            o.Service = ha.Service.Name;
                            o.ServiceCode = ha.Service.Code;
                            o.KeyResultForHis = ha.KeyResultForHis;
                            o.InsertTime = patient.InsertTime;
                            o.Push = ha.PushBHYT ? "1" : "0";
                            lstPushResultHIS.Add(o);
                        }
                    }
                }
            }
            return lstPushResultHIS;
        }

        public async Task<bool> UpdatePush(DateTime fromDate, DateTime toDate, string patientId, string push)
        {
            try
            {
                var lstPatient = new List<Patient>();

                if (string.IsNullOrEmpty(patientId))
                    lstPatient = await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate &&
                        ((p.WaitXN == false && p.ProcessXN == false && p.ValidXN == true) ||
                         (p.WaitSA == false && p.ProcessSA == false && p.ValidSA == true) ||
                         (p.WaitNS == false && p.ProcessNS == false && p.ValidNS == true) ||
                         (p.WaitXQ == false && p.ProcessXQ == false && p.ValidXQ == true) ||
                         (p.WaitDDT == false && p.ProcessDDT == false && p.ValidDDT == true) ||
                         (p.WaitSAT == false && p.ProcessSAT == false && p.ValidSAT == true))).OrderByDescending(p => p.InsertTime).ToListAsync();
                else
                    lstPatient = await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate && p.PatientId == patientId &&
                        ((p.WaitXN == false && p.ProcessXN == false && p.ValidXN == true) ||
                         (p.WaitSA == false && p.ProcessSA == false && p.ValidSA == true) ||
                         (p.WaitNS == false && p.ProcessNS == false && p.ValidNS == true) ||
                         (p.WaitXQ == false && p.ProcessXQ == false && p.ValidXQ == true) ||
                         (p.WaitDDT == false && p.ProcessDDT == false && p.ValidDDT == true) ||
                         (p.WaitSAT == false && p.ProcessSAT == false && p.ValidSAT == true))).OrderByDescending(p => p.InsertTime).ToListAsync();

                if (lstPatient != null && lstPatient.Count > 0)
                {
                    foreach (var patient in lstPatient)
                    {
                        var lstXN = patient.ResultXNs;
                        var lstCDHA = patient.ResultCDHAs;
                        if (lstXN != null && lstXN.Count > 0)
                        {
                            foreach (var xn in lstXN)
                            {
                                xn.PushBHYT = push == "0" ? false : true;
                            }
                        }

                        if (lstCDHA != null && lstCDHA.Count > 0)
                        {
                            foreach (var ha in lstCDHA)
                            {
                                ha.PushBHYT = push == "0" ? false : true;
                            }
                        }
                    }
                    await _db.SaveChangesAsync();
                }
                return true;
            }
            catch 
            {
                return false;
            }           
        }

        public async Task<bool> Push(DateTime fromDate, DateTime toDate, string patientId)
        {
            try
            {
                await Get_ResultXN_PushBHYT(fromDate, toDate, patientId);
                await Get_ResultCDHA_PushBHYT(fromDate, toDate, patientId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task Get_ResultXN_PushBHYT(DateTime from, DateTime to, string patientId)
        {
            using (var context = new LABContext())
            {
                var lstResult = new List<ResultXN>();
                if (string.IsNullOrEmpty(patientId))
                    lstResult = await context.ResultXNs.Where(p => p.Active == true && p.Patient.ValidXN == true && p.Patient.InsertTime > from && p.Patient.InsertTime < to).ToListAsync();
                else
                    lstResult = await context.ResultXNs.Where(p => p.Active == true && p.Patient.ValidXN == true && p.Patient.InsertTime > from && p.Patient.InsertTime < to && p.Patient.PatientId == patientId).ToListAsync();
                var lstResult_BHYT = new List<Result_BHYT>();
                var lstStatusTicketItemId = new List<StatusTicketItemId>();


                if (lstResult != null)
                {
                    foreach (var item in lstResult)
                    {
                        if (item.TestCode.IsTestHead) continue;
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
                        // Set status cho Ticket_Item_Id
                        var value = lstStatusTicketItemId.Where(p => p.ticket_item_id == item.TicketItemId).FirstOrDefault();
                        if (value == null)
                        {
                            lstStatusTicketItemId.Add(new StatusTicketItemId { ticket_item_id = item.TicketItemId, type = item.TypeBenhAn, status = StatusForHIS.closed, sid = item.KeyResultForHis });
                        }
                    }

                    if (lstStatusTicketItemId != null && lstStatusTicketItemId.Count > 0)
                    {
                        await UpdateStatusTicketItemId(lstStatusTicketItemId);
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
                }
            }
        }

        public async Task Get_ResultCDHA_PushBHYT(DateTime from, DateTime to, string patientId)
        {
            using (var context = new LABContext())
            {
                var lstResultImage = new List<ResultCDHA>();
                var lstStatusTicketItemId = new List<StatusTicketItemId>();
                if (string.IsNullOrEmpty(patientId))
                    lstResultImage = await context.ResultCDHAs.Where(p => p.Active == true && p.Patient.InsertTime > from && p.Patient.InsertTime < to &&
                                                         ((p.Patient.ValidSA == true && p.Service.Category.Code == "SA") ||
                                                         (p.Patient.ValidXQ == true && p.Service.Category.Code == "XQ") ||
                                                         (p.Patient.ValidNS == true && p.Service.Category.Code == "NS") ||
                                                         (p.Patient.ValidSAT == true && p.Service.Category.Code == "SAT"))).ToListAsync();
                else
                    lstResultImage = await context.ResultCDHAs.Where(p => p.Active == true && p.Patient.PatientId == patientId && p.Patient.InsertTime > from && p.Patient.InsertTime < to &&
                                                         ((p.Patient.ValidSA == true && p.Service.Category.Code == "SA") ||
                                                         (p.Patient.ValidXQ == true && p.Service.Category.Code == "XQ") ||
                                                         (p.Patient.ValidNS == true && p.Service.Category.Code == "NS") ||
                                                         (p.Patient.ValidSAT == true && p.Service.Category.Code == "SAT"))).ToListAsync();
                var lstResult_BHYT = new List<Result_BHYT>();
                if (lstResultImage != null)
                {
                    foreach (var item in lstResultImage)
                    {
                        var date = item?.Service?.Category?.Code == "SA" ? item?.Patient?.ReturnResultTimeSA :
                                      (item?.Service?.Category?.Code == "SAT" ? item?.Patient?.ReturnResultTimeSAT :
                                      (item?.Service?.Category?.Code == "XQ" ? item?.Patient?.ReturnResultTimeXQ : item?.Patient?.ReturnResultTimeNS));

                        var nguoiThucHien = item?.Service?.Category?.Code == "SA" ? item?.Patient?.UserSA?.Name :
                                  (item?.Service?.Category?.Code == "SAT" ? item?.Patient?.UserSAT?.Name :
                                  (item?.Service?.Category?.Code == "XQ" ? item?.Patient?.UserReturnXQ?.Name : item?.Patient?.UserNS?.Name));

                        var bsDocKQ = item?.Service?.Category?.Code == "SA" ? item?.Patient?.UserSA?.MaBHYT :
                                    (item?.Service?.Category?.Code == "SAT" ? item?.Patient?.UserSAT?.MaBHYT :
                                    (item?.Service?.Category?.Code == "XQ" ? item?.Patient?.UserReturnXQ?.MaBHYT : item?.Patient?.UserNS?.MaBHYT));

                        if (!string.IsNullOrEmpty(nguoiThucHien))
                        {
                            nguoiThucHien = nguoiThucHien.Split('.').Length > 1 ? nguoiThucHien.Split('.')[1].Trim() : nguoiThucHien.Trim();
                        }

                        lstResult_BHYT.Add(new Result_BHYT { patient_id = item?.Patient?.PatientId, ticket_id = item?.Patient?.TicketId, service_id = item?.Service?.Code, ticket_item_id = item.TicketItemId, type = item.Patient.BenhAn, mechine_code = item.DeviceCodeBHYT, describe = item.Description, conclusion = item.Result, date = date ?? DateTime.Now, nguoi_thuc_hien = nguoiThucHien, bs_doc_kq = bsDocKQ });
                        // check dịch vụ đó đã có kết luận chưa thì mới chuyển status
                        if (item.Result != null)
                        {
                            lstStatusTicketItemId.Add(new StatusTicketItemId { ticket_item_id = item.TicketItemId, type = item.TypeBenhAn, status = StatusForHIS.closed, sid = item.KeyResultForHis, result = item.Result });
                        }
                    }
                }
                var lstResultImageTmp = new List<ResultCDHA>();

                if (lstStatusTicketItemId != null && lstStatusTicketItemId.Count > 0)
                {
                    var ticketItemIds = lstStatusTicketItemId.Select(x => x.ticket_item_id).ToList();

                    lstResultImageTmp = lstResultImage
                                        .Where(x => ticketItemIds.Contains(x.TicketItemId))
                                        .ToList();
                }
                if (lstStatusTicketItemId != null && lstStatusTicketItemId.Count > 0)
                {
                    await UpdateStatusTicketItemId(lstStatusTicketItemId);
                    foreach (var item in lstResultImageTmp)
                    {
                        item.PushBHYT = true;
                    }
                    await context.SaveChangesAsync();
                }

                if (lstResult_BHYT != null && lstResult_BHYT.Count > 0)
                {
                    await Send_Result_BHYT(lstResult_BHYT);
                    //foreach (var item in lstResultImage)
                    //{
                    //    item.PushBHYT = true;
                    //}
                    //await context.SaveChangesAsync();
                }
            }
        }

        public async Task Send_Result_BHYT(List<Result_BHYT> lstValue)
        {
            var settingAPISendResult = await _db.Settings.Where(p => p.Code == SettingBL.APISendResult).FirstOrDefaultAsync();
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

        public async Task UpdateStatusTicketItemId(List<StatusTicketItemId> lstValue)
        {
            try
            {
                var settingAPIUpdateStatus = await _db.Settings.Where(p => p.Code == SettingBL.APIUpdateStatus).FirstOrDefaultAsync();
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
    }
}
