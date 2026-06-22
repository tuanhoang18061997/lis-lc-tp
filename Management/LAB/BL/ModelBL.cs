using Management.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Management.BL
{
    public class ModelBL
    {
    }

    public class SexModel
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public static async Task<List<SexModel>> GetListSexModel()
        {
            Task<List<SexModel>> task = new Task<List<SexModel>>(() =>
            {
                var lstSexModel = new List<SexModel>() { new SexModel { Id = "Nam", Name = "Nam"},
                                                         new SexModel { Id = "Nữ", Name = "Nữ"}};

                return lstSexModel;
            });
            task.Start();
            return await task;
        }
    }

    public class BenhAnModel
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public static async Task<List<BenhAnModel>> GetListBenhAnModel()
        {
            Task<List<BenhAnModel>> task = new Task<List<BenhAnModel>>(() =>
            {
                var lstBenhAnModel = new List<BenhAnModel>() { new BenhAnModel { Id = "out", Name = "Ngoại trú"},
                                                               new BenhAnModel { Id = "in", Name = "Nội trú"},
                                                               new BenhAnModel { Id = "pakage", Name = "Gói khám"}};
                return lstBenhAnModel;
            });
            task.Start();
            return await task;
        }
    }

    public class AdminModel
    {
        public static long Id { get; set; }
        public static string Code { get; set; } = "Admin";
        public static string Name { get; set; } = "Administrator";
        public static string Password { get; set; } = "23021988";
    }

    public class UserLogin
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
    }

    public class UserTypeModel
    {
        public static string XN { get; set; } = "XN";
        public static string NS { get; set; } = "NS";
        public static string NSCTC { get; set; } = "NSCTC";
        public static string SA { get; set; } = "SA";
        public static string SAT { get; set; } = "SAT";
        public static string DDT { get; set; } = "DDT";
        public static string XQ { get; set; } = "XQ";
        public static string TDCN { get; set; } = "TDCN";
    }

    public class FunctionModel
    {
        public static string XN = "XN";
        public static string NS = "NS";
        public static string NSCTC = "NSCTC ";
        public static string SA = "SA";
        public static string SAT = "SAT";
        public static string XQ = "XQ";
        public static string TDCN = "TDCN";
        public static string Settup = "Settup";
        public static string Report = "Report";
        public static string Search = "Search";

        public static string User = "User";
        public static string Function = "Function";
        public static string UserFunction = "UserFunction";

        public static string Hospital = "Hospital";
        public static string Location = "Location";
        public static string Object = "Object";
        public static string Doctor = "Doctor";

        public static string TestType = "TestType";
        public static string Category = "Category";
        public static string Service = "Service";
        public static string TestCode = "TestCode";
        public static string ServiceTest = "ServiceTest";
        public static string Device = "Device";
        public static string Map = "Map";
        public static string Sample = "Sample";

        public static string ManageData = "ManageData";
        public static string ManageConfigSystem = "ManageConfigSystem";
    }

    public class SessionKeyModel
    {
        public static readonly string _sessionUserLogin = "SessionUserLogin";
        public static readonly string _sessionHospital = "SessionHospital";
        public static readonly string _sessionDeviceSA = "SessionDeviceSA";
        public static readonly string _sessionDeviceXQ = "SessionDeviceXQ";
        public static readonly string _sessionDeviceDDT = "SessionDeviceDDT";
        public static readonly string _sessionDeviceNS = "SessionDeviceNS";
        public static readonly string _sessionDeviceNSCTC = "SessionDeviceNSCTC";
        public static readonly string _sessionDeviceTDCN = "SessionDeviceTDCN";

        // Add keys for date range session storage
        public static readonly string _sessionTimeSearchFrom = "SessionTimeSearchFrom";
        public static readonly string _sessionTimeSearchTo = "SessionTimeSearchTo";
        public static readonly string _sessionMaDotKham = "SessionMaDotKham";
    }

    public class ResultXNModel
    {
        public long patientId { get; set; }
        public string sid { get; set; }
        public DateTime returnResultTime { get; set; }
        public long userReturnResult { get; set; }
        public long id { get; set; }
        public int status { get; set; }
        public string result { get; set; }
        public bool validPrint { get; set; }
        public string note { get; set; }
    }

    public class ResultCDHAModel
    {
        public long patientId { get; set; }
        public long resultCDHAId { get; set; }
        public DateTime returnResultTime { get; set; }
        public long userReturnResult { get; set; }
        public string description { get; set; }
        public string result { get; set; }
        public string suggest { get; set; }
        public string sieuamtim { get; set; }
        public long signStoreId { get; set; }
        // tên thuộc tính khớp với FE: selectedImageIds
        public List<long> selectedImageIds { get; set; }    // nếu Id là string thì đổi sang List<string>
    }

    public class SieuAmTimModel : ICloneable
    {
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        public string sat_chieucao { get; set; }
        public string sat_cannang { get; set; }
        public string sat_bsa { get; set; }
        public string sat_bmi { get; set; }
        public string sat_dmchu { get; set; }
        public string sat_nhitrai { get; set; }
        public string sat_thatphai { get; set; }
        public string sat_ivsd { get; set; }
        public string sat_ivss { get; set; }
        public string sat_lvidd { get; set; }
        public string sat_lvids { get; set; }
        public string sat_lvpwd { get; set; }
        public string sat_lvpws { get; set; }
        public string sat_rvdd { get; set; }
        public string sat_lvedv { get; set; }
        public string sat_fs { get; set; }
        public string sat_ef { get; set; }
        public string sat_vantoctoida_haila { get; set; }
        public string sat_ea_haila { get; set; }
        public string sat_chenhaptoida_haila { get; set; }
        public string sat_gdmean_haila { get; set; }
        public string sat_dohovan_haila { get; set; }
        public string sat_loaiho_haila { get; set; }
        public string sat_hepvanhaila_haila { get; set; }
        public string sat_vantoctoida_bala { get; set; }
        public string sat_chenhaptoida_bala { get; set; }
        public string sat_chenhaptb_bala { get; set; }
        public string sat_hovan_bala { get; set; }
        public string sat_paps_bala { get; set; }
        public string sat_vantoctoida_dongmachphoi { get; set; }
        public string sat_chenhaptoida_dongmachphoi { get; set; }
        public string sat_gdmean_dongmachphoi { get; set; }
        public string sat_papm_dongmachphoi { get; set; }
        public string sat_papd_dongmachphoi { get; set; }
        public string sat_vantoctoida_dongmachchu { get; set; }
        public string sat_chenhaptoida_dongmachchu { get; set; }
        public string sat_hovandmchu_dongmachchu { get; set; }
        public string sat_hepvandmchu_dongmachchu { get; set; }
        public string sat_dongquavachliennhi { get; set; }
        public string sat_dongquavachlienthat { get; set; }
        public string sat_dongbatthuongkhac { get; set; }
        public string sat_tuthetim_2d { get; set; }
        public string sat_dmphoi_2d { get; set; }
        public string sat_mangngoaitim_2d { get; set; }
        public string sat_dmvanh_2d { get; set; }
        public string sat_nhitrai_2d { get; set; }
        public string sat_xoangvanh_2d { get; set; }
        public string sat_nhiphai_2d { get; set; }
        public string sat_vanhaila_2d { get; set; }
        public string sat_tinhmach_2d { get; set; }
        public string sat_vanbala_2d { get; set; }
        public string sat_dmchu_2d { get; set; }
        public string sat_vandmc_2d { get; set; }
        public string sat_vandmp_2d { get; set; }

        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string result { get; set; }
        public string suggest { get; set; }
    }

    public class ImageModel
    {
        public string imageString { get; set; }
        public string resultCDHAId { get; set; }
    }

    public class UserModel
    {
        public string code { get; set; }
        public string name { get; set; }
        public string pass { get; set; }
        public string mabhyt { get; set; }
        public string type { get; set; }
        public string cks { get; set; }
        public string cccd { get; set; }
        public bool active { get; set; }
    }

    public class UserFunctionModel
    {
        public string code { get; set; }
        public string functions { get; set; }
    }

    public class HospitalModel
    {
        public string code { get; set; }
        public string name { get; set; }
        public string nameen { get; set; }
        public string address { get; set; }
        public string phone { get; set; }
        public string website { get; set; }
        public string email { get; set; }
        public string logo { get; set; }
    }

    public class LocationModel
    {
        public string code { get; set; }
        public string name { get; set; }
        public bool active { get; set; }
    }

    public class ObjectModel
    {
        public string code { get; set; }
        public string name { get; set; }
        public bool active { get; set; }
    }

    public class DoctorModel
    {
        public string code { get; set; }
        public string name { get; set; }
        public bool active { get; set; }
    }

    public class TestTypeModel
    {
        public string code { get; set; }
        public string name { get; set; }
        public bool active { get; set; }
    }

    public class CategoryModel
    {
        public string code { get; set; }
        public string name { get; set; }
        public bool active { get; set; }
    }

    public class ServiceModel
    {
        public string code { get; set; }
        public string name { get; set; }
        public long categoryid { get; set; }
        public long printsampleid { get; set; }
        public bool processtype { get; set; }
        public int printorder { get; set; }
        public bool active { get; set; }
    }

    public class DeviceModel
    {
        public string code { get; set; }
        public string name { get; set; }
        public string protocol { get; set; }
        public bool processqc { get; set; }
        public string codebhyt { get; set; }
        public bool active { get; set; }
    }

    public class SampleModel
    {
        public string code { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string result { get; set; }
        public string suggest { get; set; }
        public string gender { get; set; }
        public long categoryid { get; set; }
        public long serviceid { get; set; }
        public bool active { get; set; }
    }

    public class TestCodeModel
    {
        public string code { get; set; }
        public string name { get; set; }
        public long testtypeid { get; set; }
        public long categoryid { get; set; }
        public string normalrangem { get; set; }
        public string normalrangef { get; set; }
        public string normalresult { get; set; }
        public string lowerlimit { get; set; }
        public string higherlimit { get; set; }
        public string lowerlimitf { get; set; }
        public string higherlimitf { get; set; }
        public string unit { get; set; }
        public bool istesthead { get; set; }
        public bool istestchild { get; set; }
        public string printorder { get; set; }
        public string codebhyt { get; set; }
        public string namebhyt { get; set; }
        public bool active { get; set; }
    }

    public class ServiceTestModel
    {
        public long serviceid { get; set; }
        public long testcodeid { get; set; }
    }

    public class Map01Model
    {
        public long id { get; set; }
    }

    public class MapModel
    {
        public long id { get; set; }
        public long deviceid { get; set; }
        public long testcodeid { get; set; }
        public string testcodein { get; set; }
        public string testcodein2 { get; set; }
        public string note { get; set; }
        public string? formatnumber { get; set; }
    }

    public class SettingModel
    {
        public string code { get; set; }
        public string name { get; set; }
        public string value { get; set; }
    }

    public class TotalReportService
    {
        public string User { get; set; }
        public double STT { get; set; }
        public string ServiceID { get; set; }
        public string ServiceName { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string DateNow { get; set; }
        public double TotalBHYT { get; set; }
        public double TotalBHYT_NoiTru { get; set; }
        public double TotalBHYT_NgoaiTru { get; set; }
        public double TotalTP { get; set; }
        public double TotalTP_NoiTru { get; set; }
        public double TotalTP_NgoaiTru { get; set; }
        public double TotalTP_Pakage { get; set; }
        public double TotalTP_Out { get; set; }
    }

    public class ReportService
    {
        public string SID;
        public string ServiceID;
        public string ServiceName;
        public string BenhAn;
        public string ObjectCode;
        public double Count;
    }

    public class TotalReportPatient
    {
        public string User { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string DateNow { get; set; }
        
        // Tổng số ca
        public double Total { get; set; }
        
        // Danh sách chi tiết các ca để có thể xem chi tiết
        public List<ReportPatientDetail> PatientDetails { get; set; } = new List<ReportPatientDetail>();
        
        // Giữ lại các trường cũ để tương thích ngược nếu cần (có thể xóa sau)
        [Obsolete("Không sử dụng nữa, dùng Total thay thế")]
        public double TotalBHYT { get; set; }
        [Obsolete("Không sử dụng nữa")]
        public double TotalBHYT_NoiTru { get; set; }
        [Obsolete("Không sử dụng nữa")]
        public double TotalBHYT_NgoaiTru { get; set; }
        [Obsolete("Không sử dụng nữa, dùng Total thay thế")]
        public double TotalTP { get; set; }
        [Obsolete("Không sử dụng nữa")]
        public double TotalTP_NoiTru { get; set; }
        [Obsolete("Không sử dụng nữa")]
        public double TotalTP_NgoaiTru { get; set; }
    }

    public class ReportPatient
    {
        public string PatientID;
        public string DayInSID;
        public string BenhAn;
        public string ObjectCode;
    }
    
    // Class mới để lưu chi tiết từng ca bệnh nhân
    public class ReportPatientDetail
    {
        public string PatientID { get; set; }
        public string PatientName { get; set; }
        public string Sid { get; set; }
        public string Seq { get; set; }
        public string MaBenhAn { get; set; }
        public string BenhAn { get; set; } // pakage, out, in
        public string ObjectCode { get; set; }
        public DateTime? InsertTime { get; set; }
    }

    public class ReportProcess
    {
        public string DoctorID { get; set; }
        public string DoctorName { get; set; }
        public string ServiceID { get; set; }
        public string ServiceName { get; set; }
        public double Count { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string DateNow { get; set; }
        public string User { get; set; }
        public List<ReportPatientDetail> Patients { get; set; } = new List<ReportPatientDetail>();
    }

    public class ReportCovid
    {
        public string SID { get; set; }
        public string PatientID { get; set; }
        public string PatientName { get; set; }
        public string Sex { get; set; }
        public DateTime? Age { get; set; }
        public string Address { get; set; }
        public string ServiceID { get; set; }
        public string ServiceName { get; set; }
        public string TestCode { get; set; }
        public string Testname { get; set; }
        public string Result { get; set; }
        public DateTime InsertTime { get; set; }
        public DateTime GetSampleTime { get; set; }
        public DateTime ReturnResultTime { get; set; }
        public string User { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string DateNow { get; set; }
    }

    public class Search
    {
        public long? PatientIdTable { get; set; }
        public string KeyResultForHis { get; set; }
        public long ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string Status { get; set; }      
    }

    public class PushResultHIS
    {
        public string PatientId { get; set; }
        public string PatientName { get; set; }
        public string Seq { get; set; }
        public string Sid { get; set; }
        public string TicketItemId { get; set; }
        public string TicketId { get; set; }
        public string Service { get; set; }
        public string ServiceCode { get; set; }
        public string KeyResultForHis { get; set; }
        public DateTime? InsertTime { get; set; }
        public string Push { get; set; }
    }

    public class DigitalSignModel
    {
        public string? ReferenceType { get; set; }
        public string? ReferenceKeyResult { get; set; }
        public long? SignId { get; set; }
        public string? SignUserId { get; set; }
        public string? TaxCode { get; set; }
        public string? TargetText { get; set; }
        public string? RequestUrl { get; set; }
        public string? ResponseData { get; set; }
        public int? Status { get; set; }
        public long? CreatorId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool Deleted { get; set; } = false;
    }

    public class TestCodeColumnModel
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
    }

    public class ServicePatientResultRow
    {
        public int STT { get; set; }
        public long PatientId { get; set; }
        public string PatientName { get; set; }
        public string Gender { get; set; }
        public DateTime? Age { get; set; }
        public string MaDotKham { get; set; }
        public string MaBenhAn { get; set; }
        public string Seq { get; set; }
        public string Sid { get; set; }
        public DateTime? DateSearch { get; set; }

        // Kết quả theo từng TestCodeId
        public Dictionary<long, string> ResultsByTestCode { get; set; } = new Dictionary<long, string>();
    }

    public class ServicePatientResultReport
    {
        public long ServiceId { get; set; }
        public string ServiceName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public List<TestCodeColumnModel> TestCodes { get; set; } = new List<TestCodeColumnModel>();
        public List<ServicePatientResultRow> Rows { get; set; } = new List<ServicePatientResultRow>();
    }

    public class ServicePatientCDHAResultRow
    {
        public int STT { get; set; }
        public long PatientId { get; set; }

        public string PatientName { get; set; }
        public string Gender { get; set; }
        public DateTime? Age { get; set; }
        public string MaDotKham { get; set; }
        public string MaBenhAn { get; set; }

        public string Seq { get; set; }
        public string Description { get; set; }
        public string Result { get; set; }
        public string Suggest { get; set; }
        public DateTime? DateSearch { get; set; }
    }

    public class ServicePatientCDHAResultReport
    {
        public long ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string Location { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public List<ServicePatientCDHAResultRow> Rows { get; set; } = new List<ServicePatientCDHAResultRow>();
    }

    public class ServiceUsageMatrixReport
    {
        public string MaDotKham { get; set; }

        /// <summary>
        /// Danh sách các dịch vụ sẽ hiển thị thành cột.
        /// </summary>
        public List<ServiceUsageColumnModel> Services { get; set; } = new();

        /// <summary>
        /// Mỗi dòng = 1 bệnh nhân.
        /// </summary>
        public List<ServiceUsageRow> Rows { get; set; } = new();
    }

    public class ServiceUsageColumnModel
    {
        public long ServiceId { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceName { get; set; }
        
        /// <summary>
        /// Tổng số bệnh nhân đã thực hiện dịch vụ này (có đánh dấu X)
        /// </summary>
        public int TotalPatients { get; set; }
    }

    public class ServiceUsageRow
    {
        public long PatientDbId { get; set; }   // Patient.Id trong DB
        public string PatientId { get; set; }   // Patient.PatientId từ HIS
        public string Seq { get; set; }
        public string MaBenhAn { get; set; }
        public string PatientName { get; set; }

        /// <summary>
        /// Key = ServiceId, Value = "X" nếu bệnh nhân có làm dịch vụ đó.
        /// UI đọc dictionary này để vẽ từng ô.
        /// </summary>
        public Dictionary<long, string> ServiceMarks { get; set; } = new();
    }
    
    public class ReportProcessPatientGroup
    {
        public string DoctorID { get; set; }
        public string DoctorName { get; set; }

        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string DateNow { get; set; }
        public string User { get; set; }

        public int TotalCount => Services?.Sum(x => x.Count) ?? 0;

        public List<ReportProcessServiceDetail> Services { get; set; } = new List<ReportProcessServiceDetail>();
    }

    public class ReportProcessServiceDetail
    {
        public string ServiceID { get; set; }
        public string ServiceName { get; set; }

        public int Count => Patients?.Count ?? 0;

        public List<ReportPatientDetail> Patients { get; set; } = new List<ReportPatientDetail>();
    }

    public class ReportProcessFlatRow
    {
        public string DoctorID { get; set; }
        public string DoctorName { get; set; }

        public string ServiceID { get; set; }
        public string ServiceName { get; set; }

        public string PatientID { get; set; }
        public string PatientName { get; set; }
        public string Sid { get; set; }
        public string Seq { get; set; }
        public string MaBenhAn { get; set; }
        public DateTime? InsertTime { get; set; }
    }

    public class ReportCategoryFilterModel
    {
        public string Code { get; set; }

        public string Name { get; set; }
    }
}
