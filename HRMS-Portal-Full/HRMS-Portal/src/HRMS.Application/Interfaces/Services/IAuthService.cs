using HRMS.Application.DTOs.Auth;

namespace HRMS.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken ct = default);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken ct = default);
    Task LogoutAsync(Guid userId, CancellationToken ct = default);
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto request, CancellationToken ct = default);
}
