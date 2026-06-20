using Management.BL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Management.Controllers
{
    public class HospitalController : Controller
    {
        public readonly ILogger<HospitalController> _logger;
        public readonly HospitalBL _hospitalBL;

        public HospitalController(ILogger<HospitalController> logger, HospitalBL hospitalBL)
        {
            _logger = logger;
            _hospitalBL = hospitalBL;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["Hospital"] = await _hospitalBL.GetListHospital();
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetInfo(string code)
        {
            ViewData["Hospital"] = await _hospitalBL.GetHospitalByCode(code);
            return PartialView("_Hospital_Info");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            ViewData["Hospital"] = await _hospitalBL.GetListHospital();
            return PartialView("_Hospital_List");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] HospitalModel hospital)
        {
            var _save = false;
            if (hospital != null)
            {
                _save = await _hospitalBL.Save(hospital);
            }
            return Content(_save.ToString());
        }
    }
}