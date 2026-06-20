using Castle.Core.Internal;
using Management.BL;
using Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Globalization;

namespace Management.Controllers
{
    public class SettupController : Controller
    {
        public readonly ILogger<SettupController> _logger;
        public readonly HospitalBL _hospitalBL;
        public readonly UserBL _userBL;
        public readonly PatientXNBL _patientBL;

        public SettupController(ILogger<SettupController> logger, HospitalBL hospitalBL, UserBL userBL, PatientXNBL patientBL)
        {
            _logger = logger;
            _hospitalBL = hospitalBL;
            _userBL = userBL;
            _patientBL = patientBL;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var _userLogin = GetUserLogin();
            if(_userLogin != null)
            {
                ViewData["lstUserFunction"] = await _userBL.GetUserFunction(_userLogin.Value);
            }          
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ToUser()
        {
            return LocalRedirect(Url.Action("Index", "User"));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ToFunction()
        {
            return LocalRedirect(Url.Action("Index", "Function"));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ToUserFunction()
        {
            return LocalRedirect(Url.Action("Index", "UserFunction"));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ToHospital()
        {
            return LocalRedirect(Url.Action("Index", "Hospital"));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ToLocation()
        {
            return LocalRedirect(Url.Action("Index", "Location"));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ToObject()
        {
            return LocalRedirect(Url.Action("Index", "Object"));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ToDoctor()
        {
            return LocalRedirect(Url.Action("Index", "Doctor"));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ToTestType()
        {
            return LocalRedirect(Url.Action("Index", "TestType"));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ToCategory()
        {
            return LocalRedirect(Url.Action("Index", "Category"));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ToService()
        {
            return LocalRedirect(Url.Action("Index", "Service"));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ToTestCode()
        {
            return LocalRedirect(Url.Action("Index", "TestCode"));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ToServiceTest()
        {
            return LocalRedirect(Url.Action("Index", "ServiceTest"));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ToDevice()
        {
            return LocalRedirect(Url.Action("Index", "Device"));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ToMap()
        {
            return LocalRedirect(Url.Action("Index", "Map"));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ToSample()
        {
            return LocalRedirect(Url.Action("Index", "Sample"));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ToManageData()
        {
            return LocalRedirect(Url.Action("Index", "Data_ResultStandard"));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ToManageConfigSystem()
        {
            return LocalRedirect(Url.Action("Index", "Setting"));
        }

        public long? GetUserLogin()
        {
            long? userId = null;
            try
            {
                userId = long.Parse(HttpContext?.User?.Claims?.FirstOrDefault(p => p.Type == "UserId")?.Value);
            }
            catch { }
            return userId;
        }
    }
}