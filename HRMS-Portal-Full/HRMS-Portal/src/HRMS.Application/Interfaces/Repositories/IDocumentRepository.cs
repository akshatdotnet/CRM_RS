using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Application.Interfaces.Repositories;

public interface IDocumentRepository : IGenericRepository<Document>
{
    Task<IEnumerable<Document>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct = default);
    Task<IEnumerable<Document>> GetByTypeAsync(DocumentType type, CancellationToken ct = default);
    Task<Document?> GetByEmployeeAndTypeAsync(Guid employeeId, DocumentType type, CancellationToken ct = default);
}
