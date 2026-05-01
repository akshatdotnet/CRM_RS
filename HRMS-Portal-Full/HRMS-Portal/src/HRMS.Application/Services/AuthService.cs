using HRMS.Application.DTOs.Auth;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace HRMS.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IJwtService _jwt;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUnitOfWork uow, IJwtService jwt, ILogger<AuthService> logger)
    {
        _uow = uow;
        _jwt = jwt;
        _logger = logger;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByEmailAsync(request.Email, ct)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is deactivated. Contact HR.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for email {Email}", request.Email);
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var (accessToken, expiry) = _jwt.GenerateAccessToken(user);
        var refreshToken = _jwt.GenerateRefreshToken();

        user.SetRefreshToken(refreshToken);
        user.LastLoginAt = DateTime.UtcNow;

        await _uow.Users.UpdateAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("User {Email} logged in successfully", request.Email);

        return new AuthResponseDto(
            user.Id,
            user.Username,
            user.Email,
            user.Role.ToString(),
            accessToken,
            refreshToken,
            expiry
        );
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken ct = default)
    {
        var principal = _jwt.ValidateToken(request.AccessToken);

        var userIdClaim = principal?
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim == null)
            throw new UnauthorizedAccessException("Invalid access token.");

        var parsedUserId = Guid.Parse(userIdClaim);

        var user = await _uow.Users.GetByRefreshTokenAsync(request.RefreshToken, ct)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (user.Id != parsedUserId || !user.IsRefreshTokenValid())
            throw new UnauthorizedAccessException("Refresh token expired. Please login again.");

        var (newAccessToken, newExpiry) = _jwt.GenerateAccessToken(user);
        var newRefreshToken = _jwt.GenerateRefreshToken();

        user.SetRefreshToken(newRefreshToken);

        await _uow.Users.UpdateAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        return new AuthResponseDto(
            user.Id,
            user.Username,
            user.Email,
            user.Role.ToString(),
            newAccessToken,
            newRefreshToken,
            newExpiry
        );
    }

    public async Task LogoutAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(userId, ct);
        if (user == null) return;

        user.RevokeRefreshToken();

        await _uow.Users.UpdateAsync(user, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto request, CancellationToken ct = default)
    {
        if (request.NewPassword != request.ConfirmNewPassword)
            throw new ArgumentException("Passwords do not match.");

        var user = await _uow.Users.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("User not found.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.SetAuditOnUpdate(userId.ToString());

        await _uow.Users.UpdateAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        return true;
    }
}


//using HRMS.Application.DTOs.Auth;
//using HRMS.Application.Interfaces.Repositories;
//using HRMS.Application.Interfaces.Services;
//using Microsoft.Extensions.Logging;

//namespace HRMS.Application.Services;

//public class AuthService : IAuthService
//{
//    private readonly IUnitOfWork _uow;
//    private readonly IJwtService _jwt;
//    private readonly ILogger<AuthService> _logger;

//    public AuthService(IUnitOfWork uow, IJwtService jwt, ILogger<AuthService> logger)
//    {
//        _uow = uow;
//        _jwt = jwt;
//        _logger = logger;
//    }

//    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken ct = default)
//    {
//        var user = await _uow.Users.GetByEmailAsync(request.Email, ct)
//            ?? throw new UnauthorizedAccessException("Invalid email or password.");

//        if (!user.IsActive)
//            throw new UnauthorizedAccessException("Account is deactivated. Contact HR.");

//        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
//        {
//            _logger.LogWarning("Failed login attempt for email {Email}", request.Email);
//            throw new UnauthorizedAccessException("Invalid email or password.");
//        }

//       // var accessToken = _jwt.GenerateAccessToken(user);
//        var refreshToken = _jwt.GenerateRefreshToken();

//        var (accessToken, expiry) = _jwt.GenerateAccessToken(user);

//        user.SetRefreshToken(refreshToken);
//        user.LastLoginAt = DateTime.UtcNow;
//        await _uow.Users.UpdateAsync(user, ct);
//        await _uow.SaveChangesAsync(ct);

//        _logger.LogInformation("User {Email} logged in successfully", request.Email);

//        return new AuthResponseDto(
//            user.Id, user.Username, user.Email,
//            user.Role.ToString(), accessToken, refreshToken,
//            DateTime.UtcNow.AddHours(1));
//    }

//    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken ct = default)
//    {
//        var userId = _jwt.ValidateTokenAndGetUserId(request.AccessToken);
//        if (userId == null)
//            throw new UnauthorizedAccessException("Invalid access token.");

//        var user = await _uow.Users.GetByRefreshTokenAsync(request.RefreshToken, ct)
//            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

//        if (user.Id != userId || !user.IsRefreshTokenValid())
//            throw new UnauthorizedAccessException("Refresh token expired. Please login again.");

//        var newAccessToken = _jwt.GenerateAccessToken(user);
//        var newRefreshToken = _jwt.GenerateRefreshToken();

//        user.SetRefreshToken(newRefreshToken);
//        await _uow.Users.UpdateAsync(user, ct);
//        await _uow.SaveChangesAsync(ct);

//        return new AuthResponseDto(
//            user.Id, user.Username, user.Email,
//            user.Role.ToString(), newAccessToken, newRefreshToken,
//            DateTime.UtcNow.AddHours(1));
//    }

//    public async Task LogoutAsync(Guid userId, CancellationToken ct = default)
//    {
//        var user = await _uow.Users.GetByIdAsync(userId, ct);
//        if (user == null) return;

//        user.RevokeRefreshToken();
//        await _uow.Users.UpdateAsync(user, ct);
//        await _uow.SaveChangesAsync(ct);
//    }

//    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto request, CancellationToken ct = default)
//    {
//        if (request.NewPassword != request.ConfirmNewPassword)
//            throw new ArgumentException("Passwords do not match.");

//        var user = await _uow.Users.GetByIdAsync(userId, ct)
//            ?? throw new KeyNotFoundException("User not found.");

//        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
//            throw new UnauthorizedAccessException("Current password is incorrect.");

//        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
//        user.SetAuditOnUpdate(userId.ToString());
//        await _uow.Users.UpdateAsync(user, ct);
//        await _uow.SaveChangesAsync(ct);
//        return true;
//    }
//}
