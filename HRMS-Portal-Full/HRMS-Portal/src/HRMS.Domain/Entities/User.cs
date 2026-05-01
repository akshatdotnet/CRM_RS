using HRMS.Domain.Common;
using HRMS.Domain.Enums;

namespace HRMS.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }

    // Navigation
    public Guid? EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    // Domain methods
    public bool IsRefreshTokenValid() =>
        RefreshToken != null && RefreshTokenExpiresAt > DateTime.UtcNow;

    public void SetRefreshToken(string token, int expiryDays = 7)
    {
        RefreshToken = token;
        RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(expiryDays);
    }

    public void RevokeRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiresAt = null;
    }
}
