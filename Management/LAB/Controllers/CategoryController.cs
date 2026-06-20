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
    public class CategoryController : Controller
    {
        public readonly ILogger<CategoryController> _logger;
        public readonly CategoryBL _categoryBL;

        public CategoryController(ILogger<CategoryController> logger, CategoryBL categoryBL)
        {
            _logger = logger;
            _categoryBL = categoryBL;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["lstCategory"] = await _categoryBL.GetList(true);
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Search(string value)
        {
            ViewData["lstCategory"] = await _categoryBL.GetListByValue(value, true);
            return PartialView("_Category_List");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetInfo(string code)
        {
            ViewData["Category"] = await _categoryBL.Get(code);
            return PartialView("_Category_Info");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            ViewData["lstCategory"] = await _categoryBL.GetList(true);
            return PartialView("_Category_List");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] CategoryModel category)
        {
            var _save = false;
            if (category != null)
            {
                _save = await _categoryBL.Save(category);
            }
            return Content(_save.ToString());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete(string code)
        {
            var _delete = await _categoryBL.Delete(code);
            return Content(_delete.ToString());
        }
    }
}