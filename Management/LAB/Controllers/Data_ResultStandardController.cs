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
    public class Data_ResultStandardController : Controller
    {
        public readonly ILogger<Data_ResultStandardController> _logger;
        public readonly ResultStandardBL _resultStandardBL;
        public readonly DeviceBL _deviceBL;

        public Data_ResultStandardController(ILogger<Data_ResultStandardController> logger, ResultStandardBL resultStandardBL, DeviceBL deviceBL)
        {
            _logger = logger;
            _resultStandardBL = resultStandardBL;
            _deviceBL = deviceBL;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var dateTimeNow = ToolBL.Get_DateNow();
            var from = new DateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, 23, 59, 59).AddDays(-1);
            var to = new DateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, 23, 59, 59);
            //ViewData["lstResultStandard"] = await _resultStandardBL.GetList(from, to);
            ViewData["lstDevice"] = await _deviceBL.GetList();
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Search(DateTime from, DateTime to, long? deviceid, string seq)
        {
            var timefrom = new DateTime(from.Year, from.Month, from.Day, 23, 59, 59).AddDays(-1);
            var timeto = new DateTime(to.Year, to.Month, to.Day, 23, 59, 59);
            ViewData["lstResultStandard"] = await _resultStandardBL.Search(timefrom, timeto, deviceid, seq);
            return PartialView("_Data_List");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(DateTime from, DateTime to, string seq, string status)
        {
            var _save = await _resultStandardBL.UpdateStatus(from, to, seq, status);
            return Content(_save.ToString());
        }
    }
}