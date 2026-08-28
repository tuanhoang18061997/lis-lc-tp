using Management.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Reflection;
using ZXing;
using static iTextSharp.text.pdf.AcroFields;
using static Management.BL.UserModel;
using Object = Management.Models.Object;
using ClosedXML.Excel;

namespace Management.BL
{
    public class ReportBL
    {
        private readonly LABContext _db;
        public ReportBL(LABContext db)
        {
            _db = db;
        }

        public byte[] ExportPatientServicesByDateToExcel(XNServiceByDateReport model)
        {
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("BaoCaoXN");

                int currentRow = 1;

                // Title
                ws.Cell(currentRow, 1).Value = $"BÁO CÁO XÉT NGHIỆM";
                ws.Range(currentRow, 1, currentRow, 9).Merge().Style
                    .Font.SetBold()
                    .Font.SetFontSize(16)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                currentRow++;

                ws.Cell(currentRow, 1).Value =
                    $"Từ ngày {model.FromDate:dd/MM/yyyy} đến {model.ToDate:dd/MM/yyyy}";
                ws.Range(currentRow, 1, currentRow, 9).Merge().Style
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                currentRow += 2;

                foreach (var group in model.DateGroups)
                {
                    // ===== HEADER NGÀY =====
                    ws.Cell(currentRow, 1).Value =
                        $"NGÀY THỰC HIỆN: {group.Date:dd/MM/yyyy}";

                    ws.Range(currentRow, 1, currentRow, 9).Merge().Style
                        .Font.SetBold()
                        .Fill.SetBackgroundColor(XLColor.LightGray);

                    currentRow++;

                    // ===== HEADER TABLE =====
                    ws.Cell(currentRow, 1).Value = "NGÀY THỰC HIỆN";
                    ws.Cell(currentRow, 2).Value = "STT";
                    ws.Cell(currentRow, 3).Value = "HỌ TÊN";
                    ws.Cell(currentRow, 4).Value = "GIỚI TÍNH";
                    ws.Cell(currentRow, 5).Value = "TUỔI";
                    ws.Cell(currentRow, 6).Value = "ĐỐI TƯỢNG";
                    ws.Cell(currentRow, 7).Value = "ĐỊA CHỈ";
                    ws.Cell(currentRow, 8).Value = "YÊU CẦU";
                    ws.Cell(currentRow, 9).Value = "NGƯỜI ĐỌC";

                    ws.Range(currentRow, 1, currentRow, 9).Style
                        .Font.SetBold()
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                        .Fill.SetBackgroundColor(XLColor.LightGray);

                    currentRow++;

                    // ===== DATA =====
                    foreach (var row in group.Rows)
                    {
                        ws.Cell(currentRow, 1).Value = row.Date.ToString("yyyy-MM-dd");
                        ws.Cell(currentRow, 2).Value = row.STT;
                        ws.Cell(currentRow, 3).Value = row.PatientName;
                        ws.Cell(currentRow, 4).Value = row.Gender;
                        ws.Cell(currentRow, 5).Value = row.AgeValue;
                        ws.Cell(currentRow, 6).Value = row.ObjectName;
                        ws.Cell(currentRow, 7).Value = row.Address;
                        ws.Cell(currentRow, 8).Value = row.ServiceName;
                        ws.Cell(currentRow, 9).Value = row.ReaderName;

                        currentRow++;
                    }

                    // ===== TOTAL =====
                    ws.Cell(currentRow, 1).Value = $"TỔNG: {group.Total}";
                    ws.Range(currentRow, 1, currentRow, 9).Merge().Style
                        .Font.SetBold()
                        .Fill.SetBackgroundColor(XLColor.LightGray);

                    currentRow += 2;
                }

                // ===== FORMAT CHUNG =====
                ws.Columns().AdjustToContents();

                // Border
                var usedRange = ws.RangeUsed();
                if (usedRange != null)
                {
                    usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        public async Task<List<TotalReportService>> GetReportByService(DateTime fromtime, DateTime totime, string location, string user, long? hospitalId)
        {
            try
            {
                var fromTimeString = fromtime.AddDays(1).ToString("dd/MM/yyyy");
                var toTimeString = totime.ToString("dd/MM/yyyy");
                var dateNow = DateTime.Now.ToString("dd/MM/yyyy");
                var lstTotalReportService = new List<TotalReportService>();
                var lstReportService = new List<ReportService>();

                if (location == "XN")
                {
                    if (hospitalId == null)
                    {
                        lstReportService = (from patient in _db.Patients
                                            join result in _db.ResultXNs on patient.Id equals result.PatientId
                                            where patient.InsertTime > fromtime && patient.InsertTime <= totime && (patient.ValidXN == true || patient.ProcessXN == true) && result.Active == true
                                            select new ReportService
                                            {
                                                SID = result.Patient.Sid,
                                                ServiceID = result.ServiceId.ToString(),
                                                ServiceName = result.Service.Name,
                                                BenhAn = patient.BenhAn, // in => Nội trú, out => Ngoại trú
                                                ObjectCode = patient.Object.Code // 01 => Thu phí, 02 => BHYT, 03 => BHBL, 04 => DV
                                            }).Distinct().ToList();
                    }
                    else
                    {
                        lstReportService = (from patient in _db.Patients
                                            join result in _db.ResultXNs on patient.Id equals result.PatientId
                                            where patient.InsertTime > fromtime && patient.InsertTime <= totime && (patient.ValidXN == true || patient.ProcessXN == true) && result.Active == true && patient.HospitalId == hospitalId
                                            select new ReportService
                                            {
                                                SID = result.Patient.Sid,
                                                ServiceID = result.ServiceId.ToString(),
                                                ServiceName = result.Service.Name,
                                                BenhAn = patient.BenhAn, // in => Nội trú, out => Ngoại trú
                                                ObjectCode = patient.Object.Code // 01 => Thu phí, 02 => BHYT, 03 => BHBL, 04 => DV
                                            }).Distinct().ToList();
                    }


                    lstReportService = (from item in lstReportService
                                        group item by new { item.ServiceID, item.ServiceName, item.BenhAn, item.ObjectCode } into p
                                        select new ReportService
                                        {
                                            ServiceID = p.Key.ServiceID,
                                            ServiceName = p.Key.ServiceName,
                                            BenhAn = p.Key.BenhAn,
                                            ObjectCode = p.Key.ObjectCode,
                                            Count = p.Count()
                                        }).ToList();
                }
                else if (location == "SA")
                {
                    lstReportService = (from patient in _db.Patients
                                        join result in _db.ResultCDHAs on patient.Id equals result.PatientId
                                        join service in _db.Services on result.ServiceId equals service.Id
                                        where patient.InsertTime > fromtime && patient.InsertTime <= totime && result.Active == true && ((service.Category.Code == "SA" && patient.ValidSA == true) || (service.Category.Code == "SAT" && patient.ValidSAT == true))
                                        select new ReportService
                                        {
                                            SID = result.Patient.Sid,
                                            ServiceID = result.ServiceId.ToString(),
                                            ServiceName = result.Service.Name,
                                            BenhAn = patient.BenhAn, // in => Nội trú, out => Ngoại trú
                                            ObjectCode = patient.Object.Code // 01 => Thu phí, 02 => BHYT, 03 => BHBL, 04 => DV
                                        }).Distinct().ToList();

                    lstReportService = (from item in lstReportService
                                        group item by new { item.ServiceID, item.ServiceName, item.BenhAn, item.ObjectCode } into p
                                        select new ReportService
                                        {
                                            ServiceID = p.Key.ServiceID,
                                            ServiceName = p.Key.ServiceName,
                                            BenhAn = p.Key.BenhAn,
                                            ObjectCode = p.Key.ObjectCode,
                                            Count = p.Count()
                                        }).ToList();
                }
                else if (location == "XQ")
                {
                    lstReportService = (from patient in _db.Patients
                                        join result in _db.ResultCDHAs on patient.Id equals result.PatientId
                                        join service in _db.Services on result.ServiceId equals service.Id
                                        where patient.InsertTime > fromtime && patient.InsertTime <= totime && result.Active == true && service.Category.Code == "XQ" && patient.ValidXQ == true
                                        select new ReportService
                                        {
                                            SID = result.Patient.Sid,
                                            ServiceID = result.ServiceId.ToString(),
                                            ServiceName = result.Service.Name,
                                            BenhAn = patient.BenhAn, // in => Nội trú, out => Ngoại trú
                                            ObjectCode = patient.Object.Code // 01 => Thu phí, 02 => BHYT, 03 => BHBL, 04 => DV
                                        }).Distinct().ToList();

                    lstReportService = (from item in lstReportService
                                        group item by new { item.ServiceID, item.ServiceName, item.BenhAn, item.ObjectCode } into p
                                        select new ReportService
                                        {
                                            ServiceID = p.Key.ServiceID,
                                            ServiceName = p.Key.ServiceName,
                                            BenhAn = p.Key.BenhAn,
                                            ObjectCode = p.Key.ObjectCode,
                                            Count = p.Count()
                                        }).ToList();
                }
                else if (location == "TDCN")
                {
                    lstReportService = (from patient in _db.Patients
                                        join result in _db.ResultCDHAs on patient.Id equals result.PatientId
                                        join service in _db.Services on result.ServiceId equals service.Id
                                        where patient.InsertTime > fromtime
                                              && patient.InsertTime <= totime
                                              && result.Active == true
                                              && service.Category.Code == "TDCN"
                                              && patient.ValidTDCN == true
                                        select new ReportService
                                        {
                                            SID = result.Patient.Sid,
                                            ServiceID = result.ServiceId.ToString(),
                                            ServiceName = result.Service.Name,
                                            BenhAn = patient.BenhAn,
                                            ObjectCode = patient.Object.Code
                                        }).Distinct().ToList();

                    lstReportService = (from item in lstReportService
                                        group item by new
                                        {
                                            item.ServiceID,
                                            item.ServiceName,
                                            item.BenhAn,
                                            item.ObjectCode
                                        } into p
                                        select new ReportService
                                        {
                                            ServiceID = p.Key.ServiceID,
                                            ServiceName = p.Key.ServiceName,
                                            BenhAn = p.Key.BenhAn,
                                            ObjectCode = p.Key.ObjectCode,
                                            Count = p.Count()
                                        }).ToList();
                }
                else if (location == "NS")
                {
                    lstReportService = (from patient in _db.Patients
                                        join result in _db.ResultCDHAs on patient.Id equals result.PatientId
                                        join service in _db.Services on result.ServiceId equals service.Id
                                        where patient.InsertTime > fromtime && patient.InsertTime <= totime && result.Active == true && service.Category.Code == "NS" && patient.ValidNS == true
                                        select new ReportService
                                        {
                                            SID = result.Patient.Sid,
                                            ServiceID = result.ServiceId.ToString(),
                                            ServiceName = result.Service.Name,
                                            BenhAn = patient.BenhAn, // in => Nội trú, out => Ngoại trú
                                            ObjectCode = patient.Object.Code // 01 => Thu phí, 02 => BHYT, 03 => BHBL, 04 => DV
                                        }).Distinct().ToList();

                    lstReportService = (from item in lstReportService
                                        group item by new { item.ServiceID, item.ServiceName, item.BenhAn, item.ObjectCode } into p
                                        select new ReportService
                                        {
                                            ServiceID = p.Key.ServiceID,
                                            ServiceName = p.Key.ServiceName,
                                            BenhAn = p.Key.BenhAn,
                                            ObjectCode = p.Key.ObjectCode,
                                            Count = p.Count()
                                        }).ToList();
                }

                if (lstReportService != null)
                {
                    var stt = 0;
                    foreach (var i in lstReportService)
                    {
                        var itemOld = lstTotalReportService.Where(p => p.ServiceID == i.ServiceID).FirstOrDefault();
                        if (itemOld != null)
                        {
                            if (i.ObjectCode == "BHYT" || i.ObjectCode == "BHBL")
                            {
                                itemOld.TotalBHYT += i.Count;
                                if (i.BenhAn == "in")
                                    itemOld.TotalBHYT_NoiTru += i.Count;
                                else
                                    itemOld.TotalBHYT_NgoaiTru += i.Count;
                            }
                            else
                            {
                                itemOld.TotalTP += i.Count;
                                if (i.BenhAn == "in")
                                    itemOld.TotalTP_NoiTru += i.Count;
                                else
                                    itemOld.TotalTP_NgoaiTru += i.Count;
                            }
                        }
                        else
                        {
                            stt++;
                            var itemNew = new TotalReportService();
                            itemNew.FromDate = fromTimeString;
                            itemNew.ToDate = toTimeString;
                            itemNew.DateNow = dateNow;
                            itemNew.User = user;
                            itemNew.STT = stt;
                            itemNew.ServiceID = i.ServiceID;
                            itemNew.ServiceName = i.ServiceName;
                            if (i.ObjectCode == "BHYT" || i.ObjectCode == "BHBL")
                            {
                                itemNew.TotalBHYT = i.Count;
                                if (i.BenhAn == "in")
                                    itemNew.TotalBHYT_NoiTru = i.Count;
                                else
                                    itemNew.TotalBHYT_NgoaiTru = i.Count;
                            }
                            else
                            {
                                itemNew.TotalTP = i.Count;
                                if (i.BenhAn == "in")
                                    itemNew.TotalTP_NoiTru = i.Count;
                                else
                                    itemNew.TotalTP_NgoaiTru = i.Count;
                            }
                            lstTotalReportService.Add(itemNew);
                        }
                    }
                }

                return lstTotalReportService;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<TotalReportService>> LC_GetReportByService(DateTime fromtime, DateTime totime, string location, string user, string categoryCode = null)
        {
            try
            {
                // Bao trọn ngày cuối: [from, endOfDay]
                var start = fromtime.Date;
                var endExclusive = totime.Date.AddDays(1);

                categoryCode = string.IsNullOrWhiteSpace(categoryCode)
                    ? null
                    : categoryCode.Trim().ToUpperInvariant();

                var fromTimeString = start.ToString("dd/MM/yyyy");
                var toTimeString = totime.Date.ToString("dd/MM/yyyy");
                var dateNow = DateTime.Now.ToString("dd/MM/yyyy");


                var lstTotalReportService = new List<TotalReportService>();
                var lstReportService = new List<ReportService>();

                if (location == "XN")
                {
                    lstReportService = (
                        from patient in _db.Patients
                        join result in _db.ResultXNs
                            on patient.Id equals result.PatientId
                        join service in _db.Services
                            on result.ServiceId equals service.Id
                        where patient.InsertTime >= start
                              && patient.InsertTime < endExclusive
                              && (patient.ValidXN == true || patient.ProcessXN == true)
                              && result.Active == true
                              && patient.Active == true

                              // Không chọn danh mục thì lấy tất cả.
                              // Có chọn thì lọc theo Service.Category.Code.
                              && (
                                    categoryCode == null
                                    || service.Category.Code == categoryCode
                                 )
                        select new ReportService
                        {
                            SID = patient.Sid,
                            ServiceID = result.ServiceId.ToString(),
                            ServiceName = service.Name,
                            BenhAn = patient.BenhAn
                        })
                        .Distinct()
                        .ToList();

                    lstReportService = (
                        from item in lstReportService
                        group item by new
                        {
                            item.ServiceID,
                            item.ServiceName,
                            item.BenhAn
                        }
                        into p
                        select new ReportService
                        {
                            ServiceID = p.Key.ServiceID,
                            ServiceName = p.Key.ServiceName,
                            BenhAn = p.Key.BenhAn,
                            Count = p.Count()
                        })
                        .ToList();
                }
                else if (location == "SA")
                {
                    lstReportService = (from patient in _db.Patients
                                        join result in _db.ResultCDHAs on patient.Id equals result.PatientId
                                        join service in _db.Services on result.ServiceId equals service.Id
                                        where patient.InsertTime > fromtime && patient.InsertTime <= totime && result.Active == true && patient.Active == true && ((service.Category.Code == "SA" && patient.ValidSA == true) || (service.Category.Code == "SAT" && patient.ValidSAT == true))
                                        select new ReportService
                                        {
                                            SID = result.Patient.Sid,
                                            ServiceID = result.ServiceId.ToString(),
                                            ServiceName = result.Service.Name,
                                            BenhAn = patient.BenhAn, // in => Nội trú, out => Ngoại trú
                                            //ObjectCode = patient.Object.Code // 01 => Thu phí, 02 => BHYT, 03 => BHBL, 04 => DV
                                        }).Distinct().ToList();

                    lstReportService = (from item in lstReportService
                                        group item by new { item.ServiceID, item.ServiceName, item.BenhAn } into p
                                        select new ReportService
                                        {
                                            ServiceID = p.Key.ServiceID,
                                            ServiceName = p.Key.ServiceName,
                                            BenhAn = p.Key.BenhAn,
                                            //ObjectCode = p.Key.ObjectCode,
                                            Count = p.Count()
                                        }).ToList();
                }
                else if (location == "XQ")
                {
                    lstReportService = (from patient in _db.Patients
                                        join result in _db.ResultCDHAs on patient.Id equals result.PatientId
                                        join service in _db.Services on result.ServiceId equals service.Id
                                        where patient.InsertTime > fromtime && patient.InsertTime <= totime && result.Active == true && patient.Active == true && service.Category.Code == "XQ" && patient.ValidXQ == true
                                        select new ReportService
                                        {
                                            SID = result.Patient.Sid,
                                            ServiceID = result.ServiceId.ToString(),
                                            ServiceName = result.Service.Name,
                                            BenhAn = patient.BenhAn, // in => Nội trú, out => Ngoại trú
                                            //ObjectCode = patient.Object.Code // 01 => Thu phí, 02 => BHYT, 03 => BHBL, 04 => DV
                                        }).Distinct().ToList();

                    lstReportService = (from item in lstReportService
                                        group item by new { item.ServiceID, item.ServiceName, item.BenhAn } into p
                                        select new ReportService
                                        {
                                            ServiceID = p.Key.ServiceID,
                                            ServiceName = p.Key.ServiceName,
                                            BenhAn = p.Key.BenhAn,
                                            //ObjectCode = p.Key.ObjectCode,
                                            Count = p.Count()
                                        }).ToList();
                }
                else if (location == "DDT")
                {
                    lstReportService = (from patient in _db.Patients
                                        join result in _db.ResultCDHAs on patient.Id equals result.PatientId
                                        join service in _db.Services on result.ServiceId equals service.Id
                                        where patient.InsertTime > fromtime && patient.InsertTime <= totime && result.Active == true && patient.Active == true && service.Category.Code == "DDT" && patient.ValidDDT == true
                                        select new ReportService
                                        {
                                            SID = result.Patient.Sid,
                                            ServiceID = result.ServiceId.ToString(),
                                            ServiceName = result.Service.Name,
                                            BenhAn = patient.BenhAn, // in => Nội trú, out => Ngoại trú
                                            //ObjectCode = patient.Object.Code // 01 => Thu phí, 02 => BHYT, 03 => BHBL, 04 => DV
                                        }).Distinct().ToList();

                    lstReportService = (from item in lstReportService
                                        group item by new { item.ServiceID, item.ServiceName, item.BenhAn } into p
                                        select new ReportService
                                        {
                                            ServiceID = p.Key.ServiceID,
                                            ServiceName = p.Key.ServiceName,
                                            BenhAn = p.Key.BenhAn,
                                            //ObjectCode = p.Key.ObjectCode,
                                            Count = p.Count()
                                        }).ToList();
                }
                else if (location == "TDCN")
                {
                    lstReportService = (from patient in _db.Patients
                                        join result in _db.ResultCDHAs on patient.Id equals result.PatientId
                                        join service in _db.Services on result.ServiceId equals service.Id
                                        where patient.InsertTime > fromtime
                                              && patient.InsertTime <= totime
                                              && result.Active == true
                                              && patient.Active == true
                                              && service.Category.Code == "TDCN"
                                              && patient.ValidTDCN == true
                                        select new ReportService
                                        {
                                            SID = result.Patient.Sid,
                                            ServiceID = result.ServiceId.ToString(),
                                            ServiceName = result.Service.Name,
                                            BenhAn = patient.BenhAn
                                        }).Distinct().ToList();

                    lstReportService = (from item in lstReportService
                                        group item by new
                                        {
                                            item.ServiceID,
                                            item.ServiceName,
                                            item.BenhAn
                                        } into p
                                        select new ReportService
                                        {
                                            ServiceID = p.Key.ServiceID,
                                            ServiceName = p.Key.ServiceName,
                                            BenhAn = p.Key.BenhAn,
                                            Count = p.Count()
                                        }).ToList();
                }
                else if (location == "NS")
                {
                    lstReportService = (from patient in _db.Patients
                                        join result in _db.ResultCDHAs on patient.Id equals result.PatientId
                                        join service in _db.Services on result.ServiceId equals service.Id
                                        where patient.InsertTime > fromtime && patient.InsertTime <= totime && result.Active == true && patient.Active == true && service.Category.Code == "NS" && patient.ValidNS == true
                                        select new ReportService
                                        {
                                            SID = result.Patient.Sid,
                                            ServiceID = result.ServiceId.ToString(),
                                            ServiceName = result.Service.Name,
                                            BenhAn = patient.BenhAn, // in => Nội trú, out => Ngoại trú
                                            //ObjectCode = patient.Object.Code // 01 => Thu phí, 02 => BHYT, 03 => BHBL, 04 => DV
                                        }).Distinct().ToList();

                    lstReportService = (from item in lstReportService
                                        group item by new { item.ServiceID, item.ServiceName, item.BenhAn } into p
                                        select new ReportService
                                        {
                                            ServiceID = p.Key.ServiceID,
                                            ServiceName = p.Key.ServiceName,
                                            BenhAn = p.Key.BenhAn,
                                            //ObjectCode = p.Key.ObjectCode,
                                            Count = p.Count()
                                        }).ToList();
                }

                if (lstReportService != null)
                {
                    var stt = 0;
                    foreach (var i in lstReportService)
                    {
                        var itemOld = lstTotalReportService.Where(p => p.ServiceID == i.ServiceID).FirstOrDefault();
                        if (itemOld != null)
                        {
                            // Tất cả đều là "dịch vụ" => cộng vào TotalTP
                            itemOld.TotalTP += i.Count;

                            if (i.BenhAn == "pakage")
                                itemOld.TotalTP_Pakage += i.Count;  // tái dụng: NoiTru = pakage
                            else
                                itemOld.TotalTP_Out += i.Count; // tái dụng: NgoaiTru = out
                        }
                        else
                        {
                            stt++;
                            var itemNew = new TotalReportService();
                            itemNew.FromDate = fromTimeString;
                            itemNew.ToDate = toTimeString;
                            itemNew.DateNow = dateNow;
                            itemNew.User = user;
                            itemNew.STT = stt;
                            itemNew.ServiceID = i.ServiceID;
                            itemNew.ServiceName = i.ServiceName;
                            // Khởi tạo theo "dịch vụ"
                            itemNew.TotalTP = i.Count;
                            itemNew.TotalTP_Pakage = (i.BenhAn == "pakage") ? i.Count : 0;
                            itemNew.TotalTP_Out = (i.BenhAn == "out") ? i.Count : 0;
                            lstTotalReportService.Add(itemNew);
                        }
                    }
                }

                return lstTotalReportService;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<TotalReportPatient>> GetReportByPatient(DateTime fromTime, DateTime toTime, string location, string user, long? hospitalId)
        {
            try
            {
                var fromTimeString = fromTime.AddDays(1).ToString("dd/MM/yyyy");
                var toTimeString = toTime.ToString("dd/MM/yyyy");
                var dateNow = DateTime.Now.ToString("dd/MM/yyyy");
                var lstTotalReportPatient = new List<TotalReportPatient>();
                var lstReportPatient = new List<ReportPatient>();

                if (location == "XN")
                {
                    if (hospitalId == null)
                    {
                        lstReportPatient = (from patient in _db.Patients
                                            join result in _db.ResultXNs on patient.Id equals result.PatientId
                                            where patient.InsertTime > fromTime && patient.InsertTime <= toTime && patient.ValidXN == true
                                            group patient by new { patient.PatientId, DayInSID = patient.Sid.Substring(0, 6), ObjectCode = patient.Object.Code, patient.BenhAn } into p
                                            select new ReportPatient
                                            {
                                                PatientID = p.Key.PatientId,
                                                DayInSID = p.Key.DayInSID,
                                                BenhAn = p.Key.BenhAn, // in => Nội trú, out => Ngoại trú
                                                ObjectCode = p.Key.ObjectCode, // 01 => Thu phí, 02 => BHYT, 03 => BHBL, 04 => DV
                                            }).ToList();
                    }
                    else
                    {
                        lstReportPatient = (from patient in _db.Patients
                                            join result in _db.ResultXNs on patient.Id equals result.PatientId
                                            where patient.InsertTime > fromTime && patient.InsertTime <= toTime && patient.ValidXN == true && patient.HospitalId == hospitalId
                                            group patient by new { patient.PatientId, DayInSID = patient.Sid.Substring(0, 6), ObjectCode = patient.Object.Code, patient.BenhAn } into p
                                            select new ReportPatient
                                            {
                                                PatientID = p.Key.PatientId,
                                                DayInSID = p.Key.DayInSID,
                                                BenhAn = p.Key.BenhAn, // in => Nội trú, out => Ngoại trú
                                                ObjectCode = p.Key.ObjectCode, // 01 => Thu phí, 02 => BHYT, 03 => BHBL, 04 => DV
                                            }).ToList();
                    }
                }
                else if (location == "SA")
                {
                    lstReportPatient = (from patient in _db.Patients
                                        join result in _db.ResultCDHAs on patient.Id equals result.PatientId
                                        join service in _db.Services on result.ServiceId equals service.Id
                                        where patient.InsertTime > fromTime && patient.InsertTime <= toTime && ((service.Category.Code == "SA" && patient.ValidSA == true) || (service.Category.Code == "SAT" && patient.ValidSAT == true))
                                        group patient by new { patient.PatientId, DayInSID = patient.Sid.Substring(0, 6), ObjectCode = patient.Object.Code, patient.BenhAn } into p
                                        select new ReportPatient
                                        {
                                            PatientID = p.Key.PatientId,
                                            DayInSID = p.Key.DayInSID,
                                            BenhAn = p.Key.BenhAn, // in => Nội trú, out => Ngoại trú
                                            ObjectCode = p.Key.ObjectCode, // 01 => Thu phí, 02 => BHYT, 03 => BHBL, 04 => DV
                                        }).ToList();
                }
                else if (location == "XQ")
                {
                    lstReportPatient = (from patient in _db.Patients
                                        join result in _db.ResultCDHAs on patient.Id equals result.PatientId
                                        join service in _db.Services on result.ServiceId equals service.Id
                                        where patient.InsertTime > fromTime && patient.InsertTime <= toTime && service.Category.Code == "XQ" && patient.ValidXQ == true
                                        group patient by new { patient.PatientId, DayInSID = patient.Sid.Substring(0, 6), ObjectCode = patient.Object.Code, patient.BenhAn } into p
                                        select new ReportPatient
                                        {
                                            PatientID = p.Key.PatientId,
                                            DayInSID = p.Key.DayInSID,
                                            BenhAn = p.Key.BenhAn, // in => Nội trú, out => Ngoại trú
                                            ObjectCode = p.Key.ObjectCode, // 01 => Thu phí, 02 => BHYT, 03 => BHBL, 04 => DV
                                        }).ToList();
                }
                else if (location == "TDCN")
                {
                    lstReportPatient = (from patient in _db.Patients
                                        join result in _db.ResultCDHAs on patient.Id equals result.PatientId
                                        join service in _db.Services on result.ServiceId equals service.Id
                                        where patient.InsertTime > fromTime
                                              && patient.InsertTime <= toTime
                                              && service.Category.Code == "TDCN"
                                              && patient.ValidTDCN == true
                                        group patient by new
                                        {
                                            patient.PatientId,
                                            DayInSID = patient.Sid.Substring(0, 6),
                                            ObjectCode = patient.Object.Code,
                                            patient.BenhAn
                                        } into p
                                        select new ReportPatient
                                        {
                                            PatientID = p.Key.PatientId,
                                            DayInSID = p.Key.DayInSID,
                                            BenhAn = p.Key.BenhAn,
                                            ObjectCode = p.Key.ObjectCode
                                        }).ToList();
                }
                else if (location == "NS")
                {
                    lstReportPatient = (from patient in _db.Patients
                                        join result in _db.ResultCDHAs on patient.Id equals result.PatientId
                                        join service in _db.Services on result.ServiceId equals service.Id
                                        where patient.InsertTime > fromTime && patient.InsertTime <= toTime && service.Category.Code == "NS" && patient.ValidNS == true
                                        group patient by new { patient.PatientId, DayInSID = patient.Sid.Substring(0, 6), ObjectCode = patient.Object.Code, patient.BenhAn } into p
                                        select new ReportPatient
                                        {
                                            PatientID = p.Key.PatientId,
                                            DayInSID = p.Key.DayInSID,
                                            BenhAn = p.Key.BenhAn, // in => Nội trú, out => Ngoại trú
                                            ObjectCode = p.Key.ObjectCode, // 01 => Thu phí, 02 => BHYT, 03 => BHBL, 04 => DV
                                        }).ToList();
                }

                if (lstReportPatient != null)
                {
                    var item = new TotalReportPatient();
                    item.FromDate = fromTimeString;
                    item.ToDate = toTimeString;
                    item.DateNow = dateNow;
                    item.User = user;
                    foreach (var i in lstReportPatient)
                    {
                        if (i.ObjectCode == "BHYT" || i.ObjectCode == "BHBL")
                        {
                            item.TotalBHYT++;
                            if (i.BenhAn == "in")
                                item.TotalBHYT_NoiTru++;
                            else
                                item.TotalBHYT_NgoaiTru++;
                        }
                        else
                        {
                            item.TotalTP++;
                            if (i.BenhAn == "in")
                                item.TotalTP_NoiTru++;
                            else
                                item.TotalTP_NgoaiTru++;
                        }
                    }

                    lstTotalReportPatient.Add(item);
                }

                return lstTotalReportPatient;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<TotalReportPatient>> LC_GetReportByPatient(DateTime fromTime, DateTime toTime, string location, string user)
        {
            try
            {
                // Bao trọn ngày cuối: [from, endOfDay]
                var start = fromTime.Date;
                var endExclusive = toTime.Date.AddDays(1);

                var fromTimeString = start.ToString("dd/MM/yyyy");
                var toTimeString = toTime.Date.ToString("dd/MM/yyyy");
                var dateNow = DateTime.Now.ToString("dd/MM/yyyy");

                var lstTotalReportPatient = new List<TotalReportPatient>();
                var lstPatientDetails = new List<ReportPatientDetail>();

                if (location == "XN")
                {
                    lstPatientDetails = (from patient in _db.Patients
                                         join result in _db.ResultXNs on patient.Id equals result.PatientId
                                         where patient.InsertTime > fromTime && patient.InsertTime <= toTime
                                               && (patient.ValidXN == true || patient.ProcessXN == true)
                                               && result.Active == true
                                               && patient.Active == true
                                         group patient by new
                                         {
                                             patient.PatientId,
                                             patient.PatientName,
                                             patient.Sid,
                                             patient.Seq,
                                             patient.MaBenhAn,
                                             patient.BenhAn,
                                             ObjectCode = patient.Object.Code,
                                             patient.InsertTime
                                         } into p
                                         select new ReportPatientDetail
                                         {
                                             PatientID = p.Key.PatientId,
                                             PatientName = p.Key.PatientName,
                                             Sid = p.Key.Sid,
                                             Seq = p.Key.Seq,
                                             MaBenhAn = p.Key.MaBenhAn,
                                             BenhAn = p.Key.BenhAn,
                                             ObjectCode = p.Key.ObjectCode,
                                             InsertTime = p.Key.InsertTime
                                         }).ToList();
                }
                else if (location == "SA")
                {
                    lstPatientDetails = (from patient in _db.Patients
                                         join result in _db.ResultCDHAs on patient.Id equals result.PatientId
                                         join service in _db.Services on result.ServiceId equals service.Id
                                         where patient.InsertTime > fromTime && patient.InsertTime <= toTime
                                               && ((service.Category.Code == "SA" && patient.ValidSA == true)
                                                   || (service.Category.Code == "SAT" && patient.ValidSAT == true))
                                               && result.Active == true
                                               && patient.Active == true
                                         group patient by new
                                         {
                                             patient.PatientId,
                                             patient.PatientName,
                                             patient.Sid,
                                             patient.Seq,
                                             patient.MaBenhAn,
                                             patient.BenhAn,
                                             ObjectCode = patient.Object.Code,
                                             patient.InsertTime
                                         } into p
                                         select new ReportPatientDetail
                                         {
                                             PatientID = p.Key.PatientId,
                                             PatientName = p.Key.PatientName,
                                             Sid = p.Key.Sid,
                                             Seq = p.Key.Seq,
                                             MaBenhAn = p.Key.MaBenhAn,
                                             BenhAn = p.Key.BenhAn,
                                             ObjectCode = p.Key.ObjectCode,
                                             InsertTime = p.Key.InsertTime
                                         }).ToList();
                }
                else if (location == "XQ")
                {
                    lstPatientDetails = (from patient in _db.Patients
                                         join result in _db.ResultCDHAs on patient.Id equals result.PatientId
                                         join service in _db.Services on result.ServiceId equals service.Id
                                         where patient.InsertTime > fromTime && patient.InsertTime <= toTime
                                               && service.Category.Code == "XQ"
                                               && patient.ValidXQ == true
                                               && result.Active == true
                                               && patient.Active == true
                                         group patient by new
                                         {
                                             patient.PatientId,
                                             patient.PatientName,
                                             patient.Sid,
                                             patient.Seq,
                                             patient.MaBenhAn,
                                             patient.BenhAn,
                                             ObjectCode = patient.Object.Code,
                                             patient.InsertTime
                                         } into p
                                         select new ReportPatientDetail
                                         {
                                             PatientID = p.Key.PatientId,
                                             PatientName = p.Key.PatientName,
                                             Sid = p.Key.Sid,
                                             Seq = p.Key.Seq,
                                             MaBenhAn = p.Key.MaBenhAn,
                                             BenhAn = p.Key.BenhAn,
                                             ObjectCode = p.Key.ObjectCode,
                                             InsertTime = p.Key.InsertTime
                                         }).ToList();
                }
                else if (location == "DDT")
                {
                    lstPatientDetails = (from patient in _db.Patients
                                         join result in _db.ResultCDHAs on patient.Id equals result.PatientId
                                         join service in _db.Services on result.ServiceId equals service.Id
                                         where patient.InsertTime > fromTime && patient.InsertTime <= toTime
                                               && service.Category.Code == "DDT"
                                               && patient.ValidDDT == true
                                               && result.Active == true
                                               && patient.Active == true
                                         group patient by new
                                         {
                                             patient.PatientId,
                                             patient.PatientName,
                                             patient.Sid,
                                             patient.Seq,
                                             patient.MaBenhAn,
                                             patient.BenhAn,
                                             ObjectCode = patient.Object.Code,
                                             patient.InsertTime
                                         } into p
                                         select new ReportPatientDetail
                                         {
                                             PatientID = p.Key.PatientId,
                                             PatientName = p.Key.PatientName,
                                             Sid = p.Key.Sid,
                                             Seq = p.Key.Seq,
                                             MaBenhAn = p.Key.MaBenhAn,
                                             BenhAn = p.Key.BenhAn,
                                             ObjectCode = p.Key.ObjectCode,
                                             InsertTime = p.Key.InsertTime
                                         }).ToList();
                }
                else if (location == "TDCN")
                {
                    lstPatientDetails = (from patient in _db.Patients
                                         join result in _db.ResultCDHAs on patient.Id equals result.PatientId
                                         join service in _db.Services on result.ServiceId equals service.Id
                                         where patient.InsertTime > fromTime
                                               && patient.InsertTime <= toTime
                                               && service.Category.Code == "TDCN"
                                               && patient.ValidTDCN == true
                                               && result.Active == true
                                               && patient.Active == true
                                         group patient by new
                                         {
                                             patient.PatientId,
                                             patient.PatientName,
                                             patient.Sid,
                                             patient.Seq,
                                             patient.MaBenhAn,
                                             patient.BenhAn,
                                             ObjectCode = patient.Object.Code,
                                             patient.InsertTime
                                         } into p
                                         select new ReportPatientDetail
                                         {
                                             PatientID = p.Key.PatientId,
                                             PatientName = p.Key.PatientName,
                                             Sid = p.Key.Sid,
                                             Seq = p.Key.Seq,
                                             MaBenhAn = p.Key.MaBenhAn,
                                             BenhAn = p.Key.BenhAn,
                                             ObjectCode = p.Key.ObjectCode,
                                             InsertTime = p.Key.InsertTime
                                         }).ToList();
                }
                else if (location == "NS")
                {
                    lstPatientDetails = (from patient in _db.Patients
                                         join result in _db.ResultCDHAs on patient.Id equals result.PatientId
                                         join service in _db.Services on result.ServiceId equals service.Id
                                         where patient.InsertTime > fromTime && patient.InsertTime <= toTime
                                               && service.Category.Code == "NS"
                                               && patient.ValidNS == true
                                               && result.Active == true
                                               && patient.Active == true
                                         group patient by new
                                         {
                                             patient.PatientId,
                                             patient.PatientName,
                                             patient.Sid,
                                             patient.Seq,
                                             patient.MaBenhAn,
                                             patient.BenhAn,
                                             ObjectCode = patient.Object.Code,
                                             patient.InsertTime
                                         } into p
                                         select new ReportPatientDetail
                                         {
                                             PatientID = p.Key.PatientId,
                                             PatientName = p.Key.PatientName,
                                             Sid = p.Key.Sid,
                                             Seq = p.Key.Seq,
                                             MaBenhAn = p.Key.MaBenhAn,
                                             BenhAn = p.Key.BenhAn,
                                             ObjectCode = p.Key.ObjectCode,
                                             InsertTime = p.Key.InsertTime
                                         }).ToList();
                }

                if (lstPatientDetails != null && lstPatientDetails.Count > 0)
                {
                    var item = new TotalReportPatient();
                    item.FromDate = fromTimeString;
                    item.ToDate = toTimeString;
                    item.DateNow = dateNow;
                    item.User = user;
                    item.Total = lstPatientDetails.Count;
                    item.PatientDetails = lstPatientDetails;

                    lstTotalReportPatient.Add(item);
                }

                return lstTotalReportPatient;
            }
            catch
            {
                return null;
            }
        }
        public async Task<List<ReportProcess>> GetReportByProcess(DateTime fromTime, DateTime toTime, string location, string user, long? hospitalId)
        {
            try
            {
                var fromTimeString = fromTime.AddDays(1).ToString("dd/MM/yyyy");
                var toTimeString = toTime.ToString("dd/MM/yyyy");
                var dateNow = DateTime.Now.ToString("dd/MM/yyyy");
                var lstReportProcess = new List<ReportProcess>();
                var lstResult = new List<ReportProcess>();

                if (location == "XN")
                {
                    if (hospitalId == null)
                    {
                        lstResult = (from result in _db.ResultXNs
                                     join patient in _db.Patients on result.PatientId equals patient.Id
                                     join us in _db.Users on patient.UserReturnResultXN equals us.Id
                                     join service in _db.Services on result.ServiceId equals service.Id
                                     where result.InsertTime > fromTime && result.InsertTime <= toTime && patient.ValidXN == true && result.Active == true
                                     group result by new { ServiceId = result.ServiceId, ServiceName = result.Service.Name, UserId = us.Id, Sid = result.Patient.Sid, UserName = us.Name } into p
                                     select new ReportProcess
                                     {
                                         DoctorID = p.Key.UserId.ToString(),
                                         DoctorName = p.Key.UserName,
                                         ServiceID = p.Key.ServiceId.ToString(),
                                         ServiceName = p.Key.ServiceName,
                                         FromDate = fromTimeString,
                                         ToDate = toTimeString,
                                         DateNow = dateNow,
                                         User = user
                                     }).ToList();
                    }
                    else
                    {
                        lstResult = (from result in _db.ResultXNs
                                     join patient in _db.Patients on result.PatientId equals patient.Id
                                     join us in _db.Users on patient.UserReturnResultXN equals us.Id
                                     join service in _db.Services on result.ServiceId equals service.Id
                                     where result.InsertTime > fromTime && result.InsertTime <= toTime && patient.ValidXN == true && result.Active == true && patient.HospitalId == hospitalId
                                     group result by new { ServiceId = result.ServiceId, ServiceName = result.Service.Name, UserId = us.Id, Sid = result.Patient.Sid, UserName = us.Name } into p
                                     select new ReportProcess
                                     {
                                         DoctorID = p.Key.UserId.ToString(),
                                         DoctorName = p.Key.UserName,
                                         ServiceID = p.Key.ServiceId.ToString(),
                                         ServiceName = p.Key.ServiceName,
                                         FromDate = fromTimeString,
                                         ToDate = toTimeString,
                                         DateNow = dateNow,
                                         User = user
                                     }).ToList();
                    }

                    lstResult = (from item in lstResult
                                 group item by new { item.DoctorID, item.DoctorName, item.ServiceID, item.ServiceName, item.FromDate, item.ToDate, item.User } into p
                                 select new ReportProcess
                                 {
                                     DoctorID = p.Key.DoctorID,
                                     DoctorName = p.Key.DoctorName,
                                     ServiceID = p.Key.ServiceID,
                                     ServiceName = p.Key.ServiceName,
                                     FromDate = fromTimeString,
                                     ToDate = toTimeString,
                                     DateNow = dateNow,
                                     User = user,
                                     Count = p.Count()
                                 }).ToList();

                }
                else if (location == "SA")
                {
                    lstResult = (from resultImage in _db.ResultCDHAs
                                 join patient in _db.Patients on resultImage.PatientId equals patient.Id
                                 join us in _db.Users on patient.UserReturnResultSA equals us.Id
                                 join service in _db.Services on resultImage.ServiceId equals service.Id
                                 where resultImage.InsertTime > fromTime && resultImage.InsertTime <= toTime && resultImage.Active == true && service.Category.Code == "SA" && patient.ValidSA == true
                                 group resultImage by new { ServiceId = resultImage.ServiceId, ServiceName = resultImage.Service.Name, UserId = us.Id, UserName = us.Name } into p
                                 select new ReportProcess
                                 {
                                     DoctorID = p.Key.UserId.ToString(),
                                     DoctorName = p.Key.UserName,
                                     ServiceID = p.Key.ServiceId.ToString(),
                                     ServiceName = p.Key.ServiceName,
                                     FromDate = fromTimeString,
                                     ToDate = toTimeString,
                                     DateNow = dateNow,
                                     User = user,
                                     Count = p.Count()
                                 }).ToList();

                    var lstResultSAT = (from resultImage in _db.ResultCDHAs
                                        join patient in _db.Patients on resultImage.PatientId equals patient.Id
                                        join us in _db.Users on patient.UserReturnResultSAT equals us.Id
                                        join service in _db.Services on resultImage.ServiceId equals service.Id
                                        where resultImage.InsertTime > fromTime && resultImage.InsertTime <= toTime && service.Category.Code == "SAT" && patient.ValidSAT == true
                                        group resultImage by new { ServiceId = resultImage.ServiceId, ServiceName = resultImage.Service.Name, UserId = us.Id, UserName = us.Name } into p
                                        select new ReportProcess
                                        {
                                            DoctorID = p.Key.UserId.ToString(),
                                            DoctorName = p.Key.UserName,
                                            ServiceID = p.Key.ServiceId.ToString(),
                                            ServiceName = p.Key.ServiceName,
                                            FromDate = fromTimeString,
                                            ToDate = toTimeString,
                                            DateNow = dateNow,
                                            User = user,
                                            Count = p.Count()
                                        }).ToList();

                    lstResult.AddRange(lstResultSAT);
                }
                else if (location == "XQ")
                {
                    lstResult = (from resultImage in _db.ResultCDHAs
                                 join patient in _db.Patients on resultImage.PatientId equals patient.Id
                                 join us in _db.Users on patient.UserReturnResultXQ equals us.Id
                                 join service in _db.Services on resultImage.ServiceId equals service.Id
                                 where resultImage.InsertTime > fromTime && resultImage.InsertTime <= toTime && resultImage.Active == true && service.Category.Code == "XQ" && patient.ValidXQ == true
                                 group resultImage by new { ServiceId = resultImage.ServiceId, ServiceName = resultImage.Service.Name, UserId = us.Id, UserName = us.Name } into p
                                 select new ReportProcess
                                 {
                                     DoctorID = p.Key.UserId.ToString(),
                                     DoctorName = p.Key.UserName,
                                     ServiceID = p.Key.ServiceId.ToString(),
                                     ServiceName = p.Key.ServiceName,
                                     FromDate = fromTimeString,
                                     ToDate = toTimeString,
                                     DateNow = dateNow,
                                     User = user,
                                     Count = p.Count()
                                 }).ToList();
                }
                else if (location == "TDCN")
                {
                    lstResult = (from resultImage in _db.ResultCDHAs
                                 join patient in _db.Patients on resultImage.PatientId equals patient.Id
                                 join us in _db.Users on patient.UserReturnResultTDCN equals us.Id
                                 join service in _db.Services on resultImage.ServiceId equals service.Id
                                 where resultImage.InsertTime > fromTime
                                       && resultImage.InsertTime <= toTime
                                       && resultImage.Active == true
                                       && service.Category.Code == "TDCN"
                                       && patient.ValidTDCN == true
                                 group resultImage by new
                                 {
                                     ServiceId = resultImage.ServiceId,
                                     ServiceName = resultImage.Service.Name,
                                     UserId = us.Id,
                                     UserName = us.Name
                                 } into p
                                 select new ReportProcess
                                 {
                                     DoctorID = p.Key.UserId.ToString(),
                                     DoctorName = p.Key.UserName,
                                     ServiceID = p.Key.ServiceId.ToString(),
                                     ServiceName = p.Key.ServiceName,
                                     FromDate = fromTimeString,
                                     ToDate = toTimeString,
                                     DateNow = dateNow,
                                     User = user,
                                     Count = p.Count()
                                 }).ToList();
                }
                else if (location == "NS")
                {
                    lstResult = (from resultImage in _db.ResultCDHAs
                                 join patient in _db.Patients on resultImage.PatientId equals patient.Id
                                 join us in _db.Users on patient.UserReturnResultNS equals us.Id
                                 join service in _db.Services on resultImage.ServiceId equals service.Id
                                 where resultImage.InsertTime > fromTime && resultImage.InsertTime <= toTime && resultImage.Active == true && service.Category.Code == "NS" && patient.ValidNS == true
                                 group resultImage by new { ServiceId = resultImage.ServiceId, ServiceName = resultImage.Service.Name, UserId = us.Id, UserName = us.Name } into p
                                 select new ReportProcess
                                 {
                                     DoctorID = p.Key.UserId.ToString(),
                                     DoctorName = p.Key.UserName,
                                     ServiceID = p.Key.ServiceId.ToString(),
                                     ServiceName = p.Key.ServiceName,
                                     FromDate = fromTimeString,
                                     ToDate = toTimeString,
                                     DateNow = dateNow,
                                     User = user,
                                     Count = p.Count()
                                 }).ToList();
                }

                return lstResult;
            }
            catch
            {
                return null;
            }
        }
        public async Task<List<ReportProcess>> LC_GetReportByProcess(DateTime fromTime, DateTime toTime, string location, string user)
        {
            try
            {
                var fromTimeString = fromTime.AddDays(1).ToString("dd/MM/yyyy");
                var toTimeString = toTime.ToString("dd/MM/yyyy");
                var dateNow = DateTime.Now.ToString("dd/MM/yyyy");
                var lstResult = new List<ReportProcess>();

                if (location == "XN")
                {
                    lstResult = (from result in _db.ResultXNs
                                 join patient in _db.Patients on result.PatientId equals patient.Id
                                 join us in _db.Users on patient.UserReturnResultXN equals us.Id
                                 join service in _db.Services on result.ServiceId equals service.Id
                                 where result.InsertTime > fromTime && result.InsertTime <= toTime && patient.ValidXN == true && result.Active == true
                                 group result by new { ServiceId = result.ServiceId, ServiceName = result.Service.Name, UserId = us.Id, Sid = result.Patient.Sid, UserName = us.Name } into p
                                 select new ReportProcess
                                 {
                                     DoctorID = p.Key.UserId.ToString(),
                                     DoctorName = p.Key.UserName,
                                     ServiceID = p.Key.ServiceId.ToString(),
                                     ServiceName = p.Key.ServiceName,
                                     FromDate = fromTimeString,
                                     ToDate = toTimeString,
                                     DateNow = dateNow,
                                     User = user
                                 }).ToList();
                }
                else if (location == "SA")
                {
                    lstResult = (from resultImage in _db.ResultCDHAs
                                 join patient in _db.Patients on resultImage.PatientId equals patient.Id
                                 join us in _db.Users on patient.UserReturnResultSA equals us.Id
                                 join service in _db.Services on resultImage.ServiceId equals service.Id
                                 where resultImage.InsertTime > fromTime && resultImage.InsertTime <= toTime && resultImage.Active == true && ((service.Category.Code == "SA" && patient.ValidSA == true) || (service.Category.Code == "SAT" && patient.ValidSAT == true))

                                 group resultImage by new { ServiceId = resultImage.ServiceId, ServiceName = resultImage.Service.Name, UserId = us.Id, UserName = us.Name } into p
                                 select new ReportProcess
                                 {
                                     DoctorID = p.Key.UserId.ToString(),
                                     DoctorName = p.Key.UserName,
                                     ServiceID = p.Key.ServiceId.ToString(),
                                     ServiceName = p.Key.ServiceName,
                                     FromDate = fromTimeString,
                                     ToDate = toTimeString,
                                     DateNow = dateNow,
                                     User = user,
                                     Count = p.Count()
                                 }).ToList();

                    //var lstResultSAT = (from resultImage in _db.ResultCDHAs
                    //                    join patient in _db.Patients on resultImage.PatientId equals patient.Id
                    //                    join us in _db.Users on patient.UserReturnResultSAT equals us.Id
                    //                    join service in _db.Services on resultImage.ServiceId equals service.Id
                    //                    where resultImage.InsertTime > fromTime && resultImage.InsertTime <= toTime && service.Category.Code == "SAT" && patient.ValidSAT == true
                    //                    group resultImage by new { ServiceId = resultImage.ServiceId, ServiceName = resultImage.Service.Name, UserId = us.Id, UserName = us.Name } into p
                    //                    select new ReportProcess
                    //                    {
                    //                        DoctorID = p.Key.UserId.ToString(),
                    //                        DoctorName = p.Key.UserName,
                    //                        ServiceID = p.Key.ServiceId.ToString(),
                    //                        ServiceName = p.Key.ServiceName,
                    //                        FromDate = fromTimeString,
                    //                        ToDate = toTimeString,
                    //                        DateNow = dateNow,
                    //                        User = user,
                    //                        Count = p.Count()
                    //                    }).ToList();

                }
                else if (location == "XQ")
                {
                    lstResult = (from resultImage in _db.ResultCDHAs
                                 join patient in _db.Patients on resultImage.PatientId equals patient.Id
                                 join us in _db.Users on patient.UserReturnResultXQ equals us.Id
                                 join service in _db.Services on resultImage.ServiceId equals service.Id
                                 where resultImage.InsertTime > fromTime && resultImage.InsertTime <= toTime && resultImage.Active == true && service.Category.Code == "XQ" && patient.ValidXQ == true
                                 group resultImage by new { ServiceId = resultImage.ServiceId, ServiceName = resultImage.Service.Name, UserId = us.Id, UserName = us.Name } into p
                                 select new ReportProcess
                                 {
                                     DoctorID = p.Key.UserId.ToString(),
                                     DoctorName = p.Key.UserName,
                                     ServiceID = p.Key.ServiceId.ToString(),
                                     ServiceName = p.Key.ServiceName,
                                     FromDate = fromTimeString,
                                     ToDate = toTimeString,
                                     DateNow = dateNow,
                                     User = user,
                                     Count = p.Count()
                                 }).ToList();
                }
                else if (location == "DDT")
                {
                    lstResult = (from resultImage in _db.ResultCDHAs
                                 join patient in _db.Patients on resultImage.PatientId equals patient.Id
                                 join us in _db.Users on patient.UserReturnResultDDT equals us.Id
                                 join service in _db.Services on resultImage.ServiceId equals service.Id
                                 where resultImage.InsertTime > fromTime && resultImage.InsertTime <= toTime && resultImage.Active == true && service.Category.Code == "DDT" && patient.ValidDDT == true
                                 group resultImage by new { ServiceId = resultImage.ServiceId, ServiceName = resultImage.Service.Name, UserId = us.Id, UserName = us.Name } into p
                                 select new ReportProcess
                                 {
                                     DoctorID = p.Key.UserId.ToString(),
                                     DoctorName = p.Key.UserName,
                                     ServiceID = p.Key.ServiceId.ToString(),
                                     ServiceName = p.Key.ServiceName,
                                     FromDate = fromTimeString,
                                     ToDate = toTimeString,
                                     DateNow = dateNow,
                                     User = user,
                                     Count = p.Count()
                                 }).ToList();
                }
                else if (location == "TDCN")
                {
                    lstResult = (from resultImage in _db.ResultCDHAs
                                 join patient in _db.Patients on resultImage.PatientId equals patient.Id
                                 join us in _db.Users on patient.UserReturnResultTDCN equals us.Id
                                 join service in _db.Services on resultImage.ServiceId equals service.Id
                                 where resultImage.InsertTime > fromTime
                                       && resultImage.InsertTime <= toTime
                                       && resultImage.Active == true
                                       && service.Category.Code == "TDCN"
                                       && patient.ValidTDCN == true
                                 group resultImage by new
                                 {
                                     ServiceId = resultImage.ServiceId,
                                     ServiceName = resultImage.Service.Name,
                                     UserId = us.Id,
                                     UserName = us.Name
                                 } into p
                                 select new ReportProcess
                                 {
                                     DoctorID = p.Key.UserId.ToString(),
                                     DoctorName = p.Key.UserName,
                                     ServiceID = p.Key.ServiceId.ToString(),
                                     ServiceName = p.Key.ServiceName,
                                     FromDate = fromTimeString,
                                     ToDate = toTimeString,
                                     DateNow = dateNow,
                                     User = user,
                                     Count = p.Count()
                                 }).ToList();
                }
                else if (location == "NS")
                {
                    lstResult = (from resultImage in _db.ResultCDHAs
                                 join patient in _db.Patients on resultImage.PatientId equals patient.Id
                                 join us in _db.Users on patient.UserReturnResultNS equals us.Id
                                 join service in _db.Services on resultImage.ServiceId equals service.Id
                                 where resultImage.InsertTime > fromTime && resultImage.InsertTime <= toTime && resultImage.Active == true && service.Category.Code == "NS" && patient.ValidNS == true
                                 group resultImage by new { ServiceId = resultImage.ServiceId, ServiceName = resultImage.Service.Name, UserId = us.Id, UserName = us.Name } into p
                                 select new ReportProcess
                                 {
                                     DoctorID = p.Key.UserId.ToString(),
                                     DoctorName = p.Key.UserName,
                                     ServiceID = p.Key.ServiceId.ToString(),
                                     ServiceName = p.Key.ServiceName,
                                     FromDate = fromTimeString,
                                     ToDate = toTimeString,
                                     DateNow = dateNow,
                                     User = user,
                                     Count = p.Count()
                                 }).ToList();
                }

                return lstResult;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<ReportProcessPatientGroup>> LC_GetReportByProcessWithPatients(
    DateTime fromTime,
    DateTime toTime,
    string location,
    string user)
        {
            try
            {
                var start = fromTime.Date;
                var end = toTime.Date.AddDays(1);

                var fromTimeString = start.ToString("dd/MM/yyyy");
                var toTimeString = toTime.Date.ToString("dd/MM/yyyy");
                var dateNow = DateTime.Now.ToString("dd/MM/yyyy");

                location = location?.Trim().ToUpperInvariant();

                var supportedLocations = new[]
                {
            "XN",
            "SA",
            "XQ",
            "DDT",
            "NS",
            "TDCN"
        };

                if (string.IsNullOrWhiteSpace(location)
                    || !supportedLocations.Contains(location))
                {
                    return new List<ReportProcessPatientGroup>();
                }

                var flatRows = new List<ReportProcessFlatRow>();

                // XN dùng bảng ResultXNs nên giữ một query riêng.
                if (location == "XN")
                {
                    flatRows = await (
                        from resultXN in _db.ResultXNs
                        join patient in _db.Patients
                            on resultXN.PatientId equals patient.Id
                        join returnUser in _db.Users
                            on patient.UserReturnResultXN equals returnUser.Id
                        join service in _db.Services
                            on resultXN.ServiceId equals service.Id
                        where patient.ReturnResultTimeXN >= start
                              && patient.ReturnResultTimeXN < end
                              && patient.UserReturnResultXN != null
                              && patient.ValidXN == true
                              && resultXN.Active == true
                              && patient.Active == true
                        group new
                        {
                            patient,
                            ReturnUser = returnUser,
                            service
                        }
                        by new
                        {
                            DoctorID = returnUser.Id,
                            DoctorName = returnUser.Name,
                            ServiceID = service.Id,
                            ServiceName = service.Name,
                            patient.PatientId,
                            patient.PatientName,
                            patient.Sid,
                            patient.Seq,
                            patient.MaBenhAn,
                            patient.InsertTime,
                            patient.Address,
                            patient.MaDotKham
                        }
                        into resultGroup
                        select new ReportProcessFlatRow
                        {
                            DoctorID = resultGroup.Key.DoctorID.ToString(),
                            DoctorName = resultGroup.Key.DoctorName,
                            ServiceID = resultGroup.Key.ServiceID.ToString(),
                            ServiceName = resultGroup.Key.ServiceName,
                            PatientID = resultGroup.Key.PatientId,
                            PatientName = resultGroup.Key.PatientName,
                            Sid = resultGroup.Key.Sid,
                            Seq = resultGroup.Key.Seq,
                            MaBenhAn = resultGroup.Key.MaBenhAn,
                            InsertTime = resultGroup.Key.InsertTime,
                            MaDotKham = resultGroup.Key.MaDotKham,
                            Address = resultGroup.Key.Address
                        }
                    ).ToListAsync();
                }
                else
                {
                    // Trên UI, location = SA bao gồm cả Siêu âm thường (SA)
                    // và Siêu âm tim (SAT). Các module còn lại chỉ có một mã.
                    var categoryCodes = location == "SA"
                        ? new[] { "SA", "SAT" }
                        : new[] { location };

                    flatRows = await (
                        from resultImage in _db.ResultCDHAs
                        join patient in _db.Patients
                            on resultImage.PatientId equals patient.Id
                        join service in _db.Services
                            on resultImage.ServiceId equals service.Id

                        let moduleCode = service.Category.Code

                        // Các biểu thức điều kiện dưới đây được EF Core dịch thành CASE.
                        // Không dùng reflection trong LINQ vì reflection không dịch được sang SQL.
                        let returnResultTime =
                            moduleCode == "SA" ? patient.ReturnResultTimeSA :
                            moduleCode == "SAT" ? patient.ReturnResultTimeSAT :
                            moduleCode == "XQ" ? patient.ReturnResultTimeXQ :
                            moduleCode == "DDT" ? patient.ReturnResultTimeDDT :
                            moduleCode == "NS" ? patient.ReturnResultTimeNS :
                            patient.ReturnResultTimeTDCN

                        let returnUserId =
                            moduleCode == "SA" ? patient.UserReturnResultSA :
                            moduleCode == "SAT" ? patient.UserReturnResultSAT :
                            moduleCode == "XQ" ? patient.UserReturnResultXQ :
                            moduleCode == "DDT" ? patient.UserReturnResultDDT :
                            moduleCode == "NS" ? patient.UserReturnResultNS :
                            patient.UserReturnResultTDCN

                        let isValid =
                            moduleCode == "SA" ? patient.ValidSA == true :
                            moduleCode == "SAT" ? patient.ValidSAT == true :
                            moduleCode == "XQ" ? patient.ValidXQ == true :
                            moduleCode == "DDT" ? patient.ValidDDT == true :
                            moduleCode == "NS" ? patient.ValidNS == true :
                            patient.ValidTDCN == true

                        join returnUser in _db.Users
                            on returnUserId equals returnUser.Id

                        where categoryCodes.Contains(moduleCode)
                              && returnResultTime >= start
                              && returnResultTime < end
                              && isValid
                              && resultImage.Active == true
                              && patient.Active == true

                        group new
                        {
                            patient,
                            ReturnUser = returnUser,
                            service
                        }
                        by new
                        {
                            DoctorID = returnUser.Id,
                            DoctorName = returnUser.Name,
                            ServiceID = service.Id,
                            ServiceName = service.Name,
                            patient.PatientId,
                            patient.PatientName,
                            patient.Sid,
                            patient.Seq,
                            patient.MaBenhAn,
                            patient.InsertTime,
                            patient.Address,
                            patient.MaDotKham
                        }
                        into resultGroup
                        select new ReportProcessFlatRow
                        {
                            DoctorID = resultGroup.Key.DoctorID.ToString(),
                            DoctorName = resultGroup.Key.DoctorName,
                            ServiceID = resultGroup.Key.ServiceID.ToString(),
                            ServiceName = resultGroup.Key.ServiceName,
                            PatientID = resultGroup.Key.PatientId,
                            PatientName = resultGroup.Key.PatientName,
                            Sid = resultGroup.Key.Sid,
                            Seq = resultGroup.Key.Seq,
                            MaBenhAn = resultGroup.Key.MaBenhAn,
                            InsertTime = resultGroup.Key.InsertTime,
                            MaDotKham = resultGroup.Key.MaDotKham,
                            Address = resultGroup.Key.Address
                        }
                    ).ToListAsync();
                }

                flatRows = flatRows
                    .OrderBy(x => x.DoctorName)
                    .ThenBy(x => x.ServiceName)
                    .ThenBy(x => x.Seq)
                    .ToList();

                return flatRows
                    .GroupBy(x => new
                    {
                        x.DoctorID,
                        x.DoctorName
                    })
                    .Select(doctorGroup => new ReportProcessPatientGroup
                    {
                        DoctorID = doctorGroup.Key.DoctorID,
                        DoctorName = doctorGroup.Key.DoctorName,
                        FromDate = fromTimeString,
                        ToDate = toTimeString,
                        DateNow = dateNow,
                        User = user,
                        Services = doctorGroup
                            .GroupBy(x => new
                            {
                                x.ServiceID,
                                x.ServiceName
                            })
                            .Select(serviceGroup => new ReportProcessServiceDetail
                            {
                                ServiceID = serviceGroup.Key.ServiceID,
                                ServiceName = serviceGroup.Key.ServiceName,
                                Patients = serviceGroup
                                    .OrderBy(x => x.Seq)
                                    .Select(x => new ReportPatientDetail
                                    {
                                        PatientID = x.PatientID,
                                        PatientName = x.PatientName,
                                        Sid = x.Sid,
                                        Seq = x.Seq,
                                        MaBenhAn = x.MaBenhAn,
                                        InsertTime = x.InsertTime,
                                        MaDotKham = x.MaDotKham,
                                        Address = x.Address
                                    })
                                    .ToList()
                            })
                            .OrderBy(x => x.ServiceName)
                            .ToList()
                    })
                    .OrderBy(x => x.DoctorName)
                    .ToList();
            }
            catch
            {
                return null;
            }
        }

        public List<ReportProcess> LC_FlattenReportProcessForView(List<ReportProcessPatientGroup> source)
        {
            if (source == null || !source.Any())
                return new List<ReportProcess>();

            return source
                .SelectMany(d => d.Services.Select(s => new ReportProcess
                {
                    DoctorID = d.DoctorID,
                    DoctorName = d.DoctorName,
                    ServiceID = s.ServiceID,
                    ServiceName = s.ServiceName,
                    Count = s.Count,
                    FromDate = d.FromDate,
                    ToDate = d.ToDate,
                    DateNow = d.DateNow,
                    User = d.User
                }))
                .OrderBy(x => x.DoctorName)
                .ThenBy(x => x.ServiceName)
                .ToList();
        }

        public List<ReportPatientDetail> LC_GetPatientsFromMergedData(
            List<ReportProcessPatientGroup> source,
            long doctorId,
            long serviceId)
        {
            if (source == null || !source.Any())
                return new List<ReportPatientDetail>();

            var doctor = source.FirstOrDefault(x => x.DoctorID == doctorId.ToString());
            if (doctor == null)
                return new List<ReportPatientDetail>();

            var service = doctor.Services.FirstOrDefault(x => x.ServiceID == serviceId.ToString());
            if (service == null)
                return new List<ReportPatientDetail>();

            return service.Patients
                .OrderBy(x => x.Seq)
                .ToList();
        }
        public async Task<List<ReportCovid>> GetReportByTestCovid(DateTime fromTime, DateTime toTime, string user, long? hospitalId)
        {
            try
            {
                if (hospitalId == null)
                {
                    var fromTimeString = fromTime.AddDays(1).ToString("dd/MM/yyyy");
                    var toTimeString = toTime.ToString("dd/MM/yyyy");
                    var dateNow = DateTime.Now.ToString("dd/MM/yyyy");
                    var lstReportCovid = (from patient in _db.Patients
                                          join result in _db.ResultXNs on patient.Id equals result.PatientId
                                          where patient.InsertTime > fromTime && patient.InsertTime <= toTime && patient.ValidXN == true &&
                                                result.Service.ProcessType == true
                                          select new ReportCovid
                                          {
                                              SID = patient.Sid,
                                              PatientID = patient.PatientId,
                                              PatientName = patient.PatientName,
                                              Sex = patient.Sex,
                                              Age = patient.Age,
                                              Address = patient.Address,
                                              ServiceID = result.ServiceId.ToString(),
                                              ServiceName = result.Service.Name,
                                              TestCode = result.TestCode.Code,
                                              Testname = result.TestCode.Name,
                                              Result = result.Result,
                                              InsertTime = patient.InsertTime ?? DateTime.Now,
                                              GetSampleTime = patient.GetSampleTimeXN ?? DateTime.Now,
                                              ReturnResultTime = patient.ReturnResultTimeXN ?? DateTime.Now,
                                              User = user,
                                              FromDate = fromTimeString,
                                              ToDate = toTimeString,
                                              DateNow = dateNow,
                                          }).ToList();


                    return lstReportCovid;
                }
                else
                {
                    var fromTimeString = fromTime.AddDays(1).ToString("dd/MM/yyyy");
                    var toTimeString = toTime.ToString("dd/MM/yyyy");
                    var dateNow = DateTime.Now.ToString("dd/MM/yyyy");
                    var lstReportCovid = (from patient in _db.Patients
                                          join result in _db.ResultXNs on patient.Id equals result.PatientId
                                          where patient.InsertTime > fromTime && patient.InsertTime <= toTime && patient.ValidXN == true &&
                                                result.Service.ProcessType == true && patient.HospitalId == hospitalId
                                          select new ReportCovid
                                          {
                                              SID = patient.Sid,
                                              PatientID = patient.PatientId,
                                              PatientName = patient.PatientName,
                                              Sex = patient.Sex,
                                              Age = patient.Age,
                                              Address = patient.Address,
                                              ServiceID = result.ServiceId.ToString(),
                                              ServiceName = result.Service.Name,
                                              TestCode = result.TestCode.Code,
                                              Testname = result.TestCode.Name,
                                              Result = result.Result,
                                              InsertTime = patient.InsertTime ?? DateTime.Now,
                                              GetSampleTime = patient.GetSampleTimeXN ?? DateTime.Now,
                                              ReturnResultTime = patient.ReturnResultTimeXN ?? DateTime.Now,
                                              User = user,
                                              FromDate = fromTimeString,
                                              ToDate = toTimeString,
                                              DateNow = dateNow,
                                          }).ToList();


                    return lstReportCovid;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<ServicePatientResultReport> LC_GetPatientResultByServiceXN(long serviceId, DateTime fromtime, DateTime totime)
        {
            // Chuẩn khoảng ngày [from, to]
            var start = fromtime.Date;
            var end = totime.Date.AddDays(1);

            // Query gốc: ResultXN theo service + khoảng thời gian + join Patient
            // Điều kiện có thể điều chỉnh cho khớp logic duyệt kết quả của bạn (ValidXN, Active,...)
            var baseQuery =
                from r in _db.ResultXNs
                join p in _db.Patients on r.PatientId equals p.Id
                where r.ServiceId == serviceId
                      && r.Active == true
                      && p.Active == true
                      && p.InsertTime >= start
                      && p.InsertTime < end
                      && (p.ValidXN == true || p.ProcessXN == true)
                select new
                {
                    Patient = p,
                    ResultXN = r
                };

            // Lấy danh sách TestCodeId xuất hiện trong khoảng này (theo service này)
            var testCodeIds = await baseQuery
                .Select(x => x.ResultXN.TestCodeId)
                .Where(id => id != null)
                .Distinct()
                .Cast<long>()
                .ToListAsync();

            // Lấy thông tin TestCode tương ứng để dựng cột
            var testCodes = await _db.TestCodes
                .Where(tc => testCodeIds.Contains(tc.Id) && tc.Active == true)
                .OrderBy(tc => tc.PrintOrder ?? int.MaxValue)
                .ThenBy(tc => tc.Id)
                .Select(tc => new TestCodeColumnModel
                {
                    Id = tc.Id,
                    Code = tc.Code,
                    Name = tc.Name,
                    Unit = tc.Unit
                })
                .ToListAsync();

            // Lấy kết quả "mới nhất" cho mỗi (Patient, TestCode)
            try
            {
                // Truy vấn dữ liệu từ SQL trước
                var resultCandidates = await baseQuery
                    .Where(x => x.ResultXN.TestCodeId != null)
                    .ToListAsync();

                // Sau khi dữ liệu đã nằm trong bộ nhớ mới thực hiện GroupBy
                var latestResults = resultCandidates
                    .GroupBy(x => new
                    {
                        PatientId = x.Patient.Id,
                        TestCodeId = x.ResultXN.TestCodeId.Value
                    })
                    .Select(g => g
                        .OrderByDescending(x =>
                            x.ResultXN.ValidTime ?? x.ResultXN.InsertTime)
                        .ThenByDescending(x => x.ResultXN.Id)
                        .First())
                    .ToList();
                // Gom theo Patient để build từng dòng
                var rows = latestResults
                    .GroupBy(x => x.Patient.Id)
                    .Select(g =>
                    {
                        var first = g.First().Patient;

                        var row = new ServicePatientResultRow
                        {
                            PatientId = first.Id,
                            PatientName = first.PatientName,
                            Gender = first.Sex,
                            Age = first.Age,
                            MaDotKham = first.MaDotKham,
                            MaBenhAn = first.MaBenhAn,
                            Seq = first.Seq,
                            Sid = first.Sid,
                            DateSearch = first.InsertTime
                        };

                        foreach (var tc in testCodes)
                        {
                            var result = g.FirstOrDefault(
                                x => x.ResultXN.TestCodeId == tc.Id);

                            row.ResultsByTestCode[tc.Id] = result?.ResultXN?.Result;
                        }

                        return row;
                    })
                    .OrderBy(x => x.DateSearch)
                    .ThenBy(x => x.PatientId)
                    .Select((row, index) =>
                    {
                        row.STT = index + 1;
                        return row;
                    })
                    .ToList();

                // Lấy tên dịch vụ (nếu cần hiển thị)
                var serviceName = await _db.Services
                    .Where(s => s.Id == serviceId)
                    .Select(s => s.Name)
                    .FirstOrDefaultAsync();

                var model = new ServicePatientResultReport
                {
                    ServiceId = serviceId,
                    ServiceName = serviceName,
                    FromDate = start,
                    ToDate = totime,
                    TestCodes = testCodes,
                    Rows = rows
                };

                return model;
            }
            catch (Exception ex)
            {
                var errorMessage = ex.GetBaseException().Message;

                System.Diagnostics.Debug.WriteLine(
                    $"LC_GetPatientResultByServiceXN lỗi: {errorMessage}");

                System.Diagnostics.Debug.WriteLine(ex.ToString());

                throw;
            }
        }

        public async Task<ServicePatientCDHAResultReport> LC_GetPatientResultByServiceCDHA(
            long serviceId,
            DateTime fromtime,
            DateTime totime,
            string location
        )
        {
            var start = fromtime.Date;
            var end = totime.Date.AddDays(1);

            // Base query: ResultCDHA theo service + thời gian + join Patient + Service
            var query =
                from r in _db.ResultCDHAs
                join p in _db.Patients on r.PatientId equals p.Id
                join s in _db.Services on r.ServiceId equals s.Id
                where r.Active == true
                      && p.Active == true
                      && r.ServiceId == serviceId
                      && p.InsertTime >= start
                      && p.InsertTime < end
                select new
                {
                    Patient = p,
                    ResultCDHA = r,
                    Service = s
                };

            // Filter theo location/category giống LC_GetReportByService
            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(x =>
                       (location == "SA" && ((x.Service.Category.Code == "SA" && x.Patient.ValidSA == true) || (x.Service.Category.Code == "SAT" && x.Patient.ValidSAT == true)))
                    || (location == "XQ" && x.Service.Category.Code == "XQ" && (x.Patient.ValidXQ == true))
                    || (location == "DDT" && x.Service.Category.Code == "DDT" && (x.Patient.ValidDDT == true))
                    || (location == "TDCN" && x.Service.Category.Code == "TDCN" && (x.Patient.ValidTDCN == true))
                    || (location == "NS" && x.Service.Category.Code == "NS" && (x.Patient.ValidNS == true)));
            }

            // Chọn bản ghi mới nhất cho mỗi bệnh nhân (tránh trùng nếu có nhiều lần chụp cùng serviceId)
            var latestPerPatient = await query
                .GroupBy(x => x.Patient.Id)
                .Select(g => g
                    .OrderByDescending(x => x.ResultCDHA.UpdateTime ?? x.ResultCDHA.InsertTime)
                    .ThenByDescending(x => x.ResultCDHA.Id)
                    .FirstOrDefault())
                .ToListAsync();

            var rows = latestPerPatient
                .Where(x => x != null)
                .Select((x, index) => new ServicePatientCDHAResultRow
                {
                    STT = index + 1,
                    PatientId = x.Patient.Id,
                    PatientName = x.Patient.PatientName,
                    Gender = x.Patient.Sex,
                    Age = x.Patient?.Age,
                    MaDotKham = x.Patient.MaDotKham,
                    MaBenhAn = x.Patient.MaBenhAn,
                    Seq = x.Patient.Seq,
                    Description = x.ResultCDHA.Description,
                    Result = x.ResultCDHA.Result,
                    Suggest = x.ResultCDHA.Suggest,
                    DateSearch = x.Patient.InsertTime,
                })
                .ToList();

            var serviceName = await _db.Services
                .Where(s => s.Id == serviceId)
                .Select(s => s.Name)
                .FirstOrDefaultAsync();

            return new ServicePatientCDHAResultReport
            {
                ServiceId = serviceId,
                ServiceName = serviceName,
                Location = location,
                FromDate = start,
                ToDate = totime,
                Rows = rows
            };
        }

        public async Task<PatientServiceResultReport> LC_GetPatientResultByDateRangeXN(DateTime fromtime, DateTime totime)
        {
            // Chuẩn khoảng ngày [from, to]
            var start = fromtime.Date;
            var end = totime.Date.AddDays(1);

            // Query gốc: lấy tất cả kết quả XN trong khoảng ngày
            var baseQuery =
                from r in _db.ResultXNs
                join p in _db.Patients on r.PatientId equals p.Id
                where r.Active == true
                      && p.Active == true
                      && p.InsertTime >= start
                      && p.InsertTime < end
                      && (p.ValidXN == true || p.ProcessXN == true)
                      && r.ServiceId != null
                select new
                {
                    Patient = p,
                    ResultXN = r
                };

            // Lấy danh sách ServiceId xuất hiện
            var serviceIds = await baseQuery
                .Select(x => x.ResultXN.ServiceId)
                .Where(id => id != null)
                .Distinct()
                .Cast<long>()
                .ToListAsync();

            // Lấy thông tin service
            var services = await _db.Services
                .Where(s => serviceIds.Contains(s.Id))
                .Select(s => new ServiceLookupModel
                {
                    Id = s.Id,
                    Name = s.Name
                })
                .ToListAsync();

            // Lấy danh sách TestCodeId xuất hiện trong toàn bộ khoảng ngày
            var testCodeIds = await baseQuery
                .Select(x => x.ResultXN.TestCodeId)
                .Where(id => id != null)
                .Distinct()
                .Cast<long>()
                .ToListAsync();

            // Lấy thông tin TestCode để dựng cột
            var testCodes = await _db.TestCodes
                .Where(tc => testCodeIds.Contains(tc.Id) && tc.Active == true)
                .OrderBy(tc => tc.PrintOrder ?? int.MaxValue)
                .ThenBy(tc => tc.Id)
                .Select(tc => new TestCodeColumnModel
                {
                    Id = tc.Id,
                    Code = tc.Code,
                    Name = tc.Name,
                    Unit = tc.Unit
                })
                .ToListAsync();

            // Lấy kết quả mới nhất cho mỗi (PatientId, ServiceId, TestCodeId)
            var latestResults = await baseQuery
                .Where(x => x.ResultXN.TestCodeId != null && x.ResultXN.ServiceId != null)
                .GroupBy(x => new
                {
                    PatientId = x.Patient.Id,
                    ServiceId = x.ResultXN.ServiceId,
                    TestCodeId = x.ResultXN.TestCodeId
                })
                .Select(g => g
                    .OrderByDescending(x => x.ResultXN.ValidTime ?? x.ResultXN.InsertTime)
                    .ThenByDescending(x => x.ResultXN.Id)
                    .FirstOrDefault())
                .ToListAsync();

            // Build từng dòng theo (PatientId + ServiceId)
            var rows = latestResults
                .GroupBy(x => new
                {
                    PatientId = x.Patient.Id,
                    ServiceId = x.ResultXN.ServiceId
                })
                .Select((g, index) =>
                {
                    var first = g.First();
                    var patient = first.Patient;
                    var serviceId = first.ResultXN.ServiceId ?? 0;

                    var serviceName = services
                        .Where(s => s.Id == serviceId)
                        .Select(s => s.Name)
                        .FirstOrDefault();

                    var row = new PatientServiceResultRow
                    {
                        STT = index + 1,
                        PatientId = patient.Id,
                        PatientName = patient.PatientName,
                        Gender = patient.Sex,
                        Age = patient?.Age,
                        MaDotKham = patient.MaDotKham,
                        MaBenhAn = patient.MaBenhAn,
                        Seq = patient.Seq,
                        Sid = patient.Sid,
                        DateSearch = patient.InsertTime,

                        ServiceId = serviceId,
                        ServiceName = serviceName
                    };

                    // Gán giá trị kết quả theo từng TestCode
                    foreach (var tc in testCodes)
                    {
                        var result = g.FirstOrDefault(x => x.ResultXN.TestCodeId == tc.Id);
                        row.ResultsByTestCode[tc.Id] = result?.ResultXN?.Result;
                    }

                    return row;
                })
                .OrderBy(x => x.DateSearch)
                .ThenBy(x => x.PatientName)
                .ThenBy(x => x.ServiceName)
                .ToList();

            var model = new PatientServiceResultReport
            {
                FromDate = start,
                ToDate = totime,
                TestCodes = testCodes,
                Rows = rows
            };

            return model;
        }

        public async Task<XNServiceByDateReport> LC_GetPatientServicesByDateXN(DateTime fromtime, DateTime totime)
        {
            var start = fromtime.Date;
            var end = totime.Date.AddDays(1);
            var currentYear = DateTime.Now.Year;

            var baseQuery =
                from r in _db.ResultXNs
                join p in _db.Patients on r.PatientId equals p.Id
                join s in _db.Services on r.ServiceId equals s.Id
                join u in _db.Users on p.UserReturnResultXN equals u.Id into userJoin
                from u in userJoin.DefaultIfEmpty()
                where r.Active == true
                      && p.Active == true
                      && r.ServiceId != null
                      && p.InsertTime >= start
                      && p.InsertTime < end
                      && (p.ValidXN == true || p.ProcessXN == true)
                select new
                {
                    Patient = p,
                    ResultXN = r,
                    Service = s,
                    User = u
                };

            var distinctRows = await baseQuery
                .GroupBy(x => new
                {
                    Date = x.Patient.InsertTime.HasValue ? x.Patient.InsertTime.Value.Date : start,
                    PatientId = x.Patient.Id,
                    ServiceId = x.ResultXN.ServiceId.Value
                })
                .Select(g => g
                    .OrderByDescending(x => x.ResultXN.ValidTime ?? x.ResultXN.InsertTime)
                    .ThenByDescending(x => x.ResultXN.Id)
                    .FirstOrDefault())
                .ToListAsync();

            if (distinctRows == null || !distinctRows.Any())
            {
                return new XNServiceByDateReport
                {
                    FromDate = start,
                    ToDate = totime,
                    DateGroups = new List<XNServiceByDateGroup>()
                };
            }

            int? CalculateAgeFromDate(DateTime? ageDate)
            {
                if (!ageDate.HasValue)
                    return null;

                return currentYear - ageDate.Value.Year;
            }

            string MapObjectName(string benhAn)
            {
                if (string.IsNullOrWhiteSpace(benhAn))
                    return "";

                switch (benhAn.Trim().ToLower())
                {
                    case "pakage":
                        return "Khám sức khỏe";
                    case "out":
                        return "Dịch vụ";
                    case "in":
                        return "Nội trú";
                    default:
                        return benhAn;
                }
            }

            var result = new XNServiceByDateReport
            {
                FromDate = start,
                ToDate = totime,
                DateGroups = distinctRows
                    .Where(x => x != null && x.Patient.InsertTime.HasValue)
                    .GroupBy(x => x.Patient.InsertTime.Value.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new XNServiceByDateGroup
                    {
                        Date = g.Key,
                        Rows = g
                            .OrderBy(x => x.Patient.Seq)
                            .ThenBy(x => x.Patient.PatientName)
                            .ThenBy(x => x.Service.Name)
                            .Select((x, index) => new XNServiceByDateRow
                            {
                                STT = index + 1,
                                Date = g.Key,
                                PatientName = x.Patient.PatientName,
                                Gender = x.Patient.Sex,
                                AgeValue = CalculateAgeFromDate(x.Patient.Age),
                                ObjectName = MapObjectName(x.Patient.BenhAn),
                                Address = x.Patient.Address,
                                ServiceName = x.Service.Name,
                                ReaderName = x.User != null ? x.User.Name : ""
                            })
                            .ToList()
                    })
                    .ToList()
            };

            return result;
        }

        public async Task<PatientServiceGroupedResultReport> LC_GetPatientResultGroupedByServiceXN(DateTime fromtime, DateTime totime)
        {
            var start = fromtime.Date;
            var end = totime.Date.AddDays(1);

            var baseQuery =
                from r in _db.ResultXNs
                join p in _db.Patients on r.PatientId equals p.Id
                where r.Active == true
                      && p.Active == true
                      && r.ServiceId != null
                      && r.TestCodeId != null
                      && p.InsertTime >= start
                      && p.InsertTime < end
                      && (p.ValidXN == true || p.ProcessXN == true)
                select new
                {
                    Patient = p,
                    ResultXN = r
                };

            // Lấy kết quả mới nhất theo từng Patient + Service + TestCode
            var latestResults = await baseQuery
                .GroupBy(x => new
                {
                    PatientId = x.Patient.Id,
                    ServiceId = x.ResultXN.ServiceId,
                    TestCodeId = x.ResultXN.TestCodeId
                })
                .Select(g => g
                    .OrderByDescending(x => x.ResultXN.ValidTime ?? x.ResultXN.InsertTime)
                    .ThenByDescending(x => x.ResultXN.Id)
                    .FirstOrDefault())
                .ToListAsync();

            if (latestResults == null || !latestResults.Any())
            {
                return new PatientServiceGroupedResultReport
                {
                    FromDate = start,
                    ToDate = totime,
                    Services = new List<ServiceGroupedResultBlock>()
                };
            }

            var serviceIds = latestResults
                .Where(x => x?.ResultXN?.ServiceId != null)
                .Select(x => x.ResultXN.ServiceId.Value)
                .Distinct()
                .ToList();

            var testCodeIds = latestResults
                .Where(x => x?.ResultXN?.TestCodeId != null)
                .Select(x => x.ResultXN.TestCodeId.Value)
                .Distinct()
                .ToList();

            var serviceLookup = await _db.Services
                .Where(s => serviceIds.Contains(s.Id))
                .Select(s => new
                {
                    s.Id,
                    s.Name
                })
                .ToListAsync();

            var testCodeLookup = await _db.TestCodes
                .Where(tc => testCodeIds.Contains(tc.Id) && tc.Active == true)
                .Select(tc => new
                {
                    tc.Id,
                    tc.Code,
                    tc.Name,
                    tc.Unit,
                    PrintOrder = tc.PrintOrder
                })
                .ToListAsync();

            var result = new PatientServiceGroupedResultReport
            {
                FromDate = start,
                ToDate = totime,
                Services = new List<ServiceGroupedResultBlock>()
            };

            // Group theo service
            var serviceGroups = latestResults
                .Where(x => x?.ResultXN?.ServiceId != null)
                .GroupBy(x => x.ResultXN.ServiceId.Value)
                .OrderBy(g =>
                {
                    var svc = serviceLookup.FirstOrDefault(s => s.Id == g.Key);
                    return svc != null ? svc.Name : string.Empty;
                })
                .ToList();

            foreach (var serviceGroup in serviceGroups)
            {
                var serviceId = serviceGroup.Key;
                var serviceName = serviceLookup
                    .Where(s => s.Id == serviceId)
                    .Select(s => s.Name)
                    .FirstOrDefault();

                // TestCode chỉ của service này
                var currentServiceTestCodes = serviceGroup
                    .Where(x => x?.ResultXN?.TestCodeId != null)
                    .Select(x => x.ResultXN.TestCodeId.Value)
                    .Distinct()
                    .ToList();

                var testCodes = testCodeLookup
                    .Where(tc => currentServiceTestCodes.Contains(tc.Id))
                    .OrderBy(tc => tc.PrintOrder ?? int.MaxValue)
                    .ThenBy(tc => tc.Id)
                    .Select(tc => new TestCodeColumnModel
                    {
                        Id = tc.Id,
                        Code = tc.Code,
                        Name = tc.Name,
                        Unit = tc.Unit
                    })
                    .ToList();

                // Mỗi dòng là 1 bệnh nhân trong service này
                var rows = serviceGroup
                    .GroupBy(x => x.Patient.Id)
                    .Select((g, index) =>
                    {
                        var first = g.First().Patient;

                        var row = new ServiceGroupedPatientRow
                        {
                            STT = index + 1,
                            PatientId = first.Id,
                            PatientName = first.PatientName,
                            Gender = first.Sex,
                            Age = first?.Age,
                            MaDotKham = first.MaDotKham,
                            MaBenhAn = first.MaBenhAn,
                            Seq = first.Seq,
                            Sid = first.Sid,
                            DateSearch = first.InsertTime,
                            ResultsByTestCode = new Dictionary<long, string>()
                        };

                        foreach (var tc in testCodes)
                        {
                            var item = g.FirstOrDefault(x => x.ResultXN.TestCodeId == tc.Id);
                            row.ResultsByTestCode[tc.Id] = item?.ResultXN?.Result;
                        }

                        return row;
                    })
                    .OrderBy(x => x.DateSearch)
                    .ThenBy(x => x.PatientName)
                    .ToList();

                result.Services.Add(new ServiceGroupedResultBlock
                {
                    ServiceId = serviceId,
                    ServiceName = serviceName,
                    TestCodes = testCodes,
                    Rows = rows
                });
            }

            return result;
        }
        public async Task<ServiceUsageMatrixReport> LC_GetServiceUsageByMaDotKham(string maDotKham)
        {
            try
            {
                // 1. Lọc danh sách bệnh nhân theo MaDotKham, DISTINCT theo PatientId
                var patients = await _db.Patients
                    .Where(p => p.Active == true && p.MaDotKham == maDotKham)
                    .GroupBy(p => new
                    {
                        p.PatientId,
                        p.MaBenhAn,
                        p.Seq,
                        p.PatientName
                    })
                    .Select(g => new
                    {
                        // Lấy 1 Id đại diện (nếu cần dùng tới Id DB)
                        Id = g.Max(x => x.Id),
                        g.Key.PatientId,
                        g.Key.Seq,
                        g.Key.MaBenhAn,
                        g.Key.PatientName
                    })
                    .OrderBy(p => p.Seq)
                    .ToListAsync();

                var report = new ServiceUsageMatrixReport
                {
                    MaDotKham = maDotKham
                };

                if (!patients.Any())
                {
                    // Không có bệnh nhân => trả về report rỗng
                    return report;
                }

                var patientIds = patients.Select(p => p.PatientId).ToList();

                // 2. Lấy các (Patient, Service) từ CĐHA
                // Rule: ResultCDHA.Result khác null, khác rỗng
                // Chỉ lấy các dịch vụ thuộc nhóm CDHA (SA, SAT, XQ, NS, DDT, TDCN)
                var cdhaUsage = await (
                    from patient in _db.Patients
                    join result in _db.ResultCDHAs on patient.Id equals result.PatientId
                    join service in _db.Services on result.ServiceId equals service.Id
                    where patientIds.Contains(patient.PatientId)
                          && patient.Active == true
                          && result.Active == true
                          && !string.IsNullOrWhiteSpace(result.Result)
                          && (
                                service.Category.Code == "SA"
                                || service.Category.Code == "SAT"
                                || service.Category.Code == "XQ"
                                || service.Category.Code == "NS"
                                || service.Category.Code == "DDT"
                                || service.Category.Code == "TDCN"
                             )
                    select new
                    {
                        PatientKey = patient.PatientId,  // KEY = PatientId (string)
                        ServiceId = service.Id,
                        ServiceCode = service.Code,
                        ServiceName = service.Name
                    }
                )
                .Distinct()
                .ToListAsync();

                // 3. Lấy các (Patient, Service) từ Xét nghiệm
                // Rule: ResultXN.Result khác null, khác rỗng, khác "."
                var xnUsage = await (
                    from patient in _db.Patients
                    join result in _db.ResultXNs on patient.Id equals result.PatientId
                    join service in _db.Services on result.ServiceId equals service.Id
                    where patientIds.Contains(patient.PatientId)
                          && patient.Active == true
                          && result.Active == true
                          && !string.IsNullOrWhiteSpace(result.Result)
                          && result.Result != "."
                    // Nếu muốn chỉ nhóm XN:
                    // && service.Category.Code == "XN"
                    select new
                    {
                        PatientKey = patient.PatientId,  // KEY = PatientId (string)
                        ServiceId = service.Id,
                        ServiceCode = service.Code,
                        ServiceName = service.Name
                    }
                )
                .Distinct()
                .ToListAsync();

                // 4. Gộp lại (CĐHA + XN)
                var allUsage = cdhaUsage.Concat(xnUsage).ToList();

                if (!allUsage.Any())
                {
                    // Không có dịch vụ nào có kết quả
                    report.Rows = patients.Select(p => new ServiceUsageRow
                    {
                        PatientDbId = p.Id,
                        PatientId = p.PatientId,
                        Seq = p.Seq,
                        PatientName = p.PatientName
                    }).ToList();

                    return report;
                }

                // 5. Danh sách dịch vụ (cột)
                report.Services = allUsage
                    .GroupBy(x => new { x.ServiceId, x.ServiceCode, x.ServiceName })
                    .OrderBy(g => g.Key.ServiceCode)
                    .Select(g => new ServiceUsageColumnModel
                    {
                        ServiceId = g.Key.ServiceId,
                        ServiceCode = g.Key.ServiceCode,
                        ServiceName = g.Key.ServiceName,
                        TotalPatients = 0 // Sẽ được cập nhật sau
                    })
                    .ToList();

                // 6. Build dictionary usage theo từng bệnh nhân (KEY = PatientId)
                var usageByPatient = allUsage
                    .GroupBy(x => x.PatientKey)
                    .ToDictionary(
                        g => g.Key,                                        // PatientId
                        g => g.Select(u => u.ServiceId).Distinct().ToHashSet()
                    );

                // 7. Tạo từng dòng bệnh nhân (1 PatientId = 1 dòng)
                var rows = new List<ServiceUsageRow>();

                foreach (var p in patients)
                {
                    var row = new ServiceUsageRow
                    {
                        PatientDbId = p.Id,
                        PatientId = p.PatientId,
                        Seq = p.Seq,
                        MaBenhAn = p.MaBenhAn,
                        PatientName = p.PatientName,
                        ServiceMarks = new Dictionary<long, string>()
                    };

                    if (usageByPatient.TryGetValue(p.PatientId, out var usedServiceIds))
                    {
                        foreach (var svc in report.Services)
                        {
                            if (usedServiceIds.Contains(svc.ServiceId))
                            {
                                row.ServiceMarks[svc.ServiceId] = "X";
                            }
                        }
                    }

                    rows.Add(row);
                }

                report.Rows = rows;

                // 8. Tính tổng số bệnh nhân cho mỗi dịch vụ
                foreach (var svc in report.Services)
                {
                    svc.TotalPatients = rows.Count(r => r.ServiceMarks.ContainsKey(svc.ServiceId) && r.ServiceMarks[svc.ServiceId] == "X");
                }

                return report;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<ReportCategoryFilterModel>> GetXNReportCategories()
        {
            var categories = await (
                from result in _db.ResultXNs.AsNoTracking()
                join service in _db.Services.AsNoTracking()
                    on result.ServiceId equals service.Id
                where result.Active == true
                      && service.Category != null
                      && service.Category.Code != null
                      && service.Category.Code != ""
                      && service.Category.GroupId == 1
                select new
                {
                    Code = service.Category.Code,
                    Name = service.Category.Name
                })
                .Distinct()
                .OrderBy(x => x.Name)
                .ToListAsync();

            return categories
                .Select(x => new ReportCategoryFilterModel
                {
                    Code = x.Code,
                    Name = x.Name
                })
                .ToList();
        }

    }
}
