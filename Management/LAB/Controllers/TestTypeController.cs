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
    public class TestTypeController : Controller
    {
        public readonly ILogger<TestTypeController> _logger;
        public readonly TestTypeBL _testTypeBL;

        public TestTypeController(ILogger<TestTypeController> logger, TestTypeBL testTypeBL)
        {
            _logger = logger;
            _testTypeBL = testTypeBL;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["lstTestType"] = await _testTypeBL.GetList(true);
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Search(string value)
        {
            ViewData["lstTestType"] = await _testTypeBL.GetListByValue(value, true);
            return PartialView("_TestType_List");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetInfo(string code)
        {
            ViewData["TestType"] = await _testTypeBL.Get(code);
            return PartialView("_TestType_Info");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            ViewData["lstTestType"] = await _testTypeBL.GetList(true);
            return PartialView("_TestType_List");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] TestTypeModel testType)
        {
            var _save = false;
            if (testType != null)
            {
                _save = await _testTypeBL.Save(testType);
            }
            return Content(_save.ToString());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete(string code)
        {
            var _delete = await _testTypeBL.Delete(code);
            return Content(_delete.ToString());
        }
    }
}