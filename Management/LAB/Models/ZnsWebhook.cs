using System;

namespace Management.Models
{
    public class ZnsWebhook
    {
        public long Id { get; set; }
        public string Source { get; set; }
        public string ExternalId { get; set; }
        public string ClientReqId { get; set; }
        public string TrackingId { get; set; }
        public string Content { get; set; }
        public string SourceHost { get; set; }
        public byte ProcessStatus { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ProcessedOn { get; set; }
    }

    public class SouthTelecomDlrRequest
    {
        public string ott { get; set; }
        public int? ottstatus { get; set; }
        public string otterrorcode { get; set; }
        public string smsid { get; set; }
        public long? receivedts { get; set; }
        public long? deliveredts { get; set; }
        public int? status { get; set; }
        public string user { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public string text { get; set; }
        public int? errorcode { get; set; }
        public int? smscount { get; set; }
        public string carrier { get; set; }
        public int? mnp { get; set; }
    }
}