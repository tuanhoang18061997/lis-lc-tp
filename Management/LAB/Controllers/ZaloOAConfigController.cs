using Management.BL;
using Management.Models;
using Microsoft.AspNetCore.Mvc;

namespace Management.Controllers
{
    public class ZaloOAConfigController : Controller
    {
        private readonly ZaloOAConfigBL _zaloOAConfigBL;

        public ZaloOAConfigController(ZaloOAConfigBL zaloOAConfigBL)
        {
            _zaloOAConfigBL = zaloOAConfigBL;
        }

        public IActionResult Index()
        {
            var data = _zaloOAConfigBL.GetAll();
            return View(data);
        }

        [HttpGet]
        public IActionResult GetById(int id)
        {
            var data = _zaloOAConfigBL.GetById(id);
            if (data == null) return NotFound();
            return Json(data);
        }

        [HttpPost]
        public IActionResult Create([FromBody] ZaloOAConfig model)
        {
            if (model == null) return BadRequest();

            var result = _zaloOAConfigBL.Insert(model, 1);
            return Json(new { success = result });
        }

        [HttpPost]
        public IActionResult Update([FromBody] ZaloOAConfig model)
        {
            if (model == null) return BadRequest();

            var result = _zaloOAConfigBL.Update(model, 1);
            return Json(new { success = result });
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var result = _zaloOAConfigBL.Delete(id);
            return Json(new { success = result });
        }
    }
}