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
    public class DeviceController : Controller
    {
        public readonly ILogger<DeviceController> _logger;
        public readonly DeviceBL _deviceBL;

        public DeviceController(ILogger<DeviceController> logger, DeviceBL deviceBL)
        {
            _logger = logger;
            _deviceBL = deviceBL;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["lstDevice"] = await _deviceBL.GetList(true);
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Search(string value)
        {
            ViewData["lstDevice"] = await _deviceBL.GetListByValue(value, true);
            return PartialView("_Device_List");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetInfo(string code)
        {
            ViewData["Device"] = await _deviceBL.Get(code);
            return PartialView("_Device_Info");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            ViewData["lstDevice"] = await _deviceBL.GetList(true);
            return PartialView("_Device_List");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] DeviceModel device)
        {
            var _save = false;
            if (device != null)
            {
                _save = await _deviceBL.Save(device);
            }
            return Content(_save.ToString());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete(string code)
        {
            var _delete = await _deviceBL.Delete(code);
            return Content(_delete.ToString());
        }
    }
}