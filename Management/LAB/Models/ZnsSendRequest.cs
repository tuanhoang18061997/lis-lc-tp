using System;

namespace Management.Models
{
    public class ZnsSendRequest
    {
        public long Id { get; set; }
        public string ClientReqId { get; set; }
        public string TrackingId { get; set; }

        public long? PatientId { get; set; }
        public string PhoneNumber { get; set; }

        public int? ZaloOAConfigId { get; set; }
        public int? ZaloOATemplateId { get; set; }
        public string TemplateZaloId { get; set; }

        public string NotificationType { get; set; }
        public string BusinessType { get; set; }
        public string BusinessId { get; set; }
        public string Sid { get; set; }

        public string Title { get; set; }
        public string Content { get; set; }
        public string JsonPayloadZns { get; set; }

        public int? StatusZns { get; set; }
        public string MessageZns { get; set; }

        public int? DLR { get; set; }
        public int? StatusDLR { get; set; }

        public DateTime? SentAt { get; set; }
        public DateTime? LastWebhookAt { get; set; }

        public DateTime CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int? UpdatedBy { get; set; }
    }
}