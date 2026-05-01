using HRMS.Application.Interfaces.Repositories;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class SalarySlipRepository : GenericRepository<SalarySlip>, ISalarySlipRepository
{
    public SalarySlipRepository(HrmsDbContext ctx) : base(ctx) { }

    public async Task<SalarySlip?> GetByEmployeeMonthYearAsync(Guid empId, int month, int year, CancellationToken ct = default)
        => await _set.Include(s => s.Employee).FirstOrDefaultAsync(s => s.EmployeeId == empId && s.Month == month && s.Year == year, ct);

    public async Task<IEnumerable<SalarySlip>> GetByEmployeeAsync(Guid empId, CancellationToken ct = default)
        => await _set.Include(s => s.Employee).Where(s => s.EmployeeId == empId)
                     .OrderByDescending(s => s.Year).ThenByDescending(s => s.Month).ToListAsync(ct);

    public async Task<IEnumerable<SalarySlip>> GetByMonthYearAsync(int month, int year, CancellationToken ct = default)
        => await _set.Include(s => s.Employee).Where(s => s.Month == month && s.Year == year).ToListAsync(ct);
}
