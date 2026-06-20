using Newtonsoft.Json;

namespace Management.Models
{
    public class ZnsApiResponse
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("tracking_id")]
        public string TrackingId { get; set; }

        [JsonProperty("errorcode")]
        public int? ErrorCode { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}