using Management.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Management.BL
{
    public class ZnsWebhookBL
    {
        private readonly LABContext _context;

        public ZnsWebhookBL(LABContext context)
        {
            _context = context;
        }

        public bool SaveAndProcessWebhook(string rawJson, string sourceHost)
        {
            var webhook = new ZnsWebhook
            {
                Source = "ZALO",
                Content = rawJson,
                SourceHost = sourceHost,
                ProcessStatus = 0,
                CreatedOn = DateTime.Now
            };

            _context.ZnsWebhooks.Add(webhook);
            _context.SaveChanges();

            try
            {
                var jObj = JObject.Parse(rawJson);

                webhook.TrackingId = jObj["tracking_id"]?.ToString();
                webhook.ExternalId = jObj["client_req_id"]?.ToString();

                var req = _context.ZnsSendRequests.FirstOrDefault(x =>
                    x.ClientReqId == webhook.ExternalId
                    || (!string.IsNullOrEmpty(webhook.TrackingId) && x.TrackingId == webhook.TrackingId));

                if (req != null)
                {
                    req.StatusZns = ParseStatus(jObj["status"]?.ToString());
                    req.MessageZns = jObj["description"]?.ToString();
                    req.StatusDLR = 1;
                    req.LastWebhookAt = DateTime.Now;
                    req.UpdatedOn = DateTime.Now;
                    req.UpdatedBy = 0;
                }

                webhook.ProcessStatus = 1;
                webhook.ProcessedOn = DateTime.Now;

                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                webhook.ProcessStatus = 2;
                webhook.ErrorMessage = ex.Message;
                webhook.ProcessedOn = DateTime.Now;
                _context.SaveChanges();
                return false;
            }
        }

        private int ParseStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status)) return 0;

            int value;
            return int.TryParse(status, out value) ? value : 0;
        }
    }
}