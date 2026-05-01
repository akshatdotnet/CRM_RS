using UserHub.Application.DTOs;
using UserHub.Application.Interfaces;
using UserHub.Domain.Interfaces;
using BCrypt.Net;

namespace UserHub.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IRoleRepository _roleRepo;
    private readonly IModuleRepository _moduleRepo;
    private readonly IAuditService _audit;

    public AuthService(IUserRepository userRepo, IRoleRepository roleRepo,
        IModuleRepository moduleRepo, IAuditService audit)
    {
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _moduleRepo = moduleRepo;
        _audit = audit;
    }

    public async Task<(bool Success, string? Error, SessionUserDto? User)> LoginAsync(string username, string password)
    {
        var user = await _userRepo.GetByUsernameAsync(username.Trim().ToLower());
        if (user == null)
            return (false, "Invalid username or password.", null);

        if (!user.IsActive || user.IsDeleted)
            return (false, "Account is inactive or deleted.", null);

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return (false, "Invalid username or password.", null);

        user.RecordLogin();
        await _userRepo.UpdateAsync(user);

        var sessionUser = await BuildSessionUserAsync(user.Id);
        await _audit.LogAsync(user.Id, user.Username, "Login", "Auth");

        return (true, null, sessionUser);
    }

    public async Task<SessionUserDto?> GetSessionUserAsync(Guid userId)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null || !user.IsActive || user.IsDeleted) return null;
        return await BuildSessionUserAsync(userId);
    }

    private async Task<SessionUserDto> BuildSessionUserAsync(Guid userId)
    {
        var user = (await _userRepo.GetByIdAsync(userId))!;
        var roles = await _userRepo.GetUserRolesAsync(userId);
        var modules = await _moduleRepo.GetAllAsync();

        var isSuperAdmin = roles.Any(r => r.IsSuperAdmin);
        var permissionMap = new Dictionary<string, PermissionSetDto>();

        if (!isSuperAdmin)
        {
            // Union merge: if any role grants a permission, user has it
            foreach (var module in modules.Where(m => m.IsActive))
            {
                var merged = new PermissionSetDto
                {
                    ModuleId = module.Id,
                    ModuleName = module.Name,
                    ModuleDisplayName = module.DisplayName
                };

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

                permissionMap[module.Name] = merged;
            }
        }
        else
        {
            // SuperAdmin: full access to everything
            foreach (var module in modules.Where(m => m.IsActive))
            {
                permissionMap[module.Name] = new PermissionSetDto
                {
                    ModuleId = module.Id,
                    ModuleName = module.Name,
                    ModuleDisplayName = module.DisplayName,
                    CanView = true, CanCreate = true, CanEdit = true,
                    CanDelete = true, CanList = true
                };
            }
        }

        return new SessionUserDto
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Roles = roles.Select(r => r.Name).ToList(),
            IsSuperAdmin = isSuperAdmin,
            Permissions = permissionMap
        };
    }
}
