using Microsoft.AspNetCore.Mvc;
using UserHub.Application.DTOs;
using UserHub.Application.Interfaces;
using UserHub.Shared.Constants;
using UserHub.Shared.Extensions;

namespace UserHub.Web.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [HttpGet]
    public IActionResult Login(string? returnUrl)
    {
        if (HttpContext.Session.GetObject<SessionUserDto>(SessionKeys.SessionUser) != null)
            return RedirectToAction("Dashboard", "Home");

        return View(new LoginDto { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto model)
    {
        if (!ModelState.IsValid) return View(model);

        var (success, error, user) = await _auth.LoginAsync(model.Username, model.Password);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Login failed.");
            return View(model);
        }

        HttpContext.Session.SetObject(SessionKeys.SessionUser, user!);
        HttpContext.Session.SetString(SessionKeys.UserId, user!.Id.ToString());
        HttpContext.Session.SetString(SessionKeys.Username, user.Username);
        HttpContext.Session.SetString(SessionKeys.FullName, user.FullName);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToAction("Dashboard", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();
}
