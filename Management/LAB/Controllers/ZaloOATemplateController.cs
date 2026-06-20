using Management.BL;
using Management.Models;
using Microsoft.AspNetCore.Mvc;

namespace Management.Controllers
{
    public class ZaloOATemplateController : Controller
    {
        private readonly ZaloOATemplateBL _zaloOATemplateBL;
        private readonly ZaloOAConfigBL _zaloOAConfigBL;

        public ZaloOATemplateController(
            ZaloOATemplateBL zaloOATemplateBL,
            ZaloOAConfigBL zaloOAConfigBL)
        {
            _zaloOATemplateBL = zaloOATemplateBL;
            _zaloOAConfigBL = zaloOAConfigBL;
        }

        public IActionResult Index()
        {
            ViewBag.ZaloOAConfigs = _zaloOAConfigBL.GetAll();
            var data = _zaloOATemplateBL.GetAll();
            return View(data);
        }

        [HttpGet]
        public IActionResult GetById(int id)
        {
            var data = _zaloOATemplateBL.GetById(id);
            if (data == null) return NotFound();
            return Json(data);
        }

        [HttpPost]
        public IActionResult Create([FromBody] ZaloOATemplate model)
        {
            if (model == null)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });

            var result = _zaloOATemplateBL.Insert(model, 1);

            return Json(new
            {
                success = result,
                message = result ? "Thêm mới thành công" : "Thêm mới thất bại"
            });
        }

        [HttpPost]
        public IActionResult Update([FromBody] ZaloOATemplate model)
        {
            if (model == null)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });

            var result = _zaloOATemplateBL.Update(model, 1);

            return Json(new
            {
                success = result,
                message = result ? "Cập nhật thành công" : "Cập nhật thất bại"
            });
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var result = _zaloOATemplateBL.Delete(id);

            return Json(new
            {
                success = result,
                message = result ? "Xóa thành công" : "Xóa thất bại"
            });
        }
    }
}