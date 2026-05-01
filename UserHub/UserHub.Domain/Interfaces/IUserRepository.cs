using UserHub.Domain.Entities;

namespace UserHub.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllAsync(bool includeDeleted = false);
    Task<(IEnumerable<User> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search = null,
        bool? isActive = null, string? sortBy = null, bool sortDesc = false);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task<bool> ExistsByUsernameAsync(string username, Guid? excludeId = null);
    Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null);
    Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId);
    Task AssignRolesAsync(Guid userId, IEnumerable<Guid> roleIds);
}
