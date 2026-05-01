using HRMS.Application.Common;
using HRMS.Application.DTOs.Employee;

namespace HRMS.Application.Interfaces.Services;

public interface IEmployeeService
{
    Task<EmployeeDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<EmployeeListDto>> GetPagedAsync(EmployeeFilterDto filter, CancellationToken ct = default);
    Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto, string createdBy, CancellationToken ct = default);
    Task<EmployeeDto> UpdateAsync(Guid id, UpdateEmployeeDto dto, string updatedBy, CancellationToken ct = default);
    Task DeleteAsync(Guid id, string deletedBy, CancellationToken ct = default);
    Task<EmployeeDto> UpdateSalaryAsync(Guid id, UpdateSalaryDto dto, string updatedBy, CancellationToken ct = default);
}
