using HRMS.Application.Common;
using HRMS.Application.DTOs.Employee;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRMS.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _svc;
    public EmployeesController(IEmployeeService svc) => _svc = svc;

    private string CurrentUser => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";

    /// <summary>Get paged list of employees. HR/Admin only.</summary>
    [HttpGet]
    [Authorize(Policy = "HROrAdmin")]
    public async Task<ActionResult<ApiResponse<PagedResult<EmployeeListDto>>>> GetAll(
        [FromQuery] EmployeeFilterDto filter, CancellationToken ct)
    {
        var result = await _svc.GetPagedAsync(filter, ct);
        return Ok(ApiResponse<PagedResult<EmployeeListDto>>.Ok(result));
    }

    /// <summary>Get employee by ID. Employees can only fetch their own record.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> GetById(Guid id, CancellationToken ct)
    {
        // Employees can only view their own record
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role == "Employee")
        {
            var empId = User.FindFirstValue("employeeId");
            if (empId != id.ToString())
                return Forbid();
        }
        var emp = await _svc.GetByIdAsync(id, ct);
        return Ok(ApiResponse<EmployeeDto>.Ok(emp));
    }

    /// <summary>Create a new employee with user account. HR/Admin only.</summary>
    [HttpPost]
    [Authorize(Policy = "HROrAdmin")]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> Create(
        [FromBody] CreateEmployeeDto dto, CancellationToken ct)
    {
        var emp = await _svc.CreateAsync(dto, CurrentUser, ct);
        return CreatedAtAction(nameof(GetById), new { id = emp.Id },
            ApiResponse<EmployeeDto>.Ok(emp, "Employee created successfully. Default password: Welcome@123"));
    }

    /// <summary>Update employee details. HR/Admin only.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "HROrAdmin")]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> Update(
        Guid id, [FromBody] UpdateEmployeeDto dto, CancellationToken ct)
    {
        var emp = await _svc.UpdateAsync(id, dto, CurrentUser, ct);
        return Ok(ApiResponse<EmployeeDto>.Ok(emp));
    }

    /// <summary>Soft delete employee. Admin only.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _svc.DeleteAsync(id, CurrentUser, ct);
        return Ok(new { message = "Employee removed successfully." });
    }

    /// <summary>Update salary structure. HR/Admin only.</summary>
    [HttpPut("{id:guid}/salary")]
    [Authorize(Policy = "HROrAdmin")]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> UpdateSalary(
        Guid id, [FromBody] UpdateSalaryDto dto, CancellationToken ct)
    {
        var emp = await _svc.UpdateSalaryAsync(id, dto, CurrentUser, ct);
        return Ok(ApiResponse<EmployeeDto>.Ok(emp));
    }
}
