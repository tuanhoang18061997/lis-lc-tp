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
    public class SampleController : Controller
    {
        public readonly ILogger<SampleController> _logger;
        public readonly SampleBL _sampleBL;
        public readonly CategoryBL _categoryBL;
        public readonly ServiceBL _serviceBL;

        public SampleController(ILogger<SampleController> logger, SampleBL sampleBL, CategoryBL categoryBL, ServiceBL serviceBL)
        {
            _logger = logger;
            _sampleBL = sampleBL;
            _categoryBL = categoryBL;
            _serviceBL = serviceBL;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["lstSample"] = await _sampleBL.GetList(true);
            ViewData["lstCategory"] = await _categoryBL.GetList(true);
            ViewData["lstService"] = await _serviceBL.GetListServiceByGroupCLS();
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Search(string value)
        {
            ViewData["lstSample"] = await _sampleBL.GetListByValue(value, true);
            ViewData["lstService"] = await _serviceBL.GetListServiceByGroupCLS();
            return PartialView("_Sample_List");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetInfo(string code)
        {
            ViewData["Sample"] = await _sampleBL.Get(code);
            ViewData["lstCategory"] = await _categoryBL.GetList(true);
            ViewData["lstService"] = await _serviceBL.GetListServiceByGroupCLS();
            return PartialView("_Sample_Info");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            ViewData["lstSample"] = await _sampleBL.GetList(true);
            ViewData["lstService"] = await _serviceBL.GetListServiceByGroupCLS();
            return PartialView("_Sample_List");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] SampleModel sample)
        {
            var _save = false;
            if (sample != null)
            {
                _save = await _sampleBL.Save(sample);
            }
            return Content(_save.ToString());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete(string code)
        {
            var _delete = await _sampleBL.Delete(code);
            return Content(_delete.ToString());
        }
    }
}