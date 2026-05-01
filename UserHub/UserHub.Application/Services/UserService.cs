using UserHub.Application.DTOs;
using UserHub.Application.Interfaces;
using UserHub.Domain.Entities;
using UserHub.Domain.Interfaces;
using BCrypt.Net;

namespace UserHub.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;
    private readonly IRoleRepository _roleRepo;
    private readonly IModuleRepository _moduleRepo;
    private readonly IAuditService _audit;
    private readonly IAuditLogRepository _auditLogRepo;

    public UserService(IUserRepository userRepo, IRoleRepository roleRepo,
        IModuleRepository moduleRepo, IAuditService audit, IAuditLogRepository auditLogRepo)
    {
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _moduleRepo = moduleRepo;
        _audit = audit;
        _auditLogRepo = auditLogRepo;
    }

    public async Task<PagedResult<UserListItemDto>> GetUsersAsync(UserQueryDto query)
    {
        var (items, total) = await _userRepo.GetPagedAsync(
            query.Page, query.PageSize, query.Search,
            query.IsActive, query.SortBy, query.SortDesc);

        var dtos = new List<UserListItemDto>();
        foreach (var u in items)
        {
            var roles = await _userRepo.GetUserRolesAsync(u.Id);
            dtos.Add(new UserListItemDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                FullName = u.FullName,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                Roles = roles.Select(r => r.Name)
            });
        }

        return new PagedResult<UserListItemDto>
        {
            Items = dtos,
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<UserDetailDto?> GetUserByIdAsync(Guid id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) return null;

        var roles = await _userRepo.GetUserRolesAsync(id);
        var roleDtos = roles.Select(r => new RoleDto
        {
            Id = r.Id, Name = r.Name, Description = r.Description,
            IsSuperAdmin = r.IsSuperAdmin, IsActive = r.IsActive
        }).ToList();

        // Build permission matrix
        var modules = await _moduleRepo.GetAllAsync();
        var isSuperAdmin = roles.Any(r => r.IsSuperAdmin);
        var permMap = new Dictionary<string, PermissionSetDto>();

        foreach (var module in modules.Where(m => m.IsActive))
        {
            var merged = new PermissionSetDto
            {
                ModuleId = module.Id, ModuleName = module.Name,
                ModuleDisplayName = module.DisplayName
            };

            if (isSuperAdmin)
            {
                merged.CanView = merged.CanCreate = merged.CanEdit =
                    merged.CanDelete = merged.CanList = true;
            }
            else
            {
                foreach (var role in roles)
                {
                    var perms = await _roleRepo.GetPermissionsAsync(role.Id);
                    var p = perms.FirstOrDefault(x => x.ModuleId == module.Id);
                    if (p != null)
                    {
                        merged.CanView |= p.CanView;
                        merged.CanCreate |= p.CanCreate;
                        merged.CanEdit |= p.CanEdit;
                        merged.CanDelete |= p.CanDelete;
                        merged.CanList |= p.CanList;
                    }
                }
            }

            permMap[module.Name] = merged;
        }

        return new UserDetailDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = roleDtos,
            Permissions = permMap
        };
    }

    public async Task<(bool Success, string? Error)> CreateUserAsync(CreateUserDto dto)
    {
        if (await _userRepo.ExistsByUsernameAsync(dto.Username))
            return (false, "Username already exists.");

        if (await _userRepo.ExistsByEmailAsync(dto.Email))
            return (false, "Email already in use.");

        var hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        var user = User.Create(dto.Username, dto.Email, hash, dto.FirstName, dto.LastName);

        if (!dto.IsActive) user.Deactivate();

        await _userRepo.AddAsync(user);

        if (dto.RoleIds.Any())
            await _userRepo.AssignRolesAsync(user.Id, dto.RoleIds);

        await _audit.LogAsync(null, "system", "CreateUser", "Users", $"Created user: {user.Username}");
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateUserAsync(EditUserDto dto)
    {
        var user = await _userRepo.GetByIdAsync(dto.Id);
        if (user == null) return (false, "User not found.");

        if (await _userRepo.ExistsByEmailAsync(dto.Email, dto.Id))
            return (false, "Email already in use.");

        user.UpdateProfile(dto.FirstName, dto.LastName, dto.Email);
        if (dto.IsActive) user.Activate(); else user.Deactivate();

        await _userRepo.UpdateAsync(user);
        await _userRepo.AssignRolesAsync(user.Id, dto.RoleIds);
        await _audit.LogAsync(null, "system", "UpdateUser", "Users", $"Updated user: {user.Username}");
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ChangePasswordAsync(ChangePasswordDto dto)
    {
        var user = await _userRepo.GetByIdAsync(dto.UserId);
        if (user == null) return (false, "User not found.");

        user.UpdatePassword(BCrypt.Net.BCrypt.HashPassword(dto.NewPassword));
        await _userRepo.UpdateAsync(user);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteUserAsync(Guid id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) return (false, "User not found.");

        user.SoftDelete();
        await _userRepo.UpdateAsync(user);
        await _audit.LogAsync(null, "system", "DeleteUser", "Users", $"Soft-deleted user: {user.Username}");
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ToggleActiveAsync(Guid id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) return (false, "User not found.");

        if (user.IsActive) user.Deactivate(); else user.Activate();
        await _userRepo.UpdateAsync(user);
        return (true, null);
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var allUsers = await _userRepo.GetAllAsync();
        var roles = await _roleRepo.GetAllAsync();
        var modules = await _moduleRepo.GetAllAsync();
        var recent = await _auditLogRepo.GetRecentAsync(10);

        return new DashboardStatsDto
        {
            TotalUsers = allUsers.Count(u => !u.IsDeleted),
            ActiveUsers = allUsers.Count(u => u.IsActive && !u.IsDeleted),
            TotalRoles = roles.Count(),
            TotalModules = modules.Count(m => m.IsActive),
            RecentActivity = recent.Select(a => new RecentActivityDto
            {
                Username = a.Username,
                Action = a.Action,
                Module = a.Module,
                OccurredAt = a.OccurredAt
            })
        };
    }
}
