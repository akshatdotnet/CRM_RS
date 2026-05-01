using UserHub.Domain.Entities;
using UserHub.Domain.Interfaces;

namespace UserHub.Infrastructure.Repositories.InMemory;

public class InMemoryRoleRepository : IRoleRepository
{
    private readonly List<Role> _roles = new();
    private readonly List<RoleModulePermission> _permissions = new();

    public void Seed(List<Role> roles) => _roles.AddRange(roles);
    public void SeedPermissions(List<RoleModulePermission> perms) => _permissions.AddRange(perms);

    public Task<Role?> GetByIdAsync(Guid id) =>
        Task.FromResult(_roles.FirstOrDefault(r => r.Id == id));

    public Task<IEnumerable<Role>> GetAllAsync(bool includeInactive = false) =>
        Task.FromResult(_roles.Where(r => includeInactive || r.IsActive).AsEnumerable());

    public Task AddAsync(Role role) { _roles.Add(role); return Task.CompletedTask; }

    public Task UpdateAsync(Role role)
    {
        var idx = _roles.FindIndex(r => r.Id == role.Id);
        if (idx >= 0) _roles[idx] = role;
        return Task.CompletedTask;
    }

    public Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null) =>
        Task.FromResult(_roles.Any(r =>
            r.Name.ToLower() == name.ToLower() &&
            (excludeId == null || r.Id != excludeId)));

    public Task<bool> IsAssignedToUsersAsync(Guid roleId) =>
        Task.FromResult(_roles.FirstOrDefault(r => r.Id == roleId)?.HasUsers ?? false);

    public Task<IEnumerable<RoleModulePermission>> GetPermissionsAsync(Guid roleId) =>
        Task.FromResult(_permissions.Where(p => p.RoleId == roleId).AsEnumerable());

    public Task SetPermissionsAsync(Guid roleId, IEnumerable<RoleModulePermission> permissions)
    {
        _permissions.RemoveAll(p => p.RoleId == roleId);
        _permissions.AddRange(permissions);
        return Task.CompletedTask;
    }
}
