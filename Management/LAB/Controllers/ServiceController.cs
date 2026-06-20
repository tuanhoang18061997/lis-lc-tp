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
    public class ServiceController : Controller
    {
        public readonly ILogger<ServiceController> _logger;
        public readonly ServiceBL _serviceBL;
        public readonly CategoryBL _categoryBL;
        public readonly PrintSampleBL _printSampleBL;

        public ServiceController(ILogger<ServiceController> logger, ServiceBL serviceBL, CategoryBL categoryBL, PrintSampleBL printSampleBL)
        {
            _logger = logger;
            _serviceBL = serviceBL;
            _categoryBL = categoryBL;
            _printSampleBL = printSampleBL;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["lstService"] = await _serviceBL.GetList(true);
            ViewData["lstCategory"] = await _categoryBL.GetList();
            ViewData["lstPrintSample"] = await _printSampleBL.GetList();
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Search(string value)
        {
            ViewData["lstService"] = await _serviceBL.GetListByValue(value, true);
            return PartialView("_Service_List");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetInfo(string code)
        {
            ViewData["Service"] = await _serviceBL.Get(code);
            ViewData["lstCategory"] = await _categoryBL.GetList();
            ViewData["lstPrintSample"] = await _printSampleBL.GetList();
            return PartialView("_Service_Info");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            ViewData["lstService"] = await _serviceBL.GetList(true);
            return PartialView("_Service_List");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] ServiceModel service)
        {
            var _save = false;
            if (service != null)
            {
                _save = await _serviceBL.Save(service);
            }
            return Content(_save.ToString());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete(string code)
        {
            var _delete = await _serviceBL.Delete(code);
            return Content(_delete.ToString());
        }
    }
}