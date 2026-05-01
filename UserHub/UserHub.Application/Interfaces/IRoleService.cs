using UserHub.Application.DTOs;

namespace UserHub.Application.Interfaces;

public interface IRoleService
{
    Task<IEnumerable<RoleDto>> GetAllRolesAsync();
    Task<RoleDto?> GetRoleByIdAsync(Guid id);
    Task<(bool Success, string? Error)> CreateRoleAsync(CreateRoleDto dto);
    Task<(bool Success, string? Error)> UpdateRoleAsync(EditRoleDto dto);
    Task<(bool Success, string? Error)> DeleteRoleAsync(Guid id);
    Task<IEnumerable<PermissionSetDto>> GetRolePermissionsAsync(Guid roleId);
    Task<(bool Success, string? Error)> SetPermissionsAsync(SetPermissionsDto dto);
}
