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
    public class LocationController : Controller
    {
        public readonly ILogger<LocationController> _logger;
        public readonly LocationBL _locationBL;

        public LocationController(ILogger<LocationController> logger, LocationBL locationBL)
        {
            _logger = logger;
            _locationBL = locationBL;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["lstLocation"] = await _locationBL.GetListLocation(true);
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Search(string value)
        {
            ViewData["lstLocation"] = await _locationBL.GetListLocationByValue(value, true);
            return PartialView("_Location_List");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetInfo(string code)
        {
            ViewData["Location"] = await _locationBL.GetLocation(code);
            return PartialView("_Location_Info");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            ViewData["lstLocation"] = await _locationBL.GetListLocation(true);
            return PartialView("_Location_List");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] LocationModel location)
        {
            var _save = false;
            if (location != null)
            {
                _save = await _locationBL.Save(location);
            }
            return Content(_save.ToString());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete(string code)
        {
            var _delete = await _locationBL.Delete(code);
            return Content(_delete.ToString());
        }
    }
}