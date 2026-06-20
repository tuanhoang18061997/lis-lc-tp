using Management.BL;

namespace Management.Models
{
    public class PatientServiceResultReport
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public List<TestCodeColumnModel> TestCodes { get; set; } = new List<TestCodeColumnModel>();
        public List<PatientServiceResultRow> Rows { get; set; } = new List<PatientServiceResultRow>();
    }

    public class PatientServiceResultRow
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

        public long ServiceId { get; set; }
        public string ServiceName { get; set; }

        public Dictionary<long, string> ResultsByTestCode { get; set; } = new Dictionary<long, string>();
    }

    public class ServiceLookupModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
    //-------------------------Model dùng cho báo cáo type = 4--------------------------------------------
    public class PatientServiceGroupedResultReport
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public List<ServiceGroupedResultBlock> Services { get; set; } = new List<ServiceGroupedResultBlock>();
    }
    public class ServiceGroupedResultBlock
    {
        public long ServiceId { get; set; }
        public string ServiceName { get; set; }

        public List<TestCodeColumnModel> TestCodes { get; set; } = new List<TestCodeColumnModel>();
        public List<ServiceGroupedPatientRow> Rows { get; set; } = new List<ServiceGroupedPatientRow>();
    }

    public class ServiceGroupedPatientRow
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

        public Dictionary<long, string> ResultsByTestCode { get; set; } = new Dictionary<long, string>();
    }
    // ----------------------------------- Model dùng cho báo cáo type = 5 ------------------------------------\
    public class XNServiceByDateReport
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<XNServiceByDateGroup> DateGroups { get; set; } = new List<XNServiceByDateGroup>();
    }

    public class XNServiceByDateGroup
    {
        public DateTime Date { get; set; }
        public List<XNServiceByDateRow> Rows { get; set; } = new List<XNServiceByDateRow>();
        public int Total => Rows?.Count ?? 0;
    }

    public class XNServiceByDateRow
    {
        public int STT { get; set; }
        public string PatientName { get; set; }
        public string Gender { get; set; }
        public int? AgeValue { get; set; }
        public string ObjectName { get; set; }
        public string Address { get; set; }
        public string ServiceName { get; set; }
        public string ReaderName { get; set; }
        public DateTime Date { get; set; }
    }
}
