using HRMS.Application.Interfaces.Repositories;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class DocumentRepository : GenericRepository<Document>, IDocumentRepository
{
    public DocumentRepository(HrmsDbContext ctx) : base(ctx) { }

    public async Task<IEnumerable<Document>> GetByEmployeeAsync(Guid empId, CancellationToken ct = default)
        => await _set.Include(d => d.Employee).Where(d => d.EmployeeId == empId)
                     .OrderByDescending(d => d.GeneratedAt).ToListAsync(ct);

    public async Task<IEnumerable<Document>> GetByTypeAsync(DocumentType type, CancellationToken ct = default)
        => await _set.Include(d => d.Employee).Where(d => d.Type == type).ToListAsync(ct);

    public async Task<Document?> GetByEmployeeAndTypeAsync(Guid empId, DocumentType type, CancellationToken ct = default)
        => await _set.FirstOrDefaultAsync(d => d.EmployeeId == empId && d.Type == type, ct);
}
