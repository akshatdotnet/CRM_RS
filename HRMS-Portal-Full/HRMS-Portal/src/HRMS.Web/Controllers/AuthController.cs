using HRMS.Application.DTOs.Auth;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRMS.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    /// <summary>Login and receive JWT tokens.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken ct)
    {
        var result = await _auth.LoginAsync(request, ct);
        return Ok(result);
    }

    /// <summary>Refresh an expired access token.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request, CancellationToken ct)
    {
        var result = await _auth.RefreshTokenAsync(request, ct);
        return Ok(result);
    }

    /// <summary>Logout and revoke refresh token.</summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _auth.LogoutAsync(userId, ct);
        return Ok(new { message = "Logged out successfully." });
    }

    /// <summary>Change password.</summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _auth.ChangePasswordAsync(userId, request, ct);
        return Ok(new { message = "Password changed successfully." });
    }
}
