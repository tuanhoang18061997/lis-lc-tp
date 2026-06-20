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
    public class DoctorController : Controller
    {
        public readonly ILogger<DoctorController> _logger;
        public readonly DoctorBL _doctorBL;

        public DoctorController(ILogger<DoctorController> logger, DoctorBL doctorBL)
        {
            _logger = logger;
            _doctorBL = doctorBL;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["lstDoctor"] = await _doctorBL.GetList(true);
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Search(string value)
        {
            ViewData["lstDoctor"] = await _doctorBL.GetListByValue(value, true);
            return PartialView("_Doctor_List");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetInfo(string code)
        {
            ViewData["Doctor"] = await _doctorBL.Get(code);
            return PartialView("_Doctor_Info");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            ViewData["lstDoctor"] = await _doctorBL.GetList(true);
            return PartialView("_Doctor_List");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] DoctorModel doctor)
        {
            var _save = false;
            if (doctor != null)
            {
                _save = await _doctorBL.Save(doctor);
            }
            return Content(_save.ToString());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete(string code)
        {
            var _delete = await _doctorBL.Delete(code);
            return Content(_delete.ToString());
        }
    }
}