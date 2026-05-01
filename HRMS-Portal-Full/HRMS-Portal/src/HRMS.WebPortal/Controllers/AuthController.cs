using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HRMS.WebPortal.Models.Auth;
using HRMS.WebPortal.Services;

namespace HRMS.WebPortal.Controllers;

public class AuthController : Controller
{
    private readonly ApiClient _api;
    public AuthController(ApiClient api) => _api = api;

    [HttpGet] public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost] [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _api.LoginAsync(model.Email, model.Password);
        if (result == null)
        {
            model.ErrorMessage = "Invalid email or password.";
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.UserId.ToString()),
            new(ClaimTypes.Name, result.Username),
            new(ClaimTypes.Email, result.Email),
            new(ClaimTypes.Role, result.Role),
            new("AccessToken", result.AccessToken),
            new("RefreshToken", result.RefreshToken),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var props = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = result.ExpiresAt
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost] [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }
}
