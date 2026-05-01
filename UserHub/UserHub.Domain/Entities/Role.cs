namespace UserHub.Domain.Entities;

public class Role
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsSuperAdmin { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
    public ICollection<RoleModulePermission> Permissions { get; private set; } = new List<RoleModulePermission>();

    private Role() { }

    public static Role Create(string name, string description, bool isSuperAdmin = false)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Role name required.");
        return new Role
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim() ?? string.Empty,
            IsSuperAdmin = isSuperAdmin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string description)
    {
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }
    public void Activate() { IsActive = true; UpdatedAt = DateTime.UtcNow; }

    public bool HasUsers => UserRoles.Any(ur => !ur.IsDeleted);
}
