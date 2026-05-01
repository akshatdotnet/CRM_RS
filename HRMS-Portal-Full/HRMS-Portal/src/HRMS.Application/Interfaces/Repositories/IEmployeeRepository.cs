using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Application.Interfaces.Repositories;

public interface IEmployeeRepository : IGenericRepository<Employee>
{
    Task<Employee?> GetWithSalaryAsync(Guid employeeId, CancellationToken ct = default);
    Task<Employee?> GetWithDocumentsAsync(Guid employeeId, CancellationToken ct = default);
    Task<Employee?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<string> GenerateEmployeeCodeAsync(CancellationToken ct = default);
    Task<IEnumerable<Employee>> GetByDepartmentAsync(Department dept, CancellationToken ct = default);
    Task<IEnumerable<Employee>> GetActiveEmployeesAsync(CancellationToken ct = default);
}
