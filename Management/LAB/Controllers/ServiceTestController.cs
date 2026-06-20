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
    public class ServiceTestController : Controller
    {
        public readonly ILogger<ServiceTestController> _logger;
        public readonly ServiceBL _serviceBL;
        public readonly ServiceTestBL _serviceTestBL;
        public readonly TestCodeBL _testCodeBL;

        public ServiceTestController(ILogger<ServiceTestController> logger, ServiceBL serviceBL, ServiceTestBL serviceTestBL, TestCodeBL testCodeBL)
        {
            _logger = logger;
            _serviceBL = serviceBL;
            _serviceTestBL = serviceTestBL;
            _testCodeBL = testCodeBL;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["lstService"] = await _serviceBL.GetListServiceByGroup(GroupBL.XN);
            ViewData["lstTestCode"] = await _testCodeBL.GetList();
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Search(string value)
        {
            ViewData["lstService"] = await _serviceBL.GetListByGroupCodeAndValue(GroupBL.XN, value);
            return PartialView("_ServiceTest_List");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Search_AddTestCode(string value)
        {
            ViewData["lstTestCode"] = await _testCodeBL.GetListByValue(value);
            return PartialView("_ServiceTest_AddTestCode");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetInfo(long id)
        {
            ViewData["lstServiceTest"] = await _serviceTestBL.GetListServiceTestByServiceId(id);
            return PartialView("_ServiceTest_Info");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] List<ServiceTestModel> lstServiceTest)
        {
            var _save = await _serviceTestBL.Save(lstServiceTest);
            return Content(_save.ToString());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] List<ServiceTestModel> lstServiceTest)
        {
            var _delete = await _serviceTestBL.Delete(lstServiceTest);
            return Content(_delete.ToString());
        }
    }
}