namespace UserHub.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; private set; }
    public Guid? UserId { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string Module { get; private set; } = string.Empty;
    public string? Details { get; private set; }
    public string? IpAddress { get; private set; }
    public DateTime OccurredAt { get; private set; }

    private AuditLog() { }

    public static AuditLog Create(Guid? userId, string username, string action,
        string module, string? details = null, string? ipAddress = null)
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Username = username,
            Action = action,
            Module = module,
            Details = details,
            IpAddress = ipAddress,
            OccurredAt = DateTime.UtcNow
        };
    }
}
