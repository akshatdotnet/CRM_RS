using UserHub.Domain.Entities;
using UserHub.Domain.Interfaces;

namespace UserHub.Infrastructure.Repositories.InMemory;

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = new();
    private readonly List<UserRole> _userRoles = new();
    private readonly List<Role> _roles = new(); // reference — injected via seed

    public void SeedRoles(List<Role> roles) => _roles.AddRange(roles);
    public void SeedUsers(List<User> users) => _users.AddRange(users);
    public void SeedUserRoles(List<UserRole> userRoles) => _userRoles.AddRange(userRoles);

    public Task<User?> GetByIdAsync(Guid id) =>
        Task.FromResult(_users.FirstOrDefault(u => u.Id == id));

    public Task<User?> GetByUsernameAsync(string username) =>
        Task.FromResult(_users.FirstOrDefault(u =>
            u.Username == username.ToLower() && !u.IsDeleted));

    public Task<User?> GetByEmailAsync(string email) =>
        Task.FromResult(_users.FirstOrDefault(u =>
            u.Email == email.ToLower() && !u.IsDeleted));

    public Task<IEnumerable<User>> GetAllAsync(bool includeDeleted = false) =>
        Task.FromResult(_users.Where(u => includeDeleted || !u.IsDeleted).AsEnumerable());

    public Task<(IEnumerable<User> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search, bool? isActive, string? sortBy, bool sortDesc)
    {
        var query = _users.Where(u => !u.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(u =>
                u.Username.Contains(s) ||
                u.Email.Contains(s) ||
                u.FirstName.ToLower().Contains(s) ||
                u.LastName.ToLower().Contains(s));
        }

        if (isActive.HasValue)
            query = query.Where(u => u.IsActive == isActive.Value);

        query = (sortBy?.ToLower(), sortDesc) switch
        {
            ("email", false) => query.OrderBy(u => u.Email),
            ("email", true) => query.OrderByDescending(u => u.Email),
            ("createdat", false) => query.OrderBy(u => u.CreatedAt),
            ("createdat", true) => query.OrderByDescending(u => u.CreatedAt),
            ("lastloginat", false) => query.OrderBy(u => u.LastLoginAt),
            ("lastloginat", true) => query.OrderByDescending(u => u.LastLoginAt),
            (_, false) => query.OrderBy(u => u.Username),
            (_, true) => query.OrderByDescending(u => u.Username),
        };

        var total = query.Count();
        var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Task.FromResult((items.AsEnumerable(), total));
    }

    public Task AddAsync(User user) { _users.Add(user); return Task.CompletedTask; }

    public Task UpdateAsync(User user)
    {
        var idx = _users.FindIndex(u => u.Id == user.Id);
        if (idx >= 0) _users[idx] = user;
        return Task.CompletedTask;
    }

    public Task<bool> ExistsByUsernameAsync(string username, Guid? excludeId = null) =>
        Task.FromResult(_users.Any(u =>
            u.Username == username.ToLower() &&
            !u.IsDeleted &&
            (excludeId == null || u.Id != excludeId)));

    public Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null) =>
        Task.FromResult(_users.Any(u =>
            u.Email == email.ToLower() &&
            !u.IsDeleted &&
            (excludeId == null || u.Id != excludeId)));

    public Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId)
    {
        var roleIds = _userRoles
            .Where(ur => ur.UserId == userId && !ur.IsDeleted)
            .Select(ur => ur.RoleId);
        var roles = _roles.Where(r => roleIds.Contains(r.Id)).AsEnumerable();
        return Task.FromResult(roles);
    }

    public Task AssignRolesAsync(Guid userId, IEnumerable<Guid> roleIds)
    {
        // Soft-remove existing
        foreach (var ur in _userRoles.Where(ur => ur.UserId == userId && !ur.IsDeleted))
            ur.Remove();

        // Add new
        foreach (var rid in roleIds)
            _userRoles.Add(UserRole.Create(userId, rid));

        return Task.CompletedTask;
    }
}
