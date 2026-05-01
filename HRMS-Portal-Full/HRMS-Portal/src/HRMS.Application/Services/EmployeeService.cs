using AutoMapper;
using HRMS.Application.Common;
using HRMS.Application.DTOs.Employee;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(IUnitOfWork uow, IMapper mapper, ILogger<EmployeeService> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<EmployeeDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var employee = await _uow.Employees.GetWithSalaryAsync(id, ct)
            ?? throw new KeyNotFoundException($"Employee {id} not found.");
        return _mapper.Map<EmployeeDto>(employee);
    }

    public async Task<PagedResult<EmployeeListDto>> GetPagedAsync(EmployeeFilterDto filter, CancellationToken ct = default)
    {
        var (items, total) = await _uow.Employees.GetPagedAsync(
            filter.Page, filter.PageSize,
            e => !e.IsDeleted &&
                 (filter.Department == null || e.Department == filter.Department) &&
                 (filter.Status == null || e.Status == filter.Status) &&
                 (string.IsNullOrEmpty(filter.Search) ||
                  e.FirstName.Contains(filter.Search) ||
                  e.LastName.Contains(filter.Search) ||
                  e.Email.Contains(filter.Search) ||
                  e.EmployeeCode.Contains(filter.Search)),
            q => filter.SortDescending
                ? q.OrderByDescending(e => e.JoiningDate)
                : q.OrderBy(e => e.JoiningDate),
            ct);

        return PagedResult<EmployeeListDto>.Create(
            _mapper.Map<IEnumerable<EmployeeListDto>>(items),
            total, filter.Page, filter.PageSize);
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto, string createdBy, CancellationToken ct = default)
    {
        // Guard: duplicate email
        if (await _uow.Employees.AnyAsync(e => e.Email == dto.Email && !e.IsDeleted, ct))
            throw new InvalidOperationException($"Employee with email {dto.Email} already exists.");

        var code = await _uow.Employees.GenerateEmployeeCodeAsync(ct);

        await _uow.BeginTransactionAsync(ct);
        try
        {
            var salary = new SalaryStructure
            {
                GrossMonthly = dto.Salary.GrossMonthly,
                BasicPercent = dto.Salary.BasicPercent,
                HRAPercent = dto.Salary.HRAPercent,
                SpecialAllowancePercent = dto.Salary.SpecialAllowancePercent,
                ProfessionalTax = dto.Salary.ProfessionalTax,
                EffectiveFrom = dto.JoiningDate,
                IsCurrent = true
            };
            salary.SetAuditOnCreate(createdBy);

            var employee = new Employee
            {
                EmployeeCode = code,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                Department = dto.Department,
                Designation = dto.Designation,
                JoiningDate = dto.JoiningDate,
                Gender = dto.Gender,
                DateOfBirth = dto.DateOfBirth,
                PanNumber = dto.PanNumber?.ToUpper(),
                Address = dto.Address,
                Status = EmploymentStatus.Active,
                SalaryStructure = salary
            };
            employee.SetAuditOnCreate(createdBy);

            await _uow.Employees.AddAsync(employee, ct);

            // Create user account
            var user = new Domain.Entities.User
            {
                Username = dto.Email.Split('@')[0],
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Welcome@123"), // Force change on first login
                Role = dto.RoleToAssign,
                IsActive = true,
                EmployeeId = employee.Id
            };
            user.SetAuditOnCreate(createdBy);
            await _uow.Users.AddAsync(user, ct);

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);

            _logger.LogInformation("Employee {Code} created by {CreatedBy}", code, createdBy);
            return _mapper.Map<EmployeeDto>(employee);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }

    public async Task<EmployeeDto> UpdateAsync(Guid id, UpdateEmployeeDto dto, string updatedBy, CancellationToken ct = default)
    {
        var employee = await _uow.Employees.GetWithSalaryAsync(id, ct)
            ?? throw new KeyNotFoundException($"Employee {id} not found.");

        if (dto.FirstName != null) employee.FirstName = dto.FirstName;
        if (dto.LastName != null) employee.LastName = dto.LastName;
        if (dto.Phone != null) employee.Phone = dto.Phone;
        if (dto.Department.HasValue) employee.Department = dto.Department.Value;
        if (dto.Designation != null) employee.Designation = dto.Designation;
        if (dto.Status.HasValue) employee.Status = dto.Status.Value;
        if (dto.PanNumber != null) employee.PanNumber = dto.PanNumber.ToUpper();
        if (dto.Address != null) employee.Address = dto.Address;
        if (dto.ResignationDate.HasValue) employee.ResignationDate = dto.ResignationDate;
        if (dto.LastWorkingDate.HasValue) employee.LastWorkingDate = dto.LastWorkingDate;

        employee.SetAuditOnUpdate(updatedBy);
        await _uow.Employees.UpdateAsync(employee, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<EmployeeDto>(employee);
    }

    public async Task DeleteAsync(Guid id, string deletedBy, CancellationToken ct = default)
    {
        var employee = await _uow.Employees.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Employee {id} not found.");

        employee.SoftDelete(deletedBy);
        await _uow.Employees.UpdateAsync(employee, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogWarning("Employee {Id} soft-deleted by {DeletedBy}", id, deletedBy);
    }

    public async Task<EmployeeDto> UpdateSalaryAsync(Guid id, UpdateSalaryDto dto, string updatedBy, CancellationToken ct = default)
    {
        var employee = await _uow.Employees.GetWithSalaryAsync(id, ct)
            ?? throw new KeyNotFoundException($"Employee {id} not found.");

        if (employee.SalaryStructure == null)
            throw new InvalidOperationException("Employee has no salary structure.");

        var sal = employee.SalaryStructure;
        sal.GrossMonthly = dto.GrossMonthly;
        sal.BasicPercent = dto.BasicPercent;
        sal.HRAPercent = dto.HRAPercent;
        sal.SpecialAllowancePercent = dto.SpecialAllowancePercent;
        sal.ProfessionalTax = dto.ProfessionalTax;
        sal.IncomeTaxMonthly = dto.IncomeTaxMonthly;
        sal.SetAuditOnUpdate(updatedBy);

        employee.SetAuditOnUpdate(updatedBy);
        await _uow.Employees.UpdateAsync(employee, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<EmployeeDto>(employee);
    }
}
