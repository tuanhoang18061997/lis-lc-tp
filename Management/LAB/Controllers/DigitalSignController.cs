using Castle.Core.Internal;
using Management.BL;
using Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Globalization;
using System.Net;

namespace Management.Controllers
{
    public class DigitalSignController : Controller
    {
        public readonly ILogger<DigitalSignController> _logger;
        public readonly DigitalSignBL _digitalSignBL;
        private readonly ToolBL _toolBL;
        public readonly UserBL _userBL;
        public readonly ResultCDHABL _resultCDHABL;
        public readonly ResultXNBL _resultXNBL;


        public DigitalSignController(ILogger<DigitalSignController> logger , DigitalSignBL digitalSignBL, ToolBL toolBL, UserBL userBL, ResultCDHABL resultCDHABL, ResultXNBL resultXNBL)
        {
            _logger = logger;
            _digitalSignBL = digitalSignBL;
            _toolBL = toolBL;
            _userBL = userBL;
            _resultCDHABL = resultCDHABL;
            _resultXNBL = resultXNBL;
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] DigitalSignModel digitalSign_model)
        {
            var _userLogin = this.GetUserLogin();
            var dateTimeNow = ToolBL.Get_DateNow();

            var _save = false;
            if (digitalSign_model != null)
            {
                _save = await _digitalSignBL.Save(digitalSign_model.ReferenceType, digitalSign_model.ReferenceKeyResult, digitalSign_model.SignId, digitalSign_model.SignUserId, digitalSign_model.TaxCode, digitalSign_model.TargetText, digitalSign_model.RequestUrl, digitalSign_model.ResponseData, digitalSign_model.Status, _userLogin.Value, dateTimeNow);
            }
            return Ok(_save);
        }

        public long? GetUserLogin()
        {
            long? userId = null;
            try
            {
                userId = long.Parse(HttpContext?.User?.Claims?.FirstOrDefault(p => p.Type == "UserId")?.Value);
            }
            catch { }
            return userId;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateSignStatus_Result(long resultCDHAId)
        {
            var _dateTime = ToolBL.Get_DateNow();
            var _userLogin = this.GetUserLogin();

            if (resultCDHAId <= 0)
                return BadRequest("resultCDHAId không hợp lệ.");

            ResultCDHA _resultCDHA;
            var _keyResult = "";
            var okUpdateResultCDHA = false;
            var okUpdateDigitalSign = false;
            try
            {
                _resultCDHA = await _resultCDHABL.GetResultCDHA(resultCDHAId);
                _keyResult = _resultCDHA.KeyResultForHis;
                okUpdateResultCDHA = await _digitalSignBL.UpdateSignStatus_CKS_CDHA(resultCDHAId, _keyResult, _userLogin.Value, _dateTime);
                okUpdateDigitalSign = await _digitalSignBL.Update(_resultCDHA.SignStoreId, _keyResult);

                if (okUpdateResultCDHA == true && okUpdateDigitalSign == true)
                {
                    return Json(new { success = true, message = "Cập nhật SignStatus thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Cập nhật SignStatus thất bại." });

                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SaveSignStoreIdForResultCDHA failed");
                return StatusCode(500, "Lỗi lưu signStoreId.");
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateSignStatus_ResultXN(long patientId)
        {
            var _dateTime = ToolBL.Get_DateNow();
            var _userLogin = this.GetUserLogin();

            if (patientId <= 0)
                return BadRequest("patientId không hợp lệ.");

            List<ResultXN> _resultXNs;
            var _keyResult = "";
            var okUpdateResultXNs = false;
            var okUpdateDigitalSign = false;
            try
            {
                _resultXNs = await _resultXNBL.GetListResultXNByPatientId_ForValidPrint(patientId);
                if(_resultXNs.Count > 0)
                {
                    _keyResult = _resultXNs[0].KeyResultForHis;
                }

                okUpdateDigitalSign = await _digitalSignBL.Update(_resultXNs[0].SignStoreId, _keyResult);
                
                foreach (var item in _resultXNs)
                {
                    if (item.Id > 0)
                    {
                        okUpdateResultXNs = await _digitalSignBL.UpdateSignStatus_CKS_XN(item.Id, item.KeyResultForHis, _userLogin.Value, _dateTime);
                    }
                }

                if (okUpdateResultXNs == true && okUpdateDigitalSign == true)
                {
                    return Json(new { success = true, message = "Cập nhật SignStatus thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Cập nhật SignStatus thất bại." });

                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SaveSignStoreIdForResultXN failed");
                return StatusCode(500, "Lỗi lưu signStoreId.");
            }
        }
    }
}