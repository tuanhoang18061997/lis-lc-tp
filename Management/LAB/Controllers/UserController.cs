using Castle.Core.Internal;
using Management.BL;
using Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Globalization;
using System.Net;

namespace Management.Controllers
{
    public class UserController : Controller
    {
        public readonly ILogger<UserController> _logger;
        public readonly HospitalBL _hospitalBL;
        public readonly UserBL _userBL;
        public readonly PatientXNBL _patientBL;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserController(ILogger<UserController> logger, HospitalBL hospitalBL, UserBL userBL, PatientXNBL patientBL, IWebHostEnvironment webHostEnvironment)

        {
            _logger = logger;
            _hospitalBL = hospitalBL;
            _userBL = userBL;
            _patientBL = patientBL;
            _webHostEnvironment = webHostEnvironment;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["lstUser"] = await _userBL.GetListUser(true);
            ViewData["lstType"] = await _userBL.GetListType();
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Search(string value)
        {
            ViewData["lstUser"] = await _userBL.GetListUserByValue(value, true);
            ViewData["lstType"] = await _userBL.GetListType();
            return PartialView("_User_List");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetInfo(string code)
        {
            ViewData["User"] = await _userBL.GetUser(code, true);
            ViewData["lstType"] = await _userBL.GetListType();
            return PartialView("_User_Info");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            ViewData["lstUser"] = await _userBL.GetListUser(true);
            return PartialView("_User_List");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetFunction(string code)
        {
            ViewData["User"] = await _userBL.GetUser(code, true);
            ViewData["lstFunction"] = await _userBL.GetListFunction();
            return PartialView("_User_Function");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Save()
        {
            var _save = false;

            try
            {
                // Check if there's a file upload (FormData)
                if (Request.HasFormContentType && Request.Form.Files.Count > 0)
                {
                    // Handle file upload with form data
                    var user = new UserModel
                    {
                        code = Request.Form["code"],
                        name = Request.Form["name"],
                        pass = Request.Form["pass"],
                        mabhyt = Request.Form["mabhyt"],
                        active = bool.Parse(Request.Form["active"]),
                        cks = Request.Form["cks"],
                        cccd = Request.Form["cccd"],
                        type = Request.Form["type"]
                    };

                    var signatureFile = Request.Form.Files["signatureImage"];
                    var signatureFileName = Request.Form["signatureFileName"];

                    if (signatureFile != null && !string.IsNullOrEmpty(signatureFileName))
                    {
                        // Save signature image
                        var uploadSuccess = await SaveSignatureImage(signatureFile, signatureFileName);
                        if (!uploadSuccess)
                        {
                            return Content("False");
                        }
                    }

                    _save = await _userBL.Save(user);
                }
                else
                {
                    // Handle JSON data (original way)
                    using var reader = new StreamReader(Request.Body);
                    var body = await reader.ReadToEndAsync();
                    var user = JsonConvert.DeserializeObject<UserModel>(body);

                    if (user != null)
                    {
                        _save = await _userBL.Save(user);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving user");
                _save = false;
            }

            return Content(_save.ToString());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete(string code)
        {
            var _delete = await _userBL.Delete(code);
            return Content(_delete.ToString());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Save_UserFunction([FromBody] UserFunctionModel userFunction)
        {
            var _save = false;
            if (userFunction != null)
            {
                _save = await _userBL.Save_UserFunction(userFunction);
            }
            return Content(_save.ToString());
        }

        private async Task<bool> SaveSignatureImage(IFormFile file, string fileName)
        {
            try
            {
                // Create the directory if it doesn't exist
                var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "cks");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Full file path
                var filePath = Path.Combine(uploadsPath, fileName);

                // Delete existing file if it exists
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                // Save the new file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving signature image: {fileName}");
                return false;
            }
        }
    }
}