using UserHub.Application.DTOs;
using UserHub.Domain.Entities;

namespace UserHub.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(Guid? userId, string username, string action, string module,
        string? details = null, string? ipAddress = null);
    Task<IEnumerable<AuditLog>> GetRecentAsync(int count = 50);
    Task<IEnumerable<AuditLog>> GetByUserAsync(Guid userId, int count = 100);
}
