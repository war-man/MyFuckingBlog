using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Stories.Services;
using Stories.VM.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Stories.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("/LogIn")]
        public IActionResult LogIn()
        {
            bool isAuthenticated = User.Identity.IsAuthenticated;
            if (!isAuthenticated)
                return View();
            return RedirectToAction("Index", "Blog");
        }

        [HttpPost]
        public async Task<IActionResult> Authentication(LogInRequest login)
        {
            var user = await _userService.AuthenticateUser(login);

            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim("Username", user.Username),
                    new Claim("IsAuthor", user.IsAuthor.ToString()),
                    new Claim("Name", user.Name),
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTime.UtcNow.AddMinutes(20)
                    });

                return RedirectToAction("Index", "Blog");
            }

            TempData["Error"] = "Sai tài khoản hoặc mật khẩu!";

            return RedirectToAction("LogIn", "User");
        }

        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Blog");
        }

        [Route("/Register")]
        public IActionResult Register()
        {
            return View();
        }

        [Route("/Author/{username}")]
        public IActionResult Author(string username)
        {
            return View();
        }
    }
}
