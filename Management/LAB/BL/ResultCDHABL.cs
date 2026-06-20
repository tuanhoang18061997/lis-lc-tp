using Management.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using Object = Management.Models.Object;

namespace Management.BL
{
    public class ResultCDHABL
    {
        private readonly LABContext _db;
        private readonly ServiceTestBL _serviceTestBL;
        public ResultCDHABL(LABContext db, ServiceTestBL serviceTestBL)
        {
            _db = db;
            _serviceTestBL = serviceTestBL;
        }

        public async Task<bool> DeleteByPatientId(long patientId, long? userInsertOrUpdate)
        {
            try
            {
                var _lstResultCDHA = await _db.ResultCDHAs.Where(p => p.Active == true && p.PatientId == patientId).ToListAsync();
                if (_lstResultCDHA != null)
                {
                    foreach (var _item in _lstResultCDHA)
                    {
                        _item.Active = false;
                        _item.UserUpdateId = userInsertOrUpdate;
                    }
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

        public async Task<bool> DeleteById(long id, long? userInsertOrUpdate)
        {
            try
            {
                var _resultCDHA = await _db.ResultCDHAs.Where(p => p.Active == true && p.Id == id).FirstOrDefaultAsync();
                if (_resultCDHA != null)
                {
                    _resultCDHA.Active = false;
                    _resultCDHA.UserUpdateId = userInsertOrUpdate;
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

        public async Task<bool> SaveService(long patientId, long serviceId, long? userInsertIdOrUpdateId, DateTime dateTime, long doctorId, string categoryCode)
        {
            try
            {
                var _resultCDHA = await _db.ResultCDHAs.Where(p => p.Active == true && p.PatientId == patientId && p.ServiceId == serviceId).FirstOrDefaultAsync();
                if (_resultCDHA != null) return false;
                _resultCDHA = new ResultCDHA();
                _resultCDHA.PatientId = patientId;
                _resultCDHA.ServiceId = serviceId;
                _resultCDHA.UserInsertId = userInsertIdOrUpdateId;
                _resultCDHA.InsertTime = dateTime;
                _resultCDHA.DoctorId = doctorId;
                _resultCDHA.KeyResultForHis = categoryCode + "-" + Guid.NewGuid().ToString();
                _resultCDHA.Active = true;
                await _db.ResultCDHAs.AddAsync(_resultCDHA);
                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Update(ResultCDHAModel resultCDHA, long? userInsertIdOrUpdateId, DateTime dateTime, Device device)
        {
            try
            {
                var _resultCDHA = await _db.ResultCDHAs.Where(p => p.Active == true && p.Id == resultCDHA.resultCDHAId).FirstOrDefaultAsync();
                if (_resultCDHA == null) return false;
                _resultCDHA.UserUpdateId = userInsertIdOrUpdateId;
                _resultCDHA.UpdateTime = dateTime;
                _resultCDHA.Description = resultCDHA.description;
                _resultCDHA.Result = resultCDHA.result;
                _resultCDHA.Suggest = resultCDHA.suggest;
                _resultCDHA.DeviceCodeBHYT = device?.CodeBHYT;
                _resultCDHA.SieuAmTim = resultCDHA.sieuamtim;
                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<ResultCDHA> GetResultCDHAByPatientId_ForValidPrint(long resultCDHAId)
        {
            try
            {
                return await _db.ResultCDHAs.Where(p => p.Active == true &&  p.Id == resultCDHAId).FirstOrDefaultAsync();
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<ResultCDHA>> GetListResultCDHAByPatientId(long patientId, string categoryCode)
        {
            try
            {
                var a = await _db.ResultCDHAs.Where(p => p.Active == true && p.PatientId == patientId && p.Service.Category.Code == categoryCode).ToListAsync();
                return await _db.ResultCDHAs.Where(p => p.Active == true && p.PatientId == patientId && p.Service.Category.Code == categoryCode).ToListAsync();
            }
            catch
            {
                return null;
            }
        }

        //public async Task<List<ImageCDHA>> GetImageForService(long id)
        //{
        //    try
        //    {
        //        return await _db.ImageCDHAs.Where(p => p.ResultCDHAId == id).ToListAsync();
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

        public async Task<ResultCDHA> GetResultCDHA(long id)
        {
            try
            {
                return await _db.ResultCDHAs.Where(p => p.Active == true &&  p.Id == id).FirstOrDefaultAsync();
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> SaveImage(long id, string imagePath, string imagePath1)
        {
            try
            {
                var _imageCDHA = new ImageCDHA();
                _imageCDHA.ResultCDHAId = id;
                _imageCDHA.ImagePath = imagePath;
                _imageCDHA.ImagePath1 = imagePath1;
                await _db.ImageCDHAs.AddAsync(_imageCDHA);
                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> DeleteImage(long id)
        {
            var imagePath1_resultCDHAId = string.Empty;
            try
            {
                
                var item = await _db.ImageCDHAs.Where(p => p.Id == id).FirstOrDefaultAsync();
                if(item != null)
                {
                    imagePath1_resultCDHAId = item.ImagePath1 + ";" + item.ResultCDHAId;
                    _db.ImageCDHAs.Remove(item);
                    await _db.SaveChangesAsync();
                }
                return imagePath1_resultCDHAId;
            }
            catch
            {
                return imagePath1_resultCDHAId;
            }
        }

        public async Task<bool> ExitResultXN_Update_GetSampleTime(long patientId, DateTime getSampleTime, string categoryCode)
        {
            try
            {
                var resultCDHA = await _db.ResultCDHAs.Where(p => p.Active == true && p.PatientId == patientId && p.Service.Category.Code == categoryCode).FirstOrDefaultAsync();
                if(resultCDHA != null)
                {
                    if (categoryCode == "SA")
                    {
                        resultCDHA.Patient.GetSampleTimeSA = getSampleTime;
                    }
                    else if (categoryCode == "SAT")
                    {
                        resultCDHA.Patient.GetSampleTimeSAT = getSampleTime;
                    }
                    else if (categoryCode == "NS")
                    {
                        resultCDHA.Patient.GetSampleTimeNS = getSampleTime;
                    }
                    else if (categoryCode == "XQ")
                    {
                        resultCDHA.Patient.GetSampleTimeXQ = getSampleTime;
                    }
                    else if (categoryCode == "DDT")
                    {
                        resultCDHA.Patient.GetSampleTimeDDT = getSampleTime;
                    }
                    else if (categoryCode == "TDCN")
                    {
                        resultCDHA.Patient.GetSampleTimeTDCN = getSampleTime;
                    }
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

        public async Task<bool> Delete(string ticketItemId, string categoryCode)
        {
            try
            {
                ResultCDHA resultCDHA = null;
                if (categoryCode == "SA")
                {
                    resultCDHA = await _db.ResultCDHAs.Where(p => p.Active == true && p.TicketItemId == ticketItemId && p.Patient.WaitSA == true).FirstOrDefaultAsync();
                }
                else if (categoryCode == "SAT")
                {
                    resultCDHA = await _db.ResultCDHAs.Where(p => p.Active == true && p.TicketItemId == ticketItemId && p.Patient.WaitSAT == true).FirstOrDefaultAsync();
                }
                else if (categoryCode == "DDT")
                {
                    resultCDHA = await _db.ResultCDHAs.Where(p => p.Active == true && p.TicketItemId == ticketItemId && p.Patient.WaitDDT == true).FirstOrDefaultAsync();
                }
                else if (categoryCode == "XQ")
                {
                    resultCDHA = await _db.ResultCDHAs.Where(p => p.Active == true && p.TicketItemId == ticketItemId && p.Patient.WaitXQ == true).FirstOrDefaultAsync();
                }
                else if (categoryCode == "NS")
                {
                    resultCDHA = await _db.ResultCDHAs.Where(p => p.Active == true && p.TicketItemId == ticketItemId && p.Patient.WaitNS == true).FirstOrDefaultAsync();
                }
                else if (categoryCode == "TDCN")
                {
                    resultCDHA = await _db.ResultCDHAs.Where(p => p.Active == true && p.TicketItemId == ticketItemId && p.Patient.WaitTDCN == true).FirstOrDefaultAsync();
                }
                if (resultCDHA != null)
                {
                    resultCDHA.Active = false;
                    await _db.SaveChangesAsync();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateSignStoreId_CKS(long resultCDHAId, long signStoreId, long userInsertIdOrUpdateId, DateTime dateTime)
        {
            try
            {
                var _resultCDHA = await _db.ResultCDHAs.Where(p => p.Active == true && p.Id == resultCDHAId).FirstOrDefaultAsync();
                if (_resultCDHA == null) return false;
                _resultCDHA.UserUpdateId = userInsertIdOrUpdateId;
                _resultCDHA.UpdateTime = dateTime;
                _resultCDHA.SignStoreId = signStoreId;
                _resultCDHA.SignStatus = 1;
                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<ResultCDHA> GetResultCDHAByKeyResultForHis(string keyResultForHis)
        {
            try
            {
                return await _db.ResultCDHAs
                    .Include(r => r.Patient)
                    .Include(r => r.Service)
                    .Where(r => r.Active == true && r.KeyResultForHis == keyResultForHis)
                    .FirstOrDefaultAsync();
            }
            catch
            {
                return null;
            }
        }
    }
}
