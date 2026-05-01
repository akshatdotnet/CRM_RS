using UserHub.Application.DTOs;

namespace UserHub.Application.Interfaces;

public interface IUserService
{
    Task<PagedResult<UserListItemDto>> GetUsersAsync(UserQueryDto query);
    Task<UserDetailDto?> GetUserByIdAsync(Guid id);
    Task<(bool Success, string? Error)> CreateUserAsync(CreateUserDto dto);
    Task<(bool Success, string? Error)> UpdateUserAsync(EditUserDto dto);
    Task<(bool Success, string? Error)> ChangePasswordAsync(ChangePasswordDto dto);
    Task<(bool Success, string? Error)> DeleteUserAsync(Guid id);
    Task<(bool Success, string? Error)> ToggleActiveAsync(Guid id);
    Task<DashboardStatsDto> GetDashboardStatsAsync();
}
