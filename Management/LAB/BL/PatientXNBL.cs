using Management.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;

namespace Management.BL
{
    public class PatientXNBL
    {
        private readonly LABContext _db;
        private readonly ResultEditUnlockBL _resultEditUnlockBL;
        public PatientXNBL(LABContext db, ResultEditUnlockBL resultEditUnlockBL)
        {
            _db = db;
            _resultEditUnlockBL = resultEditUnlockBL;
        }
        public async Task<List<Patient>> Get_ListPatient(DateTime fromDate, DateTime toDate, bool waitXN, bool processXN, bool validXN)
        {
            return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate &&
                    p.WaitXN == waitXN && p.ProcessXN == processXN && p.ValidXN == validXN).OrderByDescending(p => p.InsertTime).ToListAsync();
        }

        public async Task<List<Patient>> Get_ListPatient(DateTime fromDate, DateTime toDate)
        {
            return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate).OrderByDescending(p => p.InsertTime).ToListAsync();
        }
         
        public async Task<int> Get_CountPatient(DateTime fromDate, DateTime toDate, bool waitXN, bool processXN, bool validXN)
        {
            return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate &&
                  p.WaitXN == waitXN && p.ProcessXN == processXN && p.ValidXN == validXN).CountAsync();
        }

        public async Task<int> Get_CountPatientFullResult(DateTime fromDate, DateTime toDate, bool waitXN, bool processXN, bool validXN)
        {
            return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate &&
                   p.WaitXN == waitXN && p.ProcessXN == processXN && p.ValidXN == validXN && p.FullResultXN == true).CountAsync();
        }

        public async Task<int> Get_CountPatientNotFullResult(DateTime fromDate, DateTime toDate, bool waitXN, bool processXN, bool validXN)
        {
            return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate &&
                    p.WaitXN == waitXN && p.ProcessXN == processXN && p.ValidXN == validXN && p.FullResultXN == false).CountAsync();

        }

        public async Task<List<Patient>> Get_ListPatientByPidOrSid(DateTime fromDate, DateTime toDate, bool waitXN, bool processXN, bool validXN, string pidorseq)
        {
            if (string.IsNullOrEmpty(pidorseq))
            {
                return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate &&
                                    p.WaitXN == waitXN && p.ProcessXN == processXN && p.ValidXN == validXN).OrderByDescending(p => p.InsertTime).ToListAsync();
            }
            else
            {

                return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate &&
                                    p.WaitXN == waitXN && p.ProcessXN == processXN && p.ValidXN == validXN && (p.PatientId.Contains(pidorseq) || p.Seq.Contains(pidorseq) || p.MaBenhAn.Contains(pidorseq) || p.PatientName.Contains(pidorseq) || p.Address.Contains(pidorseq))).OrderByDescending(p => p.InsertTime).ToListAsync();
            }
        }

        public async Task<List<Patient>> Get_ListPatientByPidOrSid(DateTime fromDate, DateTime toDate, string pidorseq)
        {
            if (string.IsNullOrEmpty(pidorseq))
            {
                return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate ).OrderByDescending(p => p.InsertTime).ToListAsync();
            }
            else
            {
                return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate && (p.PatientId == pidorseq || p.Seq == pidorseq || p.MaBenhAn == pidorseq)).OrderByDescending(p => p.InsertTime).ToListAsync();
            }
        }

        public async Task<Patient> Get_PatientBySid(long id)
        {
            return await _db.Patients.Where(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<bool> DeleteById(long id)
        {
            try
            {
                var _resultCDHA = await _db.ResultCDHAs.Where(p => p.Active == true && p.PatientId == id).FirstOrDefaultAsync();
                if (_resultCDHA == null)
                {
                    var _patient = await _db.Patients.Where(p => p.Active == true && p.Id == id).FirstOrDefaultAsync();
                    if (_patient != null)
                    {
                        _patient.Active = false;
                        await _db.SaveChangesAsync();
                        return true;
                    }
                }
                else
                {
                    var _patient = await _db.Patients.Where(p => p.Active == true && p.Id == id).FirstOrDefaultAsync();
                    if (_patient != null)
                    {
                        _patient.WaitXN = false;
                        _patient.ProcessXN = false;
                        _patient.ValidXN = false;
                        await _db.SaveChangesAsync();
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> GetSample_ProcessResult_ReturnResult(long id, bool waitXN, bool processXN, bool validXN, long? userInsertOrUpdate)
        {
            try
            {
                var _patient = await _db.Patients.Where(p => p.Id == id).FirstOrDefaultAsync();
                if (_patient != null)
                {
                    _patient.WaitXN = waitXN;
                    _patient.ProcessXN = processXN;
                    _patient.ValidXN = validXN;
                    _patient.UserUpdateId = userInsertOrUpdate;
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

        public async Task<Patient> SaveOrUpdate(long? id, string patientId, string seq, string sid, string patientName, DateTime age,
                                                string sex, string obj, string benhan, string location, string doctor,
                                                DateTime getSampleTime, string address, string diagnostic,
                                                long? userInsertIdOrUpdateId, DateTime dateTime, bool waitXN, bool processXN, bool validXN, string hospital)
        {
            try
            {
                if (id == null)
                {
                    var _patient = new Patient();
                    _patient.PatientId = patientId;
                    _patient.Seq = seq;
                    _patient.Sid = sid;
                    _patient.PatientName = patientName;
                    _patient.Age = age;
                    _patient.Sex = sex;
                    _patient.ObjectId = long.Parse(obj);
                    _patient.BenhAn = benhan;
                    _patient.LocationId = long.Parse(location);
                    _patient.DoctorId = long.Parse(doctor);
                    _patient.GetSampleTimeXN = getSampleTime;
                    _patient.Address = address;
                    _patient.Diagnostic = diagnostic;
                    _patient.HospitalId = long.Parse(hospital);
                    _patient.UserInsertId = userInsertIdOrUpdateId;
                    _patient.InsertTime = dateTime;
                    _patient.WaitXN = waitXN;
                    _patient.ProcessXN = processXN;
                    _patient.ValidXN = validXN;
                    _patient.Active = true;
                    await _db.Patients.AddAsync(_patient);
                    await _db.SaveChangesAsync();
                    return _patient;
                }
                else
                {
                    var _patient = await _db.Patients.Where(p => p.Id == id).FirstOrDefaultAsync();
                    if (_patient != null)
                    {
                        _patient.PatientId = patientId;
                        _patient.Seq = seq;
                        _patient.Sid = sid;
                        _patient.PatientName = patientName;
                        _patient.Age = age;
                        _patient.Sex = sex;
                        _patient.ObjectId = long.Parse(obj);
                        _patient.BenhAn = benhan;
                        _patient.LocationId = long.Parse(location);
                        _patient.DoctorId = long.Parse(doctor);
                        _patient.GetSampleTimeXN = getSampleTime;
                        _patient.Address = address;
                        _patient.Diagnostic = diagnostic;
                        _patient.HospitalId = long.Parse(hospital);
                        _patient.UserUpdateId = userInsertIdOrUpdateId;
                        _patient.UpdateTime = dateTime;
                        _patient.WaitXN = waitXN;
                        _patient.ProcessXN = processXN;
                        _patient.ValidXN = validXN;
                        await _db.SaveChangesAsync();
                        return _patient;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task Update(long id, DateTime returnResultTime, long userReturnResult, long? userInserOrUpdate, bool fullResultXN)
        {
            try
            {
                var _patient = await _db.Patients.Where(p => p.Id == id).FirstOrDefaultAsync();
                if (_patient != null)
                {
                    // tắt 2 dòng dưới để sử dụng cho cột NotFullResultXN. vì đang check nếu NotFullResultXN == true và ReturnResultTimeXN != null thì coi như đã Valid PDF cho BS thấy nhưng chưa đủ kq
                    //_patient.ReturnResultTimeXN = returnResultTime;
                    //_patient.UserReturnResultXN = userReturnResult;
                    _patient.UserUpdateId = userInserOrUpdate;
                    _patient.FullResultXN = fullResultXN;
                    await _db.SaveChangesAsync();
                }
            }
            catch { }
        }

        public async Task<bool> Update(long id, DateTime returnResultTime, long userReturnResult, bool waitXN, bool processXN, bool validXN, long? userInsertOrUpdate, bool fullResultXN, bool notFullResultXN)
        {
            try
            {
                var _patient = await _db.Patients.Where(p => p.Id == id).FirstOrDefaultAsync();
                if (_patient != null)
                {
                    _patient.ReturnResultTimeXN = returnResultTime;
                    _patient.UserReturnResultXN = userReturnResult;
                    _patient.WaitXN = waitXN;
                    _patient.ProcessXN = processXN;
                    _patient.ValidXN = validXN;
                    _patient.UserUpdateId = userInsertOrUpdate;
                    _patient.FullResultXN = fullResultXN;
                    _patient.NotFullResultXN = notFullResultXN;
                    await _db.SaveChangesAsync();

                    if (validXN && userInsertOrUpdate.HasValue)
                    {
                        await _resultEditUnlockBL.RevokeAfterValidAsync(
                            id,
                            "XN",
                            userInsertOrUpdate.Value,
                            ToolBL.Get_DateNow());
                    }
                    return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        public async Task<List<Patient>> Get_ListPatientByPidOrSid(DateTime fromDate, DateTime toDate, bool waitXN, bool processXN, bool validXN, string pidorseq, string maDotKham)
        {
            var query = _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate &&
                                p.WaitXN == waitXN && p.ProcessXN == processXN && p.ValidXN == validXN);

            // Filter theo pidorseq nếu có
            if (!string.IsNullOrEmpty(pidorseq))
            {
                query = query.Where(p => p.PatientId.Contains(pidorseq) || p.Seq.Contains(pidorseq) || 
                                       p.MaBenhAn.Contains(pidorseq) || p.PatientName.Contains(pidorseq));
            }

            // Filter theo MaDotKham nếu có
            if (!string.IsNullOrEmpty(maDotKham))
            {
                query = query.Where(p => p.MaDotKham == maDotKham);
            }

            return await query.OrderByDescending(p => p.InsertTime).ToListAsync();
        }

        public async Task<List<Patient>> Get_ListPatientByPidOrSid(DateTime fromDate, DateTime toDate, string pidorseq, string maDotKham)
        {
            var query = _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate);

            // Filter theo pidorseq nếu có
            if (!string.IsNullOrEmpty(pidorseq))
            {
                query = query.Where(p => p.PatientId.Contains(pidorseq) || p.Seq.Contains(pidorseq) ||
                                       p.MaBenhAn.Contains(pidorseq) || p.PatientName.Contains(pidorseq));
            }

            // Filter theo MaDotKham nếu có
            if (!string.IsNullOrEmpty(maDotKham))
            {
                query = query.Where(p => p.MaDotKham == maDotKham);
            }

            return await query.OrderByDescending(p => p.InsertTime).ToListAsync();
        }

    }
}
