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
    public class MapController : Controller
    {
        public readonly ILogger<MapController> _logger;
        public readonly DeviceBL _deviceBL;
        public readonly MapBL _mapBL;
        public readonly TestCodeBL _testCodeBL;

        public MapController(ILogger<MapController> logger, DeviceBL deviceBL, MapBL mapBL, TestCodeBL testCodeBL)
        {
            _logger = logger;
            _deviceBL = deviceBL;
            _mapBL = mapBL;
            _testCodeBL = testCodeBL;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["lstDevice"] = await _deviceBL.GetList();
            ViewData["lstTestCode"] = await _testCodeBL.GetList();
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Search(string value)
        {
            ViewData["lstDevice"] = await _deviceBL.GetListByValue(value);
            return PartialView("_Map_List");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetInfo(long id)
        {
            ViewData["lstMap"] = await _mapBL.GetMapByDeviceId(id);
            return PartialView("_Map_Info");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMap(long id)
        {
            var map = await _mapBL.GetMap(id);
            var obj = new { 
                id = map.Id, 
                testcodeid = map.TestCodeId, 
                testcodename = map.TestCode?.Name, 
                testcodein = map.TestcodeIn,
                testcodein2 = map.TestcodeIn2,
                note = map.Note,
                formatnumber = map.FormatNumber
            };
            return Json(obj);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] MapModel map)
        {
            var _save = await _mapBL.Save(map);
            return Content(_save.ToString());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] List<Map01Model> lstMap01)
        {
            var _delete = await _mapBL.Delete(lstMap01);
            return Content(_delete.ToString());
        }
    }
}