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
    public class Data_PushResultHISController : Controller
    {
        public readonly ILogger<Data_PushResultHISController> _logger;
        public readonly PushResultHISBL _pushResultHISBL;

        public Data_PushResultHISController(ILogger<Data_PushResultHISController> logger, PushResultHISBL pushResultHISBL)
        {
            _logger = logger;
            _pushResultHISBL = pushResultHISBL;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Search(DateTime from, DateTime to, string patientid)
        {
            var timefrom = new DateTime(from.Year, from.Month, from.Day, 23, 59, 59).AddDays(-1);
            var timeto = new DateTime(to.Year, to.Month, to.Day, 23, 59, 59);
            ViewData["lstPushResultHIS"] = await _pushResultHISBL.Get_ListPatient(timefrom, timeto, patientid);
            return PartialView("_List");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdatePush(DateTime from, DateTime to, string patientid, string push)
        {
            var timefrom = new DateTime(from.Year, from.Month, from.Day, 23, 59, 59).AddDays(-1);
            var timeto = new DateTime(to.Year, to.Month, to.Day, 23, 59, 59);
            var _save = await _pushResultHISBL.UpdatePush(timefrom, timeto, patientid, push); ;
            return Content(_save.ToString());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Push(DateTime from, DateTime to, string patientid)
        {
            var timefrom = new DateTime(from.Year, from.Month, from.Day, 23, 59, 59).AddDays(-1);
            var timeto = new DateTime(to.Year, to.Month, to.Day, 23, 59, 59);
            var _save = await _pushResultHISBL.Push(timefrom, timeto, patientid); ;
            return Content(_save.ToString());
        }
    }
}