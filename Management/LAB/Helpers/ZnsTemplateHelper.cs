using Management.Models.DTO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Management.Helpers
{
    public static class ZnsTemplateHelper
    {
        public static object BuildTemplateDataFromParams(string templateParamsJson, Dictionary<string, object> dataMap)
        {
            IDictionary<string, object> expando = new ExpandoObject();

            if (string.IsNullOrWhiteSpace(templateParamsJson))
                return (ExpandoObject)expando;

            List<string> keys;

            try
            {
                keys = JsonConvert.DeserializeObject<List<string>>(templateParamsJson) ?? new List<string>();
            }
            catch
            {
                keys = new List<string>();
            }

            foreach (var key in keys.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct())
            {
                if (dataMap.ContainsKey(key) && dataMap[key] != null)
                {
                    expando[key] = dataMap[key];
                }
                else
                {
                    expando[key] = "";
                }
            }

            return (ExpandoObject)expando;
        }

        public static string RenderTemplateContent(string templateContent, Dictionary<string, object> dataMap)
        {
            if (string.IsNullOrWhiteSpace(templateContent))
                return null;

            var result = templateContent;

            foreach (var item in dataMap)
            {
                result = result.Replace($"<{item.Key}>", item.Value?.ToString() ?? "");
            }

            return result;
        }

        public static Dictionary<string, object> BuildZnsDataMap(ZnsResultXNSendModel model)
        {
            return new Dictionary<string, object>
            {
                // Tên khách
                { "CustomerName", model.PatientName ?? "" },
                { "customer_name", model.PatientName ?? "" },

                // Mã khách / mã bệnh nhân
                { "Customer_id", model.MaBenhAn ?? "" },
                { "patient_code", model.MaBenhAn ?? "" },

                // Số điện thoại
                { "Phone", model.Phone ?? "" },
                { "phone", model.Phone ?? "" },

                // SID / UID / PDF
                { "SID", model.Sid ?? " " },
                { "UID", model.UID ?? " " },
                { "PdfUrl", model.PdfUrl ?? " " },

                // Dịch vụ / trạng thái
                { "Service", model.Service ?? " " },
                { "service_name", model.Service ?? " " },
                { "Status", model.Status ?? " " },
                { "status", model.Status ?? " " },

                // Nếu sau này template cần thêm
                { "MaBenhAn", model.MaBenhAn ?? "" }
            };
        }
    }
}