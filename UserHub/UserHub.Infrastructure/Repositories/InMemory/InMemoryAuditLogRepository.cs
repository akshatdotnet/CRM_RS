using UserHub.Domain.Entities;
using UserHub.Domain.Interfaces;

namespace UserHub.Infrastructure.Repositories.InMemory;

public class InMemoryAuditLogRepository : IAuditLogRepository
{
    private readonly List<AuditLog> _logs = new();

    public Task AddAsync(AuditLog log) { _logs.Add(log); return Task.CompletedTask; }

    public Task<IEnumerable<AuditLog>> GetRecentAsync(int count = 50) =>
        Task.FromResult(_logs.OrderByDescending(l => l.OccurredAt).Take(count).AsEnumerable());

    public Task<IEnumerable<AuditLog>> GetByUserAsync(Guid userId, int count = 100) =>
        Task.FromResult(_logs
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.OccurredAt)
            .Take(count)
            .AsEnumerable());

    public Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, Guid? userId = null, string? module = null)
    {
        var q = _logs.AsEnumerable();
        if (userId.HasValue) q = q.Where(l => l.UserId == userId);
        if (!string.IsNullOrWhiteSpace(module)) q = q.Where(l => l.Module == module);
        q = q.OrderByDescending(l => l.OccurredAt);
        var total = q.Count();
        var items = q.Skip((page - 1) * pageSize).Take(pageSize);
        return Task.FromResult((items, total));
    }
}
