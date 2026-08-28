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
    public class MainController : Controller
    {
        public readonly ILogger<MainController> _logger;
        public readonly HospitalBL _hospitalBL;
        public readonly UserBL _userBL;
        public readonly PatientXNBL _patientBL;

        public MainController(ILogger<MainController> logger, HospitalBL hospitalBL, UserBL userBL, PatientXNBL patientBL)
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
            var _userLoginId = GetUserLogin();
            var lstFunction = await _userBL.GetUserFunction(_userLoginId.Value);
            ViewData["lstUserFunction"] = lstFunction;
            return View(lstFunction[0].User);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ToXn()
        {
            return LocalRedirect(Url.Action("GetSample", "XN_GetSample"));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ToSA()
        {
            return LocalRedirect(Url.Action("GetSample", "SA_GetSample"));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ToSAT()
        {
            return LocalRedirect(Url.Action("GetSample", "SA_TIM_GetSample"));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ToDDT()
        {
            return LocalRedirect(Url.Action("GetSample", "DDT_GetSample"));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ToNS()
        {
            return LocalRedirect(Url.Action("GetSample", "NS_GetSample"));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ToNSCTC()
        {
            return LocalRedirect(Url.Action("GetSample", "NSCTC_GetSample"));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ToTDCN()
        {
            return LocalRedirect(Url.Action("GetSample", "TDCN_GetSample"));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ToXQ()
        {
            return LocalRedirect(Url.Action("GetSample", "XQ_GetSample"));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ToReport()
        {
            return LocalRedirect(Url.Action("Index", "Report_BaoCaoThongKe"));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ToSearch()
        {
            return LocalRedirect(Url.Action("Index", "Search"));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ToSettup()
        {
            return LocalRedirect(Url.Action("Index", "Settup"));
        }

        [HttpGet]
        [Authorize]
        public IActionResult ToToolAdmin()
        {
            return LocalRedirect(Url.Action("Index", "ToolAdmin"));
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
