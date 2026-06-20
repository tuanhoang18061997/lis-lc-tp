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
    public class ObjectController : Controller
    {
        public readonly ILogger<ObjectController> _logger;
        public readonly ObjectBL _objectBL;

        public ObjectController(ILogger<ObjectController> logger, ObjectBL objectBL)
        {
            _logger = logger;
            _objectBL = objectBL;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["lstObject"] = await _objectBL.GetListObject(true);
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Search(string value)
        {
            ViewData["lstObject"] = await _objectBL.GetListObjectByValue(value, true);
            return PartialView("_Object_List");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetInfo(string code)
        {
            ViewData["Object"] = await _objectBL.GetObject(code);
            return PartialView("_Object_Info");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            ViewData["lstObject"] = await _objectBL.GetListObject(true);
            return PartialView("_Object_List");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] ObjectModel obj)
        {
            var _save = false;
            if (obj != null)
            {
                _save = await _objectBL.Save(obj);
            }
            return Content(_save.ToString());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete(string code)
        {
            var _delete = await _objectBL.Delete(code);
            return Content(_delete.ToString());
        }
    }
}