using HRMS.Application.Interfaces.Repositories;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(HrmsDbContext ctx) : base(ctx) { }

    public async Task<Employee?> GetWithSalaryAsync(Guid id, CancellationToken ct = default)
        => await _set.Include(e => e.SalaryStructure).FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<Employee?> GetWithDocumentsAsync(Guid id, CancellationToken ct = default)
        => await _set.Include(e => e.SalaryStructure).Include(e => e.Documents)
                     .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<Employee?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _set.FirstOrDefaultAsync(e => e.Email == email, ct);

    public async Task<string> GenerateEmployeeCodeAsync(CancellationToken ct = default)
    {
        // IgnoreQueryFilters to include soft-deleted in count for uniqueness
        var count = await _ctx.Employees.IgnoreQueryFilters().CountAsync(ct);
        return $"EMP-{(count + 1):D4}";
    }

    public async Task<IEnumerable<Employee>> GetByDepartmentAsync(Department dept, CancellationToken ct = default)
        => await _set.Where(e => e.Department == dept).Include(e => e.SalaryStructure).ToListAsync(ct);

    public async Task<IEnumerable<Employee>> GetActiveEmployeesAsync(CancellationToken ct = default)
        => await _set.Where(e => e.Status == EmploymentStatus.Active)
                     .Include(e => e.SalaryStructure).ToListAsync(ct);
}
