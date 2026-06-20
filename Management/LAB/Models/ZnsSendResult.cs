namespace Management.Models
{
    public class ZnsSendResult
    {
        public bool Success { get; set; }
        public long? ZnsSendRequestId { get; set; }
        public int? StatusZns { get; set; }
        public int? ErrorCode { get; set; }
        public string Message { get; set; }
        public string TrackingId { get; set; }
    }
}