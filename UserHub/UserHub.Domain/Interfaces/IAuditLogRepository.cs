using UserHub.Domain.Entities;

namespace UserHub.Domain.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log);
    Task<IEnumerable<AuditLog>> GetRecentAsync(int count = 50);
    Task<IEnumerable<AuditLog>> GetByUserAsync(Guid userId, int count = 100);
    Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, Guid? userId = null, string? module = null);
}
