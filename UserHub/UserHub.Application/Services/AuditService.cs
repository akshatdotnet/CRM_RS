using UserHub.Application.Interfaces;
using UserHub.Domain.Entities;
using UserHub.Domain.Interfaces;

namespace UserHub.Application.Services;

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _repo;

    public AuditService(IAuditLogRepository repo) => _repo = repo;

    public async Task LogAsync(Guid? userId, string username, string action,
        string module, string? details = null, string? ipAddress = null)
    {
        var log = AuditLog.Create(userId, username, action, module, details, ipAddress);
        await _repo.AddAsync(log);
    }

    public Task<IEnumerable<AuditLog>> GetRecentAsync(int count = 50) =>
        _repo.GetRecentAsync(count);

    public Task<IEnumerable<AuditLog>> GetByUserAsync(Guid userId, int count = 100) =>
        _repo.GetByUserAsync(userId, count);
}
