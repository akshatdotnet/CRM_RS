using System.ComponentModel.DataAnnotations;

namespace HRMS.Application.DTOs.Auth;

public record LoginRequestDto(
    [Required] string Email,
    [Required] string Password
);

public record AuthResponseDto(
    Guid UserId,
    string Username,
    string Email,
    string Role,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);

public record RefreshTokenRequestDto(
    [Required] string AccessToken,
    [Required] string RefreshToken
);

public record ChangePasswordDto(
    [Required] string CurrentPassword,
    [Required, MinLength(8)] string NewPassword,
    [Required] string ConfirmNewPassword
);
