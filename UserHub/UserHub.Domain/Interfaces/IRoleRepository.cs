using UserHub.Domain.Entities;

namespace UserHub.Domain.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid id);
    Task<IEnumerable<Role>> GetAllAsync(bool includeInactive = false);
    Task AddAsync(Role role);
    Task UpdateAsync(Role role);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null);
    Task<bool> IsAssignedToUsersAsync(Guid roleId);
    Task<IEnumerable<RoleModulePermission>> GetPermissionsAsync(Guid roleId);
    Task SetPermissionsAsync(Guid roleId, IEnumerable<RoleModulePermission> permissions);
}
