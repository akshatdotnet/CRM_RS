namespace UserHub.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public bool IsDeleted { get; private set; } = false;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    private User() { }

    public static User Create(string username, string email, string passwordHash, string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username required.");
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email required.");
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password required.");
        return new User
        {
            Id = Guid.NewGuid(),
            Username = username.Trim().ToLower(),
            Email = email.Trim().ToLower(),
            PasswordHash = passwordHash,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateProfile(string firstName, string lastName, string email)
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim().ToLower();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordLogin() => LastLoginAt = DateTime.UtcNow;
    public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }
    public void Activate() { IsActive = true; UpdatedAt = DateTime.UtcNow; }
    public void SoftDelete() { IsDeleted = true; IsActive = false; UpdatedAt = DateTime.UtcNow; }
    public string FullName => $"{FirstName} {LastName}".Trim();
}
