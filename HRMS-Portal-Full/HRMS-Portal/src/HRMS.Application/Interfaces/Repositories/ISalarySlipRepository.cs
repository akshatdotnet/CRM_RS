using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces.Repositories;

public interface ISalarySlipRepository : IGenericRepository<SalarySlip>
{
    Task<SalarySlip?> GetByEmployeeMonthYearAsync(Guid employeeId, int month, int year, CancellationToken ct = default);
    Task<IEnumerable<SalarySlip>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct = default);
    Task<IEnumerable<SalarySlip>> GetByMonthYearAsync(int month, int year, CancellationToken ct = default);
}
