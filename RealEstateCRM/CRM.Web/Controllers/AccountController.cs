using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEmailService _email;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEmailService email,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _email = email;
        _logger = logger;
    }

    // ── LOGIN ──────────────────────────────────────────────────────────────
    [HttpGet, AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (_signInManager.IsSignedIn(User)) return RedirectToAction("Index", "Home");
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _userManager.FindByEmailAsync(vm.Email);
        if (user == null || !user.IsActive)
        {
            ModelState.AddModelError("", "Invalid email or password.");
            _logger.LogWarning("Failed login attempt for {Email}", vm.Email);
            return View(vm);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user, vm.Password, vm.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            // Update last login timestamp
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Optional: send login alert email
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            _ = _email.SendLoginAlertEmailAsync(user.Email!, user.FullName, ip, DateTime.UtcNow);

            _logger.LogInformation("User {Email} logged in from {IP}", user.Email, ip);

            if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                return Redirect(vm.ReturnUrl);
            return RedirectToAction("Index", "Home");
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("Account {Email} is locked out", vm.Email);
            return View("Lockout");
        }

        ModelState.AddModelError("", "Invalid email or password.");
        return View(vm);
    }

    // ── LOGOUT ─────────────────────────────────────────────────────────────
    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    // ── FORGOT PASSWORD ────────────────────────────────────────────────────
    [HttpGet, AllowAnonymous]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _userManager.FindByEmailAsync(vm.Email);

        // Always show success — prevents email enumeration
        if (user != null && user.IsActive)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action(nameof(ResetPassword), "Account",
                new { userId = user.Id, token }, Request.Scheme)!;

            await _email.SendPasswordResetEmailAsync(user.Email!, user.FullName, resetLink);
            _logger.LogInformation("Password reset requested for {Email}", user.Email);
        }

        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [HttpGet, AllowAnonymous]
    public IActionResult ForgotPasswordConfirmation() => View();

    // ── RESET PASSWORD ─────────────────────────────────────────────────────
    [HttpGet, AllowAnonymous]
    public IActionResult ResetPassword(string userId, string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            return RedirectToAction(nameof(Login));
        return View(new ResetPasswordViewModel { UserId = userId, Token = token });
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _userManager.FindByIdAsync(vm.UserId);
        if (user == null) return RedirectToAction(nameof(ResetPasswordConfirmation));

        var result = await _userManager.ResetPasswordAsync(user, vm.Token, vm.NewPassword);
        if (result.Succeeded)
        {
            _logger.LogInformation("Password reset successful for {Email}", user.Email);
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        foreach (var err in result.Errors)
            ModelState.AddModelError("", err.Description);
        return View(vm);
    }

    [HttpGet, AllowAnonymous]
    public IActionResult ResetPasswordConfirmation() => View();

    // ── CHANGE PASSWORD (authenticated) ────────────────────────────────────
    [HttpGet, Authorize]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Login));

        var result = await _userManager.ChangePasswordAsync(user, vm.CurrentPassword, vm.NewPassword);
        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
            TempData["Success"] = "Password changed successfully.";
            return RedirectToAction(nameof(Profile));
        }

        foreach (var err in result.Errors)
            ModelState.AddModelError("", err.Description);
        return View(vm);
    }

    // ── PROFILE ────────────────────────────────────────────────────────────
    [HttpGet, Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Login));
        var roles = await _userManager.GetRolesAsync(user);
        return View(new ProfileViewModel
        {
            FullName = user.FullName,
            Email = user.Email ?? "",
            Role = roles.FirstOrDefault() ?? "—",
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt
        });
    }

    // ── ACCESS DENIED ──────────────────────────────────────────────────────
    [HttpGet, AllowAnonymous]
    public IActionResult AccessDenied() => View();

    [HttpGet, AllowAnonymous]
    public IActionResult Lockout() => View();
}
