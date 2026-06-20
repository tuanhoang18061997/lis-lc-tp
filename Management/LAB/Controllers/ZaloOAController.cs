using Management.BL;
using Management.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace Management.Controllers
{
    public class ZaloOAController : Controller
    {
        private readonly ZnsSendRequestBL _znsSendRequestBL;
        private readonly ZnsWebhookBL _znsWebhookBL;
        private readonly ZaloOATemplateBL _zaloOATemplateBL;
        private readonly LABContext _context;

        public ZaloOAController(
            ZnsSendRequestBL znsSendRequestBL,
            ZnsWebhookBL znsWebhookBL,
            ZaloOATemplateBL zaloOATemplateBL,
            LABContext context)
        {
            _znsSendRequestBL = znsSendRequestBL;
            _znsWebhookBL = znsWebhookBL;
            _zaloOATemplateBL = zaloOATemplateBL;
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.ZaloOATemplates = _zaloOATemplateBL.GetAll();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendResultXN([FromBody] SendResultXNZnsRequest model)
        {
            if (model == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Dữ liệu không hợp lệ"
                });
            }

            if (string.IsNullOrWhiteSpace(model.Sid))
            {
                return Json(new
                {
                    success = false,
                    message = "SID không được để trống"
                });
            }

            if (model.TemplateOAId == null )
            {
                return Json(new
                {
                    success = false,
                    message = "Template ZaloOA không hợp lệ"
                });
            }

            var result = await _znsSendRequestBL.SendResultXNAsync(
                model.Sid,
                model.TemplateOAId,
                1
            );

            return Json(new
            {
                success = result.Success,
                znsSendRequestId = result.ZnsSendRequestId,
                statusZns = result.StatusZns,
                errorCode = result.ErrorCode,
                message = result.Message,
                trackingId = result.TrackingId
            });
        }

        [HttpGet]
        public IActionResult HookSouthTelecom([FromQuery] SouthTelecomDlrRequest request)
        {
            object res;
            ZnsWebhook webhook = null;

            try
            {
                if (request == null || Request.Query == null || Request.Query.Count == 0)
                {
                    res = new
                    {
                        status = 0,
                        desc = "Request không có dữ liệu."
                    };

                    return new JsonResult(res)
                    {
                        StatusCode = 200
                    };
                }

                var rawQueryObject = Request.Query.ToDictionary(
                    x => x.Key,
                    x => x.Value.ToString()
                );

                var rawJson = JsonConvert.SerializeObject(rawQueryObject);

                var sourceHost = HttpContext.Connection.RemoteIpAddress != null
                    ? HttpContext.Connection.RemoteIpAddress.ToString()
                    : string.Empty;

                try
                {
                    webhook = new ZnsWebhook
                    {
                        ExternalId = request.smsid,
                        Source = "SOUTHTELECOM",
                        ClientReqId = request.smsid,
                        TrackingId = null,
                        Content = rawJson,
                        SourceHost = sourceHost,
                        ProcessStatus = 0,
                        ErrorMessage = null,
                        CreatedOn = DateTime.Now,
                        ProcessedOn = null
                    };

                    _context.ZnsWebhooks.Add(webhook);
                    _context.SaveChanges();
                }
                catch (Exception exSaveWebhook)
                {
                    res = new
                    {
                        status = 0,
                        desc = "Lỗi lưu webhook: " + exSaveWebhook.Message
                    };

                    return new JsonResult(res)
                    {
                        StatusCode = 200
                    };
                }

                // Mặc định: đã nhận và lưu webhook thành công
                res = new
                {
                    status = 1,
                    desc = "Success"
                };

                // Update ngược ZnsSendRequests nếu có smsid
                if (!string.IsNullOrWhiteSpace(request.smsid))
                {
                    try
                    {
                        var entity = _context.ZnsSendRequests
                            .FirstOrDefault(x => x.ClientReqId == request.smsid);

                        if (entity != null)
                        {
                            entity.StatusDLR = int.TryParse(request.otterrorcode, out var errorCode)
                                ? errorCode
                                : request.ottstatus;

                            entity.MessageZns = !string.IsNullOrWhiteSpace(request.otterrorcode)
                                ? $"DLR fail: {request.otterrorcode}"
                                : (request.ottstatus == 1 ? "DLR success" : "DLR fail");

                            entity.LastWebhookAt = DateTime.Now;
                            entity.UpdatedOn = DateTime.Now;
                            entity.UpdatedBy = 0;

                            _context.SaveChanges();
                        }

                        if (webhook != null)
                        {
                            webhook.ProcessStatus = 1;
                            webhook.ProcessedOn = DateTime.Now;
                            webhook.ErrorMessage = null;
                            _context.SaveChanges();
                        }
                    }
                    catch (Exception updateErr)
                    {
                        try
                        {
                            if (webhook != null)
                            {
                                webhook.ProcessStatus = 2;
                                webhook.ErrorMessage = updateErr.Message;
                                webhook.ProcessedOn = DateTime.Now;
                                _context.SaveChanges();
                            }
                        }
                        catch
                        {
                            // nuốt lỗi để vẫn đảm bảo HTTP 200
                        }

                        res = new
                        {
                            status = 1,
                            desc = "Success"
                        };
                    }
                }
                else
                {
                    try
                    {
                        if (webhook != null)
                        {
                            webhook.ProcessStatus = 1;
                            webhook.ProcessedOn = DateTime.Now;
                            webhook.ErrorMessage = null;
                            _context.SaveChanges();
                        }
                    }
                    catch
                    {
                        // nuốt lỗi để vẫn đảm bảo HTTP 200
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    if (webhook != null)
                    {
                        webhook.ProcessStatus = 2;
                        webhook.ErrorMessage = ex.Message;
                        webhook.ProcessedOn = DateTime.Now;
                        _context.SaveChanges();
                    }
                }
                catch
                {
                    // nuốt lỗi để vẫn đảm bảo HTTP 200
                }

                res = new
                {
                    status = 0,
                    desc = ex.Message
                };
            }

            return new JsonResult(res)
            {
                StatusCode = 200
            };
        }
    }
}