using Management.Helpers;
using Management.Models;
using Management.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Management.BL
{
    public class ZnsSendRequestBL
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ZnsSendRequestBL(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }


        #region Public Main Flow

        public async Task<ZnsSendResult> SendResultXNAsync(string sid, string templateOAId, int createdBy)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var context = scope.ServiceProvider.GetRequiredService<LABContext>();
                var hospitalBL = scope.ServiceProvider.GetRequiredService<HospitalBL>();
                var portalLinkBL = scope.ServiceProvider.GetRequiredService<PortalLinkBL>();

                var runtime = await GetRuntimeContext(context, hospitalBL, templateOAId);

                var model = await BuildResultXNSendModelAsync(context, runtime, portalLinkBL, sid);
                if (model == null)
                {
                    return new ZnsSendResult
                    {
                        Success = false,
                        StatusZns = 9,
                        Message = "Không tìm thấy dữ liệu ResultXN để gửi ZNS."
                    };
                }

                if (string.IsNullOrWhiteSpace(model.Phone))
                {
                    return new ZnsSendResult
                    {
                        Success = false,
                        StatusZns = 9,
                        Message = "Bệnh nhân chưa có số điện thoại."
                    };
                }

                if (IsDuplicate(context, model.PatientId, model.Sid, runtime.Template.Id, "ResultXN"))
                {
                    return new ZnsSendResult
                    {
                        Success = false,
                        StatusZns = 9,
                        Message = "SID này đã gửi ZNS trước đó."
                    };
                }

                var clientReqId = GenerateClientReqId(model.PatientId, model.Sid);
                var phone = FormatPhoneNumber(model.Phone);

                var dataMap = ZnsTemplateHelper.BuildZnsDataMap(model);
                var templateData = ZnsTemplateHelper.BuildTemplateDataFromParams(runtime.Template.TemplateParams, dataMap);
                var renderedContent = ZnsTemplateHelper.RenderTemplateContent(runtime.Template.Content, dataMap);

                var payloadObj = new
                {
                    client_req_id = clientReqId,
                    from = runtime.Config.ZaloOaId,
                    to = phone,
                    template_id = runtime.Template.TemplateZaloId.ToString(),
                    dlr = 1,
                    template_data = templateData
                };

                var payloadJson = JsonConvert.SerializeObject(payloadObj);

                var sendLog = new ZnsSendRequest
                {
                    ClientReqId = clientReqId,
                    PatientId = model.PatientId,
                    PhoneNumber = phone,

                    ZaloOAConfigId = runtime.Config.Id,
                    ZaloOATemplateId = runtime.Template.Id,
                    TemplateZaloId = runtime.Template.TemplateZaloId,

                    NotificationType = "LAB_RESULT",
                    BusinessType = "ResultXN",
                    BusinessId = model.Sid,
                    Sid = model.Sid,

                    Title = runtime.Template.Title,
                    Content = renderedContent,
                    JsonPayloadZns = payloadJson,

                    StatusZns = 0,
                    MessageZns = "Init",
                    DLR = 1,

                    CreatedOn = DateTime.Now,
                    CreatedBy = createdBy
                };

                context.ZnsSendRequests.Add(sendLog);
                context.SaveChanges();

                var apiResult = await CallZnsApiAsync(runtime.Config, payloadJson);

                sendLog.SentAt = DateTime.Now;
                sendLog.UpdatedOn = DateTime.Now;
                sendLog.UpdatedBy = createdBy;

                if (apiResult.Response != null)
                {
                    sendLog.StatusZns = apiResult.Response.Status;

                    if (apiResult.Response.Status == 1)
                    {
                        sendLog.TrackingId = apiResult.Response.TrackingId;
                        sendLog.MessageZns = string.IsNullOrWhiteSpace(apiResult.Response.TrackingId)
                            ? "Gửi thành công"
                            : apiResult.Response.TrackingId;
                    }
                    else
                    {
                        sendLog.MessageZns = apiResult.Response.Description ?? "Gửi ZNS thất bại.";

                        if (apiResult.Response.ErrorCode.HasValue)
                        {
                            sendLog.MessageZns = $"[{apiResult.Response.ErrorCode}] {sendLog.MessageZns}";
                        }
                    }
                }
                else
                {
                    sendLog.StatusZns = 9;
                    sendLog.MessageZns = string.IsNullOrWhiteSpace(apiResult.RawResponse)
                        ? "Không đọc được phản hồi từ ZNS."
                        : apiResult.RawResponse;
                }

                context.SaveChanges();

                return new ZnsSendResult
                {
                    Success = apiResult.Response != null && apiResult.Response.Status == 1,
                    ZnsSendRequestId = sendLog.Id,
                    StatusZns = sendLog.StatusZns,
                    ErrorCode = apiResult.Response?.ErrorCode,
                    Message = sendLog.MessageZns,
                    TrackingId = sendLog.TrackingId
                };
            }
            catch (Exception ex)
            {
                return new ZnsSendResult
                {
                    Success = false,
                    StatusZns = 9,
                    Message = ex.Message
                };
            }
        }

        #endregion

        #region DB Build Model

        private async Task<ZnsResultXNSendModel> BuildResultXNSendModelAsync(LABContext context, ZnsRuntimeContext runtime, PortalLinkBL portalLinkBL, string sid)
        {
            if (string.IsNullOrWhiteSpace(sid))
                return null;

            var patient = context.Patients
                .AsNoTracking()
                .Where(x => x.Sid == sid)
                .OrderBy(x => x.Id)
                .FirstOrDefault();

            if (patient == null)
                return null;
            //var portalResult = await portalLinkBL.BuildPortalLinkAsync(patient.MaBenhAn);

            return new ZnsResultXNSendModel
            {
                PatientId = patient.Id,
                Sid = sid,

                // sửa đúng field DB thật của bạn
                PatientName = patient.PatientName,
                Phone = patient.Phone,
                MaBenhAn = patient.MaBenhAn,

                HospitalId = runtime.HospitalId,

                Service = "Xét nghiệm",
                Status = "Đã có kết quả"
                //UID = portalResult != null && portalResult.Success ? portalResult.PortalUrl : "", // build link portal xem kết quả 
            };
        }

        private string BuildUidBySid(dynamic patient, string sid)
        {
            try
            {
                // Bạn sửa lại theo rule thật của LIS
                // Ví dụ mong muốn:
                // /0002153/BA2602068/0002153-BA2602068.pdf

                var patientCode = patient.PatientId ?? "";
                var maBenhAn = patient.MaBenhAn ?? "";

                if (string.IsNullOrWhiteSpace(patientCode) || string.IsNullOrWhiteSpace(maBenhAn))
                    return "";

                return $"/{patientCode}/{maBenhAn}/{patientCode}-{maBenhAn}.pdf";
            }
            catch
            {
                return "";
            }
        }

        #endregion

        #region Dynamic Template Data

        private Dictionary<string, object> BuildZnsDataMap(ZnsBusinessSendModel model)
        {
            return new Dictionary<string, object>
            {
                // Tên khách
                { "customer_name", model.PatientName ?? "" },
                { "CustomerName", model.PatientName ?? "" },

                // Mã bệnh nhân / mã khách hàng
                { "patient_code", model.MaBenhAn ?? "" },
                { "Customer_id", model.MaBenhAn ?? "" },

                // Số điện thoại
                { "Phone", model.Phone ?? "" },
                { "phone", model.Phone ?? "" },

                // SID / UID / trạng thái
                { "SID", model.Sid ?? "" },
                { "UID", model.UID ?? "" },
                { "Status", model.StatusText ?? "Đã có kết quả" },
                { "status", model.StatusText ?? "Đã có kết quả" },

                // Dịch vụ / bệnh viện
                { "Service", model.ServiceName ?? "Xét nghiệm máu" },
                { "service_name", model.ServiceName ?? "Xét nghiệm máu" },
                { "HospitalName", model.HospitalName ?? "" }
            };
        }

        #endregion

        #region Duplicate / Utility

        private bool IsDuplicate(LABContext context, long patientId, string sid, int zaloOATemplateId, string businessType)
        {
            return context.ZnsSendRequests
                .AsNoTracking()
                .Any(x =>
                    x.PatientId == patientId
                    && x.Sid == sid
                    && x.ZaloOATemplateId == zaloOATemplateId
                    && x.BusinessType == businessType
                    && (x.StatusZns == 1 || x.StatusZns == 3 || x.StatusZns == 4));
        }

        private string GenerateClientReqId(long patientId, string sid)
        {
            return $"LIS_{patientId}_{sid}_{DateTime.Now:yyyyMMddHHmmssfff}";
        }

        private string FormatPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return "";

            phone = phone.Trim().Replace(" ", "").Replace(".", "");

            if (phone.StartsWith("0"))
            {
                phone = "84" + phone.Substring(1);
            }

            return phone;
        }

        #endregion

        #region Call API

        private async Task<ZnsApiCallResult> CallZnsApiAsync(ZaloOAConfig config, string payloadJson)
        {
            using (var client = new HttpClient())
            {
                var authToken = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes($"{config.Username}:{config.Password}"));

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", authToken);

                var httpContent = new StringContent(payloadJson, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(config.ApiUrl, httpContent);
                var rawResponse = await response.Content.ReadAsStringAsync();

                ZnsApiResponse znsResponse = null;

                try
                {
                    znsResponse = JsonConvert.DeserializeObject<ZnsApiResponse>(rawResponse);
                }
                catch
                {
                    znsResponse = null;
                }

                return new ZnsApiCallResult
                {
                    Response = znsResponse,
                    RawResponse = rawResponse
                };
            }
        }

        #endregion

        private async Task<ZnsRuntimeContext> GetRuntimeContext(LABContext context, HospitalBL hospitalBL, string zaloOATemplateId)
        {
            var hospital = await hospitalBL.GetHospital();
            if (hospital == null)
            {
                throw new Exception("Không xác định được cơ sở đang sử dụng.");
            }

            var hospitalId = hospital.Id;

            var config = context.ZaloOAConfigs
                .AsNoTracking()
                .Where(x => x.IsActive && x.HospitalId == hospitalId)
                .OrderByDescending(x => x.IsDefault)
                .ThenByDescending(x => x.Id)
                .FirstOrDefault();

            if (config == null)
            {
                throw new Exception("Chưa cấu hình Zalo OA cho cơ sở hiện tại.");
            }

            var template = context.ZaloOATemplates
                .AsNoTracking()
                .Where(x => x.IsActive && x.HospitalId == hospitalId && x.TemplateZaloId == zaloOATemplateId)
                .FirstOrDefault();

            if (template == null)
            {
                throw new Exception("Không tìm thấy template Zalo OA thuộc cơ sở hiện tại.");
            }

            return new ZnsRuntimeContext
            {
                HospitalId = hospitalId,
                Config = config,
                Template = template
            };
        }
    }
    public class ZnsRuntimeContext
    {
        public long HospitalId { get; set; }
        public string HospitalName { get; set; }
        public ZaloOAConfig Config { get; set; }
        public ZaloOATemplate Template { get; set; }
    }
    public class ZnsApiCallResult
    {
        public ZnsApiResponse Response { get; set; }
        public string RawResponse { get; set; }
    }

    public class ZnsBusinessSendModel
    {
        public long PatientId { get; set; }
        public string Sid { get; set; }

        public string PatientName { get; set; }
        public string MaBenhAn { get; set; }
        public string Phone { get; set; }

        public int HospitalId { get; set; }
        public string HospitalName { get; set; }

        public string ServiceName { get; set; }
        public string StatusText { get; set; }
        public string UID { get; set; }

        public string Title { get; set; }
        public string Content { get; set; }
    }

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