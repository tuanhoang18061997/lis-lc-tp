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
    public class SettingController : Controller
    {
        public readonly ILogger<SettingController> _logger;
        public readonly SettingBL _settingBL;
        public readonly APIBL _aPIBL;
        public readonly APIBL_MHIS _aPIBL_MHIS;

        public SettingController(ILogger<SettingController> logger, SettingBL settingBL, APIBL aPIBL, APIBL_MHIS aPIBL_MHIS)
        {
            _logger = logger;
            _settingBL = settingBL;
            _aPIBL = aPIBL;
            _aPIBL_MHIS = aPIBL_MHIS;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["lstSetting"] = await _settingBL.GetList();
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Search(string value)
        {
            ViewData["lstSetting"] = await _settingBL.GetListByValue(value);
            return PartialView("_Setting_List");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetInfo(string code)
        {
            ViewData["Setting"] = await _settingBL.GetSettingByCode(code);
            return PartialView("_Setting_Info");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            ViewData["lstSetting"] = await _settingBL.GetList();
            return PartialView("_Setting_List");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] SettingModel setting)
        {
            var _save = false;
            if (setting != null)
            {
                _save = await _settingBL.Save(setting);
            }
            return Content(_save.ToString());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete(string code)
        {
            var _delete = await _settingBL.Delete(code);
            return Content(_delete.ToString());
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Setting_Start_AutoTask()
        {
            var _status = false;
            var _settingAPIToStart = await _settingBL.GetSetting(SettingBL.SelectAPIStartAutoTask);
            if(!string.IsNullOrEmpty(_settingAPIToStart))
            {
                if (_settingAPIToStart == "1") // Đức Tâm
                {
                    _status = await _aPIBL.StartAutoTask();
                }
                else if (_settingAPIToStart == "2") // MHIS
                {
                    _status = await _aPIBL_MHIS.StartAutoTask();
                }
            }           
            return Content(_status.ToString());
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Setting_Stop_AutoTask()
        {
            //await _aPIBL.Auto_GetPatientAndService();
            var _status = false;
            var _settingAPIToStart = await _settingBL.GetSetting(SettingBL.SelectAPIStartAutoTask);
            if (!string.IsNullOrEmpty(_settingAPIToStart))
            {
                if (_settingAPIToStart == "1") // Đức Tâm
                {
                    _status = await _aPIBL.Refresh_Properties();
                }
                else if (_settingAPIToStart == "2") // MHIS
                {
                    _status = await _aPIBL_MHIS.Refresh_Properties();
                }
            }
            return Content(_status.ToString());
        }
    }
}