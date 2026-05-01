namespace UserHub.Domain.Entities;

public class UserRole
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime AssignedAt { get; private set; }

    public User? User { get; private set; }
    public Role? Role { get; private set; }

    private UserRole() { }

    public static UserRole Create(Guid userId, Guid roleId)
    {
        return new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RoleId = roleId,
            IsDeleted = false,
            AssignedAt = DateTime.UtcNow
        };
    }

    public void Remove() => IsDeleted = true;
}
