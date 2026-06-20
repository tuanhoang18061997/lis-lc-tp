using Management.BL;
using Management.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;

namespace Management.Controllers
{
    public class AccountController : Controller
    {
        public readonly ILogger<AccountController> _logger;
        public readonly HospitalBL _hospitalBL;
        public readonly UserBL _userBL;
        public readonly TypeBL _type;
        public readonly FunctionBL _functionBL;
        public readonly SettingBL _settingBL;

        public AccountController(ILogger<AccountController> logger, HospitalBL hospitalBL, UserBL userBL, TypeBL type, FunctionBL functionBL, SettingBL settingBL)
        {
            _logger = logger;
            _hospitalBL = hospitalBL;
            _userBL = userBL;
            _type = type;
            _functionBL = functionBL;
            _settingBL = settingBL;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Login()
        {
            try
            {
                await _type.CreateTypes();
                await _functionBL.CreateFunction();
                await _userBL.CreateUserAdmin();
                await _hospitalBL.CreateHostpital();
                await _settingBL.Init();
                this.DeleteAllCookie();
            }
            catch(Exception ex) {
            }         

            var _hospital = await _hospitalBL.GetHospital();          
            this.SetCookies(SessionKeyModel._sessionHospital, _hospital.Name, 1);
            ViewData["Title"] = _hospital.Name;

            return View();
        }

        public void DeleteAllCookie()
        {
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Logged(User user)
        { 
            var _userLogin = await _userBL.GetUserByCodeAndPassword(user);
            if (_userLogin != null)
            {
                var claims = new List<Claim>() {
                        new Claim("UserId", _userLogin.Id.ToString()),
                        new Claim("UserName", _userLogin.Name)
                    };

                ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                ClaimsPrincipal principal = new ClaimsPrincipal(identity);
                var properties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddHours(4)
                };
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                              principal,
                                              properties
                                              );
                ViewData["lstUserFunction"] = await _userBL.GetUserFunction(_userLogin.Id);
                return View("/Views/Main/Index.cshtml", _userLogin);
            }
            else
            {
                ViewData["Login"] = "Tên đăng nhập hoặc mật khẩu không đúng !";
                return View("Login", user);
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(scheme: CookieAuthenticationDefaults.AuthenticationScheme);
            return View("Login");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ChangePassword()
        {
            var _userId = this.GetUserLogin();
            var _userLogin = await _userBL.GetUser(_userId??0);
            return View(_userLogin);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangedPassword(User user, string passwordNew1, string passwordNew2)
        {
            if (passwordNew1 != passwordNew2)
            {
                ViewData["ChangePassword"] = "Mật khẩu mới và Nhập lại mật khẩu mới không giống nhau !";
                return View("ChangePassword", user);
            }
            else
            {
                var _changePassword = await _userBL.ChangePassword(user.Code, user.Password, passwordNew1);
                if (!_changePassword)
                {
                    ViewData["ChangePassword"] = "Mật khẩu cũ không đúng !";
                    return View("ChangePassword", user);
                }
                else
                {
                    return LocalRedirect(Url.Action("Logout", "Account"));
                }
            }
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

        public void SetCookies(string key, string value, double time)
        {
            CookieOptions cookieOptions = new CookieOptions() { Expires = new DateTimeOffset(DateTime.Now.AddDays(time)) };
            HttpContext.Response.Cookies.Append(key, value, cookieOptions);
        }
    }
}