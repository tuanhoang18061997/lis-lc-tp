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
    public class TestCodeController : Controller
    {
        public readonly ILogger<TestCodeController> _logger;
        public readonly CategoryBL _categoryBL;
        public readonly TestTypeBL _testTypeBL;
        public readonly TestCodeBL _testCodeBL;

        public TestCodeController(ILogger<TestCodeController> logger, TestCodeBL testCodeBL, CategoryBL categoryBL, TestTypeBL testTypeBL)
        {
            _logger = logger;
            _testCodeBL = testCodeBL;
            _categoryBL = categoryBL;
            _testTypeBL = testTypeBL;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["lstTestCode"] = await _testCodeBL.GetList(true);
            ViewData["lstCategory"] = await _categoryBL.GetList();
            ViewData["lstTestType"] = await _testTypeBL.GetList();
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Search(string value)
        {
            ViewData["lstTestCode"] = await _testCodeBL.GetListByValue(value, true);
            return PartialView("_TestCode_List");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetInfo(long id)
        {
            ViewData["TestCode"] = await _testCodeBL.Get(id);
            ViewData["lstCategory"] = await _categoryBL.GetList();
            ViewData["lstTestType"] = await _testTypeBL.GetList();
            return PartialView("_TestCode_Info");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            ViewData["lstTestCode"] = await _testCodeBL.GetList(true);
            return PartialView("_TestCode_List");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] TestCodeModel testcode)
        {
            var _save = false;
            if (testcode != null)
            {
                _save = await _testCodeBL.Save(testcode);
            }
            return Content(_save.ToString());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete(string code)
        {
            var _delete = await _testCodeBL.Delete(code);
            return Content(_delete.ToString());
        }
    }
}