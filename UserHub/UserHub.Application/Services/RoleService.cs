using UserHub.Application.DTOs;
using UserHub.Application.Interfaces;
using UserHub.Domain.Entities;
using UserHub.Domain.Interfaces;

namespace UserHub.Application.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepo;
    private readonly IModuleRepository _moduleRepo;
    private readonly IAuditService _audit;

    public RoleService(IRoleRepository roleRepo, IModuleRepository moduleRepo, IAuditService audit)
    {
        _roleRepo = roleRepo;
        _moduleRepo = moduleRepo;
        _audit = audit;
    }

    public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
    {
        var roles = await _roleRepo.GetAllAsync(includeInactive: true);
        return roles.Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            IsSuperAdmin = r.IsSuperAdmin,
            IsActive = r.IsActive,
            UserCount = r.UserRoles.Count(ur => !ur.IsDeleted)
        });
    }

    public async Task<RoleDto?> GetRoleByIdAsync(Guid id)
    {
        var role = await _roleRepo.GetByIdAsync(id);
        if (role == null) return null;
        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSuperAdmin = role.IsSuperAdmin,
            IsActive = role.IsActive,
            UserCount = role.UserRoles.Count(ur => !ur.IsDeleted)
        };
    }

    public async Task<(bool Success, string? Error)> CreateRoleAsync(CreateRoleDto dto)
    {
        if (await _roleRepo.ExistsByNameAsync(dto.Name))
            return (false, "Role name already exists.");

        var role = Role.Create(dto.Name, dto.Description, dto.IsSuperAdmin);
        await _roleRepo.AddAsync(role);
        await _audit.LogAsync(null, "system", "CreateRole", "Roles", $"Created role: {role.Name}");
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateRoleAsync(EditRoleDto dto)
    {
        var role = await _roleRepo.GetByIdAsync(dto.Id);
        if (role == null) return (false, "Role not found.");

        if (await _roleRepo.ExistsByNameAsync(dto.Name, dto.Id))
            return (false, "Role name already exists.");

        role.Update(dto.Name, dto.Description);
        await _roleRepo.UpdateAsync(role);
        await _audit.LogAsync(null, "system", "UpdateRole", "Roles", $"Updated role: {role.Name}");
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteRoleAsync(Guid id)
    {
        var role = await _roleRepo.GetByIdAsync(id);
        if (role == null) return (false, "Role not found.");

        if (await _roleRepo.IsAssignedToUsersAsync(id))
            return (false, "Cannot delete role: it is assigned to one or more users.");

        role.Deactivate();
        await _roleRepo.UpdateAsync(role);
        await _audit.LogAsync(null, "system", "DeleteRole", "Roles", $"Deactivated role: {role.Name}");
        return (true, null);
    }

    public async Task<IEnumerable<PermissionSetDto>> GetRolePermissionsAsync(Guid roleId)
    {
        var modules = await _moduleRepo.GetAllAsync();
        var perms = await _roleRepo.GetPermissionsAsync(roleId);

        return modules.Where(m => m.IsActive).Select(m =>
        {
            var p = perms.FirstOrDefault(x => x.ModuleId == m.Id);
            return new PermissionSetDto
            {
                ModuleId = m.Id,
                ModuleName = m.Name,
                ModuleDisplayName = m.DisplayName,
                CanView = p?.CanView ?? false,
                CanCreate = p?.CanCreate ?? false,
                CanEdit = p?.CanEdit ?? false,
                CanDelete = p?.CanDelete ?? false,
                CanList = p?.CanList ?? false
            };
        });
    }

    public async Task<(bool Success, string? Error)> SetPermissionsAsync(SetPermissionsDto dto)
    {
        var role = await _roleRepo.GetByIdAsync(dto.RoleId);
        if (role == null) return (false, "Role not found.");

        var permissions = dto.Permissions.Select(p =>
            RoleModulePermission.Create(dto.RoleId, p.ModuleId,
                p.CanView, p.CanCreate, p.CanEdit, p.CanDelete, p.CanList));

        await _roleRepo.SetPermissionsAsync(dto.RoleId, permissions);
        await _audit.LogAsync(null, "system", "SetPermissions", "Roles", $"Permissions updated for: {role.Name}");
        return (true, null);
    }
}
