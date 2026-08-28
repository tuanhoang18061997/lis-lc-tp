using Management.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;

namespace Management.BL
{
    public class PatientCDHABL
    {
        private readonly LABContext _db;
        private readonly ResultEditUnlockBL _resultEditUnlockBL;
        public PatientCDHABL(LABContext db, ResultEditUnlockBL resultEditUnlockBL)
        {
            _db = db;
            _resultEditUnlockBL = resultEditUnlockBL;
        }
        public async Task<List<Patient>> Get_ListPatient(DateTime fromDate, DateTime toDate, bool wait, bool process, bool valid, string categoryCode)
        {
            if (categoryCode == "SA")
            {
                return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate &&
                                    p.WaitSA == wait && p.ProcessSA == process && p.ValidSA == valid).OrderByDescending(p => p.ReturnResultTimeSA).ToListAsync();
            }
            else if (categoryCode == "SAT")
            {
                return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate &&
                                    p.WaitSAT == wait && p.ProcessSAT == process && p.ValidSAT == valid).OrderByDescending(p => p.ReturnResultTimeSAT).ToListAsync();
            }
            else if (categoryCode == "DDT")
            {
                return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate &&
                                    p.WaitDDT == wait && p.ProcessDDT == process && p.ValidDDT == valid).OrderByDescending(p => p.ReturnResultTimeDDT).ToListAsync();
            }
            else if (categoryCode == "TDCN")
            {
                return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate &&
                                    p.WaitTDCN == wait && p.ProcessTDCN == process && p.ValidTDCN == valid).OrderByDescending(p => p.ReturnResultTimeTDCN).ToListAsync();
            }
            else if (categoryCode == "NS")
            {
                return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate &&
                                     p.WaitNS == wait && p.ProcessNS == process && p.ValidNS == valid).OrderByDescending(p => p.ReturnResultTimeNS).ToListAsync();
            }
            else if (categoryCode == "NSCTC")
            {
                return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate &&
                                     p.WaitNSCTC == wait && p.ProcessNSCTC == process && p.ValidNSCTC == valid).OrderByDescending(p => p.ReturnResultTimeNSCTC).ToListAsync();
            }
            else if (categoryCode == "XQ")
            {
                return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate &&
                                    p.WaitXQ == wait && p.ProcessXQ == process && p.ValidXQ == valid).OrderByDescending(p => p.ReturnResultTimeXQ).ToListAsync();
            }
            return null;
        }

        public async Task<List<Patient>> Get_ListPatient_Flexible(DateTime fromDate, DateTime toDate, string categoryCode, string viewMode)
        {
            var q = _db.Patients
                .AsNoTracking() // ✅ Tắt change tracking để tăng tốc
                .Where(p => p.Active
                         && p.InsertTime > fromDate
                         && p.InsertTime <= toDate);

            // PREFILTER theo nhóm: đã từng Wait/Process/Valid của nhóm đó
            q = ApplyCategoryTouchedFilter(q, categoryCode);

            if (viewMode == "process")
            {
                q = ExcludeWait(q, categoryCode);

                // ✅ Tối ưu: Lấy danh sách PatientId có pending trước, sau đó filter
                var patientIdsWithPending = await _db.ResultCDHAs
                    .AsNoTracking()
                    .Where(r => r.Active
                             && r.Service.Category.Code == categoryCode
                             && (r.Result == null || r.Result == ""))
                    .Select(r => r.PatientId)
                    .Distinct()
                    .ToListAsync();

                q = q.Where(p =>
                    // Process* đang bật theo category
                    ((categoryCode == "SA" && p.ProcessSA) ||
                     (categoryCode == "SAT" && p.ProcessSAT) ||
                     (categoryCode == "NS" && p.ProcessNS) ||
                     (categoryCode == "NSCTC" && p.ProcessNSCTC) ||
                     (categoryCode == "XQ" && p.ProcessXQ) ||
                     (categoryCode == "DDT" && p.ProcessDDT) ||
                     (categoryCode == "TDCN" && p.ProcessTDCN))
                    // HOẶC có trong danh sách pending
                    || patientIdsWithPending.Contains(p.Id));
            }
            else if (viewMode == "valid")
            {
                // BẮT BUỘC có cờ Valid* = true theo category
                q = q.Where(p =>
                    (categoryCode == "SA" && p.ValidSA) ||
                    (categoryCode == "SAT" && p.ValidSAT) ||
                    (categoryCode == "NS" && p.ValidNS) ||
                    (categoryCode == "NSCTC" && p.ValidNSCTC) ||
                    (categoryCode == "XQ" && p.ValidXQ) ||
                    (categoryCode == "DDT" && p.ValidDDT) ||
                    (categoryCode == "TDCN" && p.ValidTDCN)
                );

                // ✅ Tối ưu: Lấy danh sách PatientId có completed trước
                var patientIdsWithCompleted = await _db.ResultCDHAs
                    .AsNoTracking()
                    .Where(r => r.Active
                             && r.Service.Category.Code == categoryCode
                             && r.Result != null
                             && r.Result != "")
                    .Select(r => r.PatientId)
                    .Distinct()
                    .ToListAsync();

                q = q.Where(p => patientIdsWithCompleted.Contains(p.Id));
            }

            // Sắp xếp theo mốc trả kết quả của nhóm
            q = categoryCode switch
            {
                "SA" => q.OrderByDescending(p => p.ReturnResultTimeSA),
                "SAT" => q.OrderByDescending(p => p.ReturnResultTimeSAT),
                "NS" => q.OrderByDescending(p => p.ReturnResultTimeNS),
                "NSCTC" => q.OrderByDescending(p => p.ReturnResultTimeNSCTC),
                "XQ" => q.OrderByDescending(p => p.ReturnResultTimeXQ),
                "DDT" => q.OrderByDescending(p => p.ReturnResultTimeDDT),
                "TDCN" => q.OrderByDescending(p => p.ReturnResultTimeTDCN),
                _ => q.OrderByDescending(p => p.InsertTime)
            };

            return await q.ToListAsync();
        }

        public async Task<int> Get_CountPatient(DateTime fromDate, DateTime toDate, bool wait, bool process, bool valid, string categoryCode)
        {
            if (categoryCode == "SA")
            {
                if (wait)
                    return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate && p.WaitSA == wait).CountAsync();
                else
                    return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate && p.WaitSA == wait && p.ProcessSA == process && p.ValidSA == valid).CountAsync();
            }
            else if (categoryCode == "SAT")
            {
                if (wait)
                    return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate && p.WaitSAT == wait).CountAsync();
                else
                    return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate && p.WaitSAT == wait && p.ProcessSAT == process && p.ValidSAT == valid).CountAsync();
            }
            else if (categoryCode == "DDT")
            {
                if (wait)
                    return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate && p.WaitDDT == wait).CountAsync();
                else
                    return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate && p.WaitDDT == wait && p.ProcessDDT == process && p.ValidDDT == valid).CountAsync();
            }
            else if (categoryCode == "TDCN")
            {
                if (wait)
                    return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate && p.WaitTDCN == wait).CountAsync();
                else
                    return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate && p.WaitTDCN == wait && p.ProcessTDCN == process && p.ValidTDCN == valid).CountAsync();
            }
            else if (categoryCode == "NS")
            {
                if (wait)
                    return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate && p.WaitNS == wait).CountAsync();
                else
                    return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate && p.WaitNS == wait && p.ProcessNS == process && p.ValidNS == valid).CountAsync();
            }
            else if (categoryCode == "NSCTC")
            {
                if (wait)
                    return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate && p.WaitNSCTC == wait).CountAsync();
                else
                    return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate && p.WaitNSCTC == wait && p.ProcessNSCTC == process && p.ValidNSCTC == valid).CountAsync();
            }
            else if (categoryCode == "XQ")
            {
                if (wait)
                    return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate && p.WaitXQ == wait).CountAsync();
                else
                    return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate && p.WaitXQ == wait && p.ProcessXQ == process && p.ValidXQ == valid).CountAsync();
            }
            return 0;
        }

        public async Task<int> Get_CountPatient_New(DateTime fromDate, DateTime toDate, bool wait, bool process, bool valid, string categoryCode)
        {
            var baseQ = _db.Patients
                .AsNoTracking() // ✅ Thêm AsNoTracking cho count
                .Where(p =>
                    p.Active == true &&
                    p.InsertTime > fromDate && p.InsertTime <= toDate);

            // PREFILTER theo nhóm: đã từng Wait/Process/Valid của nhóm đó
            baseQ = ApplyCategoryTouchedFilter(baseQ, categoryCode);

            // 1) Tab CHỜ (giữ nguyên hành vi cũ: đếm theo cờ Wait* ở Patient)
            if (wait)
            {
                return categoryCode switch
                {
                    "SA" => await baseQ.CountAsync(p => p.WaitSA),
                    "SAT" => await baseQ.CountAsync(p => p.WaitSAT),
                    "DDT" => await baseQ.CountAsync(p => p.WaitDDT),
                    "TDCN" => await baseQ.CountAsync(p => p.WaitTDCN),
                    "NS" => await baseQ.CountAsync(p => p.WaitNS),
                    "NSCTC" => await baseQ.CountAsync(p => p.WaitNSCTC),
                    "XQ" => await baseQ.CountAsync(p => p.WaitXQ),
                    _ => 0
                };
            }

            // 2) Tab ĐANG THỰC HIỆN (PROCESS):
            if (process && !valid)
            {
                baseQ = ExcludeWait(baseQ, categoryCode);

                // ✅ Tối ưu: Lấy danh sách PatientId có pending trước
                var patientIdsWithPending = await _db.ResultCDHAs
                    .AsNoTracking()
                    .Where(r => r.Active
                             && r.Service.Category.Code == categoryCode
                             && (r.Result == null || r.Result == ""))
                    .Select(r => r.PatientId)
                    .Distinct()
                    .ToListAsync();

                // ✅ Đếm với điều kiện đơn giản hơn
                return await baseQ.CountAsync(p =>
                    ((categoryCode == "SA" && p.ProcessSA) ||
                     (categoryCode == "SAT" && p.ProcessSAT) ||
                     (categoryCode == "NS" && p.ProcessNS) ||
                     (categoryCode == "NSCTC" && p.ProcessNSCTC) ||
                     (categoryCode == "XQ" && p.ProcessXQ) ||
                     (categoryCode == "DDT" && p.ProcessDDT) ||
                     (categoryCode == "TDCN" && p.ProcessTDCN))
                    ||
                    patientIdsWithPending.Contains(p.Id)
                );
            }

            // 3) Tab ĐÃ THỰC HIỆN (VALID):
            if (valid)
            {
                baseQ = baseQ.Where(p =>
                    (categoryCode == "SA" && p.ValidSA) ||
                    (categoryCode == "SAT" && p.ValidSAT) ||
                    (categoryCode == "NS" && p.ValidNS) ||
                    (categoryCode == "NSCTC" && p.ValidNSCTC) ||
                    (categoryCode == "XQ" && p.ValidXQ) ||
                    (categoryCode == "DDT" && p.ValidDDT) ||
                    (categoryCode == "TDCN" && p.ValidTDCN)
                );

                // ✅ Tối ưu: Lấy danh sách PatientId có completed trước
                var patientIdsWithCompleted = await _db.ResultCDHAs
                    .AsNoTracking()
                    .Where(r => r.Active
                             && r.Service.Category.Code == categoryCode
                             && r.Result != null
                             && r.Result != "")
                    .Select(r => r.PatientId)
                    .Distinct()
                    .ToListAsync();

                return await baseQ.CountAsync(p => patientIdsWithCompleted.Contains(p.Id));
            }

            return 0;
        }

        public async Task<List<Patient>> Get_ListPatientByPidOrSid(DateTime fromDate, DateTime toDate, bool wait, bool process, bool valid, string pidorseq, string categoryCode)
        {
            if (string.IsNullOrEmpty(pidorseq))
            {
                if (categoryCode == "SA")
                {
                    return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate &&
                                        p.WaitSA == wait && p.ProcessSA == process && p.ValidSA == valid).OrderByDescending(p => p.ReturnResultTimeSA).ToListAsync();
                }
                else if (categoryCode == "SAT")
                {
                    return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate &&
                                        p.WaitSAT == wait && p.ProcessSAT == process && p.ValidSAT == valid).OrderByDescending(p => p.ReturnResultTimeSAT).ToListAsync();
                }
                else if (categoryCode == "DDT")
                {
                    return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate &&
                                        p.WaitDDT == wait && p.ProcessDDT == process && p.ValidDDT == valid).OrderByDescending(p => p.ReturnResultTimeDDT).ToListAsync();
                }
                else if (categoryCode == "TDCN")
                {
                    return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate &&
                                        p.WaitTDCN == wait && p.ProcessTDCN == process && p.ValidTDCN == valid).OrderByDescending(p => p.ReturnResultTimeTDCN).ToListAsync();
                }
                else if (categoryCode == "NS")
                {
                    return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate &&
                                        p.WaitNS == wait && p.ProcessNS == process && p.ValidNS == valid).OrderByDescending(p => p.ReturnResultTimeNS).ToListAsync();
                }
                else if (categoryCode == "NSCTC")
                {
                    return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate &&
                                        p.WaitNSCTC == wait && p.ProcessNSCTC == process && p.ValidNSCTC == valid).OrderByDescending(p => p.ReturnResultTimeNSCTC).ToListAsync();
                }
                else if (categoryCode == "XQ")
                {
                    return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate &&
                                      p.WaitXQ == wait && p.ProcessXQ == process && p.ValidXQ == valid).OrderByDescending(p => p.ReturnResultTimeXQ).ToListAsync();
                }
            }
            else
            {
                if (categoryCode == "SA")
                {
                    return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate &&
                                       p.WaitSA == wait && p.ProcessSA == process && p.ValidSA == valid && (p.PatientId.Contains(pidorseq) || p.Seq.Contains(pidorseq) || p.MaBenhAn.Contains(pidorseq) || p.PatientName.Contains(pidorseq))).OrderByDescending(p => p.ReturnResultTimeSA).ToListAsync();
                }
                else if (categoryCode == "SAT")
                {
                    return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate &&
                                       p.WaitSAT == wait && p.ProcessSAT == process && p.ValidSAT == valid && (p.PatientId.Contains(pidorseq) || p.Seq.Contains(pidorseq) || p.MaBenhAn.Contains(pidorseq) || p.PatientName.Contains(pidorseq))).OrderByDescending(p => p.ReturnResultTimeSAT).ToListAsync();
                }
                else if (categoryCode == "DDT")
                {
                    return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate &&
                                       p.WaitDDT == wait && p.ProcessDDT == process && p.ValidDDT == valid && (p.PatientId.Contains(pidorseq) || p.Seq.Contains(pidorseq) || p.MaBenhAn.Contains(pidorseq) || p.PatientName.Contains(pidorseq))).OrderByDescending(p => p.ReturnResultTimeDDT).ToListAsync();
                }
                else if (categoryCode == "TDCN")
                {
                    return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate &&
                                       p.WaitTDCN == wait && p.ProcessTDCN == process && p.ValidTDCN == valid && (p.PatientId.Contains(pidorseq) || p.Seq.Contains(pidorseq) || p.MaBenhAn.Contains(pidorseq) || p.PatientName.Contains(pidorseq))).OrderByDescending(p => p.ReturnResultTimeTDCN).ToListAsync();
                }
                else if (categoryCode == "NS")
                {
                    return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate &&
                                      p.WaitNS == wait && p.ProcessNS == process && p.ValidNS == valid && (p.PatientId.Contains(pidorseq) || p.Seq.Contains(pidorseq) || p.MaBenhAn.Contains(pidorseq) || p.PatientName.Contains(pidorseq))).OrderByDescending(p => p.ReturnResultTimeNS).ToListAsync();
                }
                else if (categoryCode == "NSCTC")
                {
                    return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate &&
                                      p.WaitNSCTC == wait && p.ProcessNSCTC == process && p.ValidNSCTC == valid && (p.PatientId.Contains(pidorseq) || p.Seq.Contains(pidorseq) || p.MaBenhAn.Contains(pidorseq) || p.PatientName.Contains(pidorseq))).OrderByDescending(p => p.ReturnResultTimeNSCTC).ToListAsync();
                }
                else if (categoryCode == "XQ")
                {
                    return await _db.Patients.Where(p => p.Active == true && p.InsertTime > fromDate && p.InsertTime <= toDate &&
                                      p.WaitXQ == wait && p.ProcessXQ == process && p.ValidXQ == valid && (p.PatientId.Contains(pidorseq) || p.Seq.Contains(pidorseq) || p.MaBenhAn.Contains(pidorseq) || p.PatientName.Contains(pidorseq))).OrderByDescending(p => p.ReturnResultTimeXQ).ToListAsync();
                }
            }
            return null;
        }

        public async Task<List<Patient>> Get_ListPatientByPidOrSid_New(DateTime fromDate, DateTime toDate, bool wait, bool process, bool valid, string pidorseq, string categoryCode)
        {
            // Base query: theo ngày + Active
            var q = _db.Patients.Where(p =>
                p.Active == true &&
                p.InsertTime > fromDate && p.InsertTime <= toDate);
            q = ApplyCategoryTouchedFilter(q, categoryCode);

            // Tìm kiếm theo PID/SEQ/Mã bệnh án/Tên (nếu có nhập)
            if (!string.IsNullOrEmpty(pidorseq))
            {
                q = q.Where(p =>
                    (p.PatientId != null && p.PatientId.Contains(pidorseq)) ||
                    (p.Seq != null && p.Seq.Contains(pidorseq)) ||
                    (p.MaBenhAn != null && p.MaBenhAn.Contains(pidorseq)) ||
                    (p.PatientName != null && p.PatientName.Contains(pidorseq)));
            }

            // Lọc theo tab
            if (wait)
            {
                if (categoryCode == "SA") q = q.Where(p => p.WaitSA);
                else if (categoryCode == "SAT") q = q.Where(p => p.WaitSAT);
                else if (categoryCode == "DDT") q = q.Where(p => p.WaitDDT);
                else if (categoryCode == "TDCN") q = q.Where(p => p.WaitTDCN);
                else if (categoryCode == "NS") q = q.Where(p => p.WaitNS);
                else if (categoryCode == "NSCTC") q = q.Where(p => p.WaitNSCTC);
                else if (categoryCode == "XQ") q = q.Where(p => p.WaitXQ);
            }
            else if (process && !valid)
            {
                q = ExcludeWait(q, categoryCode);
                q = q.Where(p =>
               // Process* đang bật theo category
               ((categoryCode == "SA" && p.ProcessSA) ||
                (categoryCode == "SAT" && p.ProcessSAT) ||
                (categoryCode == "NS" && p.ProcessNS) ||
                (categoryCode == "NSCTC" && p.ProcessNSCTC) ||
                (categoryCode == "XQ" && p.ProcessXQ) ||
                (categoryCode == "DDT" && p.ProcessDDT) ||
                (categoryCode == "TDCN" && p.ProcessTDCN))
               // HOẶC vẫn còn ÍT NHẤT 1 dịch vụ pending (Result null/empty)
               || _db.ResultCDHAs.Any(r =>
                      r.Active &&
                      r.PatientId == p.Id &&
                      r.Service.Category.Code == categoryCode &&
                      (r.Result == null || r.Result == "")));
                // ĐANG THỰC HIỆN: có ÍT NHẤT 1 dịch vụ pending trong ResultCDHA (Result null/rỗng)
                //q = q.Where(p => _db.ResultCDHAs.Any(r =>
                //    r.Active == true &&
                //    r.PatientId == p.Id &&
                //    r.Service.Category.Code == categoryCode &&   // lọc đúng nhóm SA/SAT/NS/XQ/DDT/TDCN
                //    (r.Result == null || r.Result == "")));
            }
            else if (valid)
            {
                q = q.Where(p =>
                    (categoryCode == "SA" && p.ValidSA) ||
                    (categoryCode == "SAT" && p.ValidSAT) ||
                    (categoryCode == "NS" && p.ValidNS) ||
                    (categoryCode == "NSCTC" && p.ValidNSCTC) ||
                    (categoryCode == "XQ" && p.ValidXQ) ||
                    (categoryCode == "DDT" && p.ValidDDT) ||
                    (categoryCode == "TDCN" && p.ValidTDCN)
                );
                // ĐÃ THỰC HIỆN: có ÍT NHẤT 1 dịch vụ completed trong ResultCDHA (Result có dữ liệu)
                q = q.Where(p => _db.ResultCDHAs.Any(r =>
                    r.Active == true &&
                    r.PatientId == p.Id &&
                    r.Service.Category.Code == categoryCode &&
                    r.Result != null && r.Result != ""
                // && r.SignStatus == 1   // mở nếu muốn chỉ tính khi đã ký
                ));
            }

            // Sắp xếp theo mốc trả kết quả của nhóm (giữ thói quen cũ)
            if (categoryCode == "SA") q = q.OrderByDescending(p => p.ReturnResultTimeSA);
            else if (categoryCode == "SAT") q = q.OrderByDescending(p => p.ReturnResultTimeSAT);
            else if (categoryCode == "NS") q = q.OrderByDescending(p => p.ReturnResultTimeNS);
            else if (categoryCode == "NSCTC") q = q.OrderByDescending(p => p.ReturnResultTimeNSCTC);
            else if (categoryCode == "XQ") q = q.OrderByDescending(p => p.ReturnResultTimeXQ);
            else if (categoryCode == "DDT") q = q.OrderByDescending(p => p.ReturnResultTimeDDT);
            else if (categoryCode == "TDCN") q = q.OrderByDescending(p => p.ReturnResultTimeTDCN);

            return await q.ToListAsync();
        }

        public async Task<Patient> Get_PatientBySid(long id)
        {
            return await _db.Patients.Where(p => p.Active == true && p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<bool> DeleteById(long id, string group)
        {
            try
            {
                var _resultXN = await _db.ResultXNs.Where(p => p.PatientId == id && p.Active == true).FirstOrDefaultAsync();
                var _resultCDHA = await _db.ResultCDHAs.Where(p => p.PatientId == id && p.Active == true).FirstOrDefaultAsync();
                if (_resultXN == null && _resultCDHA == null)
                {
                    var _patient = await _db.Patients.Where(p => p.Id == id).FirstOrDefaultAsync();
                    if (_patient != null)
                    {
                        _patient.Active = false;
                        await _db.SaveChangesAsync();
                        return true;
                    }
                }
                else
                {
                    var _patient = await _db.Patients.Where(p => p.Id == id).FirstOrDefaultAsync();
                    if (_patient != null)
                    {
                        if (group == "SA")
                        {
                            _patient.WaitSA = false;
                            _patient.ProcessSA = false;
                            _patient.ValidSA = false;
                        }
                        else if (group == "SAT")
                        {
                            _patient.WaitSAT = false;
                            _patient.ProcessSAT = false;
                            _patient.ValidSAT = false;
                        }
                        else if (group == "DDT")
                        {
                            _patient.WaitDDT = false;
                            _patient.ProcessDDT = false;
                            _patient.ValidDDT = false;
                        }
                        else if (group == "TDCN")
                        {
                            _patient.WaitTDCN = false;
                            _patient.ProcessTDCN = false;
                            _patient.ValidTDCN = false;
                        }
                        else if (group == "NS")
                        {
                            _patient.WaitNS = false;
                            _patient.ProcessNS = false;
                            _patient.ValidNS = false;
                        }
                        else if (group == "NSCTC")
                        {
                            _patient.WaitNSCTC = false;
                            _patient.ProcessNSCTC = false;
                            _patient.ValidNSCTC = false;
                        }
                        else if (group == "XQ")
                        {
                            _patient.WaitXQ = false;
                            _patient.ProcessXQ = false;
                            _patient.ValidXQ = false;
                        }
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

        public async Task<bool> GetSample_ProcessResult_ReturnResult(long id, bool wait, bool process, bool valid, long? userInsertOrUpdate, string group)
        {
            try
            {
                var _patient = await _db.Patients.Where(p => p.Active == true && p.Id == id).FirstOrDefaultAsync();
                if (_patient != null)
                {
                    if (group == "SA")
                    {
                        _patient.WaitSA = wait;
                        _patient.ProcessSA = process;
                        _patient.ValidSA = valid;
                    }
                    else if (group == "SAT")
                    {
                        _patient.WaitSAT = wait;
                        _patient.ProcessSAT = process;
                        _patient.ValidSAT = valid;
                    }
                    else if (group == "DDT")
                    {
                        _patient.WaitDDT = wait;
                        _patient.ProcessDDT = process;
                        _patient.ValidDDT = valid;
                    }
                    else if (group == "TDCN")
                    {
                        _patient.WaitTDCN = wait;
                        _patient.ProcessTDCN = process;
                        _patient.ValidTDCN = valid;
                    }
                    else if (group == "NS")
                    {
                        _patient.WaitNS = wait;
                        _patient.ProcessNS = process;
                        _patient.ValidNS = valid;
                    }
                    else if (group == "NSCTC")
                    {
                        _patient.WaitNSCTC = wait;
                        _patient.ProcessNSCTC = process;
                        _patient.ValidNSCTC = valid;
                    }
                    else if (group == "XQ")
                    {
                        _patient.WaitXQ = wait;
                        _patient.ProcessXQ = process;
                        _patient.ValidXQ = valid;
                    }
                    _patient.UserUpdateId = userInsertOrUpdate;
                    await _db.SaveChangesAsync();

                    if (valid && userInsertOrUpdate.HasValue)
                    {
                        await _resultEditUnlockBL.RevokeAfterValidAsync(
                            id,
                            group,
                            userInsertOrUpdate.Value,
                            ToolBL.Get_DateNow());
                    }
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
                                                long? userInsertIdOrUpdateId, DateTime dateTime, bool wait, bool process, bool valid, string group)
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
                    _patient.Address = address;
                    _patient.Diagnostic = diagnostic;
                    _patient.UserInsertId = userInsertIdOrUpdateId;
                    _patient.InsertTime = dateTime;
                    if (group == "SA")
                    {
                        _patient.WaitSA = wait;
                        _patient.ProcessSA = process;
                        _patient.ValidSA = valid;
                        _patient.GetSampleTimeSA = getSampleTime;
                    }
                    else if (group == "SAT")
                    {
                        _patient.WaitSAT = wait;
                        _patient.ProcessSAT = process;
                        _patient.ValidSAT = valid;
                        _patient.GetSampleTimeSAT = getSampleTime;
                    }
                    else if (group == "DDT")
                    {
                        _patient.WaitDDT = wait;
                        _patient.ProcessDDT = process;
                        _patient.ValidDDT = valid;
                        _patient.GetSampleTimeDDT = getSampleTime;
                    }
                    else if (group == "TDCN")
                    {
                        _patient.WaitTDCN = wait;
                        _patient.ProcessTDCN = process;
                        _patient.ValidTDCN = valid;
                        _patient.GetSampleTimeTDCN = getSampleTime;
                    }
                    else if (group == "NS")
                    {
                        _patient.WaitNS = wait;
                        _patient.ProcessNS = process;
                        _patient.ValidNS = valid;
                        _patient.GetSampleTimeNS = getSampleTime;
                    }
                    else if (group == "NSCTC")
                    {
                        _patient.WaitNSCTC = wait;
                        _patient.ProcessNSCTC = process;
                        _patient.ValidNSCTC = valid;
                        _patient.GetSampleTimeNSCTC = getSampleTime;
                    }
                    else if (group == "XQ")
                    {
                        _patient.WaitXQ = wait;
                        _patient.ProcessXQ = process;
                        _patient.ValidXQ = valid;
                        _patient.GetSampleTimeXQ = getSampleTime;
                    }
                    _patient.Active = true;
                    await _db.Patients.AddAsync(_patient);
                    await _db.SaveChangesAsync();
                    return _patient;
                }
                else
                {
                    var _patient = await _db.Patients.Where(p => p.Active == true && p.Id == id).FirstOrDefaultAsync();
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
                        _patient.Address = address;
                        _patient.Diagnostic = diagnostic;
                        _patient.UserUpdateId = userInsertIdOrUpdateId;
                        _patient.UpdateTime = dateTime;
                        if (group == "SA")
                        {
                            _patient.WaitSA = wait;
                            _patient.ProcessSA = process;
                            _patient.ValidSA = valid;
                            _patient.GetSampleTimeSA = getSampleTime;
                        }
                        else if (group == "SAT")
                        {
                            _patient.WaitSAT = wait;
                            _patient.ProcessSAT = process;
                            _patient.ValidSAT = valid;
                            _patient.GetSampleTimeSAT = getSampleTime;
                        }
                        else if (group == "DDT")
                        {
                            _patient.WaitDDT = wait;
                            _patient.ProcessDDT = process;
                            _patient.ValidDDT = valid;
                            _patient.GetSampleTimeDDT = getSampleTime;
                        }
                        else if (group == "TDCN")
                        {
                            _patient.WaitTDCN = wait;
                            _patient.ProcessTDCN = process;
                            _patient.ValidTDCN = valid;
                            _patient.GetSampleTimeTDCN = getSampleTime;
                        }
                        else if (group == "NS")
                        {
                            _patient.WaitNS = wait;
                            _patient.ProcessNS = process;
                            _patient.ValidNS = valid;
                            _patient.GetSampleTimeNS = getSampleTime;
                        }
                        else if (group == "NSCTC")
                        {
                            _patient.WaitNSCTC = wait;
                            _patient.ProcessNSCTC = process;
                            _patient.ValidNSCTC = valid;
                            _patient.GetSampleTimeNSCTC = getSampleTime;
                        }
                        else if (group == "XQ")
                        {
                            _patient.WaitXQ = wait;
                            _patient.ProcessXQ = process;
                            _patient.ValidXQ = valid;
                            _patient.GetSampleTimeXQ = getSampleTime;
                        }
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

        public async Task Update(long id, DateTime returnResultTime, long userReturnResult, long? userInserOrUpdate, DateTime dateTime, string group)
        {
            try
            {
                var _patient = await _db.Patients.Where(p => p.Active == true && p.Id == id).FirstOrDefaultAsync();
                if (_patient != null)
                {
                    if (group == "SA")
                    {
                        //_patient.ReturnResultTimeSA = returnResultTime;
                        //_patient.UserReturnResultSA = userReturnResult;
                    }
                    else if (group == "SAT")
                    {
                        //_patient.ReturnResultTimeSAT = returnResultTime;
                        //_patient.UserReturnResultSAT = userReturnResult;
                    }
                    else if (group == "DDT")
                    {
                        //_patient.ReturnResultTimeDDT = returnResultTime;
                        //_patient.UserReturnResultDDT = userReturnResult;
                    }
                    else if (group == "TDCN")
                    {
                        //_patient.ReturnResultTimeTDCN = returnResultTime;
                        //_patient.UserReturnResultTDCN = userReturnResult;
                    }
                    else if (group == "NS")
                    {
                        //_patient.ReturnResultTimeNS = returnResultTime;
                        //_patient.UserReturnResultNS = userReturnResult;
                    }
                    else if (group == "NSCTC")
                    {
                        //_patient.ReturnResultTimeNSCTC = returnResultTime;
                        //_patient.UserReturnResultNSCTC = userReturnResult;
                    }
                    else if (group == "XQ")
                    {
                        //_patient.ReturnResultTimeXQ = returnResultTime;
                        //_patient.UserReturnResultXQ = userReturnResult;
                    }
                    _patient.UserUpdateId = userInserOrUpdate;
                    _patient.UpdateTime = dateTime;
                    await _db.SaveChangesAsync();
                }
            }
            catch { }
        }

        public async Task<bool> Update(long id, DateTime returnResultTime, long userReturnResult, bool wait, bool process, bool valid, long? userInsertOrUpdate, string group)
        {
            try
            {
                var _patient = await _db.Patients.Where(p => p.Active == true && p.Id == id).FirstOrDefaultAsync();
                if (_patient != null)
                {
                    if (group == "SA")
                    {
                        _patient.ReturnResultTimeSA = returnResultTime;
                        _patient.UserReturnResultSA = userReturnResult;
                        _patient.WaitSA = wait;
                        _patient.ProcessSA = process;
                        _patient.ValidSA = valid;
                    }
                    else if (group == "SAT")
                    {
                        _patient.ReturnResultTimeSAT = returnResultTime;
                        _patient.UserReturnResultSAT = userReturnResult;
                        _patient.WaitSAT = wait;
                        _patient.ProcessSAT = process;
                        _patient.ValidSAT = valid;
                    }
                    else if (group == "DDT")
                    {
                        _patient.ReturnResultTimeDDT = returnResultTime;
                        _patient.UserReturnResultDDT = userReturnResult;
                        _patient.WaitDDT = wait;
                        _patient.ProcessDDT = process;
                        _patient.ValidDDT = valid;
                    }
                    else if (group == "TDCN")
                    {
                        _patient.ReturnResultTimeTDCN = returnResultTime;
                        _patient.UserReturnResultTDCN = userReturnResult;
                        _patient.WaitTDCN = wait;
                        _patient.ProcessTDCN = process;
                        _patient.ValidTDCN = valid;
                    }
                    else if (group == "NS")
                    {
                        _patient.ReturnResultTimeNS = returnResultTime;
                        _patient.UserReturnResultNS = userReturnResult;
                        _patient.WaitNS = wait;
                        _patient.ProcessNS = process;
                        _patient.ValidNS = valid;
                    }
                    else if (group == "NSCTC")
                    {
                        _patient.ReturnResultTimeNSCTC = returnResultTime;
                        _patient.UserReturnResultNSCTC = userReturnResult;
                        _patient.WaitNSCTC = wait;
                        _patient.ProcessNSCTC = process;
                        _patient.ValidNSCTC = valid;
                    }
                    else if (group == "XQ")
                    {
                        _patient.ReturnResultTimeXQ = returnResultTime;
                        _patient.UserReturnResultXQ = userReturnResult;
                        _patient.WaitXQ = wait;
                        _patient.ProcessXQ = process;
                        _patient.ValidXQ = valid;
                    }
                    _patient.UserUpdateId = userInsertOrUpdate;
                    await _db.SaveChangesAsync();

                    if (valid && userInsertOrUpdate.HasValue)
                    {
                        await _resultEditUnlockBL.RevokeAfterValidAsync(
                            id,
                            group,
                            userInsertOrUpdate.Value,
                            ToolBL.Get_DateNow());
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return false;
        }

        public async Task<Patient> GetPatientByMaBenhAn(string maBenhAn)
        {
            try
            {
                var dateTimeNow = ToolBL.Get_DateNow();
                var from = dateTimeNow.AddYears(-2); // Tìm trong 2 năm gần nhất
                var to = dateTimeNow;

                // Tìm bệnh nhân theo MaBenhAn
                var patient = await _db.Patients
                    .Where(p => p.Active == true &&
                               p.MaBenhAn == maBenhAn &&
                               p.InsertTime >= from &&
                               p.InsertTime <= to)
                    .OrderByDescending(p => p.InsertTime)
                    .FirstOrDefaultAsync();

                return patient;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private IQueryable<Patient> ApplyCategoryTouchedFilter(IQueryable<Patient> q, string categoryCode)
        {
            switch (categoryCode)
            {
                case "SA":
                    return q.Where(p => p.WaitSA || p.ProcessSA || p.ValidSA);
                case "SAT":
                    return q.Where(p => p.WaitSAT || p.ProcessSAT || p.ValidSAT);
                case "NS":
                    return q.Where(p => p.WaitNS || p.ProcessNS || p.ValidNS);
                case "NSCTC":
                    return q.Where(p => p.WaitNSCTC || p.ProcessNSCTC || p.ValidNSCTC);
                case "XQ":
                    return q.Where(p => p.WaitXQ || p.ProcessXQ || p.ValidXQ);
                case "DDT":
                    return q.Where(p => p.WaitDDT || p.ProcessDDT || p.ValidDDT);
                case "TDCN":
                    return q.Where(p => p.WaitTDCN || p.ProcessTDCN || p.ValidTDCN);
                default:
                    return q; // không biết nhóm thì giữ nguyên
            }
        }

        private IQueryable<Patient> ExcludeWait(IQueryable<Patient> q, string categoryCode)
        {
            return categoryCode switch
            {
                "SA" => q.Where(p => !p.WaitSA),
                "SAT" => q.Where(p => !p.WaitSAT),
                "DDT" => q.Where(p => !p.WaitDDT),
                "TDCN" => q.Where(p => !p.WaitTDCN),
                "NS" => q.Where(p => !p.WaitNS),
                "NSCTC" => q.Where(p => !p.WaitNSCTC),
                "XQ" => q.Where(p => !p.WaitXQ),
                _ => q
            };
        }

        public bool Remove_Result_PDF(List<string> keyResultForHisList, string categoryCode)
        {
            try
            {
                if (keyResultForHisList == null || !keyResultForHisList.Any() || string.IsNullOrEmpty(categoryCode))
                    return false;

                // Chuyển đổi category code sang tên thư mục tương ứng
                var folderName = GetPdfFolderName(categoryCode);
                if (string.IsNullOrEmpty(folderName))
                    return false;

                bool hasDeletedAny = false;

                // Lặp qua từng KeyResultForHis để xóa file tương ứng
                foreach (var keyResultForHis in keyResultForHisList)
                {
                    if (string.IsNullOrEmpty(keyResultForHis))
                        continue;

                    // Đường dẫn đến file PDF: wwwroot/pdf/{category}/{keyResultForHis}.pdf
                    var pdfPath = Path.Combine("wwwroot", "pdf", folderName, $"{keyResultForHis}.pdf");

                    // Kiểm tra file có tồn tại không
                    if (File.Exists(pdfPath))
                    {
                        // Xóa file
                        File.Delete(pdfPath);
                        hasDeletedAny = true;
                    }
                }

                return hasDeletedAny;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private string GetPdfFolderName(string categoryCode)
        {
            return categoryCode switch
            {
                "SA" => "sa",
                "SAT" => "sat",
                "DDT" => "ddt",
                "TDCN" => "tdcn",
                "NS" => "ns",
                "NSCTC" => "nsctc",
                "XQ" => "xq",
                "XN" => "xn",
                _ => null
            };
        }
    }
}
